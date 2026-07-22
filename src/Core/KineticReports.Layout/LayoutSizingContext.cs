namespace KineticReports.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Typography;

/// <summary>
/// Default implementation of <see cref="ILayoutSizingContext"/> used by the layout engine.
/// Combines a text layout provider with an optional image size resolver.
/// </summary>
public sealed class LayoutSizingContext : ILayoutSizingContext
{
    private readonly Func<string, Size?>? _imageResolver;

    /// <summary>
    /// Initialises a new <see cref="LayoutSizingContext"/>.
    /// </summary>
    /// <param name="textLayout">The text layout provider used during the LayoutSizing pass.</param>
    /// <param name="imageResolver">
    /// Optional delegate that resolves the intrinsic size of a named image.
    /// Return <see langword="null"/> when the image is unknown or its size is unavailable.
    /// </param>
    public LayoutSizingContext(ITextLayout textLayout, Func<string, Size?>? imageResolver = null)
    {
        TextLayout = textLayout;
        _imageResolver = imageResolver;
    }

    /// <inheritdoc/>
    public ITextLayout TextLayout { get; }

    /// <inheritdoc/>
    public Size? ResolveImageSize(string imageKey) => _imageResolver?.Invoke(imageKey);
}
