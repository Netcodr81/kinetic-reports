namespace KineticReports.Core.Viewer.Services;

using System.Globalization;
using System.Text.Json;
using KineticReports.Core.Definition;
using KineticReports.Core.Engine;
using KineticReports.Core.Export.Html;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.LayoutEngine;
using KineticReports.Core.Plugins;
using KineticReports.Core.Rendering;
using KineticReports.Core.Rendering.Skia;
using KineticReports.Core.Typography;
using KineticReports.Core.Visual;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of <see cref="IReportService"/> that executes and exports reports locally.
/// </summary>
public sealed class DefaultReportService : IReportService
{
    private static readonly IReadOnlyList<ReportViewerExportFormat> DefaultFormats =
    [
        new ReportViewerExportFormat("html", "HTML", "text/html", "html"),
        new ReportViewerExportFormat("pdf", "PDF", "application/pdf", "pdf")
    ];

    private readonly IReportEngine _engine;
    private readonly IHtmlExporter _htmlExporter;
    private readonly IVisualDocumentBuilder _visualDocumentBuilder;
    private readonly VisualHitTestIndexBuilder _hitTestIndexBuilder;
    private readonly VisualTextSearchIndexBuilder _textSearchIndexBuilder;
    private readonly ITextLayout _textLayout;
    private readonly IPluginManager? _pluginManager;
    private readonly ILogger<DefaultReportService> _logger;
    private IReadOnlyList<string> _latestTrace = [];
    private const string WatermarkPluginId = "kinetic.watermark";

    /// <summary>
    /// Initializes the service with required dependencies.
    /// </summary>
    public DefaultReportService(
        IReportEngine engine,
        IHtmlExporter htmlExporter,
        IVisualDocumentBuilder visualDocumentBuilder,
        VisualHitTestIndexBuilder hitTestIndexBuilder,
        VisualTextSearchIndexBuilder textSearchIndexBuilder,
        ITextLayout textLayout,
        ILogger<DefaultReportService> logger,
        IPluginManager? pluginManager = null)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _htmlExporter = htmlExporter ?? throw new ArgumentNullException(nameof(htmlExporter));
        _visualDocumentBuilder = visualDocumentBuilder ?? throw new ArgumentNullException(nameof(visualDocumentBuilder));
        _hitTestIndexBuilder = hitTestIndexBuilder ?? throw new ArgumentNullException(nameof(hitTestIndexBuilder));
        _textSearchIndexBuilder = textSearchIndexBuilder ?? throw new ArgumentNullException(nameof(textSearchIndexBuilder));
        _textLayout = textLayout ?? throw new ArgumentNullException(nameof(textLayout));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pluginManager = pluginManager;
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
            _latestTrace = ["[Render] Executing with default report service", "[Export] Format: html"];
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
        => await RenderHtmlAsync(reportDocument, definition: null, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<string> RenderHtmlAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reportDocument);

        try
        {
            _latestTrace = ["[Render] Using prebuilt ReportDocument", "[Export] Format: html"];
            return await RenderHtmlCoreAsync(reportDocument, definition, ct).ConfigureAwait(false);
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

        try
        {
            var reportDocument = await ExecuteReportAsync(definition, parameters, ct).ConfigureAwait(false);
            return await ExportReportDocumentAsync(
                reportDocument,
                formatId,
                "[Render] Executing with default report service",
                definition,
                ct)
            .ConfigureAwait(false);
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
        => await ExportAsync(reportDocument, definition: null, formatId, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<ReportViewerExportResult> ExportAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition,
        string formatId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reportDocument);
        if (string.IsNullOrWhiteSpace(formatId))
            throw new ArgumentException("Format id is required.", nameof(formatId));

        try
        {
            return await ExportReportDocumentAsync(reportDocument, formatId, "[Render] Using prebuilt ReportDocument", definition, ct)
                .ConfigureAwait(false);
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
        _logger.LogInformation("Default report pipeline mode selected: Semantic");
        _latestTrace =
        [
            .. _latestTrace,
            "[Pipeline] Mode selected: Semantic",
            "[Pipeline] Semantic mode uses HtmlExporter execution path."
        ];

        var reportDocument = await ExecuteReportAsync(definition, parameters, ct).ConfigureAwait(false);
        return await RenderHtmlCoreAsync(reportDocument, definition, ct).ConfigureAwait(false);
    }

    private async Task<string> RenderHtmlCoreAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition,
        CancellationToken ct)
    {
        _latestTrace = [.. _latestTrace, $"[Render] Engine produced {reportDocument.PageCount} page(s)"];

        using var stream = new MemoryStream();
        await _htmlExporter.ExportAsync(reportDocument, stream, ct).ConfigureAwait(false);

        stream.Position = 0;

        using var reader = new StreamReader(stream);
        var html = await reader.ReadToEndAsync().ConfigureAwait(false);
        return await ApplyHtmlPostProcessorsAsync(html, definition, ct).ConfigureAwait(false);
    }

    private async Task<string> ApplyHtmlPostProcessorsAsync(
        string html,
        ReportDefinition? definition,
        CancellationToken ct)
    {
        if (_pluginManager == null || string.IsNullOrWhiteSpace(html))
            return html;

        var postProcessors = _pluginManager.LoadedPlugins
            .OfType<IHtmlReportPostProcessorPlugin>()
            .Where(plugin => PluginExecutionPolicy.IsEnabled(definition, plugin.Id))
            .OrderBy(plugin => plugin.Order)
            .ThenBy(plugin => plugin.Id, StringComparer.Ordinal)
            .ToList();

        if (postProcessors.Count == 0)
            return html;

        var current = html;
        foreach (var postProcessor in postProcessors)
            current = await postProcessor.ProcessHtmlAsync(current, ct).ConfigureAwait(false);

        return current;
    }

    private Task<ReportDocument> ExecuteReportAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct)
    {
        var context = new LayoutSizingContext(_textLayout);
        return _engine.RunAsync(definition, parameters, context, layoutOptions: null, ct);
    }

