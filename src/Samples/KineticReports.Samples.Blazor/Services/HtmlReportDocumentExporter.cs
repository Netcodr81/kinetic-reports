namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Layout;
using KineticReports.Export.Html;
using KineticReports.Plugins;

/// <summary>
/// Adapts <see cref="IHtmlExporter"/> to the generic document exporter contract.
/// </summary>
public sealed class HtmlReportDocumentExporter : IReportDocumentExporter
{
    /// <inheritdoc/>
    public ExportFormatDescriptor Format => new("html", "HTML", "text/html", "html");

    private readonly IHtmlExporter _htmlExporter;

    /// <summary>
    /// Initializes a new <see cref="HtmlReportDocumentExporter"/>.
    /// </summary>
    public HtmlReportDocumentExporter(IHtmlExporter htmlExporter)
    {
        _htmlExporter = htmlExporter ?? throw new ArgumentNullException(nameof(htmlExporter));
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportAsync(ReportDocument reportDocument, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await _htmlExporter.ExportAsync(reportDocument, stream, cancellationToken).ConfigureAwait(false);
        return stream.ToArray();
    }
}
