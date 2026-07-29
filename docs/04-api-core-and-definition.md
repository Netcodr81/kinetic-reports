# 04 - Core API Reference (Definition, Layout, Styling, Typography)

This page documents key Core contracts and base abstractions.

## Definition Types

| Type | Kind | Purpose | Key Members |
|---|---|---|---|
| `ReportDefinition` | record | Canonical report input | `SchemaVersion`, `Id`, `Name`, `Parameters`, `DataSources`, `Styles`, `Plugins` |
| `ReportPluginToggleDefinition` | record | Per-report plugin enable/disable entry | `PluginId`, `Enabled` |
| `DataSourceDefinition` | record | Declares a data source | `Id`, `Name`, `ProviderType`, `SourceName`, `Properties` |
| `ParameterDefinition` | record | Declares report parameters | `Id`, `Name`, `Type`, `DefaultValue`, `IsRequired` |

## Core Interfaces

| Interface | Purpose | Methods |
|---|---|---|
| `ILayoutSizingContext` | Supplies measurement services during layout | `ITextLayout TextLayout`, `ResolveImageSize(...)` |
| `ITextLayout` | Text measurement + shaping contract | `MeasureText`, `ShapeText`, `GetAscent/Descent/LineGap` |
| `IRenderer` | Renders `ReportDocument` to stream | `RenderAsync(ReportDocument, Stream, RenderOptions, CancellationToken)` |
| `IDataProvider` | Executes backend query for provider implementations | `ExecuteAsync(QueryRequest, CancellationToken)` |

## Abstract Base Class

| Type | Kind | Purpose | Key Members |
|---|---|---|---|
| `LayoutBlock` | abstract class | Base for all layout blocks | `Id`, `Style`, `Bounds`, `DesiredSize`, `LayoutSize`, `Arrange` |
| `ContentBlock` | sealed class | Unified layout element with type discriminator (v3.0.0+) | `Id`, `Style`, `ContentType`, `Children`, type-specific properties |

## Layout Element Types (v3.0.0+)

All layout elements are now instances of **`ContentBlock`** differentiated by the **`BlockContentType`** enum discriminator.

| ContentType Value | Purpose | Type-Specific Properties | Typical Usage |
|---|---|---|---|
| `Text` | Text content with optional formatting | `Text: string?`, `TextRuns: IReadOnlyList<TextRun>` | Paragraphs, labels, dynamic text |
| `Image` | Raster or vector images | `SourceKey: string?`, `Stretch: ImageStretch` | Photos, logos, backgrounds |
| `Shape` | Vector shapes | `Kind: ShapeKind`, `Fill: Color?`, `Stroke: Color?`, `StrokeWidth: float` | Rectangles, ellipses, dividers |
| `Chart` | Charts and graphs | `ChartType: string?`, `ChartData: object?` | Bar/Line/Pie charts, data visualization |
| `Barcode` | Machine-readable codes | `Symbology: string?`, `Value: string?`, `ShowText: bool` | QR codes, Code128, EAN13 |
| `Container` | Generic children container | `Children: IReadOnlyList<ContentBlock>` | Layout sections, flexible grouping |
| `Table` | Table root | `Children: IReadOnlyList<ContentBlock>` (rows) | Tabular data, structured layouts |
| `Row` | Table row | `Children: IReadOnlyList<ContentBlock>` (cells) | Row within a table |
| `Cell` | Table cell | `Children: IReadOnlyList<ContentBlock>`, `ColumnIndex: int`, `ColSpan: int`, `RowSpan: int` | Cell content, merge support |
| `ReportSection` | Header/detail/footer logical grouping | `Children: IReadOnlyList<ContentBlock>` | Logical report regions (pre-pagination) |
| `PageSection` | Physical page structure (header/footer) | `Children: IReadOnlyList<ContentBlock>` | Page-level sections (post-pagination) |
| `Page` | Represents one final page | `Children: IReadOnlyList<ContentBlock>` | Output page |

### ReportSection vs PageSection (Semantic Distinction)

These two container types (`ContentType.ReportSection` and `ContentType.PageSection`) are intentionally different even though both stack children vertically.

| ContentType | Semantic Layer | Meaning | Typical Source |
|---|---|---|---|
| `ReportSection` | Logical report content (pre-pagination) | What content repeats and when (detail, group header/footer, report header/footer, page header/footer) | `IReportBuilder.Build(...)` and report-block plugins |
| `PageSection` | Physical page structure (post-pagination) | Where arranged content sits on a concrete page (page header/footer sections) | Pagination output (`PageBlock.Header`/`PageBlock.Footer`) |

Practical rule:

1. Use `ReportBlock` when modeling report intent and repetition behavior.
2. Use `PageSectionBlock` when working with already paginated page structure.

Lifecycle mapping:

```text
ReportDefinition
	-> IReportBuilder.Build(...)
	-> IReadOnlyList<ReportBlock>
	-> Layout + Pagination
	-> PageBlock
		 - Header: PageSectionBlock?
		 - Footer: PageSectionBlock?
		 - Children: body layout blocks
	-> ReportDocument
```

## Code-First Authoring Helpers

