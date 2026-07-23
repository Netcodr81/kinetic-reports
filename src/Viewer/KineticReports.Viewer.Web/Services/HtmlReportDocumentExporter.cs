namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Layout;
using KineticReports.Export.Html;
using KineticReports.Plugins;
using KineticReports.Visual;
using Microsoft.Extensions.Options;

/// <summary>
/// Exports report documents as HTML and applies visual pipeline mode when requested.
/// </summary>
public sealed class HtmlReportDocumentExporter : IReportDocumentExporter
{
    /// <inheritdoc/>
    public ExportFormatDescriptor Format => new("html", "HTML", "text/html", "html");

    private readonly IHtmlExporter _htmlExporter;
    private readonly IVisualHtmlExporter _visualHtmlExporter;
    private readonly IVisualDocumentBuilder _visualDocumentBuilder;
    private readonly RenderingPipelineOptions _pipelineOptions;

    /// <summary>
    /// Initializes a new <see cref="HtmlReportDocumentExporter"/>.
    /// </summary>
    public HtmlReportDocumentExporter(
        IHtmlExporter htmlExporter,
        IVisualHtmlExporter visualHtmlExporter,
        IVisualDocumentBuilder visualDocumentBuilder,
        IOptions<RenderingPipelineOptions> pipelineOptions)
    {
        _htmlExporter = htmlExporter ?? throw new ArgumentNullException(nameof(htmlExporter));
        _visualHtmlExporter = visualHtmlExporter ?? throw new ArgumentNullException(nameof(visualHtmlExporter));
        _visualDocumentBuilder = visualDocumentBuilder ?? throw new ArgumentNullException(nameof(visualDocumentBuilder));
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
            await _visualHtmlExporter.ExportAsync(visualDocument, stream, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await _htmlExporter.ExportAsync(reportDocument, stream, cancellationToken).ConfigureAwait(false);
        }

        return stream.ToArray();
    }
}
