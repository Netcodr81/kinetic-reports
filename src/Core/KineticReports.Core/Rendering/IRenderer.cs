namespace KineticReports.Core.Rendering;

using KineticReports.Core.Layout;

/// <summary>
/// Defines the contract for a renderer that converts a <see cref="ReportDocument"/> into
/// an output document (e.g. PDF, raster image, HTML stream).
/// </summary>
/// <remarks>
/// Renderers receive an immutable <see cref="ReportDocument"/> and must never perform
/// any layout calculations. All measurement, text wrapping, and pagination are
/// completed upstream by the layout engine before the renderer is invoked (ADR-011, ADR-013).
/// </remarks>
public interface IRenderer
{
    /// <summary>
    /// Asynchronously renders the complete report document to the specified
    /// <paramref name="output"/> stream.
    /// </summary>
    /// <param name="reportDocument">The immutable report document to render.</param>
    /// <param name="output">The writable destination stream.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the render operation.
    /// </param>
    /// <returns>A <see cref="Task"/> that completes when rendering is finished.</returns>
    Task RenderAsync(ReportDocument reportDocument, Stream output, CancellationToken cancellationToken = default);
}
