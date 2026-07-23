# 03 - Architecture and Project Map

This document maps repository projects to responsibilities.

## Solution Structure

| Project | Responsibility |
|---|---|
| `KineticReports.Core` | Primary runtime package containing domain model, authoring, engine, layout, visual model, rendering, Skia integration, built-in HTML/PDF exporters, plugin runtime, and SQL data providers |
| `KineticReports.Export.Excel` | Excel exporter |
| `KineticReports.Viewer.Web` | ASP.NET viewer hosting |
| `KineticReports.Viewer.Blazor` | Blazor viewer component library |
| `KineticReports.Viewer.Mvc` | MVC viewer package |
| `KineticReports.Samples.Blazor` | Sample application consuming Core + Viewer.Blazor |
| `KineticReports.Samples.Plugins` | Example plugin project under the Plugins folder |

## Architecture Boundaries

```mermaid
flowchart TB
	subgraph Core
	  C1[Definition]
	  C2[Authoring]
	  C3[Engine and Layout]
	  C4[Visual and Rendering]
	  C5[Built-in HTML and PDF Export]
	  C6[Plugins and Data Providers]
	end

	subgraph OptionalPackages
	  V1[Viewer.Blazor]
	  V2[Viewer.Mvc]
	  V3[Viewer.Web]
	  X1[Export.Excel]
	end

	C1 --> C3
	C2 --> C3
	C3 --> C4
	C4 --> C5
	C6 --> C5
	C4 --> V1
	C4 --> V2
	C4 --> V3
	C3 --> X1
```

## Important Constraints

- Renderer/exporter does not perform layout.
- `ReportDocument` is immutable after arrange/pagination.
- All dimensions are DIPs.
- Shared `ITextLayout` instance should be used across layout and rendering.
