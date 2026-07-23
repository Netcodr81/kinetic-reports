namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Layout;
using KineticReports.Plugins;
using KineticReports.Rendering;
using KineticReports.Rendering.Skia;
using KineticReports.Visual;
using Microsoft.Extensions.Options;

/// <summary>
/// Exports report documents as PDF and applies visual pipeline mode when requested.
/// </summary>
public sealed class PdfReportDocumentExporter : IReportDocumentExporter
{
    /// <inheritdoc/>
    public ExportFormatDescriptor Format => new("pdf", "PDF", "application/pdf", "pdf");

    private readonly IVisualDocumentBuilder _visualDocumentBuilder;
    private readonly VisualSkiaRenderer _visualSkiaRenderer;
    private readonly RenderingPipelineOptions _pipelineOptions;

    /// <summary>
    /// Initializes a new <see cref="PdfReportDocumentExporter"/>.
    /// </summary>
    public PdfReportDocumentExporter(
        IVisualDocumentBuilder visualDocumentBuilder,
        VisualSkiaRenderer visualSkiaRenderer,
        IOptions<RenderingPipelineOptions> pipelineOptions)
    {
        _visualDocumentBuilder = visualDocumentBuilder ?? throw new ArgumentNullException(nameof(visualDocumentBuilder));
        _visualSkiaRenderer = visualSkiaRenderer ?? throw new ArgumentNullException(nameof(visualSkiaRenderer));
        _pipelineOptions = pipelineOptions?.Value ?? throw new ArgumentNullException(nameof(pipelineOptions));
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportAsync(ReportDocument reportDocument, CancellationToken cancellationToken = default)
    {
        if (reportDocument == null) throw new ArgumentNullException(nameof(reportDocument));

        using var stream = new MemoryStream();
        if (_pipelineOptions.UseVisualPipeline)
        {
            var visualDocument = _visualDocumentBuilder.Build(reportDocument);
            await _visualSkiaRenderer.RenderPdfAsync(visualDocument, stream, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var legacyRenderer = new SkiaRenderer(new RenderOptions { Format = RenderFormat.Pdf });
            await legacyRenderer.RenderAsync(reportDocument, stream, cancellationToken).ConfigureAwait(false);
        }

        return stream.ToArray();
    }
}
