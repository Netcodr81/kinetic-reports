namespace KineticReports.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Typography;

/// <summary>
/// Default implementation of <see cref="IMeasureContext"/> used by the layout engine.
/// Combines a font metrics provider with an optional image size resolver.
/// </summary>
public sealed class MeasureContext : IMeasureContext
{
    private readonly Func<string, Size?>? _imageResolver;

    /// <summary>
    /// Initialises a new <see cref="MeasureContext"/>.
    /// </summary>
    /// <param name="fontMetrics">The font metrics provider used during the Measure pass.</param>
    /// <param name="imageResolver">
    /// Optional delegate that resolves the intrinsic size of a named image.
    /// Return <see langword="null"/> when the image is unknown or its size is unavailable.
    /// </param>
    public MeasureContext(IFontMetrics fontMetrics, Func<string, Size?>? imageResolver = null)
    {
        FontMetrics = fontMetrics;
        _imageResolver = imageResolver;
    }

    /// <inheritdoc/>
    public IFontMetrics FontMetrics { get; }

    /// <inheritdoc/>
    public Size? ResolveImageSize(string imageKey) => _imageResolver?.Invoke(imageKey);
}
