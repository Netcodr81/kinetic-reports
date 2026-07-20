namespace KineticReports.Rendering.Skia.Extensions;

using KineticReports.Core.Styling;
using SkiaSharp;

/// <summary>
/// Extension methods for converting <see cref="Color"/> to SkiaSharp types.
/// </summary>
internal static class ColorExtensions
{
    /// <summary>Converts a <see cref="Color"/> to an <see cref="SKColor"/>.</summary>
    internal static SKColor ToSkia(this Color color)
        => new SKColor(color.R, color.G, color.B, color.A);
}
