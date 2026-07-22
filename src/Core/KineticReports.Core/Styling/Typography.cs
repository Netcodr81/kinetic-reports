namespace KineticReports.Core.Styling;

/// <summary>
/// Describes the typographic properties of a layout element.
/// All properties are nullable — a <see langword="null"/> value means "inherit from parent."
/// </summary>
public sealed record Typography
{
    /// <summary>Gets or inits the font family name (e.g. "Inter", "Arial").</summary>
    public string? Family { get; init; }

    /// <summary>Gets or inits the font size in points.</summary>
    public float? Size { get; init; }

    /// <summary>Gets or inits the font weight.</summary>
    public FontWeight? Weight { get; init; }

    /// <summary>Gets or inits the font style (normal, italic, oblique).</summary>
    public FontStyleValue? Style { get; init; }

    /// <summary>Gets or inits the line height multiplier relative to the font size.</summary>
    public float? LineHeight { get; init; }

    /// <summary>Gets or inits the letter spacing in DIPs.</summary>
    public float? LetterSpacing { get; init; }

    /// <summary>Gets or inits the foreground text color.</summary>
    public Color? Color { get; init; }

    /// <summary>Gets or inits the horizontal text alignment.</summary>
    public TextAlignment? Alignment { get; init; }

    /// <summary>Gets or inits the vertical text alignment within the element.</summary>
    public VerticalAlignment? VerticalAlignment { get; init; }

    /// <summary>Gets or inits the text decoration flags.</summary>
    public TextDecoration? Decoration { get; init; }
}
