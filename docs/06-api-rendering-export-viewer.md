# 06 - Rendering, Export, Plugins, and Viewer API Reference

## Rendering Contracts

| Type | Kind | Purpose | Key Methods |
|---|---|---|---|
| `IRenderer` | interface | Convert `ReportLayout` into render output (PDF/PNG/etc.) | `RenderAsync(...)` |
| `IGraphicsContext` | interface | Backend-agnostic drawing primitives | `FillRectangle`, `DrawText`, `DrawImage`, etc. |
| `ReportLayoutRenderer` | class | Traverses `ReportLayout` and issues graphics calls | `Render(...)` |

## Rendering Implementations

| Type | Project | Purpose |
|---|---|---|
| `SkiaRenderer` | `KineticReports.Rendering.Skia` | Skia-backed renderer |
| `SkiaGraphicsContext` | `KineticReports.Rendering.Skia` | Skia implementation of draw primitives |
| `SkiaFontMetrics` | `KineticReports.Rendering.Skia` | `IFontMetrics` implementation |

## Export Contracts

| Type | Kind | Purpose | Methods |
|---|---|---|---|
| `IHtmlExporter` | interface | Export `ReportLayout` as HTML | `ExportAsync(ReportLayout, Stream, CancellationToken)` |
| `IReportLayoutExporter` | interface | Generic layout exporter contract for one format | `Format`, `ExportAsync(ReportLayout, CancellationToken)` |
| `IReportLayoutExporterRegistry` | interface | Resolve registered exporters by format ID | `GetAvailableFormats()`, `TryGetExporter(...)` |

## Export Implementations

| Type | Project | Purpose |
|---|---|---|
| `HtmlExporter` | `KineticReports.Export.Html` | HTML exporter with inline CSS |
| `ExcelExporter` (if present) | `KineticReports.Export.Excel` | XLSX export implementation |

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

## Junior Tips

- Exporters and renderers should **never** re-layout elements.
- If text rendering is off, verify `IFontMetrics` and CSS unit consistency.
