namespace KineticReports.Plugins;

using KineticReports.Core.Layout;

/// <summary>
/// Exports a <see cref="ReportLayout"/> for one concrete format.
/// </summary>
public interface IReportLayoutExporter
{
    /// <summary>
    /// Gets the format descriptor served by this exporter.
    /// </summary>
    ExportFormatDescriptor Format { get; }

    /// <summary>
    /// Exports the report layout and returns artifact bytes.
    /// </summary>
    /// <param name="reportLayout">Immutable layout to export.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<byte[]> ExportAsync(ReportLayout reportLayout, CancellationToken cancellationToken = default);
}
