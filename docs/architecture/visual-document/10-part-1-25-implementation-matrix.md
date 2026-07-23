# Part 1-25 Implementation Matrix

Status: Draft
Owner: Architecture + Rendering + Viewer
Last Updated: 2026-07-23

## Purpose
This matrix maps the ChatGPT recommendation (Parts 1-25) to current repository implementation status.

## Status Legend
- Implemented: Shipped in code and validated with tests.
- Partial: Foundation exists, but scope is incomplete.
- Planned: Not yet implemented; tracked in backlog.

## Matrix
| Part | Topic | Status | Current Evidence | Gaps |
|---|---|---|---|---|
| 1 | Vision | Partial | Overview and ADR chapters: 01, 02 | Needs deeper non-goals, designer relationship, and long-term principles |
| 2 | Rendering Pipeline | Implemented | ReportDocument -> VisualDocumentBuilder -> VisualDocument in code and docs | Needs richer transformation tracing docs |
| 3 | Object Model | Partial | VisualDocument/VisualPage/VisualElement hierarchy implemented | Missing many proposed element types |
| 4 | Visual Tree | Partial | Pages -> Layers -> Elements -> Children implemented | Mutation and ownership rules need stronger normative language |
| 5 | Coordinate System | Partial | DIP-centric bounds and clipping normalization implemented | Transforms/rotation/scaling contracts not fully enforced in renderers |
| 6 | Drawing Primitives | Partial | Text, image, shape, container, table placeholder supported | Rounded rect, polygon, svg, barcode, qrcode, chart drawing primitives missing |
| 7 | Typography | Planned | Baseline text rendering only | No formal font fallback, RTL/BiDi, selection/highlighting model |
| 8 | Paint System | Planned | Basic fill/stroke/opacity behavior only | No brush abstraction, gradients, patterns, blend, shadow |
| 9 | Layering | Partial | Header/body/footer layers implemented | No overlay/annotations/selection/debug layer taxonomy |
| 10 | Hit Testing | Partial | Visual hit-test index + Viewer.Web hit-test API implemented | Blazor and MVC component host interaction contract still pending |
| 11 | Search | Partial | Visual text search index + Viewer.Web search API implemented | Find-next/find-previous contract and Blazor/MVC host integration pending |
| 12 | Selection | Planned | None | No text or box selection data model and interactions |
| 13 | Hyperlinks | Planned | None | No internal/external/drillthrough link model |
| 14 | Accessibility | Planned | None | No semantic tree, reading order, alt text, tagged PDF strategy |
| 15 | Viewer Architecture | Partial | Viewer.Blazor component host + Viewer.Web optional adapter host | Standalone MVC component package and full viewport/virtualization spec pending |
| 16 | Rendering API | Partial | Existing renderer abstractions + visual adapters | No IVisualRenderer/IRenderSurface/IRenderContext/IRenderPass contracts |
| 17 | Skia Backend | Partial | VisualSkiaRenderer baseline PDF adapter implemented | Primitive parity and advanced PDF features incomplete |
| 18 | HTML Backend | Partial | VisualHtmlExporter implemented and wired | Strategy alternatives (canvas/svg/hybrid) not specified as formal modes |
| 19 | PDF Backend | Partial | Visual-mode PDF export path works | Font embedding policies, bookmarks, metadata, compression roadmap missing |
| 20 | Printing | Planned | None | No print-specific pipeline or settings model |
| 21 | Performance | Planned | None | No cache strategy, dirty regions, virtual pages, glyph/image cache plan |
| 22 | Animation (Future) | Planned | None | No roadmap notes yet |
| 23 | Serialization | Partial | schemaVersion field exists on VisualDocument | No formal JSON contract + round-trip compatibility suite |
| 24 | Diagnostics | Partial | Parity diagnostics and trace surfaces exist | No visual inspector/debug overlays/render trace viewer |
| 25 | Extensibility | Partial | Exporter registry/plugin seam exists | No extensible visual element/paint/render pass plugin model |

## Implemented Core Recommendation
The key architecture recommendation is implemented:

ReportDefinition -> Layout Engine -> ReportDocument -> VisualDocumentBuilder -> VisualDocument -> Renderer -> Output

This is present in:
- Core visual model and builder
- Visual HTML exporter
- Visual PDF (Skia) adapter
- Sample and Viewer.Web pipeline integrations

## Implemented Evidence (Code)
- Core model and builder: src/Core/KineticReports.Visual/
- Visual HTML exporter: src/Export/KineticReports.Export.Html/VisualHtmlExporter.cs
- Visual Skia renderer: src/Rendering/KineticReports.Rendering.Skia/VisualSkiaRenderer.cs
- Sample host integration: src/Samples/KineticReports.Samples.Blazor/
- Viewer.Web integration: src/Viewer/KineticReports.Viewer.Web/

## Implemented Evidence (Tests)
- tests/unit-tests/KineticReports.Visual.Tests/DefaultVisualDocumentBuilderTests.cs
- tests/unit-tests/KineticReports.Rendering.Skia.Tests/VisualSkiaRendererTests.cs
- tests/unit-tests/KineticReports.Rendering.Skia.Tests/PdfReportDocumentExporterTests.cs
- tests/unit-tests/KineticReports.Viewer.Web.Tests/Endpoints/ReportEndpointsExportTests.cs

## Decision
Treat the current state as a completed foundation phase, then execute remaining parts in staged waves to avoid destabilizing export and viewer flows.

## Hosting Direction Clarification
- Primary packaging direction: standalone embeddable viewer components (Blazor and MVC).
- Viewer.Web should be treated as an optional remote API adapter host, not a required dependency for embedded component scenarios.
