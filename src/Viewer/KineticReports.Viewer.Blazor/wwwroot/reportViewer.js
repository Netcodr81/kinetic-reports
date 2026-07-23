let dotNetRef = null;
let messageHandler = null;
let frameWindow = null;

export function registerHost(reference, frameId) {
    dotNetRef = reference;

    const frameElement = typeof frameId === "string"
        ? document.getElementById(frameId)
        : null;
    frameWindow = frameElement && frameElement.contentWindow
        ? frameElement.contentWindow
        : null;

    if (messageHandler !== null) {
        window.removeEventListener("message", messageHandler);
    }

    messageHandler = (event) => {
        const payload = event?.data;
        if (!payload || payload.type !== "kr-preview-click" || dotNetRef === null) {
            return;
        }

        if (frameWindow !== null && event.source !== frameWindow) {
            return;
        }

        const x = Number(payload.x);
        const y = Number(payload.y);
        const pageNumber = Number(payload.pageNumber);

        if (!Number.isFinite(x) || !Number.isFinite(y)) {
            return;
        }

        dotNetRef.invokeMethodAsync(
            "OnPreviewClicked",
            x,
            y,
            Number.isFinite(pageNumber) ? pageNumber : 1);
    };

    window.addEventListener("message", messageHandler);
}

export function dispose() {
    if (messageHandler !== null) {
        window.removeEventListener("message", messageHandler);
        messageHandler = null;
    }

    frameWindow = null;
    dotNetRef = null;
}
