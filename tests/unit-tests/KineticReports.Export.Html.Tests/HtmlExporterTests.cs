namespace KineticReports.Export.Html.Tests;

using System.Text;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Export.Html;
using KineticReports.Layout;
using KineticReports.Rendering.Tests.Fakes;

public class HtmlExporterTests
{
    private static readonly ResolvedStyle DefaultStyle = new() { FontFamily = "Arial", FontSize = 12f };

    [Fact]
    public async Task ExportAsync_WithSinglePage_ProducesValidHtml()
    {
        var tree = MakeSimpleLayoutTree();
        var exporter = new HtmlExporter();

        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html");
        html.ShouldContain("</html>");
        html.ShouldContain("kinetic-page");
    }

    [Fact]
    public async Task ExportAsync_WithTextElement_IncludesContent()
    {
        var textElem = new TextElement
        {
            Id = "text-1",
            Style = DefaultStyle,
            Text = "Hello World"
        };
        textElem.Measure(new Size(200f, 100f), new MeasureContext(new FakeFontMetrics()));
        textElem.Arrange(new Rect(10, 10, 100, 20));

        var page = MakePage(1, [textElem]);
        var tree = new LayoutTree { Pages = [page] };

        var exporter = new HtmlExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("Hello World");
    }

    [Fact]
    public async Task ExportAsync_WithMultiplePages_IncludesAllPages()
    {
        var pages = Enumerable.Range(1, 3).Select(n => MakePage(n)).ToList();
        var tree = new LayoutTree { Pages = pages };

        var exporter = new HtmlExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("page-1");
        html.ShouldContain("page-2");
        html.ShouldContain("page-3");
    }

    [Fact]
    public async Task ExportAsync_WithShapeElement_IncludesSvg()
    {
        var shapeElem = new ShapeElement
        {
            Id = "shape-1",
            Style = DefaultStyle,
            Kind = ShapeKind.Rectangle,
            Fill = Color.FromRgb(255, 0, 0),
            Stroke = Color.FromRgb(0, 0, 0),
            StrokeWidth = 2f
        };
        shapeElem.Arrange(new Rect(10, 10, 100, 50));

        var page = MakePage(1, [shapeElem]);
        var tree = new LayoutTree { Pages = [page] };

        var exporter = new HtmlExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<svg");
        html.ShouldContain("<rect");
        html.ShouldContain("</svg>");
    }

    [Fact]
    public async Task ExportAsync_WithTableElement_IncludesTable()
    {
        var cell1 = new CellElement { Id = "cell-1", Style = DefaultStyle, Children = [] };
        var cell2 = new CellElement { Id = "cell-2", Style = DefaultStyle, Children = [] };
        cell1.Arrange(new Rect(0, 0, 100, 50));
        cell2.Arrange(new Rect(100, 0, 100, 50));

        var row = new RowElement
        {
            Id = "row-1",
            Style = DefaultStyle,
            Cells = [cell1, cell2]
        };
        row.Arrange(new Rect(0, 0, 200, 50));

        var tableElem = new TableElement
        {
            Id = "table-1",
            Style = DefaultStyle,
            Rows = [row]
        };
        tableElem.Arrange(new Rect(0, 0, 200, 50));

        var page = MakePage(1, [tableElem]);
        var tree = new LayoutTree { Pages = [page] };

        var exporter = new HtmlExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<table");
        html.ShouldContain("<tr");
        html.ShouldContain("<td");
        html.ShouldContain("</table>");
    }

    [Fact]
    public async Task ExportAsync_WithNull_ThrowsArgumentNullException()
    {
        var exporter = new HtmlExporter();

        using var output = new MemoryStream();
        await Should.ThrowAsync<ArgumentNullException>(() => exporter.ExportAsync(null!, output));
    }

    // -----

    private static LayoutTree MakeSimpleLayoutTree() =>
        new() { Pages = [MakePage(1)] };

    private static PageElement MakePage(int pageNum, List<LayoutElement>? children = null)
    {
        var page = new PageElement
        {
            Id = $"page-{pageNum}",
            Style = DefaultStyle,
            PageWidth = 612f,
            PageHeight = 792f,
            PageNumber = pageNum,
            Children = children ?? []
        };
        page.Arrange(new Rect(0, 0, 612, 792));
        return page;
    }
}
