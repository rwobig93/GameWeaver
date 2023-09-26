window.blazorHelpers = {
    scrollToFragment: (elementId) => {
        var element = document.getElementById(elementId);

        if (element) {
            element.scrollIntoView({
                behavior: 'smooth'
            });
        }
    },
    scrollToTop: () => {
        document.documentElement.scrollTop = 0;
    }
};