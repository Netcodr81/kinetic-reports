namespace KineticReports.Viewer.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Geometry;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Layout;

/// <summary>
/// Blazor implementation of report service that executes and exports reports locally.
/// </summary>
public sealed class LocalReportService : IReportService
{
    private static readonly IReadOnlyList<ReportViewerExportFormat> DefaultFormats =
    [
        new ReportViewerExportFormat("html", "HTML", "text/html", "html")
    ];

    private readonly IReportEngine _engine;
    private readonly IHtmlExporter _exporter;
    private readonly ITextLayout _textLayout;
    private IReadOnlyList<string> _latestTrace = [];

    /// <summary>
    /// Initializes the service with required dependencies.
    /// </summary>
    public LocalReportService(
        IReportEngine engine,
        IHtmlExporter exporter,
        ITextLayout textLayout)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
        _textLayout = textLayout ?? throw new ArgumentNullException(nameof(textLayout));
    }

    /// <inheritdoc/>
    public IReadOnlyList<ReportViewerExportFormat> GetAvailableExportFormats() => DefaultFormats;

    /// <inheritdoc/>
    public async Task<string> RenderHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        try
        {
            _latestTrace = ["[Render] Executing with local viewer service", "[Export] Format: html"];
            return await RenderHtmlCoreAsync(definition, parameters, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _latestTrace = [$"[Render] Failed: {ex.GetType().Name} - {ex.Message}"];
            System.Diagnostics.Debug.WriteLine($"Report execution failed: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ReportViewerExportResult> ExportAsync(
        ReportDefinition definition,
        string formatId,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        if (string.IsNullOrWhiteSpace(formatId))
            throw new ArgumentException("Format id is required.", nameof(formatId));

        if (!string.Equals(formatId, "html", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Local viewer service supports only 'html' export. Requested '{formatId}'.");

        try
        {
            _latestTrace = ["[Render] Executing with local viewer service", "[Export] Format: html"];
            var html = await RenderHtmlCoreAsync(definition, parameters, ct).ConfigureAwait(false);
            return new ReportViewerExportResult("html", "text/html", "html", System.Text.Encoding.UTF8.GetBytes(html));
        }
        catch (Exception ex)
        {
            _latestTrace = [$"[Export] Failed: {ex.GetType().Name} - {ex.Message}"];
            System.Diagnostics.Debug.WriteLine($"HTML export failed: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetLatestTrace() => _latestTrace;

    private async Task<string> RenderHtmlCoreAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct)
    {
        var context = new LayoutSizingContext(_textLayout);
        var layoutOptions = ResolveLayoutOptions(definition);
        var reportDocument = await _engine.RunAsync(definition, parameters, context, layoutOptions, ct).ConfigureAwait(false);

        using var stream = new MemoryStream();
        await _exporter.ExportAsync(reportDocument, stream, ct).ConfigureAwait(false);
        stream.Position = 0;

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    private static LayoutOptions ResolveLayoutOptions(ReportDefinition definition)
    {
        var metadata = definition.Metadata;
        if (metadata.TryGetValue("authoring.compiledComponents", out var compiledComponents)
            && compiledComponents is not null)
        {
            return new LayoutOptions { PageMargins = new Thickness(0f) };
        }

        return new LayoutOptions();
    }
}
