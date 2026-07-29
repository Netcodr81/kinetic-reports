# Creating Reports with Code-First Authoring

This guide explains how to build reports directly in C# using the layout model.

Goal: give you complete control without JSON files or fluent convenience APIs.

## 1. When to use code-first

Use code-first when you want:

- Full C# control over block generation
- Programmatic branching and composition
- Reusable report modules in code

In this model, you create `ReportBlock` and content elements directly, usually inside a custom `IReportBuilder`.

## 2. Required setup

```csharp
using KineticReports.Core.Engine.DependencyInjection;

builder.Services.AddKineticReportsEngine();
```

If you use your own builder, register it:

```csharp
using KineticReports.Core.Engine.Building;

builder.Services.AddScoped<IReportBuilder, SalesCodeFirstReportBuilder>();
```

## 3. Minimal custom builder

```csharp
using KineticReports.Core.Engine.Building;
using KineticReports.Core.Engine.Data;
using KineticReports.Core.Engine.Expressions;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

public sealed class SalesCodeFirstReportBuilder : IReportBuilder
{
    public IReadOnlyList<ReportBlock> Build(DataContext dataContext, IExpressionEvaluator evaluator)
    {
        var blockStyle = new AppliedStyle { FontFamily = "Arial", FontSize = 11f };
        var titleStyle = new AppliedStyle { FontFamily = "Arial", FontSize = 16f, FontWeight = FontWeight.Bold };

        var title = ContentBlockFactory.CreateText("title-text", titleStyle, "Quarterly Sales");

        var header = ReportBlockFactory.Create(
            blockType: BlockType.ReportHeader,
            id: "header-1",
            style: blockStyle,
            children: [title],
            keepTogether: true);

        return [header];
    }
}
```

## 4. Core building blocks

### 4.1 Report region blocks

Create report regions with `ReportBlockFactory.Create`.

Parameters:

- `blockType`: region role (`PageHeader`, `ReportHeader`, `GroupHeader`, `Detail`, `GroupFooter`, `ReportFooter`, `PageFooter`)
- `id`: stable id
- `style`: required `AppliedStyle`
- `children`: optional child blocks
- `forcePageBreakBefore`: optional page-break behavior
- `keepTogether`: optional non-splitting hint

Page break helper:

- `ReportBlockFactory.CreatePageBreak(id, style)`

### 4.2 Content blocks

Create content using `ContentBlockFactory`.

#### Text

- `CreateText(id, style, text)`

#### Image

- `CreateImage(id, style, sourceKey, stretch)`
- Stretch values: `None`, `Fill`, `Uniform`, `UniformToFill`

#### Shapes

- `CreateRectangle(id, style, fill, stroke, strokeWidth)`
- `CreateEllipse(id, style, fill, stroke, strokeWidth)`
- `CreateLine(id, style, stroke, strokeWidth)`

#### Barcodes and QR

- `CreateBarcode(id, style, symbology, value, showText)`
- `CreateBarcode(id, style, symbologyType, value, showText)`
- `CreateQrCode(id, style, value, showText)`
- `CreateMicroQrCode(id, style, value, showText)`

Typed barcode values:

- `QrCode`, `MicroQr`, `Code128`, `Code39`, `Ean13`, `Ean8`, `UpcA`, `UpcE`, `Itf`, `Pdf417`, `DataMatrix`, `Codabar`

#### Charts

- `CreateChart(id, style, chartTypeString, chartData)`
- `CreateChart(id, style, chartTypeValue, chartData)`
- `CreateVerticalBarChart(id, style, points)`
- `CreateHorizontalBarChart(id, style, points)`
- `CreateLineChart(id, style, points)`
- `CreatePieChart(id, style, points)`

Typed chart values:

- `BarVertical`, `BarHorizontal`, `Line`, `Pie`

## 5. Chart model (code-first)

### 5.1 Data points

```csharp
var points = new List<ChartDataPoint>
{
    new() { Label = "North", Value = 46 },
    new() { Label = "South", Value = 38, Color = Color.FromRgb(16, 185, 129) }
};
```

`ChartDataPoint` properties:

- `Label` (required)
- `Value` (required)
- `Color` (optional)

### 5.2 Options

`ChartSeriesData.Options` supports:

