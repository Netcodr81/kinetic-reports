namespace KineticReports.Viewer.Mvc.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Viewer.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default in-process MVC report service backed by local engine and HTML exporter.
/// </summary>
public sealed class DefaultReportMvcService : IReportMvcService
{
    private readonly IReportService _reportService;
    private readonly ILogger<DefaultReportMvcService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultReportMvcService"/> class.
    /// </summary>
    public DefaultReportMvcService(
        IReportService reportService,
        ILogger<DefaultReportMvcService> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
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

        _logger.LogInformation("MVC viewer rendering report {ReportId}", definition.Id);

        return await _reportService
            .RenderHtmlAsync(definition, parameters, ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<string> RenderHtmlAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reportDocument);
        return _reportService.RenderHtmlAsync(reportDocument, definition, ct);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ReportViewerExportFormat> GetAvailableExportFormats()
        => _reportService.GetAvailableExportFormats();

    /// <inheritdoc/>
    public Task<ReportViewerExportResult> ExportAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition,
        string formatId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reportDocument);
        return _reportService.ExportAsync(reportDocument, definition, formatId, ct);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetLatestTrace() => _reportService.GetLatestTrace();
}