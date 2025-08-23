// A flexible download helper callable from .NET via IJSRuntime.InvokeVoidAsync("Download", options)
// Supported inputs for options.byteArray:
// - Base64 string (set options.isBase64 = true)
// - Plain text string (UTF-8)
// - Uint8Array (if passed from JS or via custom interop)
// - Data URL string ("data:...") – will be used as-is
//
// Required:
// - options.byteArray (string/ByteArray)
// - options.fileName (string)
// - options.mimeType (string), e.g., "application/json" or "application/octet-stream"
// Optional:
// - options.isBase64 (boolean), default: false

window.Download = (options) => {
    try {
        if (!options) throw new Error("No download options were provided, this is likely a bug");
        const fileName = (options.fileName && String(options.fileName).trim()) || "download";
        const mimeType = (options.mimeType && String(options.mimeType).trim()) || "application/octet-stream";
        const input = options.byteArray;

        if (input == null) throw new Error("Download content byte array provided is null or undefined");

        // If a full data URL was provided, just use it directly
        if (typeof input === "string" && input.startsWith("data:")) {
            triggerDownloadFromUrl(input, fileName);
            return;
        }

        // Build a Blob from the provided input
        const blob = toBlob(input, mimeType, !!options.isBase64);
        if (!blob) throw new Error("Failed to construct content blob for download");

        // Create an object URL and trigger a download
        const url = URL.createObjectURL(blob);
        triggerDownloadFromUrl(url, fileName, true);
    } catch (e) {
        let message = (e?.message ?? "Unknown error");
        console.error(message, e);
        throw message;
    }
};

function toBlob(input, mimeType, isBase64) {
    // Uint8Array provided
    if (typeof Uint8Array !== "undefined" && input instanceof Uint8Array) {
        return new Blob([input], { type: mimeType });
    }

    // String provided
    if (typeof input === "string") {
        if (isBase64 === true) {
            // Decode base64 string into bytes
            const byteChars = atob(input);
            const byteNumbers = new Array(byteChars.length);
            for (let i = 0; i < byteChars.length; i++) {
                byteNumbers[i] = byteChars.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            return new Blob([byteArray], { type: mimeType });
        } else {
            // Treat as UTF-8 text
            return new Blob([input], { type: mimeType });
        }
    }

    // ArrayBuffer provided
    if (typeof ArrayBuffer !== "undefined" && input instanceof ArrayBuffer) {
        return new Blob([new Uint8Array(input)], { type: mimeType });
    }

    // Fallback: try to wrap whatever it is
    try {
        return new Blob([input], { type: mimeType });
    } catch {
        return null;
    }
}

function triggerDownloadFromUrl(url, fileName, revokeAfterUse = false) {
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;

    // Ensure it's not blocked by popup blockers by simulating user click
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    // Revoke blob URL shortly after to free memory
    if (revokeAfterUse) {
        setTimeout(() => {
            try { URL.revokeObjectURL(url); } catch { /* ignore */ }
        }, 0);
    }
}