namespace KineticReports.Core.Geometry;

/// <summary>
/// Represents an axis-aligned rectangle in device-independent pixel (DIP) space.
/// One DIP equals 1/96 of an inch (ADR-014).
/// </summary>
/// <param name="X">The left edge coordinate in DIPs.</param>
/// <param name="Y">The top edge coordinate in DIPs.</param>
/// <param name="Width">The horizontal extent in DIPs. Should be non-negative.</param>
/// <param name="Height">The vertical extent in DIPs. Should be non-negative.</param>
public readonly record struct Rect(float X, float Y, float Width, float Height)
{
    /// <summary>Gets an empty rectangle at the origin with zero dimensions.</summary>
    public static readonly Rect Empty = new(0f, 0f, 0f, 0f);

    /// <summary>Gets the right edge coordinate (X + Width).</summary>
    public float Right => X + Width;

    /// <summary>Gets the bottom edge coordinate (Y + Height).</summary>
    public float Bottom => Y + Height;

    /// <summary>Gets the top-left corner as a <see cref="Point"/>.</summary>
    public Point TopLeft => new(X, Y);

    /// <summary>Gets the size of the rectangle.</summary>
    public Size Size => new(Width, Height);

    /// <summary>Gets a value indicating whether the rectangle has zero area.</summary>
    public bool IsEmpty => Width <= 0f || Height <= 0f;

    /// <summary>
    /// Creates a rectangle from two corner points.
    /// </summary>
    /// <param name="topLeft">The top-left corner.</param>
    /// <param name="bottomRight">The bottom-right corner.</param>
    /// <returns>A new <see cref="Rect"/> spanning the two points.</returns>
    public static Rect FromPoints(Point topLeft, Point bottomRight) =>
        new(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

    /// <summary>
    /// Returns a new rectangle shrunk inward by the specified <paramref name="thickness"/>.
    /// </summary>
    /// <param name="thickness">The amount to deflate on each side.</param>
    /// <returns>A new deflated <see cref="Rect"/>.</returns>
    public Rect Deflate(Thickness thickness) =>
        new(
            X + thickness.Left,
            Y + thickness.Top,
            Width - thickness.Horizontal,
            Height - thickness.Vertical);

    /// <summary>
    /// Returns a new rectangle expanded outward by the specified <paramref name="thickness"/>.
    /// </summary>
    /// <param name="thickness">The amount to inflate on each side.</param>
    /// <returns>A new inflated <see cref="Rect"/>.</returns>
    public Rect Inflate(Thickness thickness) =>
        new(
            X - thickness.Left,
            Y - thickness.Top,
            Width + thickness.Horizontal,
            Height + thickness.Vertical);

    /// <summary>
    /// Determines whether this rectangle contains the specified point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <returns><see langword="true"/> if <paramref name="p"/> is inside or on the boundary.</returns>
    public bool Contains(Point p) =>
        p.X >= X && p.X <= Right && p.Y >= Y && p.Y <= Bottom;

    /// <inheritdoc/>
    public override string ToString() => $"[{X}, {Y}, {Width} \u00d7 {Height}]";
}
