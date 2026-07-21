namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Layout;
using KineticReports.Plugins;

/// <summary>
/// Service for rendering reports to HTML preview.
/// </summary>
public interface IReportRenderService
{
    /// <summary>
    /// Executes a report and exports it as HTML.
    /// </summary>
    Task<string> RenderReportAsHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters = null!,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trace entries from the most recent render attempt.
    /// </summary>
    IReadOnlyList<string> GetLatestPluginTrace();
}

/// <summary>
/// Default implementation of <see cref="IReportRenderService"/>.
/// </summary>
public sealed class ReportRenderService : IReportRenderService
{
    private readonly IReportEngine _engine;
    private readonly IFontMetrics _fontMetrics;
    private readonly IHtmlExporter _exporter;
    private readonly IPluginService _pluginService;
    private readonly IPluginExecutionTraceStore _traceStore;

    /// <summary>
    /// Initializes a new <see cref="ReportRenderService"/>.
    /// </summary>
    public ReportRenderService(
        IReportEngine engine,
        IFontMetrics fontMetrics,
        IHtmlExporter exporter,
        IPluginService pluginService,
        IPluginExecutionTraceStore traceStore)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _fontMetrics = fontMetrics ?? throw new ArgumentNullException(nameof(fontMetrics));
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
        _pluginService = pluginService ?? throw new ArgumentNullException(nameof(pluginService));
        _traceStore = traceStore ?? throw new ArgumentNullException(nameof(traceStore));
    }

    /// <summary>
    /// Executes the report and exports it as HTML.
    /// </summary>
    public async Task<string> RenderReportAsHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters = null!,
        CancellationToken cancellationToken = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));

        _traceStore.Clear();
        _traceStore.Add($"[Render] Starting report '{definition.Id}'");

        try
        {
            var reportParameters = parameters ?? new Dictionary<string, object?>();
            var context = new LayoutSizingContext(_fontMetrics);
            var layoutOptions = new LayoutOptions();

            var ReportLayout = await _engine.RunAsync(
                definition,
                reportParameters,
                context,
                layoutOptions,
                cancellationToken).ConfigureAwait(false);

            _traceStore.Add($"[Render] Engine produced {ReportLayout.PageCount} page(s)");

            using var stream = new MemoryStream();
            await _exporter.ExportAsync(ReportLayout, stream, cancellationToken).ConfigureAwait(false);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            _traceStore.Add($"[Html] Exported HTML length: {html.Length}");

            // Deterministic plugin seam: post-process exported HTML in explicit order.
            var postProcessors = _pluginService.LoadedPlugins
                .OfType<IHtmlReportPostProcessorPlugin>()
                .OrderBy(p => p.Order)
                .ThenBy(p => p.Id, StringComparer.Ordinal);

            foreach (var postProcessor in postProcessors)
            {
                var before = html.Length;
                html = await postProcessor.ProcessHtmlAsync(html, cancellationToken).ConfigureAwait(false);
                _traceStore.Add($"[Html] {postProcessor.Id} (Order {postProcessor.Order}) transformed {before} -> {html.Length} chars");
            }

            _traceStore.Add("[Render] Completed successfully");

            return html;
        }
        catch (Exception ex)
        {
            _traceStore.Add($"[Render] Failed: {ex.GetType().Name} - {ex.Message}");
            return $@"
                <div class='alert alert-danger' role='alert'>
                    <h4 class='alert-heading'>Report Rendering Error</h4>
                    <p><strong>Error:</strong> {System.Net.WebUtility.HtmlEncode(ex.Message)}</p>
                    <details>
                        <summary>Stack trace</summary>
                        <pre style='font-size: 0.875rem; overflow-x: auto;'>{System.Net.WebUtility.HtmlEncode(ex.StackTrace)}</pre>
                    </details>
                </div>";
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetLatestPluginTrace() => _traceStore.Entries;
}
