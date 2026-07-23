namespace KineticReports.Core.Export.Document;

using KineticReports.Core.Layout;
using KineticReports.Core.Plugins;
using KineticReports.Core.Rendering.Skia;
using KineticReports.Core.Visual;

/// <summary>
/// Exports report documents as PDF using the visual document pipeline.
/// </summary>
public sealed class PdfReportDocumentExporter : IReportDocumentExporter
{
    /// <inheritdoc/>
    public ExportFormatDescriptor Format => new("pdf", "PDF", "application/pdf", "pdf");

    private readonly IVisualDocumentBuilder _visualDocumentBuilder;
    private readonly VisualSkiaRenderer _visualSkiaRenderer;

    /// <summary>
    /// Initializes a new <see cref="PdfReportDocumentExporter"/>.
    /// </summary>
    public PdfReportDocumentExporter(
        IVisualDocumentBuilder visualDocumentBuilder,
        VisualSkiaRenderer visualSkiaRenderer)
    {
        _visualDocumentBuilder = visualDocumentBuilder ?? throw new ArgumentNullException(nameof(visualDocumentBuilder));
        _visualSkiaRenderer = visualSkiaRenderer ?? throw new ArgumentNullException(nameof(visualSkiaRenderer));
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportAsync(ReportDocument reportDocument, CancellationToken cancellationToken = default)
    {
        if (reportDocument == null) throw new ArgumentNullException(nameof(reportDocument));

        using var stream = new MemoryStream();
        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        await _visualSkiaRenderer.RenderPdfAsync(visualDocument, stream, cancellationToken).ConfigureAwait(false);

        return stream.ToArray();
    }
}
