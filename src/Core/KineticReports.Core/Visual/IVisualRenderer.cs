namespace KineticReports.Visual;

/// <summary>
/// Renders a <see cref="VisualDocument"/> into one concrete output format.
/// </summary>
public interface IVisualRenderer
{
    /// <summary>
    /// Gets format metadata produced by this renderer.
    /// </summary>
    VisualRenderFormatDescriptor Format { get; }

    /// <summary>
    /// Renders a visual document to the provided output stream.
    /// </summary>
    /// <param name="visualDocument">Visual document to render.</param>
    /// <param name="output">Writable destination stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RenderAsync(VisualDocument visualDocument, Stream output, CancellationToken cancellationToken = default);
}
