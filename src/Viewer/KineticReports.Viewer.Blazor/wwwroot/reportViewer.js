let dotNetRef = null;
let previewContainer = null;
let clickHandler = null;

export function registerHost(reference, previewContainerId) {
    dotNetRef = reference;

    const containerElement = typeof previewContainerId === "string"
        ? document.getElementById(previewContainerId)
        : null;

    if (previewContainer && clickHandler) {
        previewContainer.removeEventListener("click", clickHandler);
    }

    previewContainer = containerElement;
    if (!previewContainer) {
        return;
    }

    clickHandler = (event) => {
        if (!dotNetRef) {
            return;
        }

        const target = event.target;
        const page = target && target.closest ? target.closest(".kinetic-page") : null;
        if (!page) {
            return;
        }

        const rect = page.getBoundingClientRect();
        const x = event.clientX - rect.left;
        const y = event.clientY - rect.top;

        const id = page.id || "";
        const match = id.match(/page-(\d+)/i);
        const parsedPage = match && match[1] ? parseInt(match[1], 10) : 1;
        const pageNumber = Number.isFinite(parsedPage) && parsedPage > 0 ? parsedPage : 1;

        dotNetRef.invokeMethodAsync("OnPreviewClicked", x, y, pageNumber);
    };

    previewContainer.addEventListener("click", clickHandler);
}

export function printReportMarkup(markup) {
    if (!markup || typeof markup !== "string") {
        return;
    }

    const iframe = document.createElement("iframe");
    iframe.style.position = "fixed";
    iframe.style.right = "0";
    iframe.style.bottom = "0";
    iframe.style.width = "0";
    iframe.style.height = "0";
    iframe.style.border = "0";
    iframe.setAttribute("aria-hidden", "true");

    document.body.appendChild(iframe);

    const printWindow = iframe.contentWindow;
    if (!printWindow) {
        document.body.removeChild(iframe);
        return;
    }

    const printDocument = printWindow.document;
    const trimmedMarkup = markup.trim();
    const documentMarkup = /^<!doctype|^<html/i.test(trimmedMarkup)
        ? trimmedMarkup
        : `<!doctype html><html><head><meta charset="utf-8"><base href="${window.location.origin}/"></head><body>${markup}</body></html>`;

    printDocument.open();
    printDocument.write(documentMarkup);
    printDocument.close();

    const stylesheetReady = new Promise((resolve) => {
        const head = printDocument.head || printDocument.getElementsByTagName("head")[0];
        if (!head) {
            resolve();
            return;
        }

        const link = printDocument.createElement("link");
        link.rel = "stylesheet";
        link.href = `${window.location.origin}/_content/KineticReports.Viewer.Blazor/css/kinetic-report-viewer.css`;
        link.onload = () => resolve();
        link.onerror = () => resolve();
        head.appendChild(link);
    });

    const cleanup = () => {
        if (iframe.parentNode) {
            iframe.parentNode.removeChild(iframe);
        }
    };

    const triggerPrint = () => {
        const fontsReady = printDocument.fonts && printDocument.fonts.ready
            ? printDocument.fonts.ready.catch(() => { })
            : Promise.resolve();

        Promise.all([stylesheetReady, fontsReady]).finally(() => {
            printWindow.focus();
            setTimeout(() => {
                printWindow.print();
                setTimeout(cleanup, 250);
            }, 100);
        });
    };

    if (printDocument.readyState === "complete") {
        triggerPrint();
    } else {
        iframe.onload = triggerPrint;
    }
}

export function downloadFromBase64(fileName, mimeType, base64Content) {
    if (!base64Content || typeof base64Content !== "string") {
        return;
    }

    const binary = atob(base64Content);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i += 1) {
        bytes[i] = binary.charCodeAt(i);
    }

    const blob = new Blob([bytes], { type: mimeType || "application/octet-stream" });
    const url = URL.createObjectURL(blob);

    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName || "report.bin";
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);

    URL.revokeObjectURL(url);
}

export function dispose() {
    if (previewContainer && clickHandler) {
        previewContainer.removeEventListener("click", clickHandler);
    }

    clickHandler = null;
    previewContainer = null;
    dotNetRef = null;
}
