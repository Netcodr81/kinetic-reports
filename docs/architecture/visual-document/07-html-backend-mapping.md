# HTML Backend Mapping

Status: Draft

## Scope
Defines mapping from VisualDocument nodes to semantic HTML output.

## Decisions
- Preserve report/page/region semantics in DOM.
- Emit model JSON payload for diagnostics.

## Open Questions
- Preferred strategy for complex primitives (table/chart) in phase 1 HTML.

## Non-goals
- Pixel-perfect browser-specific hacks in core mapping rules.

## Implementation Notes
- Keep CSS clipping and overflow behavior explicit at page boundaries.
