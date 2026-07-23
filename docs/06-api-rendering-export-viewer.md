# 06 - Rendering, Export, Plugins, and Viewer API Reference

## Rendering Contracts

| Type | Kind | Purpose | Key Methods |
|---|---|---|---|
| `IRenderer` | interface | Convert `ReportDocument` into render output (PDF/PNG/etc.) | `RenderAsync(...)` |
| `IGraphicsContext` | interface | Backend-agnostic drawing primitives | `FillRectangle`, `DrawText`, `DrawImage`, etc. |
| `ReportDocumentRenderer` | class | Traverses `ReportDocument` and issues graphics calls | `Render(...)` |

## Rendering Implementations

| Type | Package | Purpose |
|---|---|---|
| `SkiaRenderer` | `KineticReports.Core` | Default Skia-backed renderer |
| `SkiaGraphicsContext` | `KineticReports.Core` | Skia implementation of draw primitives |
| `SkiaTextLayout` | `KineticReports.Core` | Default `ITextLayout` implementation |

## Export Contracts

| Type | Kind | Purpose | Methods |
|---|---|---|---|
| `IHtmlExporter` | interface | Export `ReportDocument` as HTML | `ExportAsync(ReportDocument, Stream, CancellationToken)` |
| `IReportDocumentExporter` | interface | Generic document exporter contract for one format | `Format`, `ExportAsync(ReportDocument, CancellationToken)` |
| `IReportDocumentExporterRegistry` | interface | Resolve registered exporters by format ID | `GetAvailableFormats()`, `TryGetExporter(...)` |

## Export Implementations

| Type | Package | Purpose |
|---|---|---|
| `HtmlExporter` | `KineticReports.Core` | Built-in HTML exporter with inline CSS |
| `HtmlReportDocumentExporter` | `KineticReports.Core` | Registry-backed HTML document exporter |
| `PdfReportDocumentExporter` | `KineticReports.Core` | Built-in PDF document exporter using Skia |
| `ExcelExporter` (if present) | `KineticReports.Export.Excel` | XLSX example export implementation |

## Plugin Contracts

| Type | Kind | Purpose | Key Members |
|---|---|---|---|
| `IPlugin` | interface | Plugin lifecycle + metadata | `InitializeAsync`, `UnloadAsync`, metadata props |
| `IPluginManager` | interface | Discover/load/unload plugins | `DiscoverAndLoadPluginsAsync`, `LoadPluginAsync`, `UnloadPluginAsync` |
| `PluginBase` | abstract class | Common lifecycle behavior | `OnInitializeAsync`, `OnUnloadAsync` |
| `IExportFormatRegistryPlugin` | interface | Contribute discoverable format descriptors | `GetFormats()`, `Order` |
| `IExportNegotiationPlugin` | interface | Negotiate requested format to final format | `Negotiate(...)`, `Order` |
| `IExportArtifactPostProcessorPlugin` | interface | Post-process artifact bytes after export | `ProcessArtifactAsync(...)`, `Order` |

## Viewer Contracts

| Type | Project | Purpose |
|---|---|---|
| `IReportService` | Viewer.Blazor | Viewer-level report operations |
| `IReportExecutor` | Viewer.Web | Server-side report execution abstraction |
| `IReportStore` | Viewer.Web | Report definition retrieval abstraction |

## Packaging Notes

- `KineticReports.Core` is the primary package and contains the default rendering/export/runtime implementation surface.
- `KineticReports.Viewer.Blazor`, `KineticReports.Viewer.Mvc`, and `KineticReports.Viewer.Web` are optional host-specific packages that layer on top of Core.
- `KineticReports.Export.Excel` remains separate as an example exporter package.

### Viewer.Blazor Service Surface (Current)

- `RenderHtmlAsync(...)` renders a report preview payload.
- `ExportAsync(...)` exports using the currently registered local viewer formats.
- `HitTestAsync(...)` resolves page-local coordinates to a visual element.
- `SearchTextAsync(...)` returns deterministic ordered text matches.
- `GetLatestTrace()` returns latest execution/export trace entries.

### Viewer.Blazor Component Surface (Current)

- `ReportViewer` supports completion callbacks:
	- `OnExecutionComplete`
	- `OnSearchComplete`
	- `OnHitTestComplete`
- Preview click-to-hit-test is wired through Viewer.Blazor static asset interop (`wwwroot/reportViewer.js`) and page-local coordinate mapping.

## Junior Tips

- Exporters and renderers should **never** re-layout elements.
- If text rendering is off, verify `ITextLayout` and CSS unit consistency.
