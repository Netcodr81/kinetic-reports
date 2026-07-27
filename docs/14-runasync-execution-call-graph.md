# 14 - RunAsync Execution Call Graph

This document expands the runtime path that starts from any host application call:

```csharp
var reportDocument = await reportEngine.RunAsync(reportDefinition);
```

## Concrete Runtime Chain

```mermaid
sequenceDiagram
    participant Host as Host
    participant Engine as ReportEngine IReportEngine

    Host->>Engine: 1 RunAsync(reportDefinition ReportDefinition)
    Engine->>Engine: 2 RunAsync(definition ReportDefinition, cancellationToken CancellationToken)
    Engine->>Engine: 3 RunAsync(definition ReportDefinition, parameters ParameterMap, layoutSizingContext ILayoutSizingContext, layoutOptions LayoutOptions, cancellationToken CancellationToken)
```

### Data Resolution Stage

```mermaid
sequenceDiagram
    participant Engine as ReportEngine
    participant Resolver as CompositeDataResolver IDataResolver
    participant SourceResolver as ProviderDataSourceResolver IDataSourceResolver
    participant Provider as IDataProvider

    loop 4 Each DataSourceDefinition
        Engine->>Resolver: 5 ResolveAsync(definition DataSourceDefinition, parameters ParameterMap, cancellationToken CancellationToken)
        Resolver->>SourceResolver: 6 CanResolve(definition DataSourceDefinition)
        Resolver->>SourceResolver: 7 ResolveAsync(definition DataSourceDefinition, parameters ParameterMap, cancellationToken CancellationToken)
        SourceResolver->>SourceResolver: 8 FindProvider(providerType string)
        SourceResolver->>SourceResolver: 9 ResolveQueryText(definition DataSourceDefinition)
        SourceResolver->>Provider: 10 ExecuteAsync(request QueryRequest, cancellationToken CancellationToken)
        Provider-->>SourceResolver: 11 QueryResult
        SourceResolver->>SourceResolver: 12 ConvertRows(queryResult QueryResult)
        SourceResolver-->>Resolver: 13 IReadOnlyList row maps
        Resolver-->>Engine: 14 IReadOnlyList row maps
    end
```

### Block Building and Expression Stage

```mermaid
sequenceDiagram
    participant Engine as ReportEngine
    participant Builder as DefaultReportBuilder IReportBuilder
    participant Eval as DefaultExpressionEvaluator IExpressionEvaluator

    Engine->>Builder: 15 Build(dataContext DataContext, evaluator IExpressionEvaluator)
    Builder->>Builder: 16 TryCreateDefinitionBackedBuilder(definition ReportDefinition)
    Builder->>Builder: 17 ConfigureFromLayoutDefinition(layout ReportLayoutDefinition, styles StyleDefinitionList)
    Builder->>Builder: 18 AppendLayoutItem(item ReportLayoutItemDefinition, blockType BlockType)
    Builder->>Builder: 19 GetRows(dataSourceId string)
    Builder->>Eval: 20 Evaluate(expression string, context ExpressionContext)
    Eval->>Eval: 21 FunctionExpressionRuntime Evaluate or ResolveToken
    Builder->>Builder: 22 ApplyPostProcessors(blocks ReportBlockList)
    Builder-->>Engine: 23 IReadOnlyList ReportBlock
```

### Layout and Return Stage

```mermaid
sequenceDiagram
    participant Engine as ReportEngine
    participant Layout as LayoutEngine ILayoutEngine
    participant Pager as PaginationEngine
    participant Host as Host

    Engine->>Layout: 24 Layout(blocks ReportBlockList, options LayoutOptions, context ILayoutSizingContext)
    Layout->>Pager: 25 Paginate(blocks ReportBlockList, options LayoutOptions, context ILayoutSizingContext)
    Pager->>Pager: 26 MeasureBlocksVertical and LayoutSize
    Pager->>Pager: 27 BuildPage(pageNumber int, bodyBlocks BlockList, pageHeaderBlocks BlockList, pageFooterBlocks BlockList, options LayoutOptions)
    Pager->>Pager: 28 Arrange(bounds Rect)
    Pager-->>Layout: 29 IReadOnlyList PageBlock
    Layout-->>Engine: 30 ReportDocument
    Engine-->>Host: 31 ReportDocument
```

