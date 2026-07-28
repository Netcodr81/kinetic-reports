# 16 - Exporter Execution Flow

This document explains how export execution works in host-agnostic terms.

```csharp
var result = await reportService.ExportAsync(reportDocument, formatId, cancellationToken);
```

Definition-aware ReportDocument overload (for per-report plugin toggles):

```csharp
var result = await reportService.ExportAsync(reportDocument, definition, formatId, cancellationToken);
```

## Export Runtime Chain

### Service Entry and Report Execution Path

```mermaid
sequenceDiagram
    participant Host as Host
    participant Service as DefaultReportService IReportService
    participant Engine as ReportEngine IReportEngine

    Host->>Service: 1 ExportAsync(definition ReportDefinition, formatId string, parameters ParameterMap, ct CancellationToken)
    Service->>Service: 2 ExecuteReportAsync(definition ReportDefinition, parameters ParameterMap, ct CancellationToken)
    Service->>Engine: 3 RunAsync(definition ReportDefinition, parameters ParameterMap, context ILayoutSizingContext, options LayoutOptions, ct CancellationToken)
    Engine-->>Service: 4 ReportDocument
    Service->>Service: 5 ExportReportDocumentAsync(reportDocument ReportDocument, formatId string, tracePrefix string, ct CancellationToken)
    Service-->>Host: 6 ReportViewerExportResult
```

### Service Direct ReportDocument Export Path

```mermaid
sequenceDiagram
    participant Host as Host
    participant Service as DefaultReportService IReportService

    Host->>Service: 1 ExportAsync(reportDocument ReportDocument, formatId string, ct CancellationToken)
    Service->>Service: 2 ExportReportDocumentAsync(reportDocument ReportDocument, formatId string, tracePrefix string, ct CancellationToken)
    Service-->>Host: 3 ReportViewerExportResult
```

### Service Direct ReportDocument Export Path With Definition Context

```mermaid
sequenceDiagram
    participant Host as Host
    participant Service as DefaultReportService IReportService

    Host->>Service: 1 ExportAsync(reportDocument ReportDocument, definition ReportDefinition?, formatId string, ct CancellationToken)
    Service->>Service: 2 ExportReportDocumentAsync(reportDocument ReportDocument, formatId string, tracePrefix string, definition ReportDefinition?, ct CancellationToken)
    Service-->>Host: 3 ReportViewerExportResult
```

### Built In Format Branching in DefaultReportService

```mermaid
sequenceDiagram
    participant Service as DefaultReportService
    participant Html as IHtmlExporter HtmlExporter
    participant Pdf as SkiaRenderer

    alt 1 formatId html
        Service->>Service: 2 RenderHtmlCoreAsync(reportDocument ReportDocument, definition ReportDefinition?, ct CancellationToken)
        Service->>Html: 3 ExportAsync(reportDocument ReportDocument, output Stream, ct CancellationToken)
        Html-->>Service: 4 HTML bytes written to stream
        Service-->>Service: 5 Build ReportViewerExportResult html
    else 1 formatId pdf
        Service->>Service: 2 Resolve PDF watermark options from definition metadata and plugin toggles
        Service->>Pdf: 3 RenderAsync(reportDocument ReportDocument, output Stream, ct CancellationToken)
        Pdf-->>Service: 3 PDF bytes written to stream
        Service-->>Service: 4 Build ReportViewerExportResult pdf
    else 1 unsupported format
        Service-->>Service: 2 throw NotSupportedException
    end
```

## Plugin Registry Export Chain

### Registry Resolution Path

```mermaid
sequenceDiagram
    participant Host as Host
    participant Registry as ReportDocumentExporterRegistry IReportDocumentExporterRegistry
    participant Exporter as IReportDocumentExporter

    Host->>Registry: 1 TryGetExporter(formatId string, out exporter IReportDocumentExporter)
    alt 2 exporter found
        Registry-->>Host: 3 true and exporter
        Host->>Exporter: 4 ExportAsync(reportDocument ReportDocument, ct CancellationToken)
        Exporter-->>Host: 5 byte[] artifact
    else 2 exporter not found
        Registry-->>Host: 3 false and null
    end
```

### HtmlReportDocumentExporter Internal Path

```mermaid
sequenceDiagram
    participant Host as Caller
    participant Exporter as HtmlReportDocumentExporter IReportDocumentExporter
    participant VisualBuilder as IVisualDocumentBuilder
    participant VisualHtml as IVisualHtmlExporter

    Host->>Exporter: 1 ExportAsync(reportDocument ReportDocument, ct CancellationToken)
    Exporter->>VisualBuilder: 2 Build(reportDocument ReportDocument)
    VisualBuilder-->>Exporter: 3 VisualDocument
    Exporter->>VisualHtml: 4 ExportAsync(visualDocument VisualDocument, output Stream, ct CancellationToken)
    VisualHtml-->>Exporter: 5 HTML bytes written to stream
    Exporter-->>Host: 6 byte[] artifact
```

### PdfReportDocumentExporter Internal Path

```mermaid
sequenceDiagram
    participant Host as Caller
    participant Exporter as PdfReportDocumentExporter IReportDocumentExporter
    participant Renderer as SkiaRenderer IRenderer

    Host->>Exporter: 1 ExportAsync(reportDocument ReportDocument, ct CancellationToken)
    Exporter->>Renderer: 2 RenderAsync(reportDocument ReportDocument, output Stream, ct CancellationToken)
    Renderer-->>Exporter: 3 PDF bytes written to stream
    Exporter-->>Host: 4 byte[] artifact
```

