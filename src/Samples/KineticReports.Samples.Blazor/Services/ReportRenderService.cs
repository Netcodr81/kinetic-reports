namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Geometry;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Layout;
using KineticReports.Plugins;

/// <summary>
/// Service for rendering reports to HTML preview.
/// </summary>
public interface IReportRenderService
{
    /// <summary>
    /// Gets currently available export formats, including plugin-contributed formats.
    /// </summary>
    IReadOnlyList<ExportFormatDescriptor> GetAvailableExportFormats();

    /// <summary>
    /// Executes a report and exports it as HTML.
    /// </summary>
    Task<string> RenderReportAsHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a report and exports it using format registry and negotiation seams.
    /// </summary>
    Task<ReportExportResult> ExportReportAsync(
        ReportDefinition definition,
        string requestedFormat,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trace entries from the most recent render attempt.
    /// </summary>
    IReadOnlyList<string> GetLatestPluginTrace();
}

/// <summary>
/// Represents a completed export artifact.
/// </summary>
public sealed record ReportExportResult(
    string FinalFormatId,
    string MimeType,
    string FileExtension,
    byte[] Content);

/// <summary>
/// Default implementation of <see cref="IReportRenderService"/>.
/// </summary>
public sealed class ReportRenderService : IReportRenderService
{
    private readonly IReportEngine _engine;
    private readonly ITextLayout _textLayout;
    private readonly IReportDocumentExporterRegistry _exporterRegistry;
    private readonly IPluginService _pluginService;
    private readonly IPluginExecutionTraceStore _traceStore;

    /// <summary>
    /// Initializes a new <see cref="ReportRenderService"/>.
    /// </summary>
    public ReportRenderService(
        IReportEngine engine,
        ITextLayout textLayout,
        IReportDocumentExporterRegistry exporterRegistry,
        IPluginService pluginService,
        IPluginExecutionTraceStore traceStore)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _textLayout = textLayout ?? throw new ArgumentNullException(nameof(textLayout));
        _exporterRegistry = exporterRegistry ?? throw new ArgumentNullException(nameof(exporterRegistry));
        _pluginService = pluginService ?? throw new ArgumentNullException(nameof(pluginService));
        _traceStore = traceStore ?? throw new ArgumentNullException(nameof(traceStore));
    }

    /// <summary>
    /// Executes the report and exports it as HTML.
    /// </summary>
    public async Task<string> RenderReportAsHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var exportResult = await ExportReportAsync(
            definition,
            "html",
            parameters,
            cancellationToken).ConfigureAwait(false);

        return System.Text.Encoding.UTF8.GetString(exportResult.Content);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExportFormatDescriptor> GetAvailableExportFormats()
    {
        return ExportSeamPipeline.BuildRegistry(
            _exporterRegistry.GetAvailableFormats(),
            _pluginService.LoadedPlugins);
    }

    /// <inheritdoc/>
    public async Task<ReportExportResult> ExportReportAsync(
        ReportDefinition definition,
        string requestedFormat,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (string.IsNullOrWhiteSpace(requestedFormat))
        {
            throw new ArgumentException("Requested format cannot be null or empty.", nameof(requestedFormat));
        }

        _traceStore.Clear();
        _traceStore.Add($"[Render] Starting report '{definition.Id}'");

        try
        {
            var reportParameters = parameters ?? new Dictionary<string, object?>();
            var availableFormats = GetAvailableExportFormats();

            _traceStore.Add($"[Export] Registry contains: {string.Join(", ", availableFormats.Select(f => f.FormatId))}");

            var finalFormat = ExportSeamPipeline.Negotiate(
                requestedFormat,
                availableFormats,
                _pluginService.LoadedPlugins);

            _traceStore.Add($"[Export] Negotiated format: '{requestedFormat}' -> '{finalFormat}'");

            var selectedDescriptor = availableFormats.FirstOrDefault(
                f => string.Equals(f.FormatId, finalFormat, StringComparison.OrdinalIgnoreCase));

            if (selectedDescriptor is null)
            {
                throw new InvalidOperationException(
                    $"Requested export format '{finalFormat}' is not available.");
            }

            var context = new LayoutSizingContext(_textLayout);
            var layoutOptions = ResolveLayoutOptions(definition);

            var ReportDocument = await _engine.RunAsync(
                definition,
                reportParameters,
                context,
                layoutOptions,
                cancellationToken).ConfigureAwait(false);

            _traceStore.Add($"[Render] Engine produced {ReportDocument.PageCount} page(s)");

            if (!_exporterRegistry.TryGetExporter(selectedDescriptor.FormatId, out var exporter) || exporter is null)
            {
                throw new NotSupportedException(
                    $"No exporter registered for format '{selectedDescriptor.FormatId}'.");
            }

            var artifact = await exporter.ExportAsync(ReportDocument, cancellationToken).ConfigureAwait(false);

            if (string.Equals(selectedDescriptor.MimeType, "text/html", StringComparison.OrdinalIgnoreCase))
            {
                var html = System.Text.Encoding.UTF8.GetString(artifact);
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

                artifact = System.Text.Encoding.UTF8.GetBytes(html);
            }

            var preArtifactLength = artifact.Length;
            artifact = await ExportSeamPipeline.PostProcessArtifactAsync(
                selectedDescriptor.FormatId,
                artifact,
                _pluginService.LoadedPlugins,
                cancellationToken).ConfigureAwait(false);

            _traceStore.Add($"[Artifact] Post-process transformed {preArtifactLength} -> {artifact.Length} bytes");

            _traceStore.Add("[Render] Completed successfully");

            return new ReportExportResult(
                selectedDescriptor.FormatId,
                selectedDescriptor.MimeType,
                selectedDescriptor.FileExtension,
                artifact);
        }
        catch (Exception ex)
        {
            _traceStore.Add($"[Render] Failed: {ex.GetType().Name} - {ex.Message}");
            var errorHtml = $@"
                <div class='alert alert-danger' role='alert'>
                    <h4 class='alert-heading'>Report Rendering Error</h4>
                    <p><strong>Error:</strong> {System.Net.WebUtility.HtmlEncode(ex.Message)}</p>
                    <details>
                        <summary>Stack trace</summary>
                        <pre style='font-size: 0.875rem; overflow-x: auto;'>{System.Net.WebUtility.HtmlEncode(ex.StackTrace)}</pre>
                    </details>
                </div>";

            return new ReportExportResult(
                "html",
                "text/html",
                "html",
                System.Text.Encoding.UTF8.GetBytes(errorHtml));
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetLatestPluginTrace() => _traceStore.Entries;

    private static LayoutOptions ResolveLayoutOptions(ReportDefinition definition)
    {
        var metadata = definition.Metadata;
        if (metadata.TryGetValue("authoring.compiledComponents", out var compiledComponents)
            && compiledComponents is not null)
        {
            // Authored placements use full-page coordinates, so margins must be zero.
            return new LayoutOptions { PageMargins = new Thickness(0f) };
        }

        return new LayoutOptions();
    }
}
