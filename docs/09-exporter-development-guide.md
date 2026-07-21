# 09 - Exporter Development Guide (Tutorial + Reference)

## Tutorial: Build a New Exporter

Goal: Convert `ReportLayout` to a new output format.

## Step 1: Create exporter project

- New class library in `src/Export/`
- Reference `KineticReports.Core` (and optionally rendering helpers)

## Step 2: Define exporter contract

Implement `IReportLayoutExporter` with:

- `Format` (an `ExportFormatDescriptor`)
- `ExportAsync(ReportLayout reportLayout, CancellationToken ct)` returning bytes

## Step 3: Traverse the report layout

```mermaid
flowchart TB
	A[ReportLayout] --> B[Pages]
	B --> C[Header]
	B --> D[Body Children]
	B --> E[Footer]
	D --> F[Element Dispatcher]
```

Dispatch on element types (`TextElement`, `ImageElement`, `TableElement`, etc.).

## Step 4: Respect architecture rules

- No layout in exporter.
- Consume element bounds as-is.
- Keep unit consistency (DIP -> output unit mapping).

## Step 5: Register in DI

Register your exporter in host app startup so `IReportLayoutExporterRegistry` can resolve it.

Example:

- Register concrete exporter implementation as `IReportLayoutExporter`.
- Ensure the host registers `IReportLayoutExporterRegistry`.

## Reference: Exporter Responsibilities

| Concern | Exporter Must Do | Exporter Must Not Do |
|---|---|---|
| Geometry | Use `Bounds` from report layout | Recalculate bounds |
| Text | Use text runs/style where available | Reflow paragraphs independently |
| Pagination | Iterate pages in order | Repaginate content |
| Styling | Map `AppliedStyle` safely | Invent unrelated style cascade |

## Suggested Exporter API Table

| Type | Purpose |
|---|---|
| `IYourExporter` | Output format contract |
| `YourExporter` | Concrete implementation |
| `YourStyleMapper` | Map `AppliedStyle` to format-specific style |
| `YourElementWriter` | Per-element serialization helper |

## Validation Checklist

- Works for single and multiple pages.
- Supports at least text and basic shapes.
- Handles missing/optional element fields gracefully.
- Produces deterministic output for same input.
