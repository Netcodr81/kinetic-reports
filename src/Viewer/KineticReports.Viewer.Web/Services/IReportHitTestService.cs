namespace KineticReports.Viewer.Web.Services;

using KineticReports.Viewer.Web.Models;

/// <summary>
/// Resolves visual hit-test results for executed reports.
/// </summary>
public interface IReportHitTestService
{
    /// <summary>
    /// Performs a hit-test against an executed report operation.
    /// </summary>
    /// <param name="operationId">Stored report execution operation id.</param>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="x">Page-local X coordinate in DIPs.</param>
    /// <param name="y">Page-local Y coordinate in DIPs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing whether the operation was found and the hit-test payload.
    /// </returns>
    Task<(bool Found, ReportHitTestResponse Response)> HitTestAsync(
        string operationId,
        int pageNumber,
        float x,
        float y,
        CancellationToken ct = default);
}
