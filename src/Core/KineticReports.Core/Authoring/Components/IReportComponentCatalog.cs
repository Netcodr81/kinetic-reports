namespace KineticReports.Core.Authoring.Components;

/// <summary>
/// Provides available component descriptors for drag-and-drop report designers.
/// </summary>
public interface IReportComponentCatalog
{
    /// <summary>
    /// Gets all component descriptors supported by the current authoring runtime.
    /// </summary>
    IReadOnlyList<ReportComponentDescriptor> GetComponents();
}
