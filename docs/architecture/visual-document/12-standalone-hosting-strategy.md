# Standalone Hosting Strategy

Status: Draft
Owner: Viewer
Last Updated: 2026-07-23

## Scope
Defines how KineticReports viewers are packaged and embedded as standalone components.

## Decision
Primary hosting targets are standalone embeddable components:
- Blazor component package
- ASP.NET Core MVC component package

`KineticReports.Viewer.Web` remains supported as an optional remote adapter host.

## Blazor Standalone Component (Primary)
Package:
- src/Viewer/KineticReports.Viewer.Blazor

Host contract:
- Host passes `ReportDefinition` and optional parameters to viewer component.
- Host registers supporting services (engine, builder, exporters, report service adapter).
- Viewer renders in-process; no required dependency on web API endpoints.
- Use `AddKineticReportsViewerBlazor(...)` to register component-supporting services.

Reference integration:
- src/Samples/KineticReports.Samples.Blazor

## MVC Standalone Component
Package:
- src/Viewer/KineticReports.Viewer.Mvc

Host contract:
- MVC app passes `ReportDefinition` and parameters to an embeddable viewer surface.
- Reuse the same rendering pipeline and supporting services where feasible.
- Render output in-process for simple host integration.

## Optional Viewer.Web Adapter Host
Package:
- src/Viewer/KineticReports.Viewer.Web

Role:
- Optional remote API host for multi-client and decoupled deployment scenarios.
- Not required by embedded Blazor or MVC component hosting.
- Keep endpoint features additive; do not couple component package startup to endpoint availability.

## Acceptance Criteria for Packaging Direction
- Blazor sample renders reports without Viewer.Web.
- MVC sample (planned) renders reports without Viewer.Web.
- Viewer.Web remains additive and independently deployable.

## Non-goals
- Forcing all hosts through a single network API.
- Making Viewer.Web a prerequisite for component embedding.
