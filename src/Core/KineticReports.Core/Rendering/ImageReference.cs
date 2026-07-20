namespace KineticReports.Core.Rendering;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a decoded image that is ready for rendering.
/// The renderer receives an <see cref="ImageReference"/> and must not perform
/// any additional loading or decoding.
/// </summary>
public sealed record ImageReference
{
    /// <summary>Gets the source key or URI that identifies this image asset.</summary>
    public required string SourceKey { get; init; }

    /// <summary>Gets the intrinsic (natural) size of the image in DIPs.</summary>
    public required Size IntrinsicSize { get; init; }

    /// <summary>Gets the pixel width of the decoded image.</summary>
    public required int PixelWidth { get; init; }

    /// <summary>Gets the pixel height of the decoded image.</summary>
    public required int PixelHeight { get; init; }

    /// <summary>Gets the pixel format descriptor (e.g. "RGBA8888", "RGB888").</summary>
    public required string PixelFormat { get; init; }

    /// <summary>
    /// Gets the raw pixel data in the format described by <see cref="PixelFormat"/>.
    /// </summary>
    public required byte[] PixelData { get; init; }
}
