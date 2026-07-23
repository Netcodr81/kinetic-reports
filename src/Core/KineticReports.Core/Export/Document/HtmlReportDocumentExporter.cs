namespace KineticReports.Core.Export.Document;

using KineticReports.Core.Export.Html;
using KineticReports.Core.Layout;
using KineticReports.Core.Plugins;
using KineticReports.Core.Visual;

/// <summary>
/// Exports report documents as HTML using the visual document pipeline.
/// </summary>
public sealed class HtmlReportDocumentExporter : IReportDocumentExporter
{
    /// <inheritdoc/>
    public ExportFormatDescriptor Format => new("html", "HTML", "text/html", "html");

    private readonly IVisualHtmlExporter _visualHtmlExporter;
    private readonly IVisualDocumentBuilder _visualDocumentBuilder;

    /// <summary>
    /// Initializes a new <see cref="HtmlReportDocumentExporter"/>.
    /// </summary>
    public HtmlReportDocumentExporter(
        IVisualHtmlExporter visualHtmlExporter,
        IVisualDocumentBuilder visualDocumentBuilder)
    {
        _visualHtmlExporter = visualHtmlExporter ?? throw new ArgumentNullException(nameof(visualHtmlExporter));
        _visualDocumentBuilder = visualDocumentBuilder ?? throw new ArgumentNullException(nameof(visualDocumentBuilder));
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportAsync(ReportDocument reportDocument, CancellationToken cancellationToken = default)
    {
        if (reportDocument == null) throw new ArgumentNullException(nameof(reportDocument));

        using var stream = new MemoryStream();
        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        await _visualHtmlExporter.ExportAsync(visualDocument, stream, cancellationToken).ConfigureAwait(false);

        return stream.ToArray();
    }
}
