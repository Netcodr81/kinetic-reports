namespace KineticReports.Visual.Tests;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Core.Visual;
using Shouldly;
using Xunit;

public class DefaultVisualDocumentBuilderTests
{
    [Fact]
    public void Build_WithNullReportDocument_ThrowsArgumentNullException()
    {
        var sut = new DefaultVisualDocumentBuilder();

        Should.Throw<ArgumentNullException>(() => sut.Build(null!));
    }

    [Fact]
    public void Build_WithHeaderBodyFooter_MapsLayersAndPreservesSourceIds()
    {
        var headerText = CreateTextBlock("header-text", "Header", new Rect(0f, 0f, 200f, 20f));
        var footerText = CreateTextBlock("footer-text", "Footer", new Rect(0f, 0f, 200f, 20f));
        var bodyText = CreateTextBlock("body-text", "Body", new Rect(40f, 60f, 280f, 24f));

        var bodyTable = new TableBlock
        {
            Id = "body-table",
            Style = CreateStyle(),
            Columns = [new TableColumn { Width = 240f, MinWidth = 60f }],
            Rows = []
        };
        bodyTable.Arrange(new Rect(40f, 100f, 300f, 180f));

        var page = new PageBlock
        {
            Id = "page-1",
            Style = CreateStyle(),
            PageNumber = 1,
            PageWidth = 816f,
            PageHeight = 1056f,
            Header = CreateSection("header", headerText),
            Footer = CreateSection("footer", footerText),
            Children = [bodyText, bodyTable]
        };
        page.Arrange(new Rect(0f, 0f, 816f, 1056f));

        var reportDocument = new ReportDocument
        {
            Pages = [page]
        };

        var sut = new DefaultVisualDocumentBuilder();

        var visualDocument = sut.Build(reportDocument);

        visualDocument.Pages.Count.ShouldBe(1);
        visualDocument.Metadata["source.pageCount"].ShouldBe("1");

        var visualPage = visualDocument.Pages[0];
        visualPage.PageNumber.ShouldBe(1);
        visualPage.Layers.Count.ShouldBe(3);
        visualPage.Layers[0].Name.ShouldBe("header");
        visualPage.Layers[1].Name.ShouldBe("body");
        visualPage.Layers[2].Name.ShouldBe("footer");

        var headerElement = visualPage.Layers[0].Elements.Single();
        headerElement.Id.ShouldBe("header-text");
        headerElement.Metadata["source.layoutBlockType"].ShouldBe(LayoutBlockType.Text.ToString());

        var bodyElements = Flatten(visualPage.Layers[1].Elements).ToList();
        bodyElements.Any(element => element.Id == "body-text" && element is VisualText).ShouldBeTrue();
        bodyElements.Any(element => element.Id == "body-table" && element is VisualTablePlaceholder).ShouldBeTrue();

        var footerElement = visualPage.Layers[2].Elements.Single();
        footerElement.Id.ShouldBe("footer-text");
    }

    [Fact]
    public void Build_WithBarcodeAndChart_CreatesUnsupportedElementsWithDiagnostics()
    {
        var barcode = new BarcodeBlock
        {
            Id = "barcode-1",
            Style = CreateStyle(),
            Symbology = "QR",
            Value = "A123"
        };
        barcode.Arrange(new Rect(16f, 20f, 100f, 100f));

        var chart = new ChartBlock
        {
            Id = "chart-1",
            Style = CreateStyle(),
            ChartType = "Bar"
        };
        chart.Arrange(new Rect(140f, 20f, 300f, 160f));

        var page = new PageBlock
        {
            Id = "page-2",
            Style = CreateStyle(),
            PageNumber = 2,
            PageWidth = 816f,
            PageHeight = 1056f,
            Children = [barcode, chart]
        };
        page.Arrange(new Rect(0f, 0f, 816f, 1056f));

        var reportDocument = new ReportDocument { Pages = [page] };
        var sut = new DefaultVisualDocumentBuilder();

        var visualDocument = sut.Build(reportDocument);
        var bodyElements = visualDocument.Pages[0].Layers.Single(layer => layer.Name == "body").Elements;

        bodyElements.Count.ShouldBe(2);
        bodyElements[0].ShouldBeOfType<VisualUnsupportedElement>();
        bodyElements[1].ShouldBeOfType<VisualUnsupportedElement>();

        var unsupported = bodyElements.Cast<VisualUnsupportedElement>().ToList();
        unsupported.All(item => item.Metadata["diagnostic.unsupported"] == "true").ShouldBeTrue();
    }

