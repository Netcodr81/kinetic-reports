namespace KineticReports.Viewer.Web.Services;

using KineticReports.Viewer.Web.Models;

/// <summary>
/// Resolves text search results for executed reports.
/// </summary>
public interface IReportTextSearchService
{
    /// <summary>
    /// Searches text content within an executed report operation.
    /// </summary>
    /// <param name="operationId">Stored report execution operation id.</param>
    /// <param name="query">Case-insensitive search query.</param>
    /// <param name="pageNumber">Optional one-based page filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing whether the operation was found and search results payload.
    /// </returns>
    Task<(bool Found, ReportTextSearchResponse Response)> SearchAsync(
        string operationId,
        string query,
        int? pageNumber = null,
        CancellationToken ct = default);
}
