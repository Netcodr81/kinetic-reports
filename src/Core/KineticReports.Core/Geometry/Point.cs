namespace KineticReports.Core.Geometry;

/// <summary>
/// Represents a two-dimensional point in device-independent pixel (DIP) space.
/// One DIP equals 1/96 of an inch (ADR-014).
/// </summary>
/// <param name="X">The horizontal coordinate in DIPs.</param>
/// <param name="Y">The vertical coordinate in DIPs.</param>
public readonly record struct Point(float X, float Y)
{
    /// <summary>Gets the origin point (0, 0).</summary>
    public static readonly Point Zero = new(0f, 0f);

    /// <summary>
    /// Returns a new point translated by the specified delta values.
    /// </summary>
    /// <param name="dx">Horizontal delta in DIPs.</param>
    /// <param name="dy">Vertical delta in DIPs.</param>
    /// <returns>A new <see cref="Point"/> offset by (dx, dy).</returns>
    public Point Translate(float dx, float dy) => new(X + dx, Y + dy);

    /// <inheritdoc/>
    public override string ToString() => $"({X}, {Y})";
}
