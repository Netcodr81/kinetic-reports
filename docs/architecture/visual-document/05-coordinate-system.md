# Coordinate System

Status: Draft

## Scope
Defines coordinate spaces and clipping behavior for visual nodes.

## Decisions
- Use DIPs where 1 DIP = 1/96 inch.
- Bounds are page-local unless explicitly transformed.
- Clipping is explicit metadata on visual nodes.

## Open Questions
- Rotation and transform composition semantics in phase 1.

## Non-goals
- Backend-specific coordinate units in core model.

## Implementation Notes
- Builder normalizes invalid bounds and emits diagnostics.
