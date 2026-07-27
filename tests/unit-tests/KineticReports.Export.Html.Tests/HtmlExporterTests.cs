namespace KineticReports.Export.Html.Tests;

using System.Text;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.LayoutEngine;
using KineticReports.Core.Styling;
using KineticReports.Core.Export.Html;
using KineticReports.Rendering.Tests.Fakes;
using Microsoft.Extensions.Options;

public class HtmlExporterTests
{
    private static readonly AppliedStyle DefaultStyle = new() { FontFamily = "Arial", FontSize = 12f };

    [Fact]
    public async Task ExportAsync_WithSinglePage_ProducesValidHtml()
    {
        var tree = MakeSimpleLayoutTree();
        var exporter = CreateExporter();

        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html");
        html.ShouldContain("</html>");
        html.ShouldContain("kinetic-page");
        html.ShouldContain("<link rel=\"stylesheet\" href=\"/kinetic-report.css\" />");
    }

    [Fact]
    public async Task ExportAsync_WithTextBlock_IncludesContent()
    {
        var textBlock = new TextBlock
        {
            Id = "text-1",
            Style = DefaultStyle,
            Text = "Hello World"
        };
        textBlock.LayoutSize(new Size(200f, 100f), new LayoutSizingContext(new FakeTextLayout()));
        textBlock.Arrange(new Rect(10, 10, 100, 20));

        var page = MakePage(1, [textBlock]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("Hello World");
    }

    [Fact]
    public async Task ExportAsync_WithMultiplePages_IncludesAllPages()
    {
        var pages = Enumerable.Range(1, 3).Select(n => MakePage(n)).ToList();
        var tree = new ReportDocument { Pages = pages };

        var exporter = CreateExporter();
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
        var shapeElem = new ShapeBlock
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
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
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
        var cell1 = new CellBlock { Id = "cell-1", Style = DefaultStyle, Children = [] };
        var cell2 = new CellBlock { Id = "cell-2", Style = DefaultStyle, Children = [] };
        cell1.Arrange(new Rect(0, 0, 100, 50));
        cell2.Arrange(new Rect(100, 0, 100, 50));

        var row = new RowBlock
        {
            Id = "row-1",
            Style = DefaultStyle,
            Cells = [cell1, cell2]
        };
        row.Arrange(new Rect(0, 0, 200, 50));

        var tableElem = new TableBlock
        {
            Id = "table-1",
            Style = DefaultStyle,
            Rows = [row]
        };
        tableElem.Arrange(new Rect(0, 0, 200, 50));

        var page = MakePage(1, [tableElem]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<table");
        html.ShouldContain("<tr");
        html.ShouldContain("<td");
        html.ShouldContain("</table>");
    }

    [Fact]
    public async Task ExportAsync_WithHeaderRow_RendersTheadAndTh()
    {
        var headerCell = new CellBlock
        {
            Id = "header-cell-1",
            ColumnIndex = 0,
            Style = DefaultStyle,
            Children =
            [
                new TextBlock
                {
                    Id = "header-cell-1-text",
                    Style = DefaultStyle,
                    Text = "OrderId"
                }
            ]
        };

        var dataCell = new CellBlock
        {
            Id = "data-cell-1",
            ColumnIndex = 0,
            Style = DefaultStyle,
            Children =
            [
                new TextBlock
                {
                    Id = "data-cell-1-text",
                    Style = DefaultStyle,
                    Text = "SO-1001"
                }
            ]
        };

        var headerRow = new RowBlock
        {
            Id = "header-row-1",
            RowType = RowType.Header,
            Style = DefaultStyle,
            Cells = [headerCell]
        };

        var dataRow = new RowBlock
        {
            Id = "data-row-1",
            RowType = RowType.Data,
            Style = DefaultStyle,
            Cells = [dataCell]
        };

        var tableElem = new TableBlock
        {
            Id = "table-with-header",
            Style = DefaultStyle,
            Columns = [new TableColumn { Width = 180f }],
            Rows = [headerRow, dataRow]
        };

        var page = MakePage(1, [tableElem]);
        tableElem.LayoutSize(new Size(180f, 200f), new LayoutSizingContext(new FakeTextLayout()));
        tableElem.Arrange(new Rect(0, 0, 180, tableElem.DesiredSize.Height));

        var tree = new ReportDocument { Pages = [page] };
        var exporter = CreateExporter();

        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<thead>");
        html.ShouldContain("<th");
        html.ShouldContain("OrderId");
        html.ShouldContain("SO-1001");
        html.ShouldNotContain("header-cell-1-text\" class=\"element text-element\"");
    }

    [Fact]
    public async Task ExportAsync_WithNull_ThrowsArgumentNullException()
    {
        var exporter = CreateExporter();

        using var output = new MemoryStream();
        await Should.ThrowAsync<ArgumentNullException>(() => exporter.ExportAsync(null!, output));
    }

    // -----

    private static ReportDocument MakeSimpleLayoutTree() =>
        new() { Pages = [MakePage(1)] };

    private static HtmlExporter CreateExporter() =>
        new(Options.Create(new HtmlExportOptions
        {
            StylesheetHref = "/kinetic-report.css"
        }));

    private static PageBlock MakePage(int pageNum, List<LayoutBlock>? children = null)
    {
        var page = new PageBlock
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
