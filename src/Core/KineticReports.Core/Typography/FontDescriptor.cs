namespace KineticReports.Core.Typography;

using KineticReports.Core.Styling;

/// <summary>
/// Describes a font face used for text measurement and rendering.
/// </summary>
/// <param name="Family">The font family name (e.g. "Inter", "Arial").</param>
/// <param name="Size">The font size in points.</param>
/// <param name="Weight">The font weight. Defaults to <see cref="FontWeight.Normal"/>.</param>
/// <param name="Style">The font style. Defaults to <see cref="FontStyleValue.Normal"/>.</param>
public sealed record FontDescriptor(
    string Family,
    float Size,
    FontWeight Weight = FontWeight.Normal,
    FontStyleValue Style = FontStyleValue.Normal);
