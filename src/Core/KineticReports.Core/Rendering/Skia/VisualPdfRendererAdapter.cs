namespace KineticReports.Core.Rendering.Skia;

using KineticReports.Core.Visual;

/// <summary>
/// Adapts <see cref="VisualSkiaRenderer"/> to the generic <see cref="IVisualRenderer"/> contract for PDF output.
/// </summary>
public sealed class VisualPdfRendererAdapter : IVisualRenderer
{
    /// <inheritdoc/>
    public VisualRenderFormatDescriptor Format => new("pdf", "PDF", "application/pdf", "pdf");

    private readonly VisualSkiaRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="VisualPdfRendererAdapter"/>.
    /// </summary>
    public VisualPdfRendererAdapter(VisualSkiaRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    /// <inheritdoc/>
    public Task RenderAsync(VisualDocument visualDocument, Stream output, CancellationToken cancellationToken = default)
    {
        return _renderer.RenderPdfAsync(visualDocument, output, cancellationToken);
    }
}
