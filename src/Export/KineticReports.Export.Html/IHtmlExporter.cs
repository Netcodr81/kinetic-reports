namespace KineticReports.Export.Html;

using KineticReports.Core.Layout;

/// <summary>
/// Exports an immutable <see cref="ReportDocument"/> to HTML format.
/// </summary>
public interface IHtmlExporter
{
    /// <summary>
    /// Exports the report document to HTML and writes it to the output stream.
    /// </summary>
    /// <param name="reportDocument">The immutable report document to export.</param>
    /// <param name="output">The stream to write HTML to.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ExportAsync(ReportDocument reportDocument, Stream output, CancellationToken ct = default);
}
