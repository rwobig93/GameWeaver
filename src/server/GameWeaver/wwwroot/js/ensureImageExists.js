window.ensureImageExists = (options) => {
    options.img.onerror = function () {
        options.img.src = options.fallback;
        options.img.onerror = null; // Prevent infinite loop in case the default image is also missing
        return false;
    };
    
    return true;
}