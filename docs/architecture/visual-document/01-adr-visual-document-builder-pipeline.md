# ADR-001: Insert VisualDocumentBuilder Between ReportDocument and Renderers

Status: Proposed
Date: 2026-07-23
Deciders: Core, Rendering, Viewer maintainers

## Context
KineticReports currently renders from ReportDocument directly into concrete output formats.
This works, but it couples renderer behavior to layout block semantics and limits viewer-oriented features such as hit testing, selection, and search indexing.

## Decision
Introduce a dedicated translation layer:

ReportDefinition -> Layout Engine -> ReportDocument -> VisualDocumentBuilder -> VisualDocument -> Renderer

Key ownership boundaries:
- Layout owns semantic report composition and pagination.
- VisualDocumentBuilder owns semantic-to-visual translation.
- Renderers own drawing and backend output only.

## Consequences
Positive:
- Decouples layout evolution from renderer evolution.
- Enables richer viewer capabilities without modifying layout contracts.
- Supports multi-backend rendering from one visual scene graph.

Negative:
- Adds a new model and translation step to maintain.
- Requires parity checks during migration.

## Compatibility and Migration
- Legacy render path remains available behind configuration.
- Visual mode is introduced behind a feature flag.
- Cutover requires parity gates for HTML and PDF on protected samples.

## Non-goals (Phase 1)
- No immediate removal of legacy exporters/renderers.
- No full redesign of layout engine.
- No viewer interaction feature guarantees beyond baseline rendering parity.

## Rollback Strategy
- Feature flag can switch back to legacy path at runtime/config level.
- Keep legacy renderer code unchanged until Visual mode is proven stable.

## Open Questions
- Final home for IVisualDocumentBuilder interface (Engine vs Rendering shared layer).
- Whether table/chart should map to generic draw primitives or typed visual placeholders in phase 1.
