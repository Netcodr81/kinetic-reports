namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;

/// <summary>Identifies the geometric primitive drawn by a <see cref="ShapeElement"/>.</summary>
public enum ShapeKind
{
    /// <summary>An axis-aligned rectangle (optionally with rounded corners).</summary>
    Rectangle,

    /// <summary>An ellipse or circle.</summary>
    Ellipse,

    /// <summary>A straight line segment from one corner to the other of its bounds.</summary>
    Line,
}

/// <summary>
/// Represents a vector shape element (rectangle, ellipse, or line) in the report layout.
/// </summary>
public sealed class ShapeElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Shape;

    /// <summary>Gets the kind of shape to draw.</summary>
    public ShapeKind Kind { get; init; }

    /// <summary>
    /// Gets the fill color. <see langword="null"/> means the shape is not filled.
    /// </summary>
    public Color? Fill { get; init; }

    /// <summary>
    /// Gets the stroke (outline) color. <see langword="null"/> means no outline is drawn.
    /// </summary>
    public Color? Stroke { get; init; }

    /// <summary>Gets the stroke width in DIPs.</summary>
    public float StrokeWidth { get; init; } = 1f;

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        DesiredSize = availableSize;
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }
}
