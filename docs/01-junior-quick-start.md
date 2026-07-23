# 01 - Quick Start for Junior Developers

This guide helps you understand what to run, what to read first, and where to add code.

## What KineticReports Is

KineticReports is a deterministic reporting engine for .NET 10.

- **Input:** `ReportDefinition`
- **Output:** `ReportDocument` and rendered/exported formats (HTML, PDF, etc.)

## First Things to Learn

1. `ReportDefinition` is the source definition of a report.
2. `IReportEngine` orchestrates the pipeline.
3. `ILayoutEngine` computes geometry and pages.
4. `ReportDocument` is the immutable final layout model.
5. Renderers and exporters consume `ReportDocument`.

## Typical Local Run (Sample App)

- Open solution: `KineticReports.slnx`
- Run sample: `src/Samples/KineticReports.Samples.Blazor`
- Open home page, choose report type/provider, and click **Author + Compile**.
- Use **Run / Refresh** in the viewer to render the report preview.
- Use **Search** and **Hit Test** in the viewer to validate interaction behavior.
- Click **Export** to download the currently supported viewer artifact (HTML in local in-process mode).

## Project Map

- `src/Core/KineticReports.Core` - primary runtime package containing domain types, authoring, engine, layout, rendering, built-in HTML/PDF export, plugins, and SQL data providers
- `src/Viewer/*` - optional viewer packages for Blazor, MVC, and Web hosts
- `src/Export/KineticReports.Export.Excel` - example exporter package
- `src/Plugins/KineticReports.Samples.Plugins` - example plugin project
- `src/Samples/KineticReports.Samples.Blazor` - sample app consuming Core plus Viewer.Blazor

## Core Rules You Must Follow

- Coordinates are DIPs (`1/96` inch).
- Layout is immutable after Arrange.
- Pagination happens after Arrange.
- Exporters consume `ReportDocument` only.
- Text layout is centralized via `ITextLayout`.

## What to Read Next

- [02 - Report Generation Flow](./02-report-generation-flow.md)
- [07 - Extension Points Cheat Sheet](./07-extension-points.md)
