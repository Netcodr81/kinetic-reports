# Rendering Pipeline

Status: Draft

## Scope
Defines transformation stages from report definition through visual rendering.

## Decisions
- Canonical pipeline:
  ReportDefinition -> Engine -> ReportDocument -> VisualDocumentBuilder -> VisualDocument -> Renderer -> Output
- Builder is deterministic and side-effect free.

## Open Questions
- Where to persist VisualDocument snapshots for diagnostics.

## Non-goals
- Mixing layout logic into renderers.

## Implementation Notes
- Add tracing at stage boundaries.
- Add parity metrics between legacy and visual paths.
