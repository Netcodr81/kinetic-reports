namespace KineticReports.Core.Styling;

/// <summary>
/// Specifies the stylistic variant of a font face.
/// Named <c>FontStyleValue</c> to avoid ambiguity with <c>System.Drawing.FontStyle</c>.
/// </summary>
public enum FontStyleValue
{
    /// <summary>The font is drawn upright (normal).</summary>
    Normal,

    /// <summary>The font is drawn in its italic variant.</summary>
    Italic,

    /// <summary>The font is artificially slanted (oblique).</summary>
    Oblique,
}
