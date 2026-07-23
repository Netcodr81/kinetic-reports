namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Engine;
using KineticReports.Core.Export.Document;
using KineticReports.Core.Layout;
using KineticReports.Core.LayoutEngine;
using KineticReports.Core.Typography;
using Microsoft.Extensions.Logging;

/// <summary>
/// Concrete implementation that orchestrates data resolution, expression evaluation, and layout.
/// </summary>
public sealed class ReportExecutor : IReportExecutor
{
    private readonly IReportEngine _engine;
    private readonly ITextLayout _textLayout;
    private readonly ILogger<ReportExecutor> _logger;

    /// <summary>
    /// Initializes the executor with the report engine and text layout service.
    /// </summary>
    public ReportExecutor(
        IReportEngine engine,
        ITextLayout textLayout,
        ILogger<ReportExecutor> logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _textLayout = textLayout ?? throw new ArgumentNullException(nameof(textLayout));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the report using the injected engine.
    /// </summary>
    public async Task<ReportDocument> ExecuteAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        var context = new LayoutSizingContext(_textLayout);
        var layoutOptions = new LayoutOptions();

        _logger.LogInformation("Viewer.Web pipeline mode selected: Visual");
        _logger.LogInformation("Visual pipeline mode requested; report execution remains semantic and exporters apply visual rendering path where supported.");

        return await _engine.RunAsync(definition, parameters, context, layoutOptions, ct);
    }
}
