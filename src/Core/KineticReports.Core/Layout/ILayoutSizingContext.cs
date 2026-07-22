namespace KineticReports.Core.Layout;

using KineticReports.Core.Typography;

/// <summary>
/// Provides services required during the LayoutSizing pass, including text layout
/// and image size resolution.
/// </summary>
public interface ILayoutSizingContext
{
    /// <summary>Gets the text layout provider used to measure and shape text.</summary>
    ITextLayout TextLayout { get; }

    /// <summary>
    /// Resolves the intrinsic (natural) pixel size of an image identified by
    /// <paramref name="imageKey"/>, converted to DIPs.
    /// </summary>
    /// <param name="imageKey">The image source key or URI.</param>
    /// <returns>
    /// The image size in DIPs, or <see langword="null"/> if the image
    /// cannot be located or decoded.
    /// </returns>
    Geometry.Size? ResolveImageSize(string imageKey);
}
