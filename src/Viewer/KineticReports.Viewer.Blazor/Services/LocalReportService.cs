namespace KineticReports.Viewer.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Layout;

/// <summary>
/// Blazor implementation of report service that executes and exports reports locally.
/// </summary>
public sealed class LocalReportService : IReportService
{
    private readonly IReportEngine _engine;
    private readonly IHtmlExporter _exporter;
    private readonly IFontMetrics _fontMetrics;

    /// <summary>
    /// Initializes the service with required dependencies.
    /// </summary>
    public LocalReportService(
        IReportEngine engine,
        IHtmlExporter exporter,
        IFontMetrics fontMetrics)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
        _fontMetrics = fontMetrics ?? throw new ArgumentNullException(nameof(fontMetrics));
    }

    /// <summary>
    /// Executes a report locally.
    /// </summary>
    public async Task<ReportLayout?> ExecuteAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        try
        {
            var context = new LayoutSizingContext(_fontMetrics);
            var layoutOptions = new LayoutOptions();
            return await _engine.RunAsync(definition, parameters, context, layoutOptions, ct);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Report execution failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Exports a report layout to HTML.
    /// </summary>
    public async Task<string> ExportHtmlAsync(
        ReportLayout ReportLayout,
        CancellationToken ct = default)
    {
        if (ReportLayout == null) throw new ArgumentNullException(nameof(ReportLayout));

        try
        {
            using var stream = new MemoryStream();
            await _exporter.ExportAsync(ReportLayout, stream, ct);
            stream.Position = 0;

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTML export failed: {ex.Message}");
            return string.Empty;
        }
    }
}
