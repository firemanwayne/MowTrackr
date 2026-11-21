# Tesseract OCR (vendored)

Used by the Scan label feature to read equipment labels in the browser; nothing is uploaded. Loaded only when a user taps Scan label.

- tesseract.js 7.0.0 (Apache-2.0): tesseract.min.js, worker.min.js
- tesseract.js-core 7.0.0 (Apache-2.0): core/ (SIMD and relaxed-SIMD builds; older devices without SIMD are not supported)
- English language data, 4.0.0_best_int (Apache-2.0): lang/eng.traineddata.gz

They are copied unmodified from the npm packages of the same names. They are served from our own origin so the Content-Security-Policy does not need to allow a CDN.
