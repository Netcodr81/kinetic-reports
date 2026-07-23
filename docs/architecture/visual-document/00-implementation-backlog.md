# KineticReports Visual Document Implementation Backlog

## Purpose
This backlog converts the VisualDocument strategy into executable work for this repository.
It is optimized for incremental delivery with regression safety and no big-bang rewrite.

## Scope
- Introduce a VisualDocument model and builder between ReportDocument and renderers.
- Keep current HTML and PDF paths operational during migration.
- Deliver viewer-oriented capabilities (hit testing, search index, selection metadata) incrementally.
- Deliver standalone viewer components for Blazor and ASP.NET Core MVC hosts.

## Current Codebase Mapping
- Semantic document model: src/Core/KineticReports.Core/Layout
- Engine pipeline: src/Core/KineticReports.Engine
- HTML export: src/Export/KineticReports.Export.Html
- Rendering abstractions: src/Core/KineticReports.Core/Rendering and src/Rendering/KineticReports.Rendering
- Skia PDF/PNG: src/Rendering/KineticReports.Rendering.Skia
- Viewer hosts: src/Viewer/KineticReports.Viewer.Blazor and src/Viewer/KineticReports.Viewer.Web
- Sample integration surface: src/Samples/KineticReports.Samples.Blazor

## Delivery Principles
- Do not replace existing rendering pipeline until parity gates pass.
- Add feature flag for Visual pipeline selection.
- Require deterministic outputs and golden tests for each phase.
- Keep ReportDocument immutable and renderer-agnostic.
- Prefer component-first hosting contracts; Web API endpoints are optional adapter surfaces, not required runtime dependencies for embedded viewers.

## Epic E1: Architecture Guardrails and Spec Foundation
Owner: Architecture
Target Sprint: 1

### Story E1-S1: Add ADR for VisualDocumentBuilder insertion
Projects:
- docs

Tasks:
- Add ADR defining pipeline as ReportDocument -> VisualDocumentBuilder -> VisualDocument -> Renderer.
- Define ownership boundaries: Layout owns composition, Builder owns translation, Renderer owns drawing.
- Define migration policy and rollback strategy.

Acceptance Criteria:
- ADR approved in PR review.
- ADR includes compatibility strategy with existing exporters/renderers.
- ADR includes non-goals for phase 1.

### Story E1-S2: Create handbook-lite chapter skeleton
Projects:
- docs

Tasks:
- Create chapter files under docs/architecture/visual-document for:
  - Overview
  - Rendering Pipeline
  - Object Model
  - Coordinate System
  - Renderer API
  - HTML Backend Mapping
  - PDF Backend Mapping
  - Testing and Diagnostics
- Add table-of-contents page linking each chapter.

Acceptance Criteria:
- All chapter files exist and are linked from an index file.
- Each chapter has a "Status" section and unresolved decisions list.

## Epic E2: Visual Core Model
Owner: Core
Target Sprint: 1-2

### Story E2-S1: Add Visual model project
Projects:
- src/Core (new project recommended: KineticReports.Visual)

Tasks:
- Create project and wire into solution.
- Add immutable types:
  - VisualDocument
  - VisualPage
  - VisualElement (abstract)
  - VisualContainer
  - VisualText
  - VisualImage
  - VisualShape
  - VisualTablePlaceholder
  - VisualClip
  - VisualTransform
- Add lightweight metadata dictionary support for diagnostics and source linkage.

Acceptance Criteria:
- Project builds with net10.0.
- Public APIs include XML docs.
- Unit tests validate immutability and construction invariants.

### Story E2-S2: Serialization contract for VisualDocument
Projects:
- src/Core/KineticReports.Visual
- tests/unit-tests

Tasks:
- Add JSON serialization contract using System.Text.Json.
- Version stamp VisualDocument payload (schemaVersion).
- Add round-trip serialization tests.

Acceptance Criteria:
- VisualDocument serializes/deserializes losslessly for baseline types.
- schemaVersion included in output.
- Tests cover backward-compat handling for missing optional fields.

## Epic E3: VisualDocumentBuilder
Owner: Engine/Layout
Target Sprint: 2

### Story E3-S1: Create builder interface and default implementation
Projects:
- src/Core/KineticReports.Engine or src/Rendering/KineticReports.Rendering (shared placement decision in ADR)

Tasks:
- Define IVisualDocumentBuilder.
- Implement DefaultVisualDocumentBuilder mapping:
  - ReportDocument pages -> VisualPage
  - Header/body/footer blocks -> corresponding page layers or regions
  - Layout block bounds -> visual bounds
- Preserve source block id and type metadata on visual nodes.