Type aliases used in the diagrams:
- ParameterMap: string key to object value map
- ReportBlockList: ordered report block collection
- BlockList: ordered layout block collection
- StyleDefinitionList: list of style definitions

## Step by Step Description

1. Caller invokes report generation from the host application by calling the engine.
2. ReportEngine receives the call through IReportEngine and switches to the full overload.
3. CompositeDataResolver iterates each data source in the definition.
4. ProviderDataSourceResolver selects a provider by ProviderType and resolves query text.
5. IDataProvider executes the query request and returns schema and row values.
6. ProviderDataSourceResolver converts provider output into row dictionaries.
7. DefaultReportBuilder builds report blocks from definition layout and resolved data.
8. Builder reads rows and evaluates expressions per row for text, grouping, and aggregates.
9. DefaultExpressionEvaluator resolves function expressions and token expressions.
10. Builder applies post processors to finalize the ordered block list.
11. LayoutEngine starts layout and delegates to PaginationEngine.
12. PaginationEngine measures blocks, computes available body height, and creates pages.
13. PaginationEngine arranges header, body, and footer blocks into final page bounds.
14. The engine returns an immutable ReportDocument to the caller.

## Interface to Implementation Map

| Interface                | Runtime implementation                                 | Primary methods called                                                                                       |
| ------------------------ | ------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------ |
| `IReportEngine`        | `ReportEngine`                                       | `RunAsync(definition: ReportDefinition, cancellationToken: CancellationToken)` -> `RunAsync(definition: ReportDefinition, parameters: IReadOnlyDictionary<string, object?>, layoutSizingContext: ILayoutSizingContext, layoutOptions: LayoutOptions?, cancellationToken: CancellationToken)` |
| `IDataResolver`        | `CompositeDataResolver`                              | `ResolveAsync(definition: DataSourceDefinition, parameters: IReadOnlyDictionary<string, object?>, cancellationToken: CancellationToken)` |
| `IDataSourceResolver`  | `ProviderDataSourceResolver`                         | `CanResolve(definition: DataSourceDefinition)`, `ResolveAsync(definition: DataSourceDefinition, parameters: IReadOnlyDictionary<string, object?>, cancellationToken: CancellationToken)` |
| `IDataProvider`        | provider-specific implementation selected by provider type | `ExecuteAsync(request: QueryRequest, cancellationToken: CancellationToken)` |
| `IReportBuilder`       | `DefaultReportBuilder`                               | `Build(dataContext: DataContext, evaluator: IExpressionEvaluator)`, `ConfigureFromLayoutDefinition(builder: DefaultReportBuilder, layout: ReportLayoutDefinition, styles: IReadOnlyList<StyleDefinition>)`, `ApplyPostProcessors(blocks: IReadOnlyList<ReportBlock>)` |
| `IExpressionEvaluator` | `DefaultExpressionEvaluator`                         | `Evaluate(expression: string, context: ExpressionContext)` |
| `ILayoutEngine`        | `LayoutEngine`                                       | `Layout(blocks: IReadOnlyList<ReportBlock>, options: LayoutOptions, context: ILayoutSizingContext)` |

## Key Source References

- [IReportEngine](../src/Core/KineticReports.Core/Engine/IReportEngine.cs)
- [ReportEngine](../src/Core/KineticReports.Core/Engine/ReportEngine.cs)
- [CompositeDataResolver](../src/Core/KineticReports.Core/Engine/Data/CompositeDataResolver.cs)
- [ProviderDataSourceResolver](../src/Core/KineticReports.Core/Engine/Data/ProviderDataSourceResolver.cs)
- [DefaultReportBuilder](../src/Core/KineticReports.Core/Engine/Building/DefaultReportBuilder.cs)
- [DefaultExpressionEvaluator](../src/Core/KineticReports.Core/Engine/Expressions/DefaultExpressionEvaluator.cs)
- [LayoutEngine](../src/Core/KineticReports.Core/LayoutEngine/LayoutEngine.cs)
- [PaginationEngine](../src/Core/KineticReports.Core/LayoutEngine/Pagination/PaginationEngine.cs)
