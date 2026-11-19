// Reads the text on an equipment label from a photo, entirely in the browser (nothing is uploaded).
// The OCR engine (Tesseract, vendored under lib/tesseract) is loaded only when someone first taps "Scan label".
window.mowtrackrLabel = (function () {
    var BASE = 'lib/tesseract/';
    var MAX_SIDE = 1800;
    var LSTM_ONLY = 1;
    var SPARSE_TEXT = '11';

    var enginePromise = null;
    var progressTarget = null;

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
                            progressTarget.invokeMethodAsync('OnProgress', Math.round(message.progress * 100));
                        }
                    }
                });
            }).then(function (created) {
                // Labels are scattered blocks of text, not paragraphs: sparse mode also finds large logo-style names.
                return created.setParameters({ tessedit_pageseg_mode: SPARSE_TEXT }).then(function () { return created; });
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

    // Phone photos are large; a smaller, higher-contrast grayscale copy reads faster and more accurately.
    async function prepare(file) {
        var image = await decode(file);
        var width = image.naturalWidth || image.width;
        var height = image.naturalHeight || image.height;
        var scale = Math.min(1, MAX_SIDE / Math.max(width, height));
        var canvas = document.createElement('canvas');
        canvas.width = Math.round(width * scale);
        canvas.height = Math.round(height * scale);

        var context = canvas.getContext('2d', { willReadFrequently: true });
        context.drawImage(image, 0, 0, canvas.width, canvas.height);

        if (image.close) {
            image.close();
        }

        var pixels = context.getImageData(0, 0, canvas.width, canvas.height);
        var data = pixels.data;
        var low = 255;
        var high = 0;

        for (var i = 0; i < data.length; i += 4) {
            var gray = Math.round(data[i] * 0.299 + data[i + 1] * 0.587 + data[i + 2] * 0.114);
            data[i] = data[i + 1] = data[i + 2] = gray;
            if (gray < low) { low = gray; }
            if (gray > high) { high = gray; }
        }

        // Stretch the darkest and lightest tones to the full range so faint, worn or glossy labels read better.
        var range = Math.max(1, high - low);
        for (var j = 0; j < data.length; j += 4) {
            var stretched = Math.round((data[j] - low) * 255 / range);
            data[j] = data[j + 1] = data[j + 2] = stretched;
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

        // Reads the first photo chosen in a file input. Returns { text, confidence }.
        recognize: async function (input, dotNetReference) {
            var file = input.files && input.files[0];
            if (!file) {
                throw new Error('No photo was chosen.');
            }

            progressTarget = dotNetReference;

            try {
                var engine = await getWorker();
                var prepared = await prepare(file);
                var result = await engine.recognize(prepared);

                return { text: result.data.text || '', confidence: Math.round(result.data.confidence || 0) };
            } finally {
                progressTarget = null;
            }
        }
    };
})();