    [Fact]
    public void Build_WithOutOfBoundsAndNegativeBounds_NormalizesAndAddsClipAndDiagnostics()
    {
        var negativeText = CreateTextBlock("negative-text", "Negative", new Rect(-20f, -15f, 120f, 30f));
        var overflowText = CreateTextBlock("overflow-text", "Overflow", new Rect(780f, 1040f, 80f, 30f));

        var page = new PageBlock
        {
            Id = "page-3",
            Style = CreateStyle(),
            PageNumber = 3,
            PageWidth = 816f,
            PageHeight = 1056f,
            Children = [negativeText, overflowText]
        };
        page.Arrange(new Rect(0f, 0f, 816f, 1056f));

        var reportDocument = new ReportDocument { Pages = [page] };
        var sut = new DefaultVisualDocumentBuilder();

        var visualDocument = sut.Build(reportDocument);
        var elements = visualDocument.Pages[0].Layers.Single(layer => layer.Name == "body").Elements;

        elements.Count.ShouldBe(2);

        var normalizedNegative = elements.OfType<VisualText>().Single(element => element.Id == "negative-text");
        normalizedNegative.Bounds.X.ShouldBe(0f);
        normalizedNegative.Bounds.Y.ShouldBe(0f);
        normalizedNegative.Clips.Count.ShouldBe(1);
        normalizedNegative.Metadata["diagnostic.boundsNormalized"].ShouldBe("true");
        normalizedNegative.Metadata["diagnostic.pageClipApplied"].ShouldBe("true");

        var normalizedOverflow = elements.OfType<VisualText>().Single(element => element.Id == "overflow-text");
        normalizedOverflow.Bounds.Right.ShouldBeLessThanOrEqualTo(816f);
        normalizedOverflow.Bounds.Bottom.ShouldBeLessThanOrEqualTo(1056f);
        normalizedOverflow.Clips.Count.ShouldBe(1);
        normalizedOverflow.Metadata["diagnostic.boundsNormalized"].ShouldBe("true");
        normalizedOverflow.Metadata["diagnostic.pageClipApplied"].ShouldBe("true");

        visualDocument.Metadata["diagnostic.totalElements"].ShouldBe("2");
        visualDocument.Metadata["diagnostic.normalizedBounds"].ShouldBe("2");
        visualDocument.Metadata["diagnostic.clippedElements"].ShouldBe("2");
        visualDocument.Metadata["diagnostic.unsupportedElements"].ShouldBe("0");
    }

    private static SectionBlock CreateSection(string id, params LayoutBlock[] children)
    {
        var section = new SectionBlock
        {
            Id = id,
            Style = CreateStyle(),
            Children = children
        };

        section.Arrange(new Rect(0f, 0f, 816f, 40f));
        return section;
    }

    private static TextBlock CreateTextBlock(string id, string text, Rect bounds)
    {
        var block = new TextBlock
        {
            Id = id,
            Style = CreateStyle(),
            Text = text
        };

        block.Arrange(bounds);
        return block;
    }

    private static AppliedStyle CreateStyle()
    {
        return new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f
        };
    }

    private static IEnumerable<VisualElement> Flatten(IEnumerable<VisualElement> elements)
    {
        foreach (var element in elements)
        {
            yield return element;

            if (element is VisualContainer container)
            {
                foreach (var child in Flatten(container.Children))
                    yield return child;
            }
        }
    }
}
