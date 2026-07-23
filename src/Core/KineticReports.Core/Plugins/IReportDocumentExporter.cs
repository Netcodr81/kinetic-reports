namespace KineticReports.Plugins;

using KineticReports.Core.Layout;

/// <summary>
/// Exports a <see cref="ReportDocument"/> for one concrete format.
/// </summary>
public interface IReportDocumentExporter
{
    /// <summary>
    /// Gets the format descriptor served by this exporter.
    /// </summary>
    ExportFormatDescriptor Format { get; }

    /// <summary>
    /// Exports the report document and returns artifact bytes.
    /// </summary>
    /// <param name="reportDocument">Immutable document to export.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<byte[]> ExportAsync(ReportDocument reportDocument, CancellationToken cancellationToken = default);
}
