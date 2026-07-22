namespace KineticReports.Core.Styling;

/// <summary>
/// Represents an ARGB color. All channel values are in the range [0, 255].
/// </summary>
/// <param name="A">Alpha channel (0 = fully transparent, 255 = fully opaque).</param>
/// <param name="R">Red channel.</param>
/// <param name="G">Green channel.</param>
/// <param name="B">Blue channel.</param>
public readonly record struct Color(byte A, byte R, byte G, byte B)
{
    /// <summary>Gets a fully transparent color (alpha = 0).</summary>
    public static readonly Color Transparent = new(0, 0, 0, 0);

    /// <summary>Gets opaque black (#FF000000).</summary>
    public static readonly Color Black = new(255, 0, 0, 0);

    /// <summary>Gets opaque white (#FFFFFFFF).</summary>
    public static readonly Color White = new(255, 255, 255, 255);

    /// <summary>
    /// Creates a fully opaque color from RGB components.
    /// </summary>
    /// <param name="r">Red channel (0–255).</param>
    /// <param name="g">Green channel (0–255).</param>
    /// <param name="b">Blue channel (0–255).</param>
    /// <returns>A new opaque <see cref="Color"/>.</returns>
    public static Color FromRgb(byte r, byte g, byte b) => new(255, r, g, b);

    /// <summary>
    /// Creates a color from a packed ARGB integer in the format 0xAARRGGBB.
    /// </summary>
    /// <param name="argb">The packed ARGB value.</param>
    /// <returns>A new <see cref="Color"/>.</returns>
    public static Color FromArgb(uint argb) =>
        new((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);

    /// <summary>
    /// Returns this color as a packed ARGB integer in the format 0xAARRGGBB.
    /// </summary>
    /// <returns>The packed ARGB value.</returns>
    public uint ToArgb() => ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | B;

    /// <inheritdoc/>
    public override string ToString() => $"#{A:X2}{R:X2}{G:X2}{B:X2}";
}
