// Download JavaScript method to initiate a file download
// Inputs:
//  - byteArray: Base64 encoded string
//  - mimeType: Mime type of the file
//  - fileName: Default name of the file
//

window.Download = (options) => {
    let fileUrl = "data:" + options.mimeType + ";base64," + options.byteArray;

    try {
        fetch(fileUrl)
            .then(response => response.blob())
            .then(blob => {
                let link = window.document.createElement("a");
                link.href = window.URL.createObjectURL(blob);
                link.download = options.fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            });
    } catch (error) {
        console.error(error?.message ?? "Unknown error occurred attempting to download file");
    }
}