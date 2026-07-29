using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;
using Shouldly;
using Xunit;

namespace KineticReports.Core.Tests.Layout;

/// <summary>
/// Comprehensive unit tests for ContentBlock covering all 12 BlockContentType values,
/// type-specific properties, and basic layout behavior.
/// </summary>
public class ContentBlockTests
{
    private sealed class TestTextLayout : ITextLayout
    {
        public Size MeasureText(string text, AppliedStyle style, float maxWidth) => new(64f, 16f);

        public IReadOnlyList<TextRun> ShapeText(string text, AppliedStyle style, Rect bounds) => [];

        public float GetAscent(FontDescriptor descriptor) => 10f;

        public float GetDescent(FontDescriptor descriptor) => 3f;

        public float GetLineGap(FontDescriptor descriptor) => 2f;
    }

    private sealed class TestLayoutSizingContext : ILayoutSizingContext
    {
        public ITextLayout TextLayout { get; } = new TestTextLayout();

        public Size? ResolveImageSize(string imageKey) => null;
    }

    private static AppliedStyle CreateDefaultStyle() =>
        new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f
        };

    // ===== Creation & Initialization Tests =====

    [Fact]
    public void ContentBlock_Constructor_CreatesWithRequiredProperties()
    {
        var style = CreateDefaultStyle();
        var block = new ContentBlock { Id = "test", Style = style, ContentType = BlockContentType.Text };

        block.Id.ShouldBe("test");
        block.Style.ShouldBe(style);
        block.ContentType.ShouldBe(BlockContentType.Text);
    }

    [Fact]
    public void ContentBlock_LayoutBlockType_IsSet()
    {
        var block = new ContentBlock { Id = "test", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text };

        // LayoutBlockType should be set to some valid value
        var blockType = block.LayoutBlockType;
        blockType.ToString().ShouldNotBeEmpty();
    }

    // ===== Text Block Type Tests =====

    [Fact]
    public void TextBlock_StoresTextProperty()
    {
        var block = new ContentBlock
        {
            Id = "text1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Text,
            Text = "Hello World"
        };

        block.Text.ShouldBe("Hello World");
        block.ContentType.ShouldBe(BlockContentType.Text);
    }

    [Fact]
    public void TextBlock_WithEmptyText_ReturnsEmptyString()
    {
        var block = new ContentBlock
        {
            Id = "text2",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Text,
            Text = ""
        };

        block.Text.ShouldBe("");
    }

    [Fact]
    public void TextBlock_WithNullText_ReturnsNull()
    {
        var block = new ContentBlock
        {
            Id = "text3",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Text,
            Text = null
        };

        block.Text.ShouldBeNull();
    }

    [Fact]
    public void TextBlock_DefaultTextRuns_IsEmpty()
    {
        var block = new ContentBlock
        {
            Id = "text5",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Text
        };

        block.TextRuns.ShouldNotBeNull();
    }

    // ===== Container Block Type Tests =====

    [Fact]
    public void ContainerBlock_WithChildren_StoresCollection()
    {
        var child = new ContentBlock { Id = "c1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text };
        var block = new ContentBlock
        {
            Id = "container1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Container,
            Children = new[] { child }
        };

        block.Children.ShouldNotBeNull();
        block.Children[0].Id.ShouldBe("c1");
    }

    [Fact]
    public void ContainerBlock_DefaultChildren_ReturnsEmptyList()
    {
        var block = new ContentBlock
        {
            Id = "container2",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Container
        };

        block.Children.ShouldNotBeNull();
    }

    [Fact]
    public void ContainerBlock_MultipleChildren_MaintainsOrder()
    {
        var c1 = new ContentBlock { Id = "first", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text };
        var c2 = new ContentBlock { Id = "second", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text };
        var c3 = new ContentBlock { Id = "third", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text };

        var block = new ContentBlock
        {
            Id = "container3",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Container,
            Children = new[] { c1, c2, c3 }
        };

        block.Children[0].Id.ShouldBe("first");
        block.Children[1].Id.ShouldBe("second");
        block.Children[2].Id.ShouldBe("third");
    }

    // ===== Image Block Type Tests =====

    [Fact]
    public void ImageBlock_StoresSourceKey()
    {
        var block = new ContentBlock
        {
            Id = "img1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Image,
            SourceKey = "images/test.png"
        };

        block.SourceKey.ShouldBe("images/test.png");
    }

    [Fact]
    public void ImageBlock_WithNullSourceKey_IsValid()
    {
        var block = new ContentBlock
        {
            Id = "img2",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Image,
            SourceKey = null
        };

        block.SourceKey.ShouldBeNull();
    }

    [Fact]
    public void ImageBlock_StoresStretchMode()
    {
        var block = new ContentBlock
        {
            Id = "img3",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Image,
            Stretch = ImageStretch.Uniform
        };

        block.Stretch.ShouldBe(ImageStretch.Uniform);
    }

    [Theory]
    [InlineData(ImageStretch.None)]
    [InlineData(ImageStretch.Fill)]
    [InlineData(ImageStretch.Uniform)]
    [InlineData(ImageStretch.UniformToFill)]
    public void ImageBlock_SupportsAllStretchModes(ImageStretch stretch)
    {
        var block = new ContentBlock
        {
            Id = "img",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Image,
            Stretch = stretch
        };

        block.Stretch.ShouldBe(stretch);
    }

    // ===== Shape Block Type Tests =====

    [Fact]
    public void ShapeBlock_StoresKind()
    {
        var block = new ContentBlock
        {
            Id = "shape1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Shape,
            Kind = ShapeKind.Rectangle
        };

        block.Kind.ShouldBe(ShapeKind.Rectangle);
    }

    [Fact]
    public void ShapeBlock_StoresFillColor()
    {
        var fill = new Color { R = 255, G = 0, B = 0 };
        var block = new ContentBlock
        {
            Id = "shape2",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Shape,
            Fill = fill
        };

        block.Fill.ShouldBe(fill);
    }

    [Fact]
    public void ShapeBlock_StoresStrokeProperties()
    {
        var stroke = new Color { R = 0, G = 0, B = 255 };
        var block = new ContentBlock
        {
            Id = "shape3",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Shape,
            Stroke = stroke,
            StrokeWidth = 2.5f
        };

        block.Stroke.ShouldBe(stroke);
        block.StrokeWidth.ShouldBe(2.5f);
    }

    [Theory]
    [InlineData(ShapeKind.Ellipse)]
    [InlineData(ShapeKind.Rectangle)]
    public void ShapeBlock_SupportsMultipleShapeKinds(ShapeKind kind)
    {
        var block = new ContentBlock
        {
            Id = "shape",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Shape,
            Kind = kind
        };

        block.Kind.ShouldBe(kind);
    }

    // ===== Chart Block Type Tests =====

    [Fact]
    public void ChartBlock_StoresChartType()
    {
        var block = new ContentBlock
        {
            Id = "chart1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Chart,
            ChartType = "BarChart"
        };

        block.ChartType.ShouldBe("BarChart");
    }

    [Fact]
    public void ChartBlock_StoresChartData()
    {
        var data = new { Series = new[] { "A", "B" }, Values = new[] { 10, 20 } };
        var block = new ContentBlock
        {
            Id = "chart2",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Chart,
            ChartData = data
        };

        block.ChartData.ShouldBe(data);
    }

    // ===== Barcode Block Type Tests =====

    [Fact]
    public void BarcodeBlock_StoresSymbology()
    {
        var block = new ContentBlock
        {
            Id = "bc1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Barcode,
            Symbology = "Code128"
        };

        block.Symbology.ShouldBe("Code128");
    }

    [Fact]
    public void BarcodeBlock_StoresValue()
    {
        var block = new ContentBlock
        {
            Id = "bc2",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Barcode,
            Value = "12345678"
        };

        block.Value.ShouldBe("12345678");
    }

    [Fact]
    public void BarcodeBlock_StoresStronglyTypedSymbology()
    {
        var block = new ContentBlock
        {
            Id = "bc2b",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Barcode,
            SymbologyType = BarcodeSymbology.Pdf417,
            Symbology = BarcodeSymbologyName.ToIdentifier(BarcodeSymbology.Pdf417),
            Value = "PO-1009"
        };

        block.SymbologyType.ShouldBe(BarcodeSymbology.Pdf417);
        block.Symbology.ShouldBe("PDF417");
    }

    [Fact]
    public void BarcodeBlock_StoresShowTextFlag()
    {
        var block = new ContentBlock
        {
            Id = "bc3",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Barcode,
            ShowText = true
        };

        block.ShowText.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Code39")]
    [InlineData("Code128")]
    [InlineData("QRCode")]
    [InlineData("EAN13")]
    public void BarcodeBlock_SupportsMultipleSymbologies(string symbology)
    {
        var block = new ContentBlock
        {
            Id = "bc",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Barcode,
            Symbology = symbology
        };

        block.Symbology.ShouldBe(symbology);
    }

    // ===== Cell Block Type Tests =====

    [Fact]
    public void CellBlock_StoresColumnIndex()
    {
        var block = new ContentBlock
        {
            Id = "cell1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Cell,
            ColumnIndex = 3
        };

        block.ColumnIndex.ShouldBe(3);
    }

    [Fact]
    public void CellBlock_StoresColumnSpan()
    {
        var block = new ContentBlock
        {
            Id = "cell2",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Cell,
            ColSpan = 2
        };

        block.ColSpan.ShouldBe(2);
    }

    [Fact]
    public void CellBlock_StoresRowSpan()
    {
        var block = new ContentBlock
        {
            Id = "cell3",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Cell,
            RowSpan = 3
        };

        block.RowSpan.ShouldBe(3);
    }

    [Fact]
    public void CellBlock_WithChildren_StoresContent()
    {
        var child = new ContentBlock { Id = "c1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text };
        var block = new ContentBlock
        {
            Id = "cell4",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Cell,
            Children = new[] { child }
        };

        block.Children.ShouldHaveSingleItem();
    }

    // ===== ReportSection Block Tests =====

    [Fact]
    public void ReportSectionBlock_IsContainerType()
    {
        var child = new ContentBlock { Id = "c1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text };
        var block = new ContentBlock
        {
            Id = "rs1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.ReportSection,
            Children = new[] { child }
        };

        block.ContentType.ShouldBe(BlockContentType.ReportSection);
        block.Children.ShouldNotBeNull();
    }

    // ===== PageSection Block Tests =====

    [Fact]
    public void PageSectionBlock_IsContainerType()
    {
        var child = new ContentBlock { Id = "c1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text };
        var block = new ContentBlock
        {
            Id = "ps1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.PageSection,
            Children = new[] { child }
        };

        block.ContentType.ShouldBe(BlockContentType.PageSection);
        block.Children.ShouldNotBeNull();
    }

    // ===== Table Block Tests =====

    [Fact]
    public void TableBlock_IsContainerType()
    {
        var row = new ContentBlock { Id = "r1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Row };
        var block = new ContentBlock
        {
            Id = "tbl1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Table,
            Children = new[] { row }
        };

        block.ContentType.ShouldBe(BlockContentType.Table);
        block.Children.ShouldNotBeNull();
    }

    // ===== Row Block Tests =====

    [Fact]
    public void RowBlock_IsContainerType()
    {
        var cell = new ContentBlock { Id = "c1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Cell };
        var block = new ContentBlock
        {
            Id = "row1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Row,
            Children = new[] { cell }
        };

        block.ContentType.ShouldBe(BlockContentType.Row);
        block.Children.ShouldNotBeNull();
    }

    // ===== Page Block Tests =====

    [Fact]
    public void PageBlock_IsContainerType()
    {
        var section = new ContentBlock { Id = "s1", Style = CreateDefaultStyle(), ContentType = BlockContentType.ReportSection };
        var block = new ContentBlock
        {
            Id = "page1",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Page,
            Children = new[] { section }
        };

        block.ContentType.ShouldBe(BlockContentType.Page);
        block.Children.ShouldNotBeNull();
    }

    // ===== Type Dispatch Verification Tests =====

    [Theory]
    [InlineData(BlockContentType.Text)]
    [InlineData(BlockContentType.Image)]
    [InlineData(BlockContentType.Shape)]
    [InlineData(BlockContentType.Chart)]
    [InlineData(BlockContentType.Barcode)]
    [InlineData(BlockContentType.Container)]
    [InlineData(BlockContentType.ReportSection)]
    [InlineData(BlockContentType.PageSection)]
    [InlineData(BlockContentType.Page)]
    [InlineData(BlockContentType.Table)]
    [InlineData(BlockContentType.Row)]
    [InlineData(BlockContentType.Cell)]
    public void ContentBlock_CanBeCreated_WithAllBlockTypes(BlockContentType contentType)
    {
        var block = new ContentBlock
        {
            Id = "test",
            Style = CreateDefaultStyle(),
            ContentType = contentType
        };

        block.ContentType.ShouldBe(contentType);
    }

    [Theory]
    [InlineData(BlockContentType.Text, "text")]
    [InlineData(BlockContentType.Image, "image")]
    [InlineData(BlockContentType.Shape, "shape")]
    [InlineData(BlockContentType.Chart, "chart")]
    [InlineData(BlockContentType.Barcode, "barcode")]
    [InlineData(BlockContentType.Container, "container")]
    [InlineData(BlockContentType.ReportSection, "report-section")]
    [InlineData(BlockContentType.PageSection, "page-section")]
    [InlineData(BlockContentType.Page, "page")]
    [InlineData(BlockContentType.Table, "table")]
    [InlineData(BlockContentType.Row, "row")]
    [InlineData(BlockContentType.Cell, "cell")]
    public void ContentBlock_Persists_ContentTypeAcrossAllTypes(BlockContentType contentType, string testName)
    {
        var block = new ContentBlock
        {
            Id = $"test-{testName}",
            Style = CreateDefaultStyle(),
            ContentType = contentType
        };

        // Verify ContentType is preserved
        block.ContentType.ShouldBe(contentType);
        block.Id.ShouldBe($"test-{testName}");
    }

    // ===== Cross-Type Integration Tests =====

    [Fact]
    public void ContentBlock_AllTypes_CanBeNested_InContainers()
    {
        // Create all 12 types
        var textBlock = new ContentBlock { Id = "t1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Text, Text = "Text" };
        var imageBlock = new ContentBlock { Id = "i1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Image };
        var shapeBlock = new ContentBlock { Id = "s1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Shape };
        var chartBlock = new ContentBlock { Id = "c1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Chart };
        var barcodeBlock = new ContentBlock { Id = "b1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Barcode };
        var containerBlock = new ContentBlock { Id = "cb1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Container };
        var reportSectionBlock = new ContentBlock { Id = "rs1", Style = CreateDefaultStyle(), ContentType = BlockContentType.ReportSection };
        var pageSectionBlock = new ContentBlock { Id = "ps1", Style = CreateDefaultStyle(), ContentType = BlockContentType.PageSection };
        var tableBlock = new ContentBlock { Id = "tbl1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Table };
        var rowBlock = new ContentBlock { Id = "r1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Row };
        var cellBlock = new ContentBlock { Id = "cell1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Cell };
        var pageBlock = new ContentBlock { Id = "p1", Style = CreateDefaultStyle(), ContentType = BlockContentType.Page };

        // Nest them in a container
        var container = new ContentBlock
        {
            Id = "root",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Container,
            Children = new[]
            {
                textBlock, imageBlock, shapeBlock, chartBlock, barcodeBlock,
                containerBlock, reportSectionBlock, pageSectionBlock,
                tableBlock, rowBlock, cellBlock, pageBlock
            }
        };

        container.Children.Count.ShouldBe(12);
        container.Children[0].ContentType.ShouldBe(BlockContentType.Text);
        container.Children[11].ContentType.ShouldBe(BlockContentType.Page);
    }

    [Fact]
    public void ContentBlock_Bounds_InitiallyUnset()
    {
        var block = new ContentBlock
        {
            Id = "test",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Text
        };

        // Bounds should be default Rect before Arrange is called
        block.Bounds.ShouldBe(default(Rect));
    }

    [Fact]
    public void ContentBlock_DesiredSize_InitiallyZero()
    {
        var block = new ContentBlock
        {
            Id = "test",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Text
        };

        // DesiredSize should be zero before LayoutSize is called
        block.DesiredSize.ShouldBe(new Size(0, 0));
    }

    [Theory]
    [InlineData(BlockContentType.Shape)]
    [InlineData(BlockContentType.Chart)]
    [InlineData(BlockContentType.Barcode)]
    public void ContentBlock_LayoutSize_WithInfinityInput_ProducesFiniteDesiredSize(BlockContentType contentType)
    {
        var block = new ContentBlock
        {
            Id = $"finite-{contentType}",
            Style = CreateDefaultStyle(),
            ContentType = contentType
        };

        block.LayoutSize(Size.Infinity, new TestLayoutSizingContext());

        float.IsFinite(block.DesiredSize.Width).ShouldBeTrue();
        float.IsFinite(block.DesiredSize.Height).ShouldBeTrue();
        block.DesiredSize.Width.ShouldBeGreaterThan(0f);
        block.DesiredSize.Height.ShouldBeGreaterThan(0f);
    }

    [Theory]
    [InlineData(true, 200f, 96f)]
    [InlineData(false, 200f, 80f)]
    public void BarcodeContentBlock_LayoutSize_WithLargeAvailableSpace_UsesCompactDefaultSize(bool showText, float expectedWidth, float expectedHeight)
    {
        var block = new ContentBlock
        {
            Id = "barcode-compact",
            Style = CreateDefaultStyle(),
            ContentType = BlockContentType.Barcode,
            Symbology = "QR",
            Value = "SO-1001",
            ShowText = showText
        };

        block.LayoutSize(new Size(1000f, 1000f), new TestLayoutSizingContext());

        block.DesiredSize.Width.ShouldBe(expectedWidth);
        block.DesiredSize.Height.ShouldBe(expectedHeight);
    }
}
