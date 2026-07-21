namespace KineticReports.Rendering.Tests;

using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Rendering.Tests.Fakes;

public class ReportLayoutRendererTests
{
    private static readonly AppliedStyle DefaultStyle = new() { FontFamily = "Arial", FontSize = 12f };
    private readonly ReportLayoutRenderer _sut = new();
    private readonly RenderOptions _options = new() { Background = Color.White };
    private readonly FakeMeasureContext _measureContext = new();

    // -------------------------------------------------------------------------
    // Page background
    // -------------------------------------------------------------------------

    [Fact]
    public void RenderPage_DrawsBackgroundAsFirstCall()
    {
        var ctx = new RecordingGraphicsContext();
        _sut.RenderPage(MakePage(), ctx, _options);
        ctx.Calls[0].ShouldStartWith("FillRect");
    }

    [Fact]
    public void RenderPage_EmptyPage_OnlyDrawsBackground()
    {
        var ctx = new RecordingGraphicsContext();
        _sut.RenderPage(MakePage(), ctx, _options);
        ctx.Calls.ShouldHaveSingleItem();
    }

    // -------------------------------------------------------------------------
    // Text rendering
    // -------------------------------------------------------------------------

    [Fact]
    public void RenderPage_TextBlock_EmitsDrawText()
    {
        var ctx = new RecordingGraphicsContext();
        var text = MakeArrangedTextBlock("Hello");
        var page = MakePage(text);

        _sut.RenderPage(page, ctx, _options);

        ctx.Calls.ShouldContain(c => c.StartsWith("DrawText"));
    }

    [Fact]
    public void RenderPage_TextBlock_DrawsCorrectContent()
    {
        var ctx = new RecordingGraphicsContext();
        var text = MakeArrangedTextBlock("Hello");
        _sut.RenderPage(MakePage(text), ctx, _options);

        ctx.Calls.ShouldContain("DrawText(Hello)");
    }

    // -------------------------------------------------------------------------
    // Shape rendering
    // -------------------------------------------------------------------------

    [Fact]
    public void RenderPage_FilledRectangleShape_EmitsTwoFillRects()
    {
        var ctx = new RecordingGraphicsContext();
        var shape = ArrangeShape(new ShapeBlock
        {
            Id = "s1",
            Style = DefaultStyle,
            Kind = ShapeKind.Rectangle,
            Fill = Color.Black
        });
        _sut.RenderPage(MakePage(shape), ctx, _options);

        ctx.Calls.Count(c => c.StartsWith("FillRect")).ShouldBe(2); // page bg + shape
    }

    [Fact]
    public void RenderPage_StrokedEllipseShape_EmitsStrokeEllipse()
    {
        var ctx = new RecordingGraphicsContext();
        var shape = ArrangeShape(new ShapeBlock
        {
            Id = "e1",
            Style = DefaultStyle,
            Kind = ShapeKind.Ellipse,
            Stroke = Color.Black
        });
        _sut.RenderPage(MakePage(shape), ctx, _options);

        ctx.Calls.ShouldContain(c => c.StartsWith("StrokeEllipse"));
    }

    [Fact]
    public void RenderPage_LineShape_EmitsDrawLine()
    {
        var ctx = new RecordingGraphicsContext();
        var shape = ArrangeShape(new ShapeBlock
        {
            Id = "l1",
            Style = DefaultStyle,
            Kind = ShapeKind.Line,
            Stroke = Color.Black
        });
        _sut.RenderPage(MakePage(shape), ctx, _options);

        ctx.Calls.ShouldContain(c => c.StartsWith("DrawLine"));
    }

    // -------------------------------------------------------------------------
    // Opacity
    // -------------------------------------------------------------------------

    [Fact]
    public void RenderPage_ElementWithOpacityLessThanOne_PushesAndPopsOpacity()
    {
        var ctx = new RecordingGraphicsContext();
        var container = ArrangeContainer(new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f,
            Opacity = 0.5f
        });
        _sut.RenderPage(MakePage(container), ctx, _options);

