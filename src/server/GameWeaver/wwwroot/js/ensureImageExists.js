window.ensureImageExists = (options) => {
    options.img.onerror = function () {
        options.img.src = options.fallback;
        options.img.onerror = null;
    };
    
    return options.img.src;
}