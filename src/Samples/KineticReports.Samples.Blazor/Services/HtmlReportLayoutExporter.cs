namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Layout;
using KineticReports.Export.Html;
using KineticReports.Plugins;

/// <summary>
/// Adapts <see cref="IHtmlExporter"/> to the generic layout exporter contract.
/// </summary>
public sealed class HtmlReportLayoutExporter : IReportLayoutExporter
{
    /// <inheritdoc/>
    public ExportFormatDescriptor Format => new("html", "HTML", "text/html", "html");

    private readonly IHtmlExporter _htmlExporter;

    /// <summary>
    /// Initializes a new <see cref="HtmlReportLayoutExporter"/>.
    /// </summary>
    public HtmlReportLayoutExporter(IHtmlExporter htmlExporter)
    {
        _htmlExporter = htmlExporter ?? throw new ArgumentNullException(nameof(htmlExporter));
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportAsync(ReportLayout reportLayout, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await _htmlExporter.ExportAsync(reportLayout, stream, cancellationToken).ConfigureAwait(false);
        return stream.ToArray();
    }
}
