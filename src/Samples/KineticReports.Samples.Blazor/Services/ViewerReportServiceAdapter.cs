namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Viewer.Blazor;
using KineticReports.Viewer.Blazor.Services;

/// <summary>
/// Adapts sample app report rendering services to the shared Blazor viewer contract.
/// </summary>
public sealed class ViewerReportServiceAdapter : IReportService
{
    private readonly IReportRenderService _renderService;

    /// <summary>
    /// Initializes a new <see cref="ViewerReportServiceAdapter"/>.
    /// </summary>
    public ViewerReportServiceAdapter(IReportRenderService renderService)
    {
        _renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));
    }

    /// <inheritdoc/>
    public IReadOnlyList<ReportViewerExportFormat> GetAvailableExportFormats()
    {
        return _renderService
            .GetAvailableExportFormats()
            .Select(format => new ReportViewerExportFormat(
                format.FormatId,
                format.DisplayName,
                format.MimeType,
                format.FileExtension))
            .ToList();
    }

    /// <inheritdoc/>
    public Task<string> RenderHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default)
    {
        return _renderService.RenderReportAsHtmlAsync(definition, parameters, ct);
    }

    /// <inheritdoc/>
    public async Task<ReportViewerExportResult> ExportAsync(
        ReportDefinition definition,
        string formatId,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default)
    {
        var result = await _renderService.ExportReportAsync(definition, formatId, parameters, ct).ConfigureAwait(false);

        return new ReportViewerExportResult(
            result.FinalFormatId,
            result.MimeType,
            result.FileExtension,
            result.Content);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetLatestTrace() => _renderService.GetLatestPluginTrace();
}
