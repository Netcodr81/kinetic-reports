# 12 - Glossary

| Term | Meaning |
|---|---|
| `ReportDefinition` | Source definition of a report before execution |
| `DataContext` | Resolved data rows for all data sources in one run |
| Report Bands | Ordered report content (`BandElement` list) before physical layout |
| LayoutSizing | Stage that computes desired sizes |
| Arrange | Stage that assigns final bounds |
| Pagination | Stage that splits arranged content across pages |
| `ReportLayout` | Immutable final page model consumed by output components |
| DIP | Device Independent Pixel, `1/96` inch |
| Exporter | Converts `ReportLayout` to a format (HTML/XLSX/etc.) |
| Renderer | Draws `ReportLayout` using graphics context (PDF/PNG/etc.) |
| `ResolvedStyle` | Final style object after cascade/inheritance/overrides |
| Plugin | Optional module loaded by plugin manager |
| `IDataResolver` | Resolves report data sources to row sets |
| `IDataProvider` | Provider abstraction for backend query execution |
| `IFontMetrics` | Shared font measurement + shaping service |
