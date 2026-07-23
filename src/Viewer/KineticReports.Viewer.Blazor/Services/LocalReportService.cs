namespace KineticReports.Viewer.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Engine;
using KineticReports.Core.Export.Html;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.LayoutEngine;
using KineticReports.Core.Typography;
using KineticReports.Core.Visual;
using Microsoft.Extensions.Logging;

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
    private readonly IVisualHtmlExporter _visualHtmlExporter;
    private readonly IVisualDocumentBuilder _visualDocumentBuilder;
    private readonly VisualHitTestIndexBuilder _hitTestIndexBuilder;
    private readonly VisualTextSearchIndexBuilder _textSearchIndexBuilder;
    private readonly ITextLayout _textLayout;
    private readonly ILogger<LocalReportService> _logger;
    private IReadOnlyList<string> _latestTrace = [];

    /// <summary>
    /// Initializes the service with required dependencies.
    /// </summary>
    public LocalReportService(
        IReportEngine engine,
        IVisualHtmlExporter visualHtmlExporter,
        IVisualDocumentBuilder visualDocumentBuilder,
        VisualHitTestIndexBuilder hitTestIndexBuilder,
        VisualTextSearchIndexBuilder textSearchIndexBuilder,
        ITextLayout textLayout,
        ILogger<LocalReportService> logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _visualHtmlExporter = visualHtmlExporter ?? throw new ArgumentNullException(nameof(visualHtmlExporter));
        _visualDocumentBuilder = visualDocumentBuilder ?? throw new ArgumentNullException(nameof(visualDocumentBuilder));
        _hitTestIndexBuilder = hitTestIndexBuilder ?? throw new ArgumentNullException(nameof(hitTestIndexBuilder));
        _textSearchIndexBuilder = textSearchIndexBuilder ?? throw new ArgumentNullException(nameof(textSearchIndexBuilder));
        _textLayout = textLayout ?? throw new ArgumentNullException(nameof(textLayout));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    public async Task<string> RenderHtmlAsync(ReportDocument reportDocument, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reportDocument);

        try
        {
            _latestTrace = ["[Render] Using prebuilt ReportDocument", "[Export] Format: html"];
            return await RenderHtmlCoreAsync(reportDocument, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _latestTrace = [$"[Render] Failed: {ex.GetType().Name} - {ex.Message}"];
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
    public async Task<ReportViewerExportResult> ExportAsync(
        ReportDocument reportDocument,
        string formatId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reportDocument);
        if (string.IsNullOrWhiteSpace(formatId))
            throw new ArgumentException("Format id is required.", nameof(formatId));

        if (!string.Equals(formatId, "html", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Local viewer service supports only 'html' export. Requested '{formatId}'.");

        try
        {
            _latestTrace = ["[Render] Using prebuilt ReportDocument", "[Export] Format: html"];
            var html = await RenderHtmlCoreAsync(reportDocument, ct).ConfigureAwait(false);
            return new ReportViewerExportResult("html", "text/html", "html", System.Text.Encoding.UTF8.GetBytes(html));
        }
        catch (Exception ex)
        {
            _latestTrace = [$"[Export] Failed: {ex.GetType().Name} - {ex.Message}"];
            throw;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetLatestTrace() => _latestTrace;

    /// <inheritdoc/>
    public async Task<ReportViewerHitTestResult> HitTestAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        int pageNumber,
        float x,
        float y,
        CancellationToken ct = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber must be greater than zero.");

        var reportDocument = await ExecuteReportAsync(definition, parameters, ct).ConfigureAwait(false);
        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        var index = _hitTestIndexBuilder.Build(visualDocument);
        var hit = index.HitTest(pageNumber, new Point(x, y));

        if (hit == null)
        {
            return new ReportViewerHitTestResult(
                false,
                pageNumber,
                x,
                y,
                null,
                null,
                new Dictionary<string, string>());
        }

        return new ReportViewerHitTestResult(
            true,
            pageNumber,
            x,
            y,
            hit.Element.Id,
            hit.LayerName,
            hit.Element.Metadata);
    }

    /// <inheritdoc/>
    public Task<ReportViewerHitTestResult> HitTestAsync(
        ReportDocument reportDocument,
        int pageNumber,
        float x,
        float y,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reportDocument);
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber must be greater than zero.");

        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        var index = _hitTestIndexBuilder.Build(visualDocument);
        var hit = index.HitTest(pageNumber, new Point(x, y));

        if (hit == null)
        {
            return Task.FromResult(new ReportViewerHitTestResult(
                false,
                pageNumber,
                x,
                y,
                null,
                null,
                new Dictionary<string, string>()));
        }

        return Task.FromResult(new ReportViewerHitTestResult(
            true,
            pageNumber,
            x,
            y,
            hit.Element.Id,
            hit.LayerName,
            hit.Element.Metadata));
    }

    /// <inheritdoc/>
    public async Task<ReportViewerTextSearchResult> SearchTextAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        string query,
        int? pageNumber = null,
        CancellationToken ct = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("query is required.", nameof(query));
        if (pageNumber.HasValue && pageNumber.Value <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber must be greater than zero.");

        var reportDocument = await ExecuteReportAsync(definition, parameters, ct).ConfigureAwait(false);
        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        var index = _textSearchIndexBuilder.Build(visualDocument);

        var matches = index.Search(query, pageNumber)
            .Select(entry => new ReportViewerTextSearchMatch(
                entry.PageNumber,
                entry.LayerName,
                entry.ElementId,
                entry.Text))
            .ToList();

        return new ReportViewerTextSearchResult(query, pageNumber, matches);
    }

    /// <inheritdoc/>
    public Task<ReportViewerTextSearchResult> SearchTextAsync(
        ReportDocument reportDocument,
        string query,
        int? pageNumber = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reportDocument);
        if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("query is required.", nameof(query));
        if (pageNumber.HasValue && pageNumber.Value <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber must be greater than zero.");

        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        var index = _textSearchIndexBuilder.Build(visualDocument);

        var matches = index.Search(query, pageNumber)
            .Select(entry => new ReportViewerTextSearchMatch(
                entry.PageNumber,
                entry.LayerName,
                entry.ElementId,
                entry.Text))
            .ToList();

        return Task.FromResult(new ReportViewerTextSearchResult(query, pageNumber, matches));
    }

    private async Task<string> RenderHtmlCoreAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct)
    {
        _logger.LogInformation("Viewer local report pipeline mode selected: Visual");
        _latestTrace =
        [
            .. _latestTrace,
            "[Pipeline] Mode selected: Visual",
            "[Pipeline] Visual mode uses VisualHtmlExporter execution path."
        ];

        var reportDocument = await ExecuteReportAsync(definition, parameters, ct).ConfigureAwait(false);
        return await RenderHtmlCoreAsync(reportDocument, ct).ConfigureAwait(false);
    }

    private async Task<string> RenderHtmlCoreAsync(ReportDocument reportDocument, CancellationToken ct)
    {
        _latestTrace = [.. _latestTrace, $"[Render] Engine produced {reportDocument.PageCount} page(s)"];

        using var stream = new MemoryStream();
        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        await _visualHtmlExporter.ExportAsync(visualDocument, stream, ct).ConfigureAwait(false);

        stream.Position = 0;

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    private Task<ReportDocument> ExecuteReportAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct)
    {
        var context = new LayoutSizingContext(_textLayout);
        var layoutOptions = ResolveLayoutOptions(definition);
        return _engine.RunAsync(definition, parameters, context, layoutOptions, ct);
    }

    private static LayoutOptions ResolveLayoutOptions(ReportDefinition definition)
    {
        if (definition.Layout is not null)
        {
            return new LayoutOptions { PageMargins = new Thickness(0f) };
        }

        return new LayoutOptions();
    }
}
