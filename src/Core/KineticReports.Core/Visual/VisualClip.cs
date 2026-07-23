namespace KineticReports.Core.Visual;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a clipping rectangle applied to a visual element.
/// </summary>
public sealed record VisualClip
{
    /// <summary>
    /// Gets the clipping bounds in DIPs.
    /// </summary>
    public required Rect Bounds { get; init; }
}
