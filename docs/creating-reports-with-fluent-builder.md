# Creating Reports with the Fluent Builder

This guide covers authoring with `DefaultReportBuilder`.

Goal: build practical reports quickly with fluent APIs while still exposing all current configuration options.

## 1. What the fluent builder does

`DefaultReportBuilder` implements `IReportBuilder` and can:

- Add static regions (text, image, chart, barcode, shapes)
- Generate repeated row content
- Generate data-backed tables with grouping and aggregates
- Use defaults for block and text styling

It can run in two modes:

- Explicit fluent steps (you call `Add...` methods)
- Definition-backed mode (if no fluent steps are added and `ReportDefinition.Layout` is present)

## 2. Setup

```csharp
using KineticReports.Core.Engine.DependencyInjection;

builder.Services.AddKineticReportsEngine();
```

Optional builder pre-configuration during registration:

```csharp
using KineticReports.Core.Engine.Building;
using KineticReports.Core.Styling;

builder.Services.AddKineticReportsEngine(configureBuilder: b =>
{
    b.WithDefaultBlockStyle(new AppliedStyle { FontFamily = "Arial", FontSize = 11f });
    b.WithDefaultTextStyle(new AppliedStyle { FontFamily = "Arial", FontSize = 11f });
});
```

## 3. Fluent API catalog

### 3.1 Builder creation and defaults

- `DefaultReportBuilder.Create()`
- `WithDefaultBlockStyle(AppliedStyle style)`
- `WithDefaultTextStyle(AppliedStyle style)`

### 3.2 Static region methods

- `AddTextRegion(id, text, blockType, blockStyle, textStyle)`
- `AddImageRegion(id, sourceKey, stretch, blockType, blockStyle, imageStyle)`
- `AddChartRegion(id, chartTypeString, chartData, blockType, blockStyle, chartStyle)`
- `AddChartRegion(id, chartTypeValue, chartData, blockType, blockStyle, chartStyle)`
- `AddVerticalBarChartRegion(id, points, blockType, blockStyle, chartStyle)`
- `AddHorizontalBarChartRegion(id, points, blockType, blockStyle, chartStyle)`
- `AddLineChartRegion(id, points, blockType, blockStyle, chartStyle)`
- `AddPieChartRegion(id, points, blockType, blockStyle, chartStyle)`
- `AddBarcodeRegion(id, symbologyString, value, showText, blockType, blockStyle, barcodeStyle)`
- `AddBarcodeRegion(id, symbologyType, value, showText, blockType, blockStyle, barcodeStyle)`
- `AddQrCodeRegion(id, value, showText, blockType, blockStyle, barcodeStyle)`
- `AddMicroQrCodeRegion(id, value, showText, blockType, blockStyle, barcodeStyle)`
- `AddRectangleRegion(id, fill, stroke, strokeWidth, blockType, blockStyle, shapeStyle)`
- `AddEllipseRegion(id, fill, stroke, strokeWidth, blockType, blockStyle, shapeStyle)`
- `AddLineRegion(id, stroke, strokeWidth, blockType, blockStyle, shapeStyle)`
- `AddPageBreak(id, style)`

### 3.3 Dynamic row methods

- `ForEachRow(dataSourceId, Func<RowBuildContext, ReportBlock> blockFactory)`
- `ForEachRowText(dataSourceId, blockIdPrefix, textExpression, blockType, blockStyle, textStyle)`

`RowBuildContext` gives you:

- `DataSourceId`
- `RowIndex`
- `Row`
- `ExpressionContext`
- `Evaluate(expression)`
- `EvaluateAsString(expression)`

### 3.4 Table method

- `AddDataSourceTable(...)`

This is the richest API in the builder and supports:

- Header inclusion and repeating headers
- Grouping (`groupByExpression`)
- Group header style overrides
- Footer aggregates with multiple aggregate kinds
- Footer label and label column index
- Full style slot overrides for region/table/rows/cells

### 3.5 Low-level composition methods

- `AddBlock(ReportBlock block)`
- `AddBlocks(IEnumerable<ReportBlock> blocks)`
- `AddStep(Func<DataContext, IExpressionEvaluator, IReadOnlyList<ReportBlock>> step)`

## 4. Block and content role enums

`BlockType` values:

- `PageHeader`, `ReportHeader`, `GroupHeader`, `Detail`, `GroupFooter`, `ReportFooter`, `PageFooter`

Image stretch values:

- `None`, `Fill`, `Uniform`, `UniformToFill`

Chart type values:

- `BarVertical`, `BarHorizontal`, `Line`, `Pie`

Barcode symbology values:

- `QrCode`, `MicroQr`, `Code128`, `Code39`, `Ean13`, `Ean8`, `UpcA`, `UpcE`, `Itf`, `Pdf417`, `DataMatrix`, `Codabar`

## 5. Chart options in fluent mode

When using chart APIs, pass a `ChartSeriesData` payload:

```csharp
var chartData = new ChartSeriesData
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
        YAxisLabel = "Pipeline ($M)",
        ShowAxes = true,
        ShowGridLines = true,
        ShowTicks = true,
        ShowTickLabels = true,
        YAxisTickCount = 5,
        AxisLineWidth = 1.2f,
        GridLineWidth = 0.8f,
        LabelFontSize = 9f,
        LineColor = Color.FromRgb(37, 99, 235),
        LineWidth = 2.4f,
        ShowMarkers = true,
        ShowLegend = true,
        LegendPosition = PieLegendPosition.Right,
        LegendMarkerSize = 10f,
        LegendFontSize = 9f,
        LegendTextColor = Color.FromRgb(17, 24, 39)
    }
};
```

Chart options available:

