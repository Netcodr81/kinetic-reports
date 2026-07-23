namespace KineticReports.Core.Visual;

/// <summary>
/// Represents a vector shape drawing primitive.
/// </summary>
public sealed record VisualShape : VisualElement
{
    /// <summary>
    /// Gets the shape kind.
    /// </summary>
    public VisualShapeKind Kind { get; init; } = VisualShapeKind.Rectangle;

    /// <summary>
    /// Gets an optional fill color in hex format.
    /// </summary>
    public string? FillColorHex { get; init; }

    /// <summary>
    /// Gets an optional stroke color in hex format.
    /// </summary>
    public string? StrokeColorHex { get; init; }

    /// <summary>
    /// Gets the stroke width in DIPs.
    /// </summary>
    public float StrokeWidth { get; init; } = 1f;
}

/// <summary>
/// Enumerates supported vector shape kinds.
/// </summary>
public enum VisualShapeKind
{
    /// <summary>
    /// Rectangle shape.
    /// </summary>
    Rectangle,

    /// <summary>
    /// Ellipse shape.
    /// </summary>
    Ellipse,

    /// <summary>
    /// Line shape.
    /// </summary>
    Line,

    /// <summary>
    /// Arbitrary path shape.
    /// </summary>
    Path
}
