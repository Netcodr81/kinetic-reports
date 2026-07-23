# PDF Backend Mapping

Status: Draft

## Scope
Defines mapping from VisualDocument nodes to PDF via Skia backend.

## Decisions
- Reuse existing Skia drawing primitives where possible.
- Keep SkiaSharp dependency at 4.x.

## Open Questions
- Font embedding policy and deterministic metadata normalization.

## Non-goals
- Removing existing ReportDocument-based Skia path in phase 1.

## Implementation Notes
- Validate multipage ordering and clipping parity with HTML path.
