window.getClientTimeZone = function () {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
};
