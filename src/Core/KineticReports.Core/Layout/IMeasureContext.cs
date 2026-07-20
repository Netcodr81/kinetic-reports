namespace KineticReports.Core.Layout;

using KineticReports.Core.Typography;

/// <summary>
/// Provides services required during the Measure pass, including font metrics
/// and image size resolution.
/// </summary>
public interface IMeasureContext
{
    /// <summary>Gets the font metrics provider used to measure text glyphs.</summary>
    IFontMetrics FontMetrics { get; }

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