| Type | Kind | Purpose | Key Members |
|---|---|---|---|
| `CodeBlockType` | enum | Strongly typed code-first block taxonomy | Structure, Content, Supporting block values |
| `CodeBlockFamily` | enum | Coarse grouping for tooling and validation | `Structure`, `Content`, `Supporting` |
| `CodeBlockTypeExtensions` | static class | Canonical alias collapsing and runtime mapping | `GetFamily`, `ToCanonicalType`, `ToLayoutBlockType` |
| `ReportBlockFactory` | static class | Creates concrete `ReportBlock` instances from typed roles | `Create(...)`, `CreatePageBreak(...)` |
| `ContentBlockFactory` | static class | Creates strongly typed content blocks for code-first builders | `CreateText`, `CreateImage`, `CreateRectangle`, `CreateEllipse`, `CreateLine`, `CreateBarcode`, `CreateQrCode`, `CreateChart` |
| `ReportDsl` | static class | Minimal fluent helpers for common report patterns | `TextRegion`, `TextCell` |
| `ReportLayoutBuilder` | sealed class | Fluent builder for assembling ordered report blocks | `Add`, `AddRange`, `AddTextRegion`, `AddPageBreak`, `BeginTable`, `CreateTableRegion`, `Build` |

### Simplification Rule

For code-first builders, prefer canonical runtime block types and collapse aliases early:

1. `List` -> `Group`
2. `Divider` -> `Line`
3. `Spacer` -> `Panel`
4. Token blocks (`PageNumber`, `TotalPages`, `CurrentDate`, `CurrentTime`, `DocumentInfo`) -> `Text`
5. `Header`/`Footer` -> `Section`

This keeps the Core runtime model small while still allowing a broader authoring vocabulary.

`ReportLayoutBuilder.TableRegionBuilder` provides table fluency via:

1. `AddHeaderRow(...)`
2. `AddDataRow(...)`
3. `BuildBlock()`
4. `EndTable()`

### Fluent Builder Example

The following example builds a simple report layout with a page header, a data table,
and a page footer.

```csharp
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

var regionStyle = new AppliedStyle
{
	FontFamily = "Arial",
	FontSize = 12f,
	Padding = new KineticReports.Core.Geometry.Thickness(0f, 0f, 0f, 8f)
};

var headerTextStyle = new AppliedStyle
{
	FontFamily = "Arial",
	FontSize = 18f,
	FontWeight = FontWeight.Bold
};

var bodyTextStyle = new AppliedStyle
{
	FontFamily = "Consolas",
	FontSize = 11f
};

var footerTextStyle = new AppliedStyle
{
	FontFamily = "Arial",
	FontSize = 10f,
	TextColor = Color.FromRgb(90, 90, 90)
};

var headerCellStyle = new AppliedStyle
{
	FontFamily = "Arial",
	FontSize = 11f,
	FontWeight = FontWeight.SemiBold,
	TextColor = Color.White,
	Background = Color.FromRgb(31, 58, 111),
	Padding = new KineticReports.Core.Geometry.Thickness(6f, 8f, 6f, 8f),
	Border = Border.Uniform(1f, Color.FromRgb(199, 210, 230))
};

var dataCellStyle = new AppliedStyle
{
	FontFamily = "Consolas",
	FontSize = 11f,
	Padding = new KineticReports.Core.Geometry.Thickness(5f, 8f, 5f, 8f),
	Border = Border.Uniform(1f, Color.FromRgb(220, 226, 239))
};

var rowStyle = new AppliedStyle { FontFamily = "Arial", FontSize = 11f };

var columns = new[]
{
	new TableColumn { Width = 120f },
	new TableColumn { Width = 180f },
	new TableColumn { Width = 120f }
};

var blocks = new ReportLayoutBuilder()
	.AddTextRegion(
		id: "page-header",
		blockType: BlockType.PageHeader,
		blockStyle: regionStyle,
		textStyle: headerTextStyle,
		text: "Sales Summary")
	.BeginTable(
		regionId: "sales-table-region",
		regionStyle: regionStyle,
		tableId: "sales-table",
		tableStyle: bodyTextStyle,
		columns: columns,
		blockType: BlockType.Detail,
		repeatHeaders: true)
	.AddHeaderRow("sales-header-row", rowStyle, headerCellStyle, ["OrderId", "Customer", "Amount"])
	.AddDataRow("sales-row-1", rowStyle, dataCellStyle, ["SO-1001", "Contoso", "1250.00"])
	.AddDataRow("sales-row-2", rowStyle, dataCellStyle, ["SO-1002", "Fabrikam", "980.00"])
	.EndTable()
	.AddTextRegion(
		id: "page-footer",
		blockType: BlockType.PageFooter,
		blockStyle: regionStyle,
		textStyle: footerTextStyle,
		text: "Generated by KineticReports")
	.Build();
```

`blocks` is now an ordered `IReadOnlyList<ReportBlock>` ready for layout and pagination.

## Styling Types

| Type | Kind | Purpose |
|---|---|---|
| `AppliedStyle` | sealed record | Fully computed style consumed by layout/render/export |
| `StyleDefinition` | record | Named style inputs before resolution |
| `Color` | readonly record struct | ARGB color |
| `Border`/`BorderSide` | record(s) | Border model |

## Geometry Types

| Type | Kind | Notes |
|---|---|---|
| `Point` | readonly record struct | X/Y in DIPs |
| `Size` | readonly record struct | Width/Height in DIPs |
| `Rect` | readonly record struct | Rectangle bounds in DIPs |
| `Thickness` | readonly record struct | Left/Top/Right/Bottom in DIPs |

## Junior Tips

- If an element appears but text is wrong, check `ITextLayout` + `AppliedStyle` first.
- If an element is missing, verify it exists in report blocks before checking layout/exporter/renderer.
