# KineticReports Documentation (Junior Developer Edition)

Welcome. This documentation explains **how KineticReports works end-to-end** and how to extend it safely.

## Start Here

1. [01 - Quick Start for Junior Developers](./01-junior-quick-start.md)
2. [02 - Report Generation Flow](./02-report-generation-flow.md)
3. [03 - Architecture and Project Map](./03-architecture-project-map.md)

## API Reference

4. [04 - Core API Reference (Definition, Layout, Styling, Typography)](./04-api-core-and-definition.md)
5. [05 - Engine and Layout API Reference](./05-api-engine-layout.md)
6. [06 - Rendering, Export, Plugins, and Viewer API Reference](./06-api-rendering-export-viewer.md)

## Extension Guides

7. [07 - Extension Points Cheat Sheet](./07-extension-points.md)
8. [08 - Plugin Development Guide (Tutorial + Reference)](./08-plugin-development-guide.md)
9. [09 - Exporter Development Guide (Tutorial + Reference)](./09-exporter-development-guide.md)
10. [10 - Data Provider Development Guide (Tutorial + Reference)](./10-data-provider-development-guide.md)

## Operations

11. [11 - Troubleshooting and Debugging](./11-troubleshooting-and-debugging.md)
12. [12 - Glossary](./12-glossary.md)

---

## End-to-End Mental Model

```mermaid
flowchart LR
	A[ReportDefinition] --> B[Data Resolution]
	B --> C[Expression Evaluation]
	C --> D[Report Bands Build]
	D --> E[Measure]
	E --> F[Arrange]
	F --> G[Pagination]
	G --> H[ReportLayout]
	H --> I[Renderer or Exporter]
```

**Rule of thumb:**
- Engine builds the report content.
- Layout engine computes placement.
- Renderers/exporters only consume final `ReportLayout`.
