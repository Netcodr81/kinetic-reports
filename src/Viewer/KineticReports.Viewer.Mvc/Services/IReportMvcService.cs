namespace KineticReports.Viewer.Mvc.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Viewer.Services;

/// <summary>
/// Contract for server-side MVC report rendering operations.
/// </summary>
public interface IReportMvcService
{
    /// <summary>
    /// Renders the supplied report definition to HTML.
    /// </summary>
    Task<string> RenderHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default);

    /// <summary>
    /// Renders the supplied prebuilt report document to HTML.
    /// </summary>
    Task<string> RenderHtmlAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets available export formats.
    /// </summary>
    IReadOnlyList<ReportViewerExportFormat> GetAvailableExportFormats();

    /// <summary>
    /// Exports the supplied prebuilt report document.
    /// </summary>
    Task<ReportViewerExportResult> ExportAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition,
        string formatId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns diagnostic trace entries from the latest render operation.
    /// </summary>
    IReadOnlyList<string> GetLatestTrace();
}
