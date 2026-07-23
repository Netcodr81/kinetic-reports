namespace KineticReports.Core.Export.Html;

using KineticReports.Core.Visual;

/// <summary>
/// Adapts <see cref="IVisualHtmlExporter"/> to the generic <see cref="IVisualRenderer"/> contract.
/// </summary>
public sealed class VisualHtmlRendererAdapter : IVisualRenderer
{
    /// <inheritdoc/>
    public VisualRenderFormatDescriptor Format => new("html", "HTML", "text/html", "html");

    private readonly IVisualHtmlExporter _exporter;

    /// <summary>
    /// Initializes a new <see cref="VisualHtmlRendererAdapter"/>.
    /// </summary>
    public VisualHtmlRendererAdapter(IVisualHtmlExporter exporter)
    {
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
    }

    /// <inheritdoc/>
    public Task RenderAsync(VisualDocument visualDocument, Stream output, CancellationToken cancellationToken = default)
    {
        return _exporter.ExportAsync(visualDocument, output, cancellationToken);
    }
}
