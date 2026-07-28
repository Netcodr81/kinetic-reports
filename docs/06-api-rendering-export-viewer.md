# 06 - Rendering, Export, Plugins, and Viewer API Reference

## Rendering Contracts

| Type | Kind | Purpose | Key Methods |
|---|---|---|---|
| `IRenderer` | interface | Convert `ReportDocument` into render output (PDF/PNG/etc.) | `RenderAsync(...)` |
| `IGraphicsContext` | interface | Backend-agnostic drawing primitives | `FillRectangle`, `DrawText`, `DrawImage`, etc. |
| `ReportDocumentRenderer` | class | Traverses `ReportDocument` and issues graphics calls | `Render(...)` |
| `RenderOptions` | record | Renderer execution options | `Format`, `Dpi`, `Background`, `PdfWatermark` |
| `PdfWatermarkOptions` | record | Optional PDF watermark overlay settings | `Text`, `Opacity`, `RotationDegrees`, `FontSize`, `Color` |

## Rendering Implementations

| Type | Package | Purpose |
|---|---|---|
| `SkiaRenderer` | `KineticReports.Core` | Default Skia-backed renderer |
| `SkiaGraphicsContext` | `KineticReports.Core` | Skia implementation of draw primitives |
| `SkiaTextLayout` | `KineticReports.Core` | Default `ITextLayout` implementation |

## Export Contracts

Deep dive:

- [16 - Exporter Execution Flow](./16-exporter-execution-flow.md)

| Type | Kind | Purpose | Methods |
|---|---|---|---|
| `IHtmlExporter` | interface | Export `ReportDocument` as HTML | `ExportAsync(ReportDocument, Stream, CancellationToken)` |
| `IReportDocumentExporter` | interface | Generic document exporter contract for one format | `Format`, `ExportAsync(ReportDocument, CancellationToken)` |
| `IReportDocumentExporterRegistry` | interface | Resolve registered exporters by format ID | `GetAvailableFormats()`, `TryGetExporter(...)` |

## Export Implementations

| Type | Package | Purpose |
|---|---|---|
| `HtmlExporter` | `KineticReports.Core` | Built-in HTML exporter that links a host-provided stylesheet |
| `HtmlReportDocumentExporter` | `KineticReports.Core` | Registry-backed HTML document exporter |
| `PdfReportDocumentExporter` | `KineticReports.Core` | Built-in PDF document exporter using Skia |
| `ExcelExporter` (if present) | `KineticReports.Export.Excel` | XLSX example export implementation |

HTML export now expects the host to provide the stylesheet location through `HtmlExportOptions.StylesheetHref` and will emit a `<link rel="stylesheet">` only when that option is configured.

## Plugin Contracts

| Type | Kind | Purpose | Key Members |
|---|---|---|---|
| `IPlugin` | interface | Plugin lifecycle + metadata | `InitializeAsync`, `UnloadAsync`, metadata props |
| `IPluginManager` | interface | Discover/load/unload plugins | `DiscoverAndLoadPluginsAsync`, `LoadPluginAsync`, `UnloadPluginAsync` |
| `PluginBase` | abstract class | Common lifecycle behavior | `OnInitializeAsync`, `OnUnloadAsync` |
| `IReportBlocksPostProcessorPlugin` | interface | Post-process logical report blocks before layout | `ProcessBlocks(...)`, `Order` |
| `IHtmlReportPostProcessorPlugin` | interface | Post-process exported HTML | `ProcessHtmlAsync(...)`, `Order` |
| `IExportFormatRegistryPlugin` | interface | Contribute discoverable format descriptors | `GetFormats()`, `Order` |
| `IExportNegotiationPlugin` | interface | Negotiate requested format to final format | `Negotiate(...)`, `Order` |
| `IExportArtifactPostProcessorPlugin` | interface | Post-process artifact bytes after export | `ProcessArtifactAsync(...)`, `Order` |

Per-report plugin execution control is modeled in Core definition types:

- `ReportDefinition.Plugins`
- `ReportPluginToggleDefinition` (`PluginId`, `Enabled`)

Default behavior:

1. Explicit `enabled: false` skips a matching plugin for that report.
2. Unlisted plugins are treated as enabled.
3. Plugin id matching is case-insensitive.

## Viewer Contracts

| Type | Project | Purpose |
|---|---|---|
| `IReportService` | `KineticReports.Core` | Viewer-level report operations (host-neutral contract) |
| `DefaultReportService` | `KineticReports.Core` | Default local implementation of `IReportService` |
| `IReportExecutor` | Viewer.Web | Server-side report execution abstraction |
| `IReportStore` | Viewer.Web | Report definition retrieval abstraction |
| `IReportMvcService` | Viewer.Mvc | MVC rendering contract for server-side HTML rendering |
| `DefaultReportMvcService` | Viewer.Mvc | Default local implementation of `IReportMvcService` |

## Packaging Notes

- `KineticReports.Core` is the primary package and contains the default rendering/export/runtime implementation surface.
- `KineticReports.Viewer.Blazor`, `KineticReports.Viewer.Mvc`, and `KineticReports.Viewer.Web` are optional host-specific packages that layer on top of Core.
- `KineticReports.Export.Excel` remains separate as an example exporter package.

### Viewer.Blazor Service Surface (Current)

- `ReportViewer` consumes `KineticReports.Core.Viewer.Services.IReportService` through DI.
- `KineticReports.Viewer.Blazor` registers `DefaultReportService` as the default scoped implementation.
- `RenderHtmlAsync(...)` renders a report preview payload.
- `ExportAsync(...)` exports using the currently registered local viewer formats.
- `HitTestAsync(...)` resolves page-local coordinates to a visual element.
- `SearchTextAsync(...)` returns deterministic ordered text matches.
- `GetLatestTrace()` returns latest execution/export trace entries.

Current `IReportService` overloads include:

1. `RenderHtmlAsync(ReportDefinition, parameters, ct)`
2. `RenderHtmlAsync(ReportDocument, ct)`
3. `RenderHtmlAsync(ReportDocument, ReportDefinition?, ct)`
4. `ExportAsync(ReportDefinition, formatId, parameters, ct)`
5. `ExportAsync(ReportDocument, formatId, ct)`
6. `ExportAsync(ReportDocument, ReportDefinition?, formatId, ct)`

The definition-aware `ReportDocument` overloads allow hosts to preserve a prebuilt
document flow while still applying definition-scoped plugin toggles.

PDF watermark note:

1. HTML watermarking runs via `IHtmlReportPostProcessorPlugin`.
2. PDF watermarking runs during Skia PDF rendering using `RenderOptions.PdfWatermark`.
3. In the sample host, PDF watermark options are read from report metadata keys under
   `plugins.kinetic.watermark.*`.

### Viewer.Blazor Component Surface (Current)

- `ReportViewer` supports completion callbacks:
  - `OnExecutionComplete`
  - `OnSearchComplete`
  - `OnHitTestComplete`
- Preview click-to-hit-test is wired through Viewer.Blazor static asset interop (`wwwroot/reportViewer.js`) and page-local coordinate mapping.

## Junior Tips

- Exporters and renderers should **never** re-layout elements.
- If text rendering is off, verify `ITextLayout` and CSS unit consistency.
