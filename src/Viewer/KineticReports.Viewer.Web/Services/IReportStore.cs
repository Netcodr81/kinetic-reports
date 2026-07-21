namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Layout;

/// <summary>
/// Stores and retrieves report layouts from executed reports.
/// </summary>
public interface IReportStore
{
    /// <summary>Saves a report layout with the given operation ID.</summary>
    Task SaveAsync(string operationId, ReportLayout ReportLayout, CancellationToken ct = default);

    /// <summary>Retrieves a stored report layout by operation ID.</summary>
    Task<ReportLayout?> RetrieveAsync(string operationId, CancellationToken ct = default);

    /// <summary>Clears the stored result.</summary>
    Task ClearAsync(string operationId, CancellationToken ct = default);
}