Acceptance Criteria:
- Builder handles all currently emitted layout block types.
- Unsupported types are represented as explicit placeholders with diagnostics.
- Unit tests compare expected visual tree for representative sample documents.

### Story E3-S2: Coordinate normalization and clipping rules
Projects:
- src/Core/KineticReports.Visual
- src/Core/KineticReports.Engine (or builder home)

Tasks:
- Define canonical coordinate behavior in DIPs.
- Normalize negative/overflow bounds handling in builder.
- Carry clip/transform metadata where available.

Acceptance Criteria:
- Documented coordinate contract implemented in code.
- Tests cover page boundary clipping metadata generation.

## Epic E4: Dual Pipeline Integration and Feature Flag
Owner: Viewer/Samples
Target Sprint: 2-3

### Story E4-S1: Add render mode feature flag
Projects:
- src/Samples/KineticReports.Samples.Blazor
- src/Viewer/KineticReports.Viewer.Blazor
- src/Viewer/KineticReports.Viewer.Web

Tasks:
- Add config switch: Rendering:PipelineMode = Legacy or Visual.
- Wire services to select legacy exporter path vs visual-renderer path.
- Add trace logs indicating selected mode.

Acceptance Criteria:
- Toggling mode requires no code change and no app restart in dev when feasible.
- Trace output clearly indicates pipeline branch.

### Story E4-S2: Introduce parity comparison hook
Projects:
- src/Samples/KineticReports.Samples.Blazor
- tests/integration-tests

Tasks:
- Build comparison utility for:
  - page count
  - block/element count
  - bounds delta summary
- Expose comparison report in sample app diagnostics panel.

Acceptance Criteria:
- Comparison runs for all built-in authored samples.
- Failing parity writes clear diagnostics.

## Epic E5: HTML Visual Renderer
Owner: Export
Target Sprint: 3

### Story E5-S1: Add VisualHtmlExporter
Projects:
- src/Export/KineticReports.Export.Html

Tasks:
- Introduce VisualHtmlExporter consuming VisualDocument.
- Preserve semantic wrappers already introduced for report/page/header/main/footer.
- Emit script model payload for visual tree diagnostics.

Acceptance Criteria:
- Output HTML passes existing smoke expectations.
- Visual exporter supports text/image/shape/container and table placeholder.
- No regression in current sample reports when mode is Visual.

### Story E5-S2: Reuse and adapt stylesheet tokens
Projects:
- src/Export/KineticReports.Export.Html/Styles

Tasks:
- Ensure CSS classes cover visual node types.
- Add explicit clipping and overflow behavior for page and region containers.
- Add test snapshots for CSS regressions used by viewer.

Acceptance Criteria:
- No side clipping for authored examples under Visual mode.
- Multi-page overlap regressions are not present in baseline samples.

## Epic E6: PDF Visual Renderer (Skia-backed)
Owner: Rendering
Target Sprint: 3-4

### Story E6-S1: Add VisualSkiaRenderer adapter
Projects:
- src/Rendering/KineticReports.Rendering.Skia

Tasks:
- Add adapter to draw VisualDocument primitives onto Skia canvas.
- Preserve existing font, image cache, and path drawing behavior where possible.
- Keep SkiaSharp version at 4.x.

Acceptance Criteria:
- Multi-page PDF export works with Visual mode.
- Visual PDF output validated on baseline samples.
- Existing Skia renderer remains available for rollback.

### Story E6-S2: Deterministic rendering and diff harness
Projects:
- tests/integration-tests
- tests/golden

Tasks:
- Add deterministic PDF smoke checks (metadata normalization strategy where needed).
- Add PNG page rasterization snapshot comparison harness for visual diffs.

Acceptance Criteria:
- Golden comparisons are stable across CI runs.
- Diff output is actionable and includes page/index information.

## Epic E7: Viewer Interaction Capabilities
Owner: Viewer
Target Sprint: 4

### Story E7-S1: Hit test index and element lookup
Projects:
- src/Viewer/KineticReports.Viewer.Blazor
- src/Viewer/KineticReports.Viewer.Web

Tasks:
- Build element lookup map from visual ids to bounds/metadata.
- Implement point-to-element hit testing helper.
- Surface selected element diagnostics panel in sample viewer.

Acceptance Criteria:
- Clicking preview resolves element id consistently.
- Works for text/image/shape/container/table placeholder nodes.

### Story E7-S2: Text search index for viewer
Projects:
- src/Viewer/KineticReports.Viewer.Blazor
- src/Viewer/KineticReports.Viewer.Web

Tasks:
- Build per-page text index from VisualText nodes.
- Implement find-next/find-previous navigation contract.
- Highlight matched nodes in HTML preview mode.

Acceptance Criteria:
- Search returns deterministic ordered results.
- Navigation jumps to proper page and element.

