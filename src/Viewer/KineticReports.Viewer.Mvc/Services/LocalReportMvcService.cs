namespace KineticReports.Viewer.Mvc.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Engine;
using KineticReports.Core.Export.Html;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.LayoutEngine;
using KineticReports.Core.Typography;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default in-process MVC report service backed by local engine and HTML exporter.
/// </summary>
public sealed class LocalReportMvcService : IReportMvcService
{
    private readonly IReportEngine _engine;
    private readonly IHtmlExporter _exporter;
    private readonly ITextLayout _textLayout;
    private readonly ILogger<LocalReportMvcService> _logger;
    private IReadOnlyList<string> _latestTrace = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalReportMvcService"/> class.
    /// </summary>
    public LocalReportMvcService(
        IReportEngine engine,
        IHtmlExporter exporter,
        ITextLayout textLayout,
        ILogger<LocalReportMvcService> logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
        _textLayout = textLayout ?? throw new ArgumentNullException(nameof(textLayout));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<string> RenderHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        _latestTrace = ["[Render] Executing with local MVC viewer service", "[Export] Format: html"];

        var context = new LayoutSizingContext(_textLayout);
        var layoutOptions = ResolveLayoutOptions(definition);

        _logger.LogInformation("MVC viewer rendering report {ReportId}", definition.Id);

        var reportDocument = await _engine
            .RunAsync(definition, parameters, context, layoutOptions, ct)
            .ConfigureAwait(false);

        using var stream = new MemoryStream();
        await _exporter.ExportAsync(reportDocument, stream, ct).ConfigureAwait(false);
        stream.Position = 0;

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetLatestTrace() => _latestTrace;

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
