namespace KineticReports.Core.Plugins;

/// <summary>
/// Resolves report exporters by format ID.
/// </summary>
public interface IReportDocumentExporterRegistry
{
    /// <summary>
    /// Gets formats that are backed by a registered exporter.
    /// </summary>
    IReadOnlyList<ExportFormatDescriptor> GetAvailableFormats();

    /// <summary>
    /// Tries to resolve an exporter for a format ID.
    /// </summary>
    /// <param name="formatId">Format ID, such as html or pdf.</param>
    /// <param name="exporter">Resolved exporter when found.</param>
    /// <returns><c>true</c> when an exporter is found; otherwise <c>false</c>.</returns>
    bool TryGetExporter(string formatId, out IReportDocumentExporter? exporter);
}
