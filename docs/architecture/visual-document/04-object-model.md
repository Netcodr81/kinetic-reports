# Object Model

Status: Draft

## Scope
Defines VisualDocument, VisualPage, VisualLayer, and VisualElement hierarchy.

## Decisions
- Use immutable record types for visual nodes.
- Include metadata dictionaries for diagnostics and source mapping.

## Open Questions
- Required metadata keys for viewer and exporter use cases.

## Non-goals
- Backend-specific payloads in core model.

## Implementation Notes
- Phase 1 element set: container, text, image, shape, table placeholder.
