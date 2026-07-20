namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Layout;

/// <summary>
/// Stores and retrieves layout trees from executed reports.
/// </summary>
public interface IReportStore
{
    /// <summary>Saves a layout tree with the given operation ID.</summary>
    Task SaveAsync(string operationId, LayoutTree layoutTree, CancellationToken ct = default);

    /// <summary>Retrieves a stored layout tree by operation ID.</summary>
    Task<LayoutTree?> RetrieveAsync(string operationId, CancellationToken ct = default);

    /// <summary>Clears the stored result.</summary>
    Task ClearAsync(string operationId, CancellationToken ct = default);
}