    private async Task<ReportViewerExportResult> ExportReportDocumentAsync(
        ReportDocument reportDocument,
        string formatId,
        string tracePrefix,
        ReportDefinition? definition,
        CancellationToken ct)
    {
        if (string.Equals(formatId, "html", StringComparison.OrdinalIgnoreCase))
        {
            _latestTrace = [tracePrefix, "[Export] Format: html"];
            var html = await RenderHtmlCoreAsync(reportDocument, definition, ct).ConfigureAwait(false);
            return new ReportViewerExportResult("html", "text/html", "html", System.Text.Encoding.UTF8.GetBytes(html));
        }

        if (string.Equals(formatId, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            _latestTrace = [tracePrefix, "[Export] Format: pdf", $"[Render] Engine produced {reportDocument.PageCount} page(s)"];

            var pdfRenderer = CreatePdfRenderer(definition);
            using var stream = new MemoryStream();
            await pdfRenderer.RenderAsync(reportDocument, stream, ct).ConfigureAwait(false);

            return new ReportViewerExportResult("pdf", "application/pdf", "pdf", stream.ToArray());
        }

        throw new NotSupportedException($"Default report service supports only 'html' and 'pdf' export. Requested '{formatId}'.");
    }

    private static SkiaRenderer CreatePdfRenderer(ReportDefinition? definition)
    {
        var options = new RenderOptions
        {
            Format = RenderFormat.Pdf,
            PdfWatermark = ResolvePdfWatermark(definition)
        };

        return new SkiaRenderer(options);
    }

    private static PdfWatermarkOptions? ResolvePdfWatermark(ReportDefinition? definition)
    {
        if (definition == null)
            return null;

        if (!PluginExecutionPolicy.IsEnabled(definition, WatermarkPluginId))
            return null;

        var enabled = ReadMetadataBool(definition, "plugins.kinetic.watermark.enabled", fallback: true);
        if (!enabled)
            return null;

        var text = ReadMetadataString(definition, "plugins.kinetic.watermark.message", "KineticReports");
        var opacity = ReadMetadataFloat(definition, "plugins.kinetic.watermark.opacity", 0.08f);
        var rotation = ReadMetadataFloat(definition, "plugins.kinetic.watermark.rotationDegrees", -30f);
        var fontSize = ReadMetadataFloat(definition, "plugins.kinetic.watermark.fontSizePx", 72f);
        var colorHex = ReadMetadataString(definition, "plugins.kinetic.watermark.textColorHex", "#000000");

        return new PdfWatermarkOptions
        {
            Text = string.IsNullOrWhiteSpace(text) ? "KineticReports" : text,
            Opacity = Math.Clamp(opacity, 0.01f, 1f),
            RotationDegrees = Math.Clamp(rotation, -360f, 360f),
            FontSize = Math.Clamp(fontSize, 8f, 300f),
            Color = ParseHexColorOrDefault(colorHex, KineticReports.Core.Styling.Color.Black)
        };
    }

    private static string ReadMetadataString(ReportDefinition definition, string key, string fallback)
    {
        if (!definition.Metadata.TryGetValue(key, out var raw) || raw == null)
            return fallback;

        if (raw is string s)
            return string.IsNullOrWhiteSpace(s) ? fallback : s;

        if (raw is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var value = element.GetString();
                return string.IsNullOrWhiteSpace(value) ? fallback : value;
            }
        }

        return raw.ToString() ?? fallback;
    }

    private static bool ReadMetadataBool(ReportDefinition definition, string key, bool fallback)
    {
        if (!definition.Metadata.TryGetValue(key, out var raw) || raw == null)
            return fallback;

        if (raw is bool b)
            return b;

        if (raw is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.True)
                return true;

            if (element.ValueKind == JsonValueKind.False)
                return false;

            if (element.ValueKind == JsonValueKind.String
                && bool.TryParse(element.GetString(), out var parsedBool))
            {
                return parsedBool;
            }
        }

        return bool.TryParse(raw.ToString(), out var parsed) ? parsed : fallback;
    }

    private static float ReadMetadataFloat(ReportDefinition definition, string key, float fallback)
    {
        if (!definition.Metadata.TryGetValue(key, out var raw) || raw == null)
            return fallback;

        if (raw is float f)
            return f;

        if (raw is double d)
            return (float)d;

        if (raw is decimal m)
            return (float)m;

        if (raw is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var n))
                return (float)n;

            if (element.ValueKind == JsonValueKind.String
                && float.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedText))
            {
                return parsedText;
            }
        }

        return float.TryParse(raw.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }

    private static KineticReports.Core.Styling.Color ParseHexColorOrDefault(
        string? hex,
        KineticReports.Core.Styling.Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return fallback;

        var value = hex.Trim();
        if (value.StartsWith('#'))
            value = value[1..];

        if (value.Length == 6 && uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            var r = (byte)((rgb >> 16) & 0xFF);
            var g = (byte)((rgb >> 8) & 0xFF);
            var b = (byte)(rgb & 0xFF);
            return KineticReports.Core.Styling.Color.FromRgb(r, g, b);
        }

        if (value.Length == 8 && uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var argb))
            return KineticReports.Core.Styling.Color.FromArgb(argb);

        return fallback;
    }

}