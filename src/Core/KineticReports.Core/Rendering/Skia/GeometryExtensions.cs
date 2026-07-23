namespace KineticReports.Rendering.Skia.Extensions;

using KineticReports.Core.Geometry;
using SkiaSharp;

/// <summary>
/// Extension methods for converting geometry types to SkiaSharp equivalents.
/// </summary>
internal static class GeometryExtensions
{
    /// <summary>
    /// Converts a <see cref="Rect"/> to an <see cref="SKRect"/>, scaling by
    /// <paramref name="scale"/> (pixels-per-DIP).
    /// </summary>
    internal static SKRect ToSkia(this Rect rect, float scale = 1f)
        => new SKRect(
            rect.X * scale,
            rect.Y * scale,
            rect.Right * scale,
            rect.Bottom * scale);

    /// <summary>
    /// Converts a <see cref="Point"/> to an <see cref="SKPoint"/>, scaling by
    /// <paramref name="scale"/>.
    /// </summary>
    internal static SKPoint ToSkia(this Point point, float scale = 1f)
        => new SKPoint(point.X * scale, point.Y * scale);
}
