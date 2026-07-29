window.kineticReportsMvcViewer = window.kineticReportsMvcViewer || {};

(function (viewerApi) {
    function decodeBase64Utf8(value) {
        if (!value) {
            return "";
        }

        const binary = atob(value);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i += 1) {
            bytes[i] = binary.charCodeAt(i);
        }

        if (window.TextDecoder) {
            return new TextDecoder("utf-8").decode(bytes);
        }

        let text = "";
        for (let i = 0; i < bytes.length; i += 1) {
            text += String.fromCharCode(bytes[i]);
        }

        return decodeURIComponent(escape(text));
    }

    function printReportMarkup(markup) {
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
        printDocument.open();
        printDocument.write(markup);
        printDocument.close();

        const cleanup = () => {
            if (iframe.parentNode) {
                iframe.parentNode.removeChild(iframe);
            }
        };

        const triggerPrint = () => {
            const head = printDocument.head || printDocument.getElementsByTagName("head")[0];
            if (head) {
                const link = printDocument.createElement("link");
                link.rel = "stylesheet";
                link.href = `${window.location.origin}/_content/KineticReports.Viewer.Mvc/css/kinetic-report-viewer.css`;
                head.appendChild(link);
            }

            printWindow.focus();
            setTimeout(() => {
                printWindow.print();
                setTimeout(cleanup, 300);
            }, 120);
        };

        if (printDocument.readyState === "complete") {
            triggerPrint();
        } else {
            iframe.onload = triggerPrint;
        }
    }

    function initialize(viewerId) {
        if (!viewerId) {
            return;
        }

        const root = document.getElementById(viewerId);
        if (!root) {
            return;
        }

        const pages = Array.from(root.querySelectorAll(".preview-content .kinetic-page"));
        const indicator = root.querySelector("[data-role='page-indicator']");
        const printMarkupInput = root.querySelector("[data-role='print-markup']");

        let currentPage = pages.length > 0 ? 1 : 0;

        const firstButton = root.querySelector("[data-action='first']");
        const prevButton = root.querySelector("[data-action='prev']");
        const nextButton = root.querySelector("[data-action='next']");
        const lastButton = root.querySelector("[data-action='last']");
        const printButton = root.querySelector("[data-action='print']");

        function showPage(pageIndex) {
            const page = pages[pageIndex - 1];
            if (page) {
                page.style.display = "block";
            }
        }

        function hidePage(pageIndex) {
            const page = pages[pageIndex - 1];
            if (page) {
                page.style.display = "none";
            }
        }

        function setCurrentPage(nextPage) {
            if (pages.length === 0) {
                currentPage = 0;
                updateButtons();
                return;
            }

            const clampedPage = Math.min(Math.max(nextPage, 1), pages.length);
            if (currentPage > 0 && currentPage !== clampedPage) {
                hidePage(currentPage);
            }

            currentPage = clampedPage;
            showPage(currentPage);
            updateButtons();
        }

        function updateButtons() {
            const hasPages = pages.length > 0;
            const canBack = hasPages && currentPage > 1;
            const canForward = hasPages && currentPage < pages.length;

            if (firstButton) firstButton.disabled = !canBack;
            if (prevButton) prevButton.disabled = !canBack;
            if (nextButton) nextButton.disabled = !canForward;
            if (lastButton) lastButton.disabled = !canForward;
            if (printButton) printButton.disabled = !hasPages;

            if (indicator) {
                const displayPage = hasPages ? currentPage : 0;
                indicator.textContent = `Page ${displayPage} of ${Math.max(pages.length, 0)}`;
            }
        }

        if (firstButton) {
            firstButton.addEventListener("click", () => {
                if (pages.length === 0) return;
                setCurrentPage(1);
            });
        }

        if (prevButton) {
            prevButton.addEventListener("click", () => {
                if (pages.length === 0 || currentPage <= 1) return;
                setCurrentPage(currentPage - 1);
            });
        }

        if (nextButton) {
            nextButton.addEventListener("click", () => {
                if (pages.length === 0 || currentPage >= pages.length) return;
                setCurrentPage(currentPage + 1);
            });
        }

        if (lastButton) {
            lastButton.addEventListener("click", () => {
                if (pages.length === 0) return;
                setCurrentPage(pages.length);
            });
        }

        if (printButton) {
            printButton.addEventListener("click", () => {
                const encoded = printMarkupInput ? printMarkupInput.value : "";
                const markup = decodeBase64Utf8(encoded);
                printReportMarkup(markup);
            });
        }

        setCurrentPage(currentPage);
    }

    viewerApi.initialize = initialize;
    viewerApi.printReportMarkup = printReportMarkup;
})(window.kineticReportsMvcViewer);