- Axes and grid: `ShowAxes`, `ShowGridLines`, `ShowTicks`, `ShowTickLabels`
- Axis labels: `XAxisLabel`, `YAxisLabel`
- Scale: `MinValue`, `MaxValue`, `XAxisTickCount`, `YAxisTickCount`
- Styling: `AxisColor`, `GridLineColor`, `LabelColor`, `AxisLineWidth`, `GridLineWidth`, `TickLength`, `LabelFontSize`
- Bar: `BarGapRatio`
- Line: `LineColor`, `LineWidth`, `ShowMarkers`
- Pie labels: `ShowPieLabels`, `PieLabelColor`, `PieLabelFontWeight`
- Legend (pie, bar, line): `ShowLegend`, `LegendPosition`, `LegendMarkerSize`, `LegendFontSize`, `LegendTextColor`

Legend positions:

- `Right`, `Bottom`, `Left`, `Top`

Example:

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
        ShowGridLines = true,
        ShowLegend = true,
        LegendPosition = PieLegendPosition.Right,
        LineColor = Color.FromRgb(37, 99, 235),
        LineWidth = 2.4f
    }
};

var chart = ContentBlockFactory.CreateChart(
    id: "pipeline-chart",
    style: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
    chartType: ChartTypeName.Line,
    chartData: chartData);
```

## 6. Building tables manually

If you want full table control without fluent helpers, create `TableBlock` with rows and cells.

### 6.1 Table primitives

- `TableColumn`: `Width`, `MinWidth`, `MaxWidth`, `Grow`
- `RowBlock`: `RowType`, `KeepTogether`, `Cells`
- `CellBlock`: `ColumnIndex`, `ColSpan`, `RowSpan`, `Children`

### 6.2 Example

```csharp
var cellStyle = new AppliedStyle
{
    FontFamily = "Arial",
    FontSize = 11f,
    Padding = new Thickness(8f, 6f),
    Border = Border.Uniform(1f, Color.Black)
};

var textStyle = new AppliedStyle
{
    FontFamily = "Arial",
    FontSize = 11f
};

var table = new TableBlock
{
    Id = "orders-table",
    Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
    Columns =
    [
        new TableColumn { MinWidth = 120f, Grow = 1f },
        new TableColumn { Width = 90f, MinWidth = 90f, Grow = 0f }
    ],
    Rows =
    [
        new RowBlock
        {
            Id = "header-row",
            RowType = RowType.Header,
            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f, FontWeight = FontWeight.SemiBold },
            Cells =
            [
                ReportDsl.TextCell("h-c1", 0, cellStyle, textStyle, "Customer"),
                ReportDsl.TextCell("h-c2", 1, cellStyle, textStyle, "Amount")
            ]
        },
        new RowBlock
        {
            Id = "data-row-1",
            RowType = RowType.Data,
            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            Cells =
            [
                ReportDsl.TextCell("d1-c1", 0, cellStyle, textStyle, "Acme"),
                ReportDsl.TextCell("d1-c2", 1, cellStyle, textStyle, "12500")
            ]
        }
    ],
    RepeatHeaders = true
};
```

## 7. Running the report

```csharp
using KineticReports.Core.Definition;
using KineticReports.Core.Engine;

var engine = services.GetRequiredService<IReportEngine>();

var definition = new ReportDefinition
{
    SchemaVersion = "1.0",
    Id = "code-first-1",
    Name = "Code First Report",
    Parameters = [],
    DataSources = [],
    Styles = [],
    Plugins = []
};

var document = await engine.RunAsync(definition, ct);
```

Important:

- Your `IReportBuilder.Build(...)` creates the visual block output.
- `ReportDefinition.Layout` is optional when you use a custom builder.

## 8. Style patterns for code-first

Recommended pattern:

- Create a small style palette once
- Reuse styles across all blocks
- Keep one default text style and one default region style

Example helper:

```csharp
public static class ReportStyles
{
    public static AppliedStyle Region => new() { FontFamily = "Arial", FontSize = 11f };
    public static AppliedStyle Title => new() { FontFamily = "Arial", FontSize = 16f, FontWeight = FontWeight.Bold };
    public static AppliedStyle Cell => new()
    {
        FontFamily = "Arial",
        FontSize = 11f,
        Padding = new Thickness(8f, 6f),
        Border = Border.Uniform(1f, Color.Black)
    };
}
```

## 9. Checklist

- Every block has a stable id
- All required fields on factory methods are set
- Table cell `ColumnIndex` values are consistent with defined columns
- Barcode values are valid for selected symbology
- Chart data has at least one point

## 10. Troubleshooting

- Empty output: verify your custom builder is registered as `IReportBuilder`
- Layout overlaps: verify table columns and span values
- Missing image: verify `sourceKey` and image resolver configuration
- No chart legend: set `ShowLegend = true` and choose `LegendPosition`
