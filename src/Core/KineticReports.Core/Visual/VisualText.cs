namespace KineticReports.Visual;

/// <summary>
/// Represents a text drawing primitive.
/// </summary>
public sealed record VisualText : VisualElement
{
    /// <summary>
    /// Gets the rendered text.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the font family.
    /// </summary>
    public string FontFamily { get; init; } = "Arial";

    /// <summary>
    /// Gets the font size in DIPs.
    /// </summary>
    public float FontSize { get; init; } = 12f;

    /// <summary>
    /// Gets an optional font weight token.
    /// </summary>
    public string? FontWeight { get; init; }

    /// <summary>
    /// Gets an optional text color in hex format.
    /// </summary>
    public string? TextColorHex { get; init; }
}
