namespace KineticReports.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Typography;

/// <summary>
/// Default implementation of <see cref="ILayoutSizingContext"/> used by the layout engine.
/// Combines a font metrics provider with an optional image size resolver.
/// </summary>
public sealed class LayoutSizingContext : ILayoutSizingContext
{
    private readonly Func<string, Size?>? _imageResolver;

    /// <summary>
    /// Initialises a new <see cref="LayoutSizingContext"/>.
    /// </summary>
    /// <param name="fontMetrics">The font metrics provider used during the LayoutSizing pass.</param>
    /// <param name="imageResolver">
    /// Optional delegate that resolves the intrinsic size of a named image.
    /// Return <see langword="null"/> when the image is unknown or its size is unavailable.
    /// </param>
    public LayoutSizingContext(IFontMetrics fontMetrics, Func<string, Size?>? imageResolver = null)
    {
        FontMetrics = fontMetrics;
        _imageResolver = imageResolver;
    }

    /// <inheritdoc/>
    public IFontMetrics FontMetrics { get; }

    /// <inheritdoc/>
    public Size? ResolveImageSize(string imageKey) => _imageResolver?.Invoke(imageKey);
}
