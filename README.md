# KineticReports

Package layout:
- `KineticReports.Core` is the primary runtime package and contains authoring, engine, layout, rendering, built-in HTML/PDF export, plugin contracts/runtime, and SQL data providers.
- `KineticReports.Viewer.Blazor`, `KineticReports.Viewer.Mvc`, and `KineticReports.Viewer.Web` are optional viewer packages layered on Core.
- `KineticReports.Export.Excel` remains an example exporter package.
- `KineticReports.Samples.Blazor` consumes Core plus Viewer.Blazor.

The complete project documentation is in [`/docs`](./docs/README.md).

Start with:
- [Quick Start for Junior Developers](./docs/01-junior-quick-start.md)
- [Report Generation Flow](./docs/02-report-generation-flow.md)
- [Architecture and Project Map](./docs/03-architecture-project-map.md)
