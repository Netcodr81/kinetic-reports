namespace KineticReports.Layout.Tests;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Layout.Tests.Fakes;

public class LayoutEngineTests
{
    private static readonly AppliedStyle DefaultStyle = new() { FontFamily = "Arial", FontSize = 12f };
    private readonly LayoutSizingContext _context = new(new FakeTextLayout());
    private readonly LayoutOptions _options = new();
    private readonly LayoutEngine _sut = new();

    // -------------------------------------------------------------------------
    // Empty input
    // -------------------------------------------------------------------------

    [Fact]
    public void Layout_WithNoContentRegions_ProducesExactlyOnePage()
    {
        var tree = _sut.Layout([], _options, _context);
        tree.PageCount.ShouldBe(1);
    }

    [Fact]
    public void Layout_WithNoContentRegions_ProducesBlankPageWithCorrectDimensions()
    {
        var tree = _sut.Layout([], _options, _context);
        var page = tree.Pages[0];
        page.PageWidth.ShouldBe(_options.PageWidth);
        page.PageHeight.ShouldBe(_options.PageHeight);
        page.PageNumber.ShouldBe(1);
    }

    // -------------------------------------------------------------------------
    // Single page
    // -------------------------------------------------------------------------

    [Fact]
    public void Layout_WithSingleSmallContentRegion_FitsOnOnePage()
    {
        var contentRegion = MakeContentRegion(BlockType.Detail, 100f);
        var tree = _sut.Layout([contentRegion], _options, _context);
        tree.PageCount.ShouldBe(1);
    }

    [Fact]
    public void Layout_WithSingleContentRegion_ArrangesContentRegionWithinBodyArea()
    {
        var contentRegion = MakeContentRegion(BlockType.Detail, 100f);
        var tree = _sut.Layout([contentRegion], _options, _context);
        var page = tree.Pages[0];

        // Content region should be inside the page bounds.
        page.Children.ShouldHaveSingleItem();
        page.Children[0].Bounds.X.ShouldBe(_options.PageMargins.Left);
        page.Children[0].Bounds.Y.ShouldBe(_options.PageMargins.Top);
        page.Children[0].Bounds.Width.ShouldBe(_options.PageWidth - _options.PageMargins.Horizontal);
        page.Children[0].Bounds.Height.ShouldBe(100f);
    }

    // -------------------------------------------------------------------------
    // Multi-page pagination
    // -------------------------------------------------------------------------

    [Fact]
    public void Layout_WhenContentRegionsExceedBodyHeight_SpillsToNextPage()
    {
        // Body area = 1056 - 96*2 = 864. Each region = 300, so 3 regions = 900 > 864.
        var contentRegions = new[]
        {
            MakeContentRegion(BlockType.Detail, 300f),
            MakeContentRegion(BlockType.Detail, 300f),
            MakeContentRegion(BlockType.Detail, 300f)
        };
        var tree = _sut.Layout(contentRegions, _options, _context);
        tree.PageCount.ShouldBe(2);
    }

    [Fact]
    public void Layout_WhenContentRegionsFitExactly_StaysOnOnePage()
    {
        // Body area = 864. Two content regions totalling exactly 864.
        var contentRegions = new[]
        {
            MakeContentRegion(BlockType.Detail, 432f),
            MakeContentRegion(BlockType.Detail, 432f)
        };
        var tree = _sut.Layout(contentRegions, _options, _context);
        tree.PageCount.ShouldBe(1);
    }

    // -------------------------------------------------------------------------
    // ForcePageBreakBefore
    // -------------------------------------------------------------------------

    [Fact]
    public void Layout_WithForcePageBreakBefore_StartsNewPage()
    {
        var first = MakeContentRegion(BlockType.Detail, 100f);
        var forced = MakeContentRegion(BlockType.Detail, 100f, forceBreak: true);
        var tree = _sut.Layout([first, forced], _options, _context);
        tree.PageCount.ShouldBe(2);
    }

    [Fact]
    public void Layout_ForcePageBreakOnFirstContentRegion_DoesNotCreateEmptyPage()
    {
        // ForcePageBreakBefore on the very first content region should not emit an empty page.
        var forced = MakeContentRegion(BlockType.Detail, 100f, forceBreak: true);
        var tree = _sut.Layout([forced], _options, _context);
        tree.PageCount.ShouldBe(1);
    }

    // -------------------------------------------------------------------------
    // Page header / footer
    // -------------------------------------------------------------------------

    [Fact]
    public void Layout_WithPageHeaderContentRegion_HeaderAppearsOnEveryPage()
    {
        var header = MakeContentRegion(BlockType.PageHeader, 50f);
        var detail1 = MakeContentRegion(BlockType.Detail, 500f);
        var detail2 = MakeContentRegion(BlockType.Detail, 500f);
        var tree = _sut.Layout([header, detail1, detail2], _options, _context);

        foreach (var page in tree.Pages)
            page.Header.ShouldNotBeNull();
    }

