namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Layout;

/// <summary>
/// Concrete implementation that orchestrates data resolution, expression evaluation, and layout.
/// </summary>
public sealed class ReportExecutor : IReportExecutor
{
    private readonly IReportEngine _engine;
    private readonly ITextLayout _textLayout;

    /// <summary>
    /// Initializes the executor with the report engine and text layout service.
    /// </summary>
    public ReportExecutor(IReportEngine engine, ITextLayout textLayout)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _textLayout = textLayout ?? throw new ArgumentNullException(nameof(textLayout));
    }

    /// <summary>
    /// Executes the report using the injected engine.
    /// </summary>
    public async Task<ReportLayout> ExecuteAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        var context = new LayoutSizingContext(_textLayout);
        var layoutOptions = new LayoutOptions();

        return await _engine.RunAsync(definition, parameters, context, layoutOptions, ct);
    }
}
