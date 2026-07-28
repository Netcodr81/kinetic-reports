# 08 - Plugin Development Guide (Tutorial + Reference)

## Tutorial: Create a Basic Plugin

## Step 1: Create a class library

- Target `net10.0`
- Reference `KineticReports.Core`

## Step 2: Implement plugin type

Use `PluginBase` for simpler lifecycle handling.

```mermaid
flowchart LR
    A[Plugin DLL] --> B[DefaultPluginManager discovers assembly]
    B --> C[Find IPlugin implementations]
    C --> D[InitializeAsync]
    D --> E[Plugin available]
```

## Step 3: Add metadata + lifecycle hooks

Set `Id`, `Name`, `Version`, and optionally `Author`/`Description`.

## Step 4: Register plugin services

Inside `OnInitializeAsync`, register/resolve services required by your plugin behavior.

## Step 5: Load plugin

Use `IPluginManager.DiscoverAndLoadPluginsAsync(...)` or `LoadPluginAsync(...)`.

## Pipeline Seams for Plugins

Plugins can participate in deterministic seams:

1. **Before layout:** `IReportBlocksPostProcessorPlugin`
2. **Export registry:** `IExportFormatRegistryPlugin`
3. **Export negotiation:** `IExportNegotiationPlugin`
4. **After exporter output:** `IExportArtifactPostProcessorPlugin`
5. **After HTML export:** `IHtmlReportPostProcessorPlugin`

Per-report plugin gating:

1. `ReportDefinition.Plugins` controls whether a loaded plugin runs for that report.
2. Add `ReportPluginToggleDefinition` entries (`PluginId`, `Enabled`).
3. If a plugin id is not listed, it is enabled by default.

```mermaid
flowchart LR
    A[IReportBuilder.Build] --> B[Report Blocks]
    B --> C[IReportBlocksPostProcessorPlugin]
    C --> D[ILayoutEngine.Layout]
    D --> E[ReportDocument]
    E --> F[IHtmlExporter.ExportAsync]
    F --> G[IExportArtifactPostProcessorPlugin]
    G --> H[IHtmlReportPostProcessorPlugin]
    H --> I[Final HTML]
```

Ordering rules for both seams:

- Plugins run by ascending `Order`.
- Ties are resolved by `Id` using ordinal string comparison.
- This guarantees deterministic output for the same input/plugin set.

Execution context note:

1. Some plugin seams need `ReportDefinition` context to evaluate per-report toggles.
2. If your host uses a prebuilt `ReportDocument`, use the definition-aware service
    overloads to pass `ReportDefinition` alongside the document.
3. In Core viewer service, this is available through:
    - `RenderHtmlAsync(ReportDocument, ReportDefinition?, ct)`
    - `ExportAsync(ReportDocument, ReportDefinition?, formatId, ct)`

PDF watermarking note:

1. HTML watermark plugins do not automatically affect PDF output.
2. PDF watermarking runs in the PDF renderer (Skia) using `RenderOptions.PdfWatermark`.
3. The sample watermark plugin flow maps report metadata keys
    (`plugins.kinetic.watermark.*`) into PDF watermark render options.

## Reference: Plugin API

| Type | Member | Description |
|---|---|---|
| `IPlugin` | `Id` | Unique plugin id |
| `IPlugin` | `Name` | Human-readable display name |
| `IPlugin` | `Version` | Semantic version |
| `IPlugin` | `InitializeAsync(IServiceProvider)` | Called after load |
| `IPlugin` | `UnloadAsync()` | Called before unload |
| `PluginBase` | `OnInitializeAsync()` | Override hook for startup logic |
| `PluginBase` | `OnUnloadAsync()` | Override hook for cleanup logic |
| `IReportBlocksPostProcessorPlugin` | `ProcessBlocks(...)` | Transforms or extends report blocks before layout |
| `IExportFormatRegistryPlugin` | `GetFormats()` | Contributes export format descriptors |
| `IExportNegotiationPlugin` | `Negotiate(...)` | Maps requested format to final format |
| `IExportArtifactPostProcessorPlugin` | `ProcessArtifactAsync(...)` | Transforms exported bytes |
| `IHtmlReportPostProcessorPlugin` | `ProcessHtmlAsync(...)` | Post-processes exported HTML |
| `IPluginManager` | `DiscoverAndLoadPluginsAsync(...)` | Bulk load from folder |
| `IPluginManager` | `UnloadPluginAsync(pluginId)` | Unload one plugin |
| `IReportDocumentExporter` | `Format` + `ExportAsync(...)` | Host-registered exporter for a concrete format |
| `IReportDocumentExporterRegistry` | `TryGetExporter(...)` | Resolves exporter without host conditionals |

Sample host note:

- The Blazor sample integrates the viewer through `AddKineticReportsViewerBlazor(...)` and the Core viewer service contract `KineticReports.Core.Viewer.Services.IReportService`.
- The default implementation is `KineticReports.Core.Viewer.Services.DefaultReportService`.
- The Blazor sample uses `KineticReports.Core.Plugins.PluginManagerExtensions.AddPluginManager(...)` so plugin discovery/loading follows the default Core path.
- Exporter-registry wiring remains available in adapter hosts such as `KineticReports.Viewer.Web`.

Packaging note:

- Plugin contracts and runtime types now ship from `KineticReports.Core`.
- The repository's example plugin lives at `src/Plugins/KineticReports.Samples.Plugins`.

## Example: Implement multiple seams in one plugin

- Implement `IReportBlocksPostProcessorPlugin` to add or transform `ReportBlock`s.
- Implement `IHtmlReportPostProcessorPlugin` to inject HTML/CSS overlays or policies.
- Implement `IExportArtifactPostProcessorPlugin` for non-HTML artifact transforms.
- Keep transformations idempotent where possible (safe on repeated runs).

## Example Plugin Ideas

1. Watermark injection hook.
2. Custom data provider registration.
3. Custom exporter registration.
4. Report policy validator.

## Testing Checklist

- Plugin loads without exceptions.
- Metadata values are correct.
- Unload cleans up state.
- Host app works when plugin is missing.
