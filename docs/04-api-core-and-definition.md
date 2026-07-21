# 04 - Core API Reference (Definition, Layout, Styling, Typography)

This page documents key Core contracts and base abstractions.

## Definition Types

| Type | Kind | Purpose | Key Members |
|---|---|---|---|
| `ReportDefinition` | record | Canonical report input | `SchemaVersion`, `Id`, `Name`, `Parameters`, `DataSources`, `Styles` |
| `DataSourceDefinition` | record | Declares a data source | `Id`, `Name`, `ProviderType`, `Properties` |
| `ParameterDefinition` | record | Declares report parameters | `Id`, `Name`, `Type`, `DefaultValue`, `IsRequired` |

## Core Interfaces

| Interface | Purpose | Methods |
|---|---|---|
| `ILayoutSizingContext` | Supplies measurement services during layout | `ITextLayout TextLayout`, `ResolveImageSize(...)` |
| `ITextLayout` | Text measurement + shaping contract | `MeasureText`, `ShapeText`, `GetAscent/Descent/LineGap` |
| `IRenderer` | Renders `ReportLayout` to stream | `RenderAsync(ReportLayout, Stream, RenderOptions, CancellationToken)` |
| `IDataProvider` | Executes backend query for provider implementations | `ExecuteAsync(QueryRequest, CancellationToken)` |

## Abstract Base Class

| Type | Kind | Purpose | Key Members |
|---|---|---|---|
| `LayoutElement` | abstract class | Base for all layout elements | `Id`, `Style`, `Bounds`, `DesiredSize`, `LayoutSize`, `Arrange` |

## Layout Element Types

| Element | Role |
|---|---|
| `PageElement` | Represents one final page |
| `ReportBlock` | Header/detail/footer grouping and repetition |
| `SectionElement` | Structural grouping (header/body/footer areas) |
| `ContainerElement` | Generic children container |
| `TextElement` | Text content and text runs |
| `TableElement` | Table root |
| `RowElement` | Table row |
| `CellElement` | Table cell |
| `ImageElement` | Images |
| `ShapeElement` | Vector shapes |
| `ChartElement` | Charts |
| `BarcodeElement` | Barcodes |

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
