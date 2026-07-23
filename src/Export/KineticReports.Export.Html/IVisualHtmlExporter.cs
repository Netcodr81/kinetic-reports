namespace KineticReports.Export.Html;

using KineticReports.Visual;

/// <summary>
/// Exports an immutable <see cref="VisualDocument"/> to HTML format.
/// </summary>
public interface IVisualHtmlExporter
{
    /// <summary>
    /// Exports the visual document to HTML and writes it to the output stream.
    /// </summary>
    /// <param name="visualDocument">The visual document to export.</param>
    /// <param name="output">The stream to write HTML to.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ExportAsync(VisualDocument visualDocument, Stream output, CancellationToken ct = default);
}
