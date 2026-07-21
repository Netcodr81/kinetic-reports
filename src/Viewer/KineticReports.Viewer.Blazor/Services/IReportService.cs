namespace KineticReports.Viewer.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;

/// <summary>
/// Contract for report execution and export operations in Blazor components.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Executes a report and returns the report document.
    /// </summary>
    Task<ReportDocument?> ExecuteAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default);

    /// <summary>
    /// Exports a report document to HTML.
    /// </summary>
    Task<string> ExportHtmlAsync(
        ReportDocument reportDocument,
        CancellationToken ct = default);
}
