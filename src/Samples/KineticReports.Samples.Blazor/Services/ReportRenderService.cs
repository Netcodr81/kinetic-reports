namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Layout;

/// <summary>
/// Service for rendering reports to HTML preview.
/// </summary>
public interface IReportRenderService
{
    /// <summary>
    /// Executes a report and exports it as HTML.
    /// </summary>
    Task<string> RenderReportAsHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters = null!,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IReportRenderService"/>.
/// </summary>
public sealed class ReportRenderService : IReportRenderService
{
    private readonly IReportEngine _engine;
    private readonly IFontMetrics _fontMetrics;
    private readonly IHtmlExporter _exporter;

    /// <summary>
    /// Initializes a new <see cref="ReportRenderService"/>.
    /// </summary>
    public ReportRenderService(
        IReportEngine engine,
        IFontMetrics fontMetrics,
        IHtmlExporter exporter)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _fontMetrics = fontMetrics ?? throw new ArgumentNullException(nameof(fontMetrics));
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
    }

    /// <summary>
    /// Executes the report and exports it as HTML.
    /// </summary>
    public async Task<string> RenderReportAsHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters = null!,
        CancellationToken cancellationToken = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));

        try
        {
            var reportParameters = parameters ?? new Dictionary<string, object?>();
            var context = new MeasureContext(_fontMetrics);
            var layoutOptions = new LayoutOptions();

            var layoutTree = await _engine.RunAsync(
                definition,
                reportParameters,
                context,
                layoutOptions,
                cancellationToken).ConfigureAwait(false);

            using var stream = new MemoryStream();
            await _exporter.ExportAsync(layoutTree, stream, cancellationToken).ConfigureAwait(false);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return $@"
                <div class='alert alert-danger' role='alert'>
                    <h4 class='alert-heading'>Report Rendering Error</h4>
                    <p><strong>Error:</strong> {System.Net.WebUtility.HtmlEncode(ex.Message)}</p>
                    <details>
                        <summary>Stack trace</summary>
                        <pre style='font-size: 0.875rem; overflow-x: auto;'>{System.Net.WebUtility.HtmlEncode(ex.StackTrace)}</pre>
                    </details>
                </div>";
        }
    }
}
