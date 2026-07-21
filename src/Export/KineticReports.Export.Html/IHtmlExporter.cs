namespace KineticReports.Export.Html;

using KineticReports.Core.Layout;

/// <summary>
/// Exports an immutable <see cref="ReportLayout"/> to HTML format.
/// </summary>
public interface IHtmlExporter
{
    /// <summary>
    /// Exports the report layout to HTML and writes it to the output stream.
    /// </summary>
    /// <param name="reportLayout">The immutable report layout to export.</param>
    /// <param name="output">The stream to write HTML to.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ExportAsync(ReportLayout reportLayout, Stream output, CancellationToken ct = default);
}