## Step by Step Description

1. Host calls IReportService.ExportAsync with either ReportDefinition or ReportDocument.
2. Host may pass ReportDefinition alongside ReportDocument for definition-scoped plugin behavior.
3. For ReportDefinition input, DefaultReportService executes the report first using IReportEngine.
4. DefaultReportService routes export by formatId in ExportReportDocumentAsync.
5. HTML format uses IHtmlExporter.ExportAsync and applies HTML post-processors in deterministic order.
6. PDF format uses SkiaRenderer.RenderAsync and can apply a watermark overlay via RenderOptions.PdfWatermark.
7. Unsupported format IDs raise NotSupportedException in DefaultReportService.
8. For plugin-style export, the host resolves an exporter from IReportDocumentExporterRegistry.
9. Resolved IReportDocumentExporter performs format-specific export and returns artifact bytes.
10. HtmlReportDocumentExporter converts ReportDocument to VisualDocument and then writes HTML via IVisualHtmlExporter.
11. PdfReportDocumentExporter renders ReportDocument to PDF using SkiaRenderer.

## Interface to Implementation Map

| Interface | Runtime implementation | Primary methods called |
| --- | --- | --- |
| IReportService | DefaultReportService | ExportAsync(definition ReportDefinition, formatId string, parameters ParameterMap, ct CancellationToken), ExportAsync(reportDocument ReportDocument, formatId string, ct CancellationToken), ExportAsync(reportDocument ReportDocument, definition ReportDefinition?, formatId string, ct CancellationToken) |
| IReportEngine | ReportEngine | RunAsync(definition ReportDefinition, parameters ParameterMap, context ILayoutSizingContext, options LayoutOptions, ct CancellationToken) |
| IHtmlExporter | HtmlExporter | ExportAsync(reportDocument ReportDocument, output Stream, ct CancellationToken) |
| IReportDocumentExporterRegistry | ReportDocumentExporterRegistry | GetAvailableFormats(), TryGetExporter(formatId string, out exporter IReportDocumentExporter) |
| IReportDocumentExporter | HtmlReportDocumentExporter, PdfReportDocumentExporter, plugin exporters | ExportAsync(reportDocument ReportDocument, ct CancellationToken) |
| IVisualDocumentBuilder | host-registered implementation | Build(reportDocument ReportDocument) |
| IVisualHtmlExporter | VisualHtmlExporter | ExportAsync(visualDocument VisualDocument, output Stream, ct CancellationToken) |
| IRenderer | SkiaRenderer | RenderAsync(reportDocument ReportDocument, output Stream, ct CancellationToken) |

Watermark-specific behavior in current sample host:

1. `ReportDefinition.Plugins` controls whether `kinetic.watermark` is enabled.
2. HTML watermark is applied through `IHtmlReportPostProcessorPlugin`.
3. PDF watermark is applied in Skia rendering using report metadata keys under
    `plugins.kinetic.watermark.*`.

Type aliases used in this document:
- ParameterMap: string key to object value map

## Key Source References

- [src/Core/KineticReports.Core/Viewer/Services/IReportService.cs](../src/Core/KineticReports.Core/Viewer/Services/IReportService.cs)
- [src/Core/KineticReports.Core/Viewer/Services/DefaultReportService.cs](../src/Core/KineticReports.Core/Viewer/Services/DefaultReportService.cs)
- [src/Core/KineticReports.Core/Plugins/IReportDocumentExporter.cs](../src/Core/KineticReports.Core/Plugins/IReportDocumentExporter.cs)
- [src/Core/KineticReports.Core/Plugins/IReportDocumentExporterRegistry.cs](../src/Core/KineticReports.Core/Plugins/IReportDocumentExporterRegistry.cs)
- [src/Core/KineticReports.Core/Plugins/ReportDocumentExporterRegistry.cs](../src/Core/KineticReports.Core/Plugins/ReportDocumentExporterRegistry.cs)
- [src/Core/KineticReports.Core/Export/Document/HtmlReportDocumentExporter.cs](../src/Core/KineticReports.Core/Export/Document/HtmlReportDocumentExporter.cs)
- [src/Core/KineticReports.Core/Export/Document/PdfReportDocumentExporter.cs](../src/Core/KineticReports.Core/Export/Document/PdfReportDocumentExporter.cs)
- [src/Core/KineticReports.Core/Export/Html/IHtmlExporter.cs](../src/Core/KineticReports.Core/Export/Html/IHtmlExporter.cs)
- [src/Core/KineticReports.Core/Export/Html/IVisualHtmlExporter.cs](../src/Core/KineticReports.Core/Export/Html/IVisualHtmlExporter.cs)
- [src/Core/KineticReports.Core/Export/Html/HtmlExporter.cs](../src/Core/KineticReports.Core/Export/Html/HtmlExporter.cs)
- [src/Core/KineticReports.Core/Export/Html/VisualHtmlExporter.cs](../src/Core/KineticReports.Core/Export/Html/VisualHtmlExporter.cs)
- [src/Core/KineticReports.Core/Visual/IVisualDocumentBuilder.cs](../src/Core/KineticReports.Core/Visual/IVisualDocumentBuilder.cs)
- [src/Core/KineticReports.Core/Rendering/Skia/SkiaRenderer.cs](../src/Core/KineticReports.Core/Rendering/Skia/SkiaRenderer.cs)
