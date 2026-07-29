namespace KineticReports.Rendering.Tests;

using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Rendering.Tests.Fakes;

public class ReportDocumentRendererTests
{
    private static readonly AppliedStyle DefaultStyle = new() { FontFamily = "Arial", FontSize = 12f };
    private readonly ReportDocumentRenderer _sut = new();
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

    [Fact]
    public void RenderPage_TextBlock_WithSystemTokens_ReplacesTokensUsingPageContext()
    {
        var ctx = new RecordingGraphicsContext();
        var text = MakeArrangedTextBlock("Page {PageNumber} - {CurrentDate}");

        _sut.RenderPage(MakePage(text), ctx, _options);

        ctx.Calls.ShouldContain(c => c == "DrawText(Page 1 - " + DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + ")");
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

        var header = new PageSectionBlock
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

    [Fact]
    public void RenderPage_QrBarcodeInWideRegion_DrawsCenteredSquareImage()
    {
        var ctx = new ImageBoundsRecordingGraphicsContext();
        var barcode = new ContentBlock
        {
            Id = "barcode-qr-square-1",
            Style = DefaultStyle,
            ContentType = BlockContentType.Barcode,
            Symbology = "QR",
            Value = "SO-1001",
            ShowText = true
        };
        barcode.Arrange(new Rect(0, 0, 180, 120));

        _sut.RenderPage(MakePage(barcode), ctx, _options);

        ctx.LastImageBounds.Width.ShouldBe(102f);
        ctx.LastImageBounds.Height.ShouldBe(102f);
        ctx.LastImageBounds.X.ShouldBe(39f);
        ctx.LastImageBounds.Y.ShouldBe(0f);
    }

    [Fact]
    public void RenderPage_MicroQrBarcodeInWideRegion_DrawsCenteredSquareImage()
    {
        var ctx = new ImageBoundsRecordingGraphicsContext();
        var barcode = new ContentBlock
        {
            Id = "barcode-micro-square-1",
            Style = DefaultStyle,
            ContentType = BlockContentType.Barcode,
            SymbologyType = BarcodeSymbology.MicroQr,
            Value = "SO-1001",
            ShowText = true
        };
        barcode.Arrange(new Rect(0, 0, 180, 120));

        _sut.RenderPage(MakePage(barcode), ctx, _options);

        ctx.LastImageBounds.Width.ShouldBe(102f);
        ctx.LastImageBounds.Height.ShouldBe(102f);
        ctx.LastImageBounds.X.ShouldBe(39f);
        ctx.LastImageBounds.Y.ShouldBe(0f);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private PageBlock MakePage(
        LayoutBlock? child = null,
        PageSectionBlock? header = null,
        PageSectionBlock? footer = null)
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

    private sealed class ImageBoundsRecordingGraphicsContext : IGraphicsContext
    {
        public Rect LastImageBounds { get; private set; }

        public void FillRectangle(Rect bounds, Color color) { }
        public void FillEllipse(Rect bounds, Color color) { }
        public void StrokeRectangle(Rect bounds, Color color, float strokeWidth) { }
        public void StrokeEllipse(Rect bounds, Color color, float strokeWidth) { }
        public void DrawLine(Point from, Point to, Color color, float strokeWidth) { }
        public void DrawPath(PathGeometry path) { }
        public void DrawText(TextRun run) { }
        public void DrawImage(Rect destinationBounds, ImageReference image, ImageStretch stretch) => LastImageBounds = destinationBounds;
        public void PushClip(Rect clip) { }
        public void PopClip() { }
        public void PushOpacity(float opacity) { }
        public void PopOpacity() { }
    }
}
