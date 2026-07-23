namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Layout;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Concrete implementation that orchestrates data resolution, expression evaluation, and layout.
/// </summary>
public sealed class ReportExecutor : IReportExecutor
{
    private readonly IReportEngine _engine;
    private readonly ITextLayout _textLayout;
    private readonly RenderingPipelineOptions _pipelineOptions;
    private readonly ILogger<ReportExecutor> _logger;

    /// <summary>
    /// Initializes the executor with the report engine and text layout service.
    /// </summary>
    public ReportExecutor(
        IReportEngine engine,
        ITextLayout textLayout,
        IOptions<RenderingPipelineOptions> pipelineOptions,
        ILogger<ReportExecutor> logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _textLayout = textLayout ?? throw new ArgumentNullException(nameof(textLayout));
        _pipelineOptions = pipelineOptions?.Value ?? throw new ArgumentNullException(nameof(pipelineOptions));
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
        var selectedPipeline = _pipelineOptions.UseVisualPipeline ? "Visual" : "Legacy";

        _logger.LogInformation("Viewer.Web pipeline mode selected: {PipelineMode}", selectedPipeline);
        if (_pipelineOptions.UseVisualPipeline)
        {
            _logger.LogInformation("Visual pipeline mode requested; report execution remains semantic and exporters apply visual rendering path where supported.");
        }

        return await _engine.RunAsync(definition, parameters, context, layoutOptions, ct);
    }
}
