# 01 - Quick Start for Junior Developers

This guide helps you understand what to run, what to read first, and where to add code.

## What KineticReports Is

KineticReports is a deterministic reporting engine for .NET 10.

- **Input:** `ReportDefinition`
- **Output:** `ReportLayout` and rendered/exported formats (HTML, PDF, etc.)

## First Things to Learn

1. `ReportDefinition` is the source definition of a report.
2. `IReportEngine` orchestrates the pipeline.
3. `ILayoutEngine` computes geometry and pages.
4. `ReportLayout` is the immutable final layout model.
5. Renderers and exporters consume `ReportLayout`.

## Typical Local Run (Sample App)

- Open solution: `KineticReports.slnx`
- Run sample: `src/Samples/KineticReports.Samples.Blazor`
- Open home page and click **View** on sample reports.

## Project Map

- `src/Core/KineticReports.Core` - domain types and contracts
- `src/Core/KineticReports.Engine` - orchestration pipeline
- `src/Core/KineticReports.Layout` - LayoutSizing/Arrange/Pagination
- `src/Rendering/*` - renderer pipeline
- `src/Export/*` - exporters
- `src/Plugins/*` - plugin infrastructure

## Core Rules You Must Follow

- Coordinates are DIPs (`1/96` inch).
- Layout is immutable after Arrange.
- Pagination happens after Arrange.
- Exporters consume `ReportLayout` only.
- Font metrics are centralized via `IFontMetrics`.

## What to Read Next

- [02 - Report Generation Flow](./02-report-generation-flow.md)
- [07 - Extension Points Cheat Sheet](./07-extension-points.md)
