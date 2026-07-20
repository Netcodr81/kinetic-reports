namespace KineticReports.Viewer.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;

/// <summary>
/// Contract for report execution and export operations in Blazor components.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Executes a report and returns the layout tree.
    /// </summary>
    Task<LayoutTree?> ExecuteAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default);

    /// <summary>
    /// Exports a layout tree to HTML.
    /// </summary>
    Task<string> ExportHtmlAsync(
        LayoutTree layoutTree,
        CancellationToken ct = default);
}
