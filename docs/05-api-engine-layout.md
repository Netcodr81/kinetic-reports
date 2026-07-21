# 05 - Engine and Layout API Reference

This page documents how orchestration and layout contracts work.

## Engine Interfaces

| Interface                | Purpose                                        | Key Methods                       |
| ------------------------ | ---------------------------------------------- | --------------------------------- |
| `IReportEngine`        | Main orchestration entry point                 | `RunAsync(...)`                 |
| `IDataResolver`        | Resolves rows for each`DataSourceDefinition` | `ResolveAsync(...)`             |
| `IExpressionEvaluator` | Evaluates expression strings                   | `Evaluate(expression, context)` |
| `IReportBuilder`       | Builds ordered report blocks from resolved data | `Build(dataContext, evaluator)` |

## Engine Runtime Types

| Type                  | Purpose                                          |
| --------------------- | ------------------------------------------------ |
| `ReportEngine`      | Default`IReportEngine` implementation          |
| `DataContext`       | Stores resolved rows keyed by data-source id     |
| `ExpressionContext` | Evaluation context with current row + parameters |
| `LiteralEvaluator`  | Default evaluator behavior                       |

## Layout Interfaces and Types

| Type                    | Kind      | Purpose                          | Key Members                                    |
| ----------------------- | --------- | -------------------------------- | ---------------------------------------------- |
| `ILayoutEngine`       | interface | Turns blocks into`ReportLayout` | `Layout(...)`                                |
| `LayoutEngine`        | class     | Default implementation           | delegates to pagination engine                 |
| `LayoutOptions`       | record    | Page setup and margins           | `PageWidth`, `PageHeight`, `PageMargins` |
| `LayoutSizingContext` | class     | Default LayoutSizing context     | `TextLayout`, image-size resolver           |
| `ReportLayout`        | record    | Final immutable output           | `Pages`                                      |

## Pagination Behavior

- Page headers/footers are partitioned by `BandKind`.
- Body blocks are measured then distributed by available body area.
- `ForcePageBreakBefore` and `KeepTogether` affect page splits.

## Pipeline Diagram

```mermaid
flowchart LR
	A[IReportEngine.RunAsync] --> B[Resolve data sources]
	B --> C[Build logical blocks]
	C --> D[ILayoutEngine.Layout]
	D --> E[LayoutSizing]
	E --> F[Arrange]
	F --> G[Pagination]
	G --> H[ReportLayout]
```

## Junior Tips

- Empty page usually means empty blocks from `IReportBuilder`.
- Data exists but page is empty usually means resolver/tree-builder mismatch on data-source IDs.
