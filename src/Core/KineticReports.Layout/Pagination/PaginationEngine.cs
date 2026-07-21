namespace KineticReports.Layout.Pagination;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

/// <summary>
/// Distributes a flat list of measured <see cref="ReportBlock"/> objects across
/// one or more <see cref="PageBlock"/> instances, respecting page breaks,
/// <see cref="ReportBlock.KeepTogether"/>, and <see cref="ReportBlock.ForcePageBreakBefore"/>.
/// </summary>
internal sealed class PaginationEngine
{
    /// <summary>
    /// Runs the full LayoutSizing → Arrange → Paginate pipeline and returns the ordered
    /// list of pages that form the <see cref="ReportLayout"/>.
    /// </summary>
    /// <param name="blocks">All blocks, including page-header and page-footer blocks.</param>
    /// <param name="options">Page dimensions and margin configuration.</param>
    /// <param name="context">Text layout and image-resolution services.</param>
    /// <returns>An ordered, non-empty list of fully arranged pages.</returns>
    internal IReadOnlyList<PageBlock> Paginate(
        IReadOnlyList<ReportBlock> blocks,
        LayoutOptions options,
        ILayoutSizingContext context)
    {
        // Default style used for structural page/section elements created by the engine.
        var engineStyle = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        // --- Partition blocks ---
        var pageHeaderBlocks = blocks.Where(b => b.Kind == BlockType.PageHeader).ToList();
        var pageFooterBlocks = blocks.Where(b => b.Kind == BlockType.PageFooter).ToList();
        var bodyBlocks = blocks.Where(b => b.Kind != BlockType.PageHeader && b.Kind != BlockType.PageFooter).ToList();

        // --- LayoutSizing ---
        float contentWidth = options.PageWidth - options.PageMargins.Horizontal;
        var measureSize = new Size(contentWidth, float.PositiveInfinity);

        float pageHeaderHeight = MeasureBlocksVertical(pageHeaderBlocks, measureSize, context);
        float pageFooterHeight = MeasureBlocksVertical(pageFooterBlocks, measureSize, context);

        foreach (var block in bodyBlocks)
            block.LayoutSize(measureSize, context);

        // --- Compute available body height ---
        float bodyAreaHeight = options.PageHeight
            - options.PageMargins.Vertical
            - pageHeaderHeight
            - pageFooterHeight;

        // Guard against degenerate configurations.
        if (bodyAreaHeight <= 0f)
            bodyAreaHeight = options.PageHeight - options.PageMargins.Vertical;

        // --- Distribute body blocks across pages ---
        var pages = new List<PageBlock>();
        int pageNumber = 1;
        float accumulatedHeight = 0f;
        var currentPage = new List<ReportBlock>();

        foreach (var block in bodyBlocks)
        {
            bool forceBreak = block.ForcePageBreakBefore && currentPage.Count > 0;
            bool doesNotFit = accumulatedHeight + block.DesiredSize.Height > bodyAreaHeight;

            if ((forceBreak || doesNotFit) && currentPage.Count > 0)
            {
                pages.Add(BuildPage(
                    pageNumber++, currentPage,
                    pageHeaderBlocks, pageFooterBlocks,
                    options, pageHeaderHeight, pageFooterHeight,
                    engineStyle));

                currentPage = [];
                accumulatedHeight = 0f;
            }

            currentPage.Add(block);
            accumulatedHeight += block.DesiredSize.Height;
        }

        // Always produce at least one page (even an empty report emits a blank page).
        pages.Add(BuildPage(
            pageNumber, currentPage,
            pageHeaderBlocks, pageFooterBlocks,
            options, pageHeaderHeight, pageFooterHeight,
            engineStyle));

        return pages;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static float MeasureBlocksVertical(
        List<ReportBlock> blocks, Size measureSize, ILayoutSizingContext context)
    {
        float total = 0f;
        foreach (var block in blocks)
        {
            block.LayoutSize(measureSize, context);
            total += block.DesiredSize.Height;
        }
        return total;
    }

    private static PageBlock BuildPage(
        int pageNumber,
        List<ReportBlock> bodyBlocks,
        List<ReportBlock> pageHeaderBlocks,
        List<ReportBlock> pageFooterBlocks,
        LayoutOptions options,
        float pageHeaderHeight,
        float pageFooterHeight,
        AppliedStyle engineStyle)
    {
        float marginLeft = options.PageMargins.Left;
        float marginTop = options.PageMargins.Top;
        float contentWidth = options.PageWidth - options.PageMargins.Horizontal;

        // --- Arrange page-header blocks ---
        SectionBlock? header = null;
        if (pageHeaderBlocks.Count > 0)
        {
            float y = marginTop;
            var arranged = new List<LayoutBlock>(pageHeaderBlocks.Count);
            foreach (var block in pageHeaderBlocks)
            {
                block.Arrange(new Rect(marginLeft, y, contentWidth, block.DesiredSize.Height));
                arranged.Add(block);
                y += block.DesiredSize.Height;
            }

            header = new SectionBlock
            {
                Id = $"PageHeader-p{pageNumber}",
                Style = engineStyle,
                Children = arranged
            };
            header.Arrange(new Rect(marginLeft, marginTop, contentWidth, pageHeaderHeight));
        }

        // --- Arrange page-footer blocks ---
        SectionBlock? footer = null;
        if (pageFooterBlocks.Count > 0)
        {
            float footerTop = options.PageHeight - options.PageMargins.Bottom - pageFooterHeight;
            float y = footerTop;
            var arranged = new List<LayoutBlock>(pageFooterBlocks.Count);
            foreach (var block in pageFooterBlocks)
            {
                block.Arrange(new Rect(marginLeft, y, contentWidth, block.DesiredSize.Height));
                arranged.Add(block);
                y += block.DesiredSize.Height;
            }

            footer = new SectionBlock
            {
                Id = $"PageFooter-p{pageNumber}",
                Style = engineStyle,
                Children = arranged
            };
            footer.Arrange(new Rect(marginLeft, footerTop, contentWidth, pageFooterHeight));
        }

        // --- Arrange body blocks ---
        float bodyTop = marginTop + pageHeaderHeight;
        float bodyY = bodyTop;
        var children = new List<LayoutBlock>(bodyBlocks.Count);
        foreach (var block in bodyBlocks)
        {
            block.Arrange(new Rect(marginLeft, bodyY, contentWidth, block.DesiredSize.Height));
            children.Add(block);
            bodyY += block.DesiredSize.Height;
        }

        // --- Build and finalise the page ---
        var page = new PageBlock
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
