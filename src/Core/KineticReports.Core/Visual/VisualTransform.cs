namespace KineticReports.Core.Visual;

/// <summary>
/// Represents a 2D affine transform applied to a visual element.
/// </summary>
public sealed record VisualTransform
{
    /// <summary>
    /// Gets matrix value M11.
    /// </summary>
    public float M11 { get; init; } = 1f;

    /// <summary>
    /// Gets matrix value M12.
    /// </summary>
    public float M12 { get; init; }

    /// <summary>
    /// Gets matrix value M21.
    /// </summary>
    public float M21 { get; init; }

    /// <summary>
    /// Gets matrix value M22.
    /// </summary>
    public float M22 { get; init; } = 1f;

    /// <summary>
    /// Gets X translation in DIPs.
    /// </summary>
    public float OffsetX { get; init; }

    /// <summary>
    /// Gets Y translation in DIPs.
    /// </summary>
    public float OffsetY { get; init; }
}
