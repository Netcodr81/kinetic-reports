# Renderer API

Status: Draft

## Scope
Defines renderer contracts for consuming VisualDocument.

## Decisions
- Visual renderers consume VisualDocument only.
- Renderer implementations remain backend-specific.
- Introduce `IVisualRenderer` as the baseline visual rendering contract.
- Use adapter shims to bridge backend-specific exporters/renderers into `IVisualRenderer`.

## Open Questions
- Future expansion beyond single-pass rendering (`IRenderSurface`, `IRenderContext`, `IRenderPass`).

## Non-goals
- Removing existing renderer contracts in phase 1.

## Implementation Notes
- Implemented baseline contract in `KineticReports.Visual`:
	- `IVisualRenderer`
	- `VisualRenderFormatDescriptor`
- Implemented adapter shims:
	- `VisualHtmlRendererAdapter` (HTML backend)
	- `VisualPdfRendererAdapter` (Skia PDF backend)
- Hosts register adapters in DI to support format-driven visual rendering discovery.