        ctx.Calls.ShouldContain(c => c.StartsWith("PushOpacity"));
        ctx.Calls.ShouldContain("PopOpacity");
    }

    [Fact]
    public void RenderPage_ElementWithFullOpacity_DoesNotPushOpacity()
    {
        var ctx = new RecordingGraphicsContext();
        _sut.RenderPage(MakePage(ArrangeContainer(DefaultStyle)), ctx, _options);
        ctx.Calls.ShouldNotContain(c => c.StartsWith("PushOpacity"));
    }

    // -------------------------------------------------------------------------
    // Clipping
    // -------------------------------------------------------------------------

    [Fact]
    public void RenderPage_ElementWithOverflowHidden_PushesAndPopsClip()
    {
        var ctx = new RecordingGraphicsContext();
        _sut.RenderPage(MakePage(ArrangeContainer(DefaultStyle)), ctx, _options);

        ctx.Calls.ShouldContain("PushClip");
        ctx.Calls.ShouldContain("PopClip");
    }

    // -------------------------------------------------------------------------
    // Header before body
    // -------------------------------------------------------------------------

    [Fact]
    public void RenderPage_WithHeader_RendersHeaderBeforeBodyChildren()
    {
        var ctx = new RecordingGraphicsContext();
        var headerText = MakeArrangedTextBlock("Header");
        var bodyText = MakeArrangedTextBlock("Body");

        var header = new SectionBlock
        {
            Id = "hdr",
            Style = DefaultStyle,
            Children = [headerText]
        };
        header.Arrange(new Rect(0, 0, 612, 50));

        var page = MakePage(bodyText, header: header);
        _sut.RenderPage(page, ctx, _options);

        int hi = ctx.Calls.ToList().IndexOf("DrawText(Header)");
        int bi = ctx.Calls.ToList().IndexOf("DrawText(Body)");
        hi.ShouldBeGreaterThanOrEqualTo(0);
        bi.ShouldBeGreaterThanOrEqualTo(0);
        hi.ShouldBeLessThan(bi);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private PageBlock MakePage(
        LayoutBlock? child = null,
        SectionBlock? header = null,
        SectionBlock? footer = null)
    {
        var children = child is null
            ? (IReadOnlyList<LayoutBlock>)[]
            : [child];
        var page = new PageBlock
        {
            Id = "page-1",
            Style = DefaultStyle,
            PageWidth = 612f,
            PageHeight = 792f,
            PageNumber = 1,
            Header = header,
            Footer = footer,
            Children = children
        };
        page.Arrange(new Rect(0, 0, 612, 792));
        return page;
    }

    private TextBlock MakeArrangedTextBlock(string text)
    {
        var elem = new TextBlock
        {
            Id = Guid.NewGuid().ToString(),
            Style = DefaultStyle,
            Text = text
        };
        elem.LayoutSize(new Size(200f, 100f), _measureContext);
        elem.Arrange(new Rect(0, 0, 200, 15));
        return elem;
    }

    private static ContainerBlock ArrangeContainer(AppliedStyle style)
    {
        var c = new ContainerBlock { Id = "c1", Style = style, Children = [] };
        c.Arrange(new Rect(0, 0, 100, 50));
        return c;
    }

    private static ShapeBlock ArrangeShape(ShapeBlock shape)
    {
        shape.Arrange(new Rect(10, 10, 100, 50));
        return shape;
    }

    // Minimal ILayoutSizingContext backed by FakeTextLayout
    private sealed class FakeMeasureContext : KineticReports.Core.Layout.ILayoutSizingContext
    {
        public KineticReports.Core.Typography.ITextLayout TextLayout { get; } = new FakeTextLayout();
        public Size? ResolveImageSize(string imageKey) => null;
    }
}
