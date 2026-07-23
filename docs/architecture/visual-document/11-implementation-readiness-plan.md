# Implementation Readiness Plan

Status: Draft
Owner: Architecture + Rendering + Viewer
Last Updated: 2026-07-23

## Goal
Convert the current VisualDocument foundation into an execution-ready program for the remaining Parts 7-25 while preserving backward compatibility.

## Readiness Summary
- Foundation complete: Builder split, visual core model, visual HTML path, visual PDF baseline, dual pipeline feature flag.
- Main risk: Advanced viewer and rendering features are not yet modeled (hit testing, search, selection, accessibility).
- Recommended approach: Four implementation waves with strict quality gates.
- Hosting direction: standalone component-first (Blazor + MVC), with Viewer.Web treated as an optional adapter host.

## Wave 1: Model and Contract Expansion (Parts 3, 6, 16, 23)
Target: 1-2 sprints

### Scope
- Add missing visual element types (barcode, svg, hyperlink/bookmark, annotation, selection).
- Add formal serialization contract and round-trip tests.
- Define visual renderer contracts (IVisualRenderer family).

### Deliverables
- Expanded types under src/Core/KineticReports.Visual/
- New renderer interfaces and adapter shims
- JSON contract tests under tests/unit-tests/KineticReports.Visual.Tests/

### Acceptance Gates
- All new elements carry metadata and bounds invariants.
- Serialization round-trip passes for all baseline types.
- Legacy pipeline remains fully operational.

## Wave 2: Viewer Interaction Foundation (Parts 10, 11, 12, 15)
Target: 2 sprints

### Scope
- Add hit-test index generation from VisualDocument.
- Add per-page text index and search navigation.
- Add selection model and basic highlight rendering.

### Deliverables
- Viewer services for hit-test and search
- Component contracts for selection and query results (Blazor/MVC first)
- Optional endpoint adapter contracts where remote hosting is needed
- Integration tests for page/layer/element targeting

### Acceptance Gates
- Deterministic hit-test outcomes for overlapping elements.
- Search ordering is stable across runs.
- Selection rendering verified in embedded component hosts (Blazor, MVC).

## Wave 3: Backend Capability Depth (Parts 7, 8, 17, 18, 19, 20, 21)
Target: 2-3 sprints

### Scope
- Typography upgrades (fallback, RTL/BiDi strategy, wrapping edge cases).
- Paint abstraction and backend mapping (brushes, gradients, dashes, shadows).
- PDF backend depth: metadata, bookmarks/hyperlinks, compression policy.
- Performance baseline and cache strategy.

### Deliverables
- Contract docs and implementation in rendering backends
- Deterministic golden diff harness for HTML/PDF
- Performance baseline reports and thresholds

### Acceptance Gates
- Golden outputs stable in CI with actionable diffs.
- PDF artifacts pass smoke checks for metadata and links.
- Performance regression budget established and enforced.

## Wave 4: Accessibility, Diagnostics, Extensibility (Parts 14, 24, 25, 22 future)
Target: 2 sprints

### Scope
- Accessibility model (semantic order, alt text, tagged output roadmap).
- Visual inspector and debug overlays.
- Plugin extension points for visual elements and render operations.
- Capture animation roadmap as non-goal/future chapter.

### Deliverables
- Accessibility chapter + minimal implementation path
- Diagnostics tooling in viewer hosts
- Extension APIs with sample plugin implementation

### Acceptance Gates
- Accessibility metadata appears in model and export path.
- Inspector can identify selected element id, bounds, and source metadata.
- Extension samples compile and run in test hosts.

## Cross-Cutting Quality Gates
- Dual-mode matrix tests (Legacy + Visual) for all relevant suites.
- No regressions in existing sample reports and viewer exports.
- XML docs on all new public APIs.
- SkiaSharp remains on 4.x.

## Immediate Backlog (Ready Next)
1. Add search match highlighting in Viewer.Blazor preview output.
2. Expand Blazor interaction regression tests (search/hit-test callbacks and click bridge).
3. Create MVC sample host integration (package exists; sample validation pending).
4. Add hyperlink/bookmark elements and passthrough metadata.
5. Add accessibility metadata baseline for component hosts.

## Suggested Work Breakdown (First 2 Weeks)
- Day 1-2: Contract design PR (serialization + renderer APIs).
- Day 3-5: Implement serialization + tests.
- Day 6-8: Implement hit-test index + tests.
- Day 9-10: Implement text search index + tests.

## Risk Register
- Risk: Model explosion from too many element types too early.
  - Mitigation: Introduce minimal types with metadata-first strategy.
- Risk: Renderer divergence between HTML and Skia.
  - Mitigation: Add parity harness and shared fixtures before feature depth.
- Risk: Viewer coupling to one backend behavior.
  - Mitigation: Keep viewer contracts backend-neutral and model-driven.

## Definition of Ready (Per Story)
- Chapter section exists with decisions and non-goals.
- Contract sketch reviewed by architecture owner.
- Unit test strategy defined before implementation.

## Definition of Done (Per Story)
- Code merged with tests and docs updated.
- Legacy and Visual mode tests pass.
- Diagnostics output updated if behavior changes.
- Component host scenario validated (Blazor and, where applicable, MVC) without requiring Viewer.Web endpoints.
