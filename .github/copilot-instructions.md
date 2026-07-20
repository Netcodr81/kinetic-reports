# Copilot Instructions — KineticReports

## Project Overview
KineticReports is a cross-platform, deterministic reporting engine for .NET 10+.
It is UI-framework agnostic, data-source agnostic, and renderer agnostic.

## Solution Structure

| Project | Purpose |
|---|---|
| `KineticReports.Core` | Domain primitives: geometry, styling, layout element hierarchy, rendering contracts, report definition |
| `KineticReports.Engine` | Orchestrates data resolution, expression evaluation, and the layout pipeline |
| `KineticReports.Layout` | Implements the Measure, Arrange, and Pagination passes |
| `KineticReports.Rendering` | Abstract renderer base and shared rendering utilities |
| `KineticReports.Rendering.Skia` | SkiaSharp-backed renderer implementation |
| `KineticReports.Export.Html` | HTML exporter |
| `KineticReports.Export.Excel` | Excel (XLSX) exporter |
| `KineticReports.Data.SqlServer` | SQL Server data provider |
| `KineticReports.Viewer.Web` | ASP.NET Core report viewer host |
| `KineticReports.Viewer.Blazor` | Blazor report viewer component library |

## Key Constraints

### SkiaSharp Version
**Always use SkiaSharp version 4.x.** Never reference SkiaSharp v2 or v3 packages.
The correct package reference is:
```xml
<PackageReference Include="SkiaSharp" Version="4.*" />
```
This applies to every project that references SkiaSharp (primarily `KineticReports.Rendering.Skia`).
`KineticReports.Core` has **no dependency** on SkiaSharp — it is renderer-agnostic.

### Target Framework
All projects target `net10.0`. No multi-targeting unless explicitly required.

### Nullable & Implicit Usings
All projects have `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.

## Architecture Rules

- **Renderers never perform layout.** Renderers receive an immutable `LayoutTree` only.
- **Layout is immutable after Arrange** (ADR-011).
- **Pagination occurs after Arrange** (ADR-012).
- **Exporters consume `LayoutTree` only** (ADR-013).
- **All coordinates are Device Independent Pixels (DIPs)** — 1 DIP = 1/96 inch (ADR-014).
- **Font metrics are centralized** via `IFontMetrics` (ADR-015).
- **`ReportDefinition` is immutable** during engine execution.

## Namespace Conventions

```
KineticReports.Core.Geometry      — Point, Size, Rect, Thickness
KineticReports.Core.Styling       — Color, Typography, Border, StyleDefinition, ResolvedStyle
KineticReports.Core.Typography    — IFontMetrics, FontDescriptor
KineticReports.Core.Layout        — LayoutElement, all 12 element types, LayoutTree, IMeasureContext
KineticReports.Core.Rendering     — TextRun, PathGeometry, ImageReference, IRenderer
KineticReports.Core.Definition    — ReportDefinition, ParameterDefinition, DataSourceDefinition
```

## Coding Standards

- All public APIs **must have XML doc comments** (`<summary>` at minimum; `<param>` and `<returns>` where applicable).
- Use `readonly record struct` for geometry value types (Point, Size, Rect, Thickness).
- Use `sealed record` for immutable reference types (ResolvedStyle, TextRun, etc.).
- Use `sealed class` for concrete layout element types.
- Use `abstract class` for `LayoutElement` base.
- Use `required` properties instead of constructor parameters for complex init types.
- Prefer `IReadOnlyList<T>` and `IReadOnlyDictionary<K,V>` for collection properties.
- Initialize collection properties to `[]` (not null) as defaults.
- Internal layout engine state (e.g. `SetTextRuns`) uses `internal` visibility — never `public`.

## Layout Pipeline (spec §2)

```
ReportDefinition → Data Resolution → Expression Evaluation
  → Logical Object Tree → Measure Pass → Arrange Pass
  → Pagination → LayoutTree → Renderer
```

## Style Cascade Order (spec §10)

Theme → Report Defaults → Named Style → Parent Inheritance → Local Override → ResolvedStyle (immutable)