    [Fact]
    public void Layout_WithPageFooterContentRegion_FooterAppearsOnEveryPage()
    {
        var footer = MakeContentRegion(BlockType.PageFooter, 50f);
        var detail1 = MakeContentRegion(BlockType.Detail, 500f);
        var detail2 = MakeContentRegion(BlockType.Detail, 500f);
        var tree = _sut.Layout([footer, detail1, detail2], _options, _context);

        foreach (var page in tree.Pages)
            page.Footer.ShouldNotBeNull();
    }

    [Fact]
    public void Layout_PageHeaderReducesBodyArea()
    {
        // Header = 100. Body area = 864 - 100 = 764.
        // Three detail content regions × 300 = 900. Without header: 2 pages. With header: still 2.
        // But two regions × 400 = 800 > 764 → should require 2 pages.
        var header = MakeContentRegion(BlockType.PageHeader, 100f);
        var detail1 = MakeContentRegion(BlockType.Detail, 400f);
        var detail2 = MakeContentRegion(BlockType.Detail, 400f);
        var tree = _sut.Layout([header, detail1, detail2], _options, _context);
        tree.PageCount.ShouldBe(2);
    }

    // -------------------------------------------------------------------------
    // Page numbering
    // -------------------------------------------------------------------------

    [Fact]
    public void Layout_PagesAreNumberedSequentially()
    {
        var contentRegions = new[]
        {
            MakeContentRegion(BlockType.Detail, 500f),
            MakeContentRegion(BlockType.Detail, 500f),
            MakeContentRegion(BlockType.Detail, 500f)
        };
        var tree = _sut.Layout(contentRegions, _options, _context);

        for (int i = 0; i < tree.PageCount; i++)
            tree.Pages[i].PageNumber.ShouldBe(i + 1);
    }

    // -------------------------------------------------------------------------
    // ReportLayout immutability
    // -------------------------------------------------------------------------

    [Fact]
    public void LayoutTree_Pages_IsReadOnly()
    {
        var tree = _sut.Layout([], _options, _context);
        tree.Pages.ShouldBeAssignableTo<IReadOnlyList<PageBlock>>();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ReportBlock MakeContentRegion(BlockType kind, float height, bool forceBreak = false)
    {
        var style = DefaultStyle;
        var id = Guid.NewGuid().ToString();

        return kind switch
        {
            BlockType.PageHeader => new PageHeaderBlock
            {
                Id = id,
                Style = style,
                ForcePageBreakBefore = forceBreak,
                Children = [new FixedHeightElement(height, style)]
            },
            BlockType.PageFooter => new PageFooterBlock
            {
                Id = id,
                Style = style,
                ForcePageBreakBefore = forceBreak,
                Children = [new FixedHeightElement(height, style)]
            },
            BlockType.GroupHeader => new GroupHeaderBlock
            {
                Id = id,
                Style = style,
                ForcePageBreakBefore = forceBreak,
                Children = [new FixedHeightElement(height, style)]
            },
            BlockType.GroupFooter => new GroupFooterBlock
            {
                Id = id,
                Style = style,
                ForcePageBreakBefore = forceBreak,
                Children = [new FixedHeightElement(height, style)]
            },
            BlockType.ReportHeader => new HeaderBlock
            {
                Id = id,
                Style = style,
                ForcePageBreakBefore = forceBreak,
                Children = [new FixedHeightElement(height, style)]
            },
            BlockType.ReportFooter => new FooterBlock
            {
                Id = id,
                Style = style,
                ForcePageBreakBefore = forceBreak,
                Children = [new FixedHeightElement(height, style)]
            },
            _ => new DetailBlock
            {
                Id = id,
                Style = style,
                ForcePageBreakBefore = forceBreak,
                Children = [new FixedHeightElement(height, style)]
            }
        };
    }

    /// <summary>
    /// A minimal layout element whose desired size is hard-coded to a fixed height.
    /// Used to make content-region measurements predictable in tests.
    /// </summary>
    private sealed class FixedHeightElement : LayoutBlock
    {
        private readonly float _height;

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public FixedHeightElement(float height, AppliedStyle style)
        {
            _height = height;
            Id = Guid.NewGuid().ToString();
            Style = style;
        }

        public override LayoutBlockType LayoutBlockType => LayoutBlockType.Container;

        public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
            => DesiredSize = new Size(availableSize.Width, _height);

        public override void Arrange(Rect finalRect)
            => Bounds = finalRect;
    }
}
