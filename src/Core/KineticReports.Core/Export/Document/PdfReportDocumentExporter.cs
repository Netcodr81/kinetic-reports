namespace KineticReports.Core.Export.Document;

using KineticReports.Core.Layout;
using KineticReports.Core.Plugins;
using KineticReports.Core.Rendering;
using KineticReports.Core.Rendering.Skia;

/// <summary>
/// Exports report documents as PDF using the full report-document renderer pipeline.
/// </summary>
public sealed class PdfReportDocumentExporter : IReportDocumentExporter
{
    /// <inheritdoc/>
    public ExportFormatDescriptor Format => new("pdf", "PDF", "application/pdf", "pdf");

    private readonly SkiaRenderer _skiaRenderer;

    /// <summary>
    /// Initializes a new <see cref="PdfReportDocumentExporter"/>.
    /// </summary>
    public PdfReportDocumentExporter()
    {
        _skiaRenderer = new SkiaRenderer(new RenderOptions { Format = RenderFormat.Pdf });
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportAsync(ReportDocument reportDocument, CancellationToken cancellationToken = default)
    {
        if (reportDocument == null) throw new ArgumentNullException(nameof(reportDocument));

        using var stream = new MemoryStream();
        await _skiaRenderer.RenderAsync(reportDocument, stream, cancellationToken).ConfigureAwait(false);

        return stream.ToArray();
    }
}
