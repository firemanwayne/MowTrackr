// Layout helpers for the Blazor shell.
window.mowtrackrLayout = (function () {
    var PHONE_QUERY = '(max-width: 768px)';

    return {
        // True on phone-sized screens, where the navigation menu is a drawer rather than a permanent column.
        isMobile: function () {
            return window.matchMedia(PHONE_QUERY).matches;
        }
    };
})();