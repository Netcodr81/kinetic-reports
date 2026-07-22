namespace KineticReports.Core.Rendering;

using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;

/// <summary>Identifies the kind of operation in a <see cref="PathCommand"/>.</summary>
public enum PathCommandKind
{
    /// <summary>Move the current point to the specified location without drawing.</summary>
    MoveTo,

    /// <summary>Draw a straight line from the current point to the specified point.</summary>
    LineTo,

    /// <summary>
    /// Draw a cubic Bézier curve using two control points and an end point.
    /// </summary>
    CubicBezierTo,

    /// <summary>Close the current subpath by drawing a line back to the start point.</summary>
    Close,
}

/// <summary>
/// Represents a single drawing command within a <see cref="PathGeometry"/>.
/// </summary>
/// <param name="Kind">The command type.</param>
/// <param name="Points">
/// The control points for this command.
/// <list type="bullet">
///   <item><see cref="PathCommandKind.MoveTo"/> / <see cref="PathCommandKind.LineTo"/>: 1 point (destination).</item>
///   <item><see cref="PathCommandKind.CubicBezierTo"/>: 3 points (cp1, cp2, end).</item>
///   <item><see cref="PathCommandKind.Close"/>: 0 points.</item>
/// </list>
/// </param>
public sealed record PathCommand(PathCommandKind Kind, IReadOnlyList<Point> Points);

/// <summary>
/// Describes a vector path for rendering.
/// Paths are produced by the layout engine and consumed by renderers to draw
/// shapes, borders, and decorations.
/// </summary>
public sealed record PathGeometry
{
    /// <summary>Gets the sequence of drawing commands that define the path.</summary>
    public required IReadOnlyList<PathCommand> Commands { get; init; }

    /// <summary>
    /// Gets the fill color.
    /// <see langword="null"/> means the path interior is not filled.
    /// </summary>
    public Color? Fill { get; init; }

    /// <summary>
    /// Gets the stroke color.
    /// <see langword="null"/> means no stroke is drawn.
    /// </summary>
    public Color? Stroke { get; init; }

    /// <summary>Gets the stroke width in DIPs.</summary>
    public float StrokeWidth { get; init; } = 1f;
}
