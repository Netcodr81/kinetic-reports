# Release Notes

## 1.0.0

- Consolidated the default runtime into `KineticReports.Core`.
- Moved authoring, engine, layout, visual model, rendering, Skia, built-in HTML/PDF export, plugins, and SQL data providers into the Core package.
- Kept `KineticReports.Viewer.Blazor`, `KineticReports.Viewer.Mvc`, and `KineticReports.Viewer.Web` as optional host-specific viewer packages.
- Kept `KineticReports.Export.Excel` as a separate example exporter package.
- Moved the sample watermark plugin into `src/Plugins/KineticReports.Samples.Plugins`.
- Updated the sample Blazor app to consume `KineticReports.Core` plus `KineticReports.Viewer.Blazor`.
