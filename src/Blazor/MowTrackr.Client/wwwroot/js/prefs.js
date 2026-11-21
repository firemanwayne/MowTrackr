// Theme + preference helpers. Loaded in <head> so the saved theme is applied before first paint.
window.mowtrackr = (function () {
    var THEME_KEY = 'mowtrackr.theme';
    var LANG_KEY = 'mowtrackr.lang';
    var CSS_ROOT = '_content/Radzen.Blazor/css/';

    function read(key) {
        try { return localStorage.getItem(key); } catch (e) { return null; }
    }

    function write(key, value) {
        try { localStorage.setItem(key, value); } catch (e) { /* storage unavailable */ }
    }

    function applyTheme(theme) {
        var dark = theme === 'Dark';
        document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
        var link = document.getElementById('radzen-theme');
        var href = CSS_ROOT + (dark ? 'standard-dark.css' : 'standard.css');
        if (link && link.getAttribute('href') !== href) { link.setAttribute('href', href); }
    }

    function storedTheme() {
        var stored = read(THEME_KEY);
        if (stored === 'Dark' || stored === 'Light') { return stored; }
        var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        return prefersDark ? 'Dark' : 'Light';
    }

    // The page language follows the chosen language, so screen readers pronounce it correctly.
    function applyLanguage(language) {
        var code = String(language || 'en').toLowerCase().indexOf('es') === 0 ? 'es' : 'en';
        document.documentElement.setAttribute('lang', code);
    }

    applyTheme(storedTheme());
    applyLanguage(read(LANG_KEY) || navigator.language);

    return {
        applyTheme: function (theme) { write(THEME_KEY, theme); applyTheme(theme); },
        getTheme: function () { return storedTheme(); },
        getLanguage: function () { return read(LANG_KEY) || (navigator.language || 'en'); },
        setLanguage: function (language) { write(LANG_KEY, language); applyLanguage(language); }
    };
})();