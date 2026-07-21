namespace KineticReports.Layout.Pagination;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

/// <summary>
/// Distributes a flat list of measured <see cref="BandElement"/> objects across
/// one or more <see cref="PageElement"/> instances, respecting page breaks,
/// <see cref="BandElement.KeepTogether"/>, and <see cref="BandElement.ForcePageBreakBefore"/>.
/// </summary>
internal sealed class PaginationEngine
{
    /// <summary>
    /// Runs the full LayoutSizing → Arrange → Paginate pipeline and returns the ordered
    /// list of pages that form the <see cref="ReportLayout"/>.
    /// </summary>
    /// <param name="bands">All bands, including page-header and page-footer bands.</param>
    /// <param name="options">Page dimensions and margin configuration.</param>
    /// <param name="context">Text layout and image-resolution services.</param>
    /// <returns>An ordered, non-empty list of fully arranged pages.</returns>
    internal IReadOnlyList<PageElement> Paginate(
        IReadOnlyList<BandElement> bands,
        LayoutOptions options,
        ILayoutSizingContext context)
    {
        // Default style used for structural page/section elements created by the engine.
        var engineStyle = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f };

        // --- Partition bands ---
        var pageHeaderBands = bands.Where(b => b.Kind == BandKind.PageHeader).ToList();
        var pageFooterBands = bands.Where(b => b.Kind == BandKind.PageFooter).ToList();
        var bodyBands = bands.Where(b => b.Kind != BandKind.PageHeader && b.Kind != BandKind.PageFooter).ToList();

        // --- LayoutSizing ---
        float contentWidth = options.PageWidth - options.PageMargins.Horizontal;
        var measureSize = new Size(contentWidth, float.PositiveInfinity);

        float pageHeaderHeight = MeasureBandsVertical(pageHeaderBands, measureSize, context);
        float pageFooterHeight = MeasureBandsVertical(pageFooterBands, measureSize, context);

        foreach (var band in bodyBands)
            band.LayoutSize(measureSize, context);

        // --- Compute available body height ---
        float bodyAreaHeight = options.PageHeight
            - options.PageMargins.Vertical
            - pageHeaderHeight
            - pageFooterHeight;

        // Guard against degenerate configurations.
        if (bodyAreaHeight <= 0f)
            bodyAreaHeight = options.PageHeight - options.PageMargins.Vertical;

        // --- Distribute body bands across pages ---
        var pages = new List<PageElement>();
        int pageNumber = 1;
        float accumulatedHeight = 0f;
        var currentPage = new List<BandElement>();

        foreach (var band in bodyBands)
        {
            bool forceBreak = band.ForcePageBreakBefore && currentPage.Count > 0;
            bool doesNotFit = accumulatedHeight + band.DesiredSize.Height > bodyAreaHeight;

            if ((forceBreak || doesNotFit) && currentPage.Count > 0)
            {
                pages.Add(BuildPage(
                    pageNumber++, currentPage,
                    pageHeaderBands, pageFooterBands,
                    options, pageHeaderHeight, pageFooterHeight,
                    engineStyle));

                currentPage = [];
                accumulatedHeight = 0f;
            }

            currentPage.Add(band);
            accumulatedHeight += band.DesiredSize.Height;
        }

        // Always produce at least one page (even an empty report emits a blank page).
        pages.Add(BuildPage(
            pageNumber, currentPage,
            pageHeaderBands, pageFooterBands,
            options, pageHeaderHeight, pageFooterHeight,
            engineStyle));

        return pages;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static float MeasureBandsVertical(
        List<BandElement> bands, Size measureSize, ILayoutSizingContext context)
    {
        float total = 0f;
        foreach (var b in bands)
        {
            b.LayoutSize(measureSize, context);
            total += b.DesiredSize.Height;
        }
        return total;
    }

    private static PageElement BuildPage(
        int pageNumber,
        List<BandElement> bodyBands,
        List<BandElement> pageHeaderBands,
        List<BandElement> pageFooterBands,
        LayoutOptions options,
        float pageHeaderHeight,
        float pageFooterHeight,
        ResolvedStyle engineStyle)
    {
        float marginLeft = options.PageMargins.Left;
        float marginTop = options.PageMargins.Top;
        float contentWidth = options.PageWidth - options.PageMargins.Horizontal;

        // --- Arrange page-header bands ---
        SectionElement? header = null;
        if (pageHeaderBands.Count > 0)
        {
            float y = marginTop;
            var arranged = new List<LayoutElement>(pageHeaderBands.Count);
            foreach (var band in pageHeaderBands)
            {
                band.Arrange(new Rect(marginLeft, y, contentWidth, band.DesiredSize.Height));
                arranged.Add(band);
                y += band.DesiredSize.Height;
            }

            header = new SectionElement
            {
                Id = $"PageHeader-p{pageNumber}",
                Style = engineStyle,
                Children = arranged
            };
            header.Arrange(new Rect(marginLeft, marginTop, contentWidth, pageHeaderHeight));
        }

        // --- Arrange page-footer bands ---
        SectionElement? footer = null;
        if (pageFooterBands.Count > 0)
        {
            float footerTop = options.PageHeight - options.PageMargins.Bottom - pageFooterHeight;
            float y = footerTop;
            var arranged = new List<LayoutElement>(pageFooterBands.Count);
            foreach (var band in pageFooterBands)
            {
                band.Arrange(new Rect(marginLeft, y, contentWidth, band.DesiredSize.Height));
                arranged.Add(band);
                y += band.DesiredSize.Height;
            }

            footer = new SectionElement
            {
                Id = $"PageFooter-p{pageNumber}",
                Style = engineStyle,
                Children = arranged
            };
            footer.Arrange(new Rect(marginLeft, footerTop, contentWidth, pageFooterHeight));
        }

        // --- Arrange body bands ---
        float bodyTop = marginTop + pageHeaderHeight;
        float bodyY = bodyTop;
        var children = new List<LayoutElement>(bodyBands.Count);
        foreach (var band in bodyBands)
        {
            band.Arrange(new Rect(marginLeft, bodyY, contentWidth, band.DesiredSize.Height));
            children.Add(band);
            bodyY += band.DesiredSize.Height;
        }

        // --- Build and finalise the page ---
        var page = new PageElement
        {
            Id = $"Page-{pageNumber}",
            Style = engineStyle,
            PageWidth = options.PageWidth,
            PageHeight = options.PageHeight,
            PageNumber = pageNumber,
            Header = header,
            Footer = footer,
            Children = children
        };

        page.Arrange(new Rect(0f, 0f, options.PageWidth, options.PageHeight));

        return page;
    }
}
