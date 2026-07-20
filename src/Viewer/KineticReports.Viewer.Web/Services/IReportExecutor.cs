namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;

/// <summary>
/// Orchestrates the full report execution pipeline: data resolution → layout → rendering/export.
/// </summary>
public interface IReportExecutor
{
    /// <summary>
    /// Executes a report and returns the immutable layout tree.
    /// </summary>
    Task<LayoutTree> ExecuteAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default);
}
