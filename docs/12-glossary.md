# 12 - Glossary

| Term | Meaning |
|---|---|
| `ReportDefinition` | Source definition of a report before execution |
| `DataContext` | Resolved data rows for all data sources in one run |
| Report Blocks | Ordered report content (`ReportBlock` list) before physical layout |
| LayoutSizing | Stage that computes desired sizes |
| Arrange | Stage that assigns final bounds |
| Pagination | Stage that splits arranged content across pages |
| `ReportDocument` | Immutable final page model consumed by output components |
| DIP | Device Independent Pixel, `1/96` inch |
| Exporter | Converts `ReportDocument` to a format (HTML/XLSX/etc.) |
| Renderer | Draws `ReportDocument` using graphics context (PDF/PNG/etc.) |
| `AppliedStyle` | Final style object after cascade/inheritance/overrides |
| Plugin | Optional module loaded by plugin manager |
| `IDataResolver` | Resolves report data sources to row sets |
| `IDataProvider` | Provider abstraction for backend query execution |
| `ITextLayout` | Shared text measurement + shaping service |
