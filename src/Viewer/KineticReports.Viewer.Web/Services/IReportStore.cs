namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Layout;

/// <summary>
/// Stores and retrieves report documents from executed reports.
/// </summary>
public interface IReportStore
{
    /// <summary>Saves a report document with the given operation ID.</summary>
    Task SaveAsync(string operationId, ReportDocument reportDocument, CancellationToken ct = default);

    /// <summary>Retrieves a stored report document by operation ID.</summary>
    Task<ReportDocument?> RetrieveAsync(string operationId, CancellationToken ct = default);

    /// <summary>Clears the stored result.</summary>
    Task ClearAsync(string operationId, CancellationToken ct = default);
}
