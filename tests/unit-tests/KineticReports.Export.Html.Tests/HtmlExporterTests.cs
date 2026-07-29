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
    public async Task ExportAsync_WithImageSourceKeyAndNoImageReference_RendersImageTag()
    {
        var imageElem = new ImageBlock
        {
            Id = "image-1",
            Style = DefaultStyle,
            SourceKey = "https://example.com/logo.png",
            Stretch = ImageStretch.Uniform
        };
        imageElem.Arrange(new Rect(20, 30, 120, 60));

        var page = MakePage(1, [imageElem]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<img");
        html.ShouldContain("src=\"https://example.com/logo.png\"");
        html.ShouldContain("object-fit: contain;");
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
    public async Task ExportAsync_WithQrBarcodeContentBlock_RendersEmbeddedBarcodeImage()
    {
        var barcode = new ContentBlock
        {
            Id = "barcode-qr-1",
            Style = DefaultStyle,
            ContentType = BlockContentType.Barcode,
            Symbology = "QR",
            Value = "https://kineticreports.dev/demo",
            ShowText = true
        };
        barcode.Arrange(new Rect(20, 40, 140, 140));

        var page = MakePage(1, [barcode]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("data:image/png;base64,");
        html.ShouldContain("barcode-caption");
        html.ShouldContain("https://kineticreports.dev/demo");
    }

    [Fact]
    public async Task ExportAsync_WithQrBarcodeInWideRegion_RendersSquareBarcodeImage()
    {
        var barcode = new ContentBlock
        {
            Id = "barcode-qr-square-1",
            Style = DefaultStyle,
            ContentType = BlockContentType.Barcode,
            Symbology = "QR",
            Value = "SO-1001",
            ShowText = true
        };
        barcode.Arrange(new Rect(20, 40, 180, 120));

        var page = MakePage(1, [barcode]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("width: 102.0px; height: 102.0px; object-fit: fill;");
    }

    [Fact]
    public async Task ExportAsync_WithUnsupportedBarcodeSymbology_RendersPlaceholder()
    {
        var barcode = new ContentBlock
        {
            Id = "barcode-unsupported-1",
            Style = DefaultStyle,
            ContentType = BlockContentType.Barcode,
            Symbology = "NoSuchSymbology",
            Value = "12345",
            ShowText = false
        };
        barcode.Arrange(new Rect(20, 40, 160, 80));

        var page = MakePage(1, [barcode]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("visual-barcode-placeholder");
        html.ShouldContain("[Barcode: NoSuchSymbology]");
    }

    [Theory]
    [InlineData(ChartTypeName.BarVertical, "<rect")]
    [InlineData(ChartTypeName.BarHorizontal, "<rect")]
    [InlineData(ChartTypeName.Line, "<polyline")]
    [InlineData(ChartTypeName.Pie, "<path")]
    public async Task ExportAsync_WithChartContentBlock_RendersSvgByChartType(ChartTypeName chartType, string marker)
    {
        var chart = ContentBlockFactory.CreateChart(
            id: "chart-1",
            style: DefaultStyle,
            chartType: chartType,
            chartData: new ChartSeriesData
            {
                Points =
                [
                    new ChartDataPoint { Label = "A", Value = 20 },
                    new ChartDataPoint { Label = "B", Value = 35 },
                    new ChartDataPoint { Label = "C", Value = 45 }
                ]
            });

        chart.Arrange(new Rect(20, 40, 220, 140));

        var page = MakePage(1, [chart]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<svg");
        html.ShouldContain(marker);
    }

    [Fact]
    public async Task ExportAsync_WithChartOptions_RendersAxisLabelsAndGuides()
    {
        var chart = ContentBlockFactory.CreateChart(
            id: "chart-axis-1",
            style: DefaultStyle,
            chartType: ChartTypeName.Line,
            chartData: new ChartSeriesData
            {
                Points =
                [
                    new ChartDataPoint { Label = "Jan", Value = 18 },
                    new ChartDataPoint { Label = "Feb", Value = 22 },
                    new ChartDataPoint { Label = "Mar", Value = 27 }
                ],
                Options = new ChartOptions
                {
                    XAxisLabel = "Month",
                    YAxisLabel = "Revenue",
                    ShowAxes = true,
                    ShowGridLines = true,
                    ShowTicks = true,
                    ShowTickLabels = true
                }
            });

        chart.Arrange(new Rect(20, 40, 220, 140));

        var page = MakePage(1, [chart]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain(">Month<");
        html.ShouldContain(">Revenue<");
        html.ShouldContain("<line");
        html.ShouldContain("<text");
    }

    [Fact]
    public async Task ExportAsync_WithPieChart_RendersSliceLabels()
    {
        var chart = ContentBlockFactory.CreateChart(
            id: "pie-labels-1",
            style: DefaultStyle,
            chartType: ChartTypeName.Pie,
            chartData: new ChartSeriesData
            {
                Points =
                [
                    new ChartDataPoint { Label = "Enterprise", Value = 44 },
                    new ChartDataPoint { Label = "Mid-Market", Value = 31 },
                    new ChartDataPoint { Label = "SMB", Value = 25 }
                ],
                Options = new ChartOptions
                {
                    ShowTickLabels = true,
                    LabelFontSize = 9f
                }
            });

        chart.Arrange(new Rect(20, 40, 220, 140));

        var page = MakePage(1, [chart]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("Enterprise (44");
        html.ShouldContain("Mid-Market (31");
        html.ShouldContain("SMB (25");
    }

    [Fact]
    public async Task ExportAsync_WithPieLegendAndLabelStyle_RendersLegendAndWeightedLabels()
    {
        var chart = ContentBlockFactory.CreateChart(
            id: "pie-style-legend-1",
            style: DefaultStyle,
            chartType: ChartTypeName.Pie,
            chartData: new ChartSeriesData
            {
                Points =
                [
                    new ChartDataPoint { Label = "Enterprise", Value = 44 },
                    new ChartDataPoint { Label = "Mid-Market", Value = 31 },
                    new ChartDataPoint { Label = "SMB", Value = 25 }
                ],
                Options = new ChartOptions
                {
                    ShowPieLabels = true,
                    PieLabelColor = Color.FromRgb(31, 41, 55),
                    PieLabelFontWeight = FontWeight.Bold,
                    ShowLegend = true,
                    LegendPosition = PieLegendPosition.Right,
                    LegendTextColor = Color.FromRgb(17, 24, 39)
                }
            });

        chart.Arrange(new Rect(20, 40, 280, 160));

        var page = MakePage(1, [chart]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("font-weight=\"700\"");
        html.ShouldContain("fill=\"rgba(31,41,55,1.00)\"");
        html.ShouldContain(">Enterprise<");
        html.ShouldContain(">Mid-Market<");
        html.ShouldContain(">SMB<");
    }

    [Fact]
    public async Task ExportAsync_WithManyPieLegendItems_UsesCompactLegendWithoutTruncation()
    {
        var points = Enumerable.Range(1, 12)
            .Select(i => new ChartDataPoint { Label = $"Segment {i}", Value = 10 + i })
            .ToList();

        var chart = ContentBlockFactory.CreateChart(
            id: "pie-legend-compact-1",
            style: DefaultStyle,
            chartType: ChartTypeName.Pie,
            chartData: new ChartSeriesData
            {
                Points = points,
                Options = new ChartOptions
                {
                    ShowLegend = true,
                    LegendPosition = PieLegendPosition.Right,
                    LegendFontSize = 9f,
                    LegendMarkerSize = 10f
                }
            });

        chart.Arrange(new Rect(20, 40, 300, 170));

        var page = MakePage(1, [chart]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        for (var i = 1; i <= 12; i++)
        {
            html.ShouldContain($">Segment {i}<");
        }
    }

    [Fact]
    public async Task ExportAsync_WithManyLineLegendItems_UsesCompactLegendWithoutTruncation()
    {
        var points = Enumerable.Range(1, 12)
            .Select(i => new ChartDataPoint { Label = $"Series {i}", Value = 10 + i })
            .ToList();

        var chart = ContentBlockFactory.CreateChart(
            id: "line-legend-compact-1",
            style: DefaultStyle,
            chartType: ChartTypeName.Line,
            chartData: new ChartSeriesData
            {
                Points = points,
                Options = new ChartOptions
                {
                    ShowLegend = true,
                    LegendPosition = PieLegendPosition.Right,
                    LegendFontSize = 9f,
                    LegendMarkerSize = 10f,
                    ShowTickLabels = false,
                    ShowTicks = false,
                    ShowGridLines = false,
                    ShowAxes = false
                }
            });

        chart.Arrange(new Rect(20, 40, 300, 170));

        var page = MakePage(1, [chart]);
        var tree = new ReportDocument { Pages = [page] };

        var exporter = CreateExporter();
        using var output = new MemoryStream();
        await exporter.ExportAsync(tree, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        for (var i = 1; i <= 12; i++)
        {
            html.ShouldContain($">Series {i}<");
        }
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
