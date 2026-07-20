namespace KineticReports.Export.Html;

using KineticReports.Core.Layout;

/// <summary>
/// Exports an immutable <see cref="LayoutTree"/> to HTML format.
/// </summary>
public interface IHtmlExporter
{
    /// <summary>
    /// Exports the layout tree to HTML and writes it to the output stream.
    /// </summary>
    /// <param name="tree">The immutable layout tree to export.</param>
    /// <param name="output">The stream to write HTML to.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ExportAsync(LayoutTree tree, Stream output, CancellationToken ct = default);
}
