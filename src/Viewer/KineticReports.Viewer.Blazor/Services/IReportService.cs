namespace KineticReports.Viewer.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;

/// <summary>
/// Contract for report execution and export operations in Blazor components.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Executes a report and returns the report layout.
    /// </summary>
    Task<ReportLayout?> ExecuteAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default);

    /// <summary>
    /// Exports a report layout to HTML.
    /// </summary>
    Task<string> ExportHtmlAsync(
        ReportLayout ReportLayout,
        CancellationToken ct = default);
}
