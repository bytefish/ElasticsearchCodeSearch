export function isDevice() {
    return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini|mobile/i.test(navigator.userAgent);
}

export function switchHighlightStyle(dark) {

    const darkNode = document.querySelector(`link[title="dark"]`);
    const lightNode = document.querySelector(`link[title="dark"]`);

    if (dark) {
        if (darkNode) {
            darkNode.removeAttribute("disabled");
        }

        if (lightNode) {
            lightNode.setAttribute("disabled", "disabled");
        }
    }
    else {
        if (lightNode) {
            lightNode.removeAttribute("disabled");
        }

        if (darkNode) {
            darkNode.setAttribute("disabled", "disabled")
        }
    }
}

export function isDarkMode() {
    let matched = window.matchMedia("(prefers-color-scheme: dark)").matches;

    if (matched)
        return true;
    else
        return false;
}

export function switchDirection(dir) {
    document.dir = dir;
    const container = document.getElementById('container');
    container.style.direction = dir;
}