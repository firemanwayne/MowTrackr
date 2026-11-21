// Reads the text on an equipment label from a photo, entirely in the browser (nothing is uploaded).
// The OCR engine (Tesseract, vendored under lib/tesseract) is loaded only when someone first taps "Scan label".
//
// Every photo is read several times, because stamped data plates are hard: a plain grayscale pass finds the brand and
// model, and enlarged, inverted, black-and-white passes at a couple of thresholds find light-on-dark serial numbers
// (the right threshold varies with the photo, so more than one is tried). The texts are joined and the parser picks the
// fields out of the combined result.
window.mowtrackrLabel = (function () {
    var BASE = 'lib/tesseract/';
    var MAX_SIDE = 1800;
    var MAX_UPSCALE = 3;
    var LSTM_ONLY = 1;
    var BLOCK_TEXT = '6';

    var enginePromise = null;
    var progressTarget = null;
    var passIndex = 0;

    // plate: null reads a plain grayscale copy; a number is the share of the automatic (Otsu) threshold to use for
    // the enlarged black-and-white plate copy. Lower values keep only the brightest lettering.
    var PASSES = [null, 0.87, 0.75];

    function loadScript(src) {
        return new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = src;
            script.onload = resolve;
            script.onerror = function () { reject(new Error('Could not load ' + src)); };
            document.head.appendChild(script);
        });
    }

    // Starts the engine once and keeps it for later scans.
    function getWorker() {
        if (!enginePromise) {
            enginePromise = loadScript(BASE + 'tesseract.min.js').then(function () {
                return window.Tesseract.createWorker('eng', LSTM_ONLY, {
                    workerPath: BASE + 'worker.min.js',
                    corePath: BASE + 'core',
                    langPath: BASE + 'lang',
                    workerBlobURL: false,
                    logger: function (message) {
                        if (progressTarget && message.status === 'recognizing text') {
                            // The passes share the 0-100 range.
                            var overall = (passIndex + message.progress) / PASSES.length;
                            progressTarget.invokeMethodAsync('OnProgress', Math.round(overall * 100));
                        }
                    }
                });
            }).then(function (created) {
                // Block mode reads a label as lines of text; sparse mode missed whole lines on stamped plates.
                return created.setParameters({ tessedit_pageseg_mode: BLOCK_TEXT }).then(function () { return created; });
            }).catch(function (error) {
                enginePromise = null;
                throw error;
            });
        }

        return enginePromise;
    }

    // Decodes the photo without a blob: URL (the Content-Security-Policy only allows self and data: images).
    function decode(file) {
        if (window.createImageBitmap) {
            return window.createImageBitmap(file);
        }

        return new Promise(function (resolve, reject) {
            var reader = new FileReader();
            reader.onerror = function () { reject(new Error('That file is not an image the browser can read.')); };
            reader.onload = function () {
                var image = new Image();
                image.onload = function () { resolve(image); };
                image.onerror = function () { reject(new Error('That file is not an image the browser can read.')); };
                image.src = reader.result;
            };
            reader.readAsDataURL(file);
        });
    }

    // Picks the gray level that best separates dark from light pixels (Otsu's method).
    function otsuThreshold(grays) {
        var histogram = new Array(256).fill(0);
        for (var i = 0; i < grays.length; i++) {
            histogram[grays[i]]++;
        }

        var total = grays.length;
        var sumAll = 0;
        for (var t = 0; t < 256; t++) {
            sumAll += t * histogram[t];
        }

        var sumBackground = 0;
        var weightBackground = 0;
        var best = 0;
        var bestVariance = -1;

        for (var level = 0; level < 256; level++) {
            weightBackground += histogram[level];
            if (weightBackground === 0) { continue; }

            var weightForeground = total - weightBackground;
            if (weightForeground === 0) { break; }

            sumBackground += level * histogram[level];
            var meanBackground = sumBackground / weightBackground;
            var meanForeground = (sumAll - sumBackground) / weightForeground;
            var variance = weightBackground * weightForeground * Math.pow(meanBackground - meanForeground, 2);

            if (variance > bestVariance) {
                bestVariance = variance;
                best = level;
            }
        }

        return best;
    }

    // Builds the image for one pass. With no threshold factor it is a smaller, higher-contrast grayscale copy; with one
    // it is enlarged, inverted and thresholded to black on white, which is how Tesseract reads light lettering stamped on
    // a dark plate best.
    async function prepare(file, thresholdFactor) {
        var plate = thresholdFactor !== null;
        var image = await decode(file);
        var width = image.naturalWidth || image.width;
        var height = image.naturalHeight || image.height;
        var scale = Math.min(plate ? MAX_UPSCALE : 1, MAX_SIDE / Math.max(width, height));
        var canvas = document.createElement('canvas');
        canvas.width = Math.round(width * scale);
        canvas.height = Math.round(height * scale);

        var context = canvas.getContext('2d', { willReadFrequently: true });
        context.imageSmoothingQuality = 'high';
        context.drawImage(image, 0, 0, canvas.width, canvas.height);

        if (image.close) {
            image.close();
        }

        var pixels = context.getImageData(0, 0, canvas.width, canvas.height);
        var data = pixels.data;
        var grays = new Uint8Array(data.length / 4);
        var low = 255;
        var high = 0;

        for (var i = 0, p = 0; i < data.length; i += 4, p++) {
            var gray = Math.round(data[i] * 0.299 + data[i + 1] * 0.587 + data[i + 2] * 0.114);
            grays[p] = gray;
            if (gray < low) { low = gray; }
            if (gray > high) { high = gray; }
        }

        // Stretch the darkest and lightest tones to the full range so faint, worn or glossy labels read better.
        var range = Math.max(1, high - low);
        for (var k = 0; k < grays.length; k++) {
            grays[k] = Math.round((grays[k] - low) * 255 / range);
        }

        var threshold = plate ? Math.round(otsuThreshold(grays) * thresholdFactor) : -1;

        for (var j = 0, q = 0; j < data.length; j += 4, q++) {
            var value = grays[q];

            if (plate) {
                // Light lettering on a dark plate becomes dark lettering on white.
                value = value > threshold ? 0 : 255;
            }

            data[j] = data[j + 1] = data[j + 2] = value;
        }

        context.putImageData(pixels, 0, 0);

        return new Promise(function (resolve) { canvas.toBlob(resolve, 'image/png'); });
    }

    return {
        // Opens the camera (or photo picker) attached to a hidden file input.
        openPicker: function (input) {
            input.value = '';
            input.click();
        },

        // Reads the first photo chosen in a file input. Returns { text, confidence } with every pass combined.
        recognize: async function (input, dotNetReference) {
            var file = input.files && input.files[0];
            if (!file) {
                throw new Error('No photo was chosen.');
            }

            progressTarget = dotNetReference;

            try {
                var engine = await getWorker();
                var texts = [];
                var confidence = 0;

                for (passIndex = 0; passIndex < PASSES.length; passIndex++) {
                    var result = await engine.recognize(await prepare(file, PASSES[passIndex]));

                    texts.push(result.data.text || '');
                    confidence = Math.max(confidence, Math.round(result.data.confidence || 0));
                }

                return { text: texts.join('\n'), confidence: confidence };
            } finally {
                progressTarget = null;
            }
        }
    };
})();
