namespace KineticReports.Core.Engine;

using KineticReports.Core.Definition;
using KineticReports.Core.Engine.Building;
using KineticReports.Core.Engine.Data;
using KineticReports.Core.Engine.Expressions;
using KineticReports.Core.Layout;
using KineticReports.Core.LayoutEngine;
using KineticReports.Core.Typography;

/// <summary>
/// Default implementation of <see cref="IReportEngine"/>.
/// Runs the following pipeline in order:
/// <list type="number">
///   <item><description>Resolve all data sources via <see cref="IDataResolver"/>.</description></item>
///   <item><description>Build report blocks via <see cref="IReportBuilder"/>.</description></item>
///   <item><description>Run the layout pipeline via <see cref="ILayoutEngine"/>.</description></item>
/// </list>
/// </summary>
public sealed class ReportEngine : IReportEngine
{
    private readonly IDataResolver _dataResolver;
    private readonly IExpressionEvaluator _expressionEvaluator;
    private readonly IReportBuilder _blocksBuilder;
    private readonly ILayoutEngine _layoutEngine;
    private readonly ITextLayout? _textLayout;

    /// <summary>
    /// Initialises a new <see cref="ReportEngine"/> with all required collaborators.
    /// </summary>
    /// <param name="dataResolver">Resolves data sources into typed row collections.</param>
    /// <param name="expressionEvaluator">Evaluates field-reference expressions against data rows.</param>
    /// <param name="blocksBuilder">Builds the ordered report blocks from the definition and data.</param>
    /// <param name="layoutEngine">Runs LayoutSizing, Arrange, and Pagination passes.</param>
    /// <param name="textLayout">
    /// Optional text layout service used by the convenience <c>RunAsync(ReportDefinition, CancellationToken)</c>
    /// overload to create a default <see cref="LayoutSizingContext"/>.
    /// </param>
    public ReportEngine(
        IDataResolver dataResolver,
        IExpressionEvaluator expressionEvaluator,
        IReportBuilder blocksBuilder,
        ILayoutEngine layoutEngine,
        ITextLayout? textLayout = null)
    {
        _dataResolver = dataResolver;
        _expressionEvaluator = expressionEvaluator;
        _blocksBuilder = blocksBuilder;
        _layoutEngine = layoutEngine;
        _textLayout = textLayout;
    }

    /// <inheritdoc/>
    public Task<ReportDocument> RunAsync(
        ReportDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_textLayout is null)
        {
            throw new InvalidOperationException(
                "No ITextLayout service is available for default report execution. " +
                "Register an ITextLayout implementation in DI or call the RunAsync overload " +
                "that accepts parameters and ILayoutSizingContext.");
        }

        return RunAsync(
            definition,
            parameters: new Dictionary<string, object?>(),
            layoutSizingContext: new LayoutSizingContext(_textLayout),
            layoutOptions: null,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ReportDocument> RunAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        ILayoutSizingContext layoutSizingContext,
        LayoutOptions? layoutOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(layoutSizingContext);

        var options = layoutOptions ?? new LayoutOptions();

        // 1. Resolve all declared data sources.
        var resolvedSources = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>>(
            definition.DataSources.Count);

        foreach (var ds in definition.DataSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rows = await _dataResolver
                .ResolveAsync(ds, parameters, cancellationToken)
                .ConfigureAwait(false);
            resolvedSources[ds.Id] = rows;
        }

        var dataContext = new DataContext
        {
            Definition = definition,
            DataSources = resolvedSources
        };

        // 2. Build report blocks (data binding + expression evaluation).
        var blocks = _blocksBuilder.Build(dataContext, _expressionEvaluator);

        // 3. Run the layout pipeline: LayoutSizing → Arrange → Pagination.
        var reportDocument = _layoutEngine.Layout(blocks, options, layoutSizingContext);

        return new ReportDocument
        {
            Pages = reportDocument.Pages,
            Metadata = new ReportDocumentMetadata
            {
                ReportId = definition.Id,
                ReportName = definition.Name,
                FileName = definition.Name,
                Properties = definition.Metadata.ToDictionary(
                    pair => pair.Key,
                    pair => (object?)pair.Value,
                    StringComparer.Ordinal)
            }
        };
    }
}
