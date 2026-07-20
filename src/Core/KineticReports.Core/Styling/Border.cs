namespace KineticReports.Core.Styling;

/// <summary>
/// Describes the four-sided border of a layout element.
/// A <see langword="null"/> side means no border is drawn on that edge.
/// </summary>
public sealed record Border
{
    /// <summary>Gets or inits the top border side.</summary>
    public BorderSide? Top { get; init; }

    /// <summary>Gets or inits the right border side.</summary>
    public BorderSide? Right { get; init; }

    /// <summary>Gets or inits the bottom border side.</summary>
    public BorderSide? Bottom { get; init; }

    /// <summary>Gets or inits the left border side.</summary>
    public BorderSide? Left { get; init; }

    /// <summary>Gets or inits the corner radius applied to all corners, in DIPs.</summary>
    public float CornerRadius { get; init; }

    /// <summary>
    /// Creates a border with the same style on all four sides.
    /// </summary>
    /// <param name="width">The border width in DIPs.</param>
    /// <param name="color">The border color.</param>
    /// <param name="style">The line style. Defaults to <see cref="BorderLineStyle.Solid"/>.</param>
    /// <returns>A new <see cref="Border"/> with uniform sides.</returns>
    public static Border Uniform(
        float width,
        Color color,
        BorderLineStyle style = BorderLineStyle.Solid)
    {
        var side = new BorderSide(width, color, style);
        return new Border { Top = side, Right = side, Bottom = side, Left = side };
    }
}