- Axes/grid/ticks: `ShowAxes`, `ShowGridLines`, `ShowTicks`, `ShowTickLabels`
- Labels and scale: `XAxisLabel`, `YAxisLabel`, `MinValue`, `MaxValue`, `XAxisTickCount`, `YAxisTickCount`
- Styling: `AxisColor`, `GridLineColor`, `LabelColor`, `AxisLineWidth`, `GridLineWidth`, `TickLength`, `LabelFontSize`
- Bar/line: `BarGapRatio`, `LineColor`, `LineWidth`, `ShowMarkers`
- Pie labels: `ShowPieLabels`, `PieLabelColor`, `PieLabelFontWeight`
- Legend for pie/bar/line: `ShowLegend`, `LegendPosition`, `LegendMarkerSize`, `LegendFontSize`, `LegendTextColor`

Legend positions:

- `Right`, `Bottom`, `Left`, `Top`

## 6. Data source table API reference

`AddDataSourceTable` parameters:

- `dataSourceId`: source id from `DataContext`
- `regionId`: wrapper region id
- `tableId`: table id
- `columns`: list of `DataSourceTableColumn`
- `blockType`: region role
- `regionStyle`, `tableStyle`, `headerRowStyle`, `headerCellStyle`, `dataRowStyle`, `dataCellStyle`
- `repeatHeaders`, `includeHeader`
- `groupByExpression`
- `groupHeaderRowStyle`, `groupHeaderCellStyle`
- `footerAggregates`
- `footerRowStyle`, `footerCellStyle`
- `footerLabel`, `footerLabelColumnIndex`

`DataSourceTableColumn`:

- `Header` (required)
- `ValueExpression` (required)
- `Column` (`TableColumn` with `Width`, `MinWidth`, `MaxWidth`, `Grow`)

`TableAggregateDefinition`:

- `ColumnIndex` (required)
- `ValueExpression` (required)
- `Kind` (default `Sum`)
- `FormatString` (optional)

`TableAggregateKind` values:

- `Sum`, `Average`, `Min`, `Max`, `Count`, `CountNonNull`

## 7. End-to-end fluent example

```csharp
using KineticReports.Core.Definition;
using KineticReports.Core.Engine;
using KineticReports.Core.Engine.Building;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

var blockStyle = new AppliedStyle { FontFamily = "Arial", FontSize = 11f };
var titleStyle = new AppliedStyle { FontFamily = "Arial", FontSize = 16f, FontWeight = FontWeight.Bold };

var chartData = new ChartSeriesData
{
    Points =
    [
        new ChartDataPoint { Label = "Jan", Value = 18 },
        new ChartDataPoint { Label = "Feb", Value = 22 },
        new ChartDataPoint { Label = "Mar", Value = 27 }
    ],
    Options = new ChartOptions
    {
        ShowGridLines = true,
        ShowLegend = true,
        LegendPosition = PieLegendPosition.Right
    }
};

var reportBuilder = DefaultReportBuilder.Create()
    .WithDefaultBlockStyle(blockStyle)
    .WithDefaultTextStyle(blockStyle)
    .AddTextRegion("title", "Quarterly Sales", BlockType.ReportHeader, textStyle: titleStyle)
    .AddLineChartRegion("trend", chartData.Points, BlockType.Detail)
    .AddChartRegion("trend-custom", ChartTypeName.Line, chartData, BlockType.Detail)
    .ForEachRowText("orders", "row", "{CustomerName}")
    .AddDataSourceTable(
        dataSourceId: "orders",
        regionId: "orders-region",
        tableId: "orders-table",
        columns:
        [
            new DefaultReportBuilder.DataSourceTableColumn
            {
                Header = "Customer",
                ValueExpression = "{CustomerName}",
                Column = new TableColumn { MinWidth = 120f, Grow = 1f }
            },
            new DefaultReportBuilder.DataSourceTableColumn
            {
                Header = "Amount",
                ValueExpression = "{Amount}",
                Column = new TableColumn { Width = 90f, MinWidth = 90f, Grow = 0f }
            }
        ],
        groupByExpression: "{Region}",
        footerAggregates:
        [
            new DefaultReportBuilder.TableAggregateDefinition
            {
                ColumnIndex = 1,
                ValueExpression = "{Amount}",
                Kind = DefaultReportBuilder.TableAggregateKind.Sum,
                FormatString = "0.00"
            }
        ],
        footerLabel: "Grand Total",
        footerLabelColumnIndex: 0)
    .AddPageBreak("break-1");

services.AddScoped<IReportBuilder>(_ => reportBuilder);

var engine = services.GetRequiredService<IReportEngine>();

var definition = new ReportDefinition
{
    SchemaVersion = "1.0",
    Id = "fluent-1",
    Name = "Fluent Report",
    Parameters = [],
    DataSources = [],
    Styles = [],
    Plugins = []
};

var document = await engine.RunAsync(definition, ct);
```

## 8. Definition-backed mode (no fluent steps)

If you do not add fluent steps and the incoming `ReportDefinition` has `Layout`, `DefaultReportBuilder` automatically maps layout items into blocks.

This lets you use one builder implementation for both:

- JSON layout definitions
- Fluent code composition

## 9. Important notes

- There is no `BuildDefinition()` API on `DefaultReportBuilder` in current implementation.
- `DefaultReportBuilder.Build(...)` returns `IReadOnlyList<ReportBlock>` for engine layout.
- Use JSON authoring if you need file-based report definitions.

## 10. Troubleshooting

- Empty output: ensure the builder is registered as `IReportBuilder`
- No table rows: verify `dataSourceId` and resolved data
- Missing chart legend: set `ShowLegend = true`
- Aggregate column mismatch: validate `ColumnIndex` against table columns
