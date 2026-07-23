namespace KineticReports.Viewer.Mvc.Services;

using KineticReports.Core.Definition;

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
    /// Returns diagnostic trace entries from the latest render operation.
    /// </summary>
    IReadOnlyList<string> GetLatestTrace();
}
