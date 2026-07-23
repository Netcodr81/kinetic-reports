# Overview

Status: Draft

## Scope
Defines objectives, constraints, and high-level architecture for the VisualDocument subsystem.

## Decisions
- VisualDocument is a renderer-agnostic scene graph in DIPs.
- ReportDocument remains the semantic output of layout and pagination.
- Translation occurs only in VisualDocumentBuilder.

## Open Questions
- Required minimum primitive set for phase 1.
- Cross-backend handling for unsupported features.

## Non-goals
- Replacing ReportDocument.
- Introducing backend-specific model types into the visual core.

## Implementation Notes
- Keep immutable records for visual model types.
- Preserve source ids for diagnostics and hit testing.