## Epic E8: Testing, Telemetry, and Rollout
Owner: QA/Platform
Target Sprint: 4+

### Story E8-S1: Full test matrix and CI gates
Projects:
- tests/unit-tests
- tests/integration-tests

Tasks:
- Add matrix coverage for Legacy and Visual pipeline modes.
- Add authored sample regression suite covering multipage, header/footer repeat, wide tables, charts, barcodes, QR placeholders.
- Add failure triage docs.

Acceptance Criteria:
- CI blocks merge on visual parity failure for protected sample set.
- Test reports identify failing mode and renderer.

### Story E8-S2: Progressive default rollout
Projects:
- src/Samples/KineticReports.Samples.Blazor
- src/Viewer/KineticReports.Viewer.Blazor
- src/Viewer/KineticReports.Viewer.Web

Tasks:
- Default Visual mode for sample app first.
- Collect telemetry and defect rate for 2 sprint cycles.
- Promote Visual mode default in viewer web host after acceptance criteria are met.

Acceptance Criteria:
- Defect rate and rollback incidents remain under agreed threshold.
- Legacy mode retained as fallback until final cutover decision.

## Epic E9: Standalone Viewer Packaging (Blazor + MVC)
Owner: Viewer
Target Sprint: 5+

### Story E9-S1: Blazor standalone component contract
Projects:
- src/Viewer/KineticReports.Viewer.Blazor
- src/Samples/KineticReports.Samples.Blazor

Tasks:
- Confirm `KineticReports.Viewer.Blazor` remains component-library only (no endpoint runtime dependency).
- Keep viewer contract host-friendly: host passes report definition + parameters, viewer renders in-process.
- Provide registration guidance for required supporting services.

Acceptance Criteria:
- Sample.Blazor integration works without Viewer.Web endpoints.
- Viewer component renders after host registers required services.

### Story E9-S2: MVC standalone component package
Projects:
- src/Viewer (new: KineticReports.Viewer.Mvc)
- tests/unit-tests

Tasks:
- Create MVC/Razor Class Library viewer package for embedding in MVC apps.
- Expose a simple host API: pass report definition + parameters and render output.
- Reuse shared visual pipeline services where possible.

Acceptance Criteria:
- MVC sample host renders report with minimal setup.
- No required dependency on Viewer.Web endpoint host.

### Story E9-S3: Optional API adapter host
Projects:
- src/Viewer/KineticReports.Viewer.Web

Tasks:
- Position Viewer.Web as optional remote adapter for multi-client scenarios.
- Keep endpoint contracts additive and non-blocking for component-only hosts.

Acceptance Criteria:
- Component hosts (Blazor, MVC) run without Viewer.Web.
- Viewer.Web remains a supported optional deployment mode.

## Backlog Prioritization (Top 12)
1. E1-S1 ADR for pipeline insertion
2. E2-S1 Visual model project scaffold
3. E3-S1 DefaultVisualDocumentBuilder
4. E4-S1 Feature flag wiring in viewers/samples
5. E5-S1 VisualHtmlExporter baseline
6. E4-S2 Parity comparison hook
7. E6-S1 VisualSkiaRenderer adapter baseline
8. E2-S2 VisualDocument serialization versioning
9. E3-S2 Coordinate normalization and clipping rules
10. E5-S2 CSS and overflow hardening
11. E7-S1 Hit test lookup
12. E8-S1 CI matrix and parity gates

## Sprint Plan (Suggested)
Sprint 1:
- E1-S1, E1-S2, E2-S1

Sprint 2:
- E2-S2, E3-S1, E3-S2, E4-S1

Sprint 3:
- E5-S1, E5-S2, E4-S2, E6-S1

Sprint 4:
- E6-S2, E7-S1, E7-S2, E8-S1

Sprint 5:
- E8-S2 and cutover readiness review

## Definition of Done
- Code complete with XML docs for public APIs.
- Unit and integration tests added and passing.
- Golden artifacts updated and reviewed.
- Documentation updated in docs/architecture/visual-document.
- Trace logs and diagnostics included for new pipeline paths.
- No regression in existing legacy mode behavior.

## Risks and Mitigations
- Risk: visual/legacy divergence during migration.
  - Mitigation: parity checker, feature flags, dual-mode CI gates.
- Risk: performance regressions in viewer.
  - Mitigation: add per-page render timing and memory metrics before default switch.
- Risk: schema churn in VisualDocument.
  - Mitigation: schemaVersion and compatibility tests from day one.

## Cutover Criteria
- Visual mode passes parity checks on protected report suite.
- PDF and HTML outputs are accepted for baseline scenarios.
- Viewer interaction features required for release are complete.
- Stakeholder sign-off on migration checklist.
