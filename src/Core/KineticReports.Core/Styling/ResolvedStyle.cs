namespace KineticReports.Core.Styling;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a fully resolved, immutable style produced by the style cascade.
/// The layout engine and renderers consume only <see cref="ResolvedStyle"/> instances.
/// </summary>
/// <remarks>
/// Style resolution order (spec §10):
/// Theme → Report Defaults → Named Style → Parent Inheritance → Local Override → ResolvedStyle.
/// </remarks>
public sealed record ResolvedStyle
{
    /// <summary>Gets the resolved font family name.</summary>
    public required string FontFamily { get; init; }

    /// <summary>Gets the resolved font size in points.</summary>
    public required float FontSize { get; init; }

    /// <summary>Gets the resolved font weight.</summary>
    public FontWeight FontWeight { get; init; } = FontWeight.Normal;

    /// <summary>Gets the resolved font style.</summary>
    public FontStyleValue FontStyle { get; init; } = FontStyleValue.Normal;

    /// <summary>Gets the resolved line height multiplier (relative to font size).</summary>
    public float LineHeight { get; init; } = 1.2f;

    /// <summary>Gets the resolved letter spacing in DIPs.</summary>
    public float LetterSpacing { get; init; }

    /// <summary>Gets the resolved foreground text color.</summary>
    public Color TextColor { get; init; } = Color.Black;

    /// <summary>Gets the resolved horizontal text alignment.</summary>
    public TextAlignment TextAlignment { get; init; } = TextAlignment.Left;

    /// <summary>Gets the resolved vertical text alignment.</summary>
    public VerticalAlignment VerticalAlignment { get; init; } = VerticalAlignment.Top;

    /// <summary>Gets the resolved text decoration flags.</summary>
    public TextDecoration TextDecoration { get; init; } = TextDecoration.None;

    /// <summary>Gets the resolved border, or <see langword="null"/> if no border is applied.</summary>
    public Border? Border { get; init; }

    /// <summary>Gets the resolved background fill color.</summary>
    public Color Background { get; init; } = Color.Transparent;

    /// <summary>Gets the resolved margin (outside the border) in DIPs.</summary>
    public Thickness Margin { get; init; } = Thickness.Zero;

    /// <summary>Gets the resolved padding (inside the border) in DIPs.</summary>
    public Thickness Padding { get; init; } = Thickness.Zero;

    /// <summary>Gets the resolved element opacity in the range [0.0, 1.0].</summary>
    public float Opacity { get; init; } = 1f;

    /// <summary>Gets the resolved overflow behavior.</summary>
    public Overflow Overflow { get; init; } = Overflow.Hidden;
}
