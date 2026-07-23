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

export function dispose() {
    if (previewContainer && clickHandler) {
        previewContainer.removeEventListener("click", clickHandler);
    }

    clickHandler = null;
    previewContainer = null;
    dotNetRef = null;
}
