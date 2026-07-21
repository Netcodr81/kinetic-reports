namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;

/// <summary>Specifies how an image is scaled within the bounds of an <see cref="ImageElement"/>.</summary>
public enum ImageStretch
{
    /// <summary>The image is drawn at its natural (intrinsic) size.</summary>
    None,

    /// <summary>The image is stretched to exactly fill the bounds, ignoring the aspect ratio.</summary>
    Fill,

    /// <summary>The image is scaled uniformly to fit within the bounds, preserving aspect ratio.</summary>
    Uniform,

    /// <summary>
    /// The image is scaled uniformly to fill the bounds, cropping any overflow
    /// while preserving the aspect ratio.
    /// </summary>
    UniformToFill,
}

/// <summary>
/// Represents an image element in the report layout.
/// </summary>
public sealed class ImageElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Image;

    /// <summary>Gets the source key or URI that identifies the image asset.</summary>
    public required string SourceKey { get; init; }

    /// <summary>Gets the scaling mode for the image within its bounds.</summary>
    public ImageStretch Stretch { get; init; } = ImageStretch.Uniform;

    /// <summary>
    /// Gets the resolved image reference set by the layout engine.
    /// Populated after <see cref="Measure"/> is called; <see langword="null"/> before then.
    /// </summary>
    public ImageReference? ImageReference { get; private set; }

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        var intrinsic = context.ResolveImageSize(SourceKey);
        DesiredSize = intrinsic ?? availableSize;
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }

    /// <summary>
    /// Sets the resolved image reference. Called internally by the layout engine.
    /// </summary>
    /// <param name="reference">The resolved image data.</param>
    internal void SetImageReference(ImageReference reference) => ImageReference = reference;
}
