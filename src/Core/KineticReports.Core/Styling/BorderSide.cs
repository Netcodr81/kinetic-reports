namespace KineticReports.Core.Styling;

/// <summary>Describes the appearance of a single border side.</summary>
/// <param name="Width">The border width in DIPs.</param>
/// <param name="Color">The border color.</param>
/// <param name="Style">The line style. Defaults to <see cref="BorderLineStyle.Solid"/>.</param>
public sealed record BorderSide(
    float Width,
    Color Color,
    BorderLineStyle Style = BorderLineStyle.Solid);
