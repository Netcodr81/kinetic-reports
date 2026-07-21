namespace KineticReports.Core.Geometry;

/// <summary>
/// Represents a two-dimensional size (width × height) in device-independent pixels (DIPs).
/// One DIP equals 1/96 of an inch (ADR-014).
/// </summary>
/// <param name="Width">The horizontal extent in DIPs. Should be non-negative.</param>
/// <param name="Height">The vertical extent in DIPs. Should be non-negative.</param>
public readonly record struct Size(float Width, float Height)
{
    /// <summary>Gets a zero-area size (0 × 0).</summary>
    public static readonly Size Zero = new(0f, 0f);

    /// <summary>
    /// Gets a size with positive-infinity dimensions, used during unconstrained LayoutSizing passes.
    /// </summary>
    public static readonly Size Infinity = new(float.PositiveInfinity, float.PositiveInfinity);

    /// <summary>Gets a value indicating whether both dimensions are zero.</summary>
    public bool IsEmpty => Width == 0f && Height == 0f;

    /// <summary>
    /// Returns a new size inflated by the specified deltas.
    /// </summary>
    /// <param name="dw">Amount to add to the width.</param>
    /// <param name="dh">Amount to add to the height.</param>
    /// <returns>A new inflated <see cref="Size"/>.</returns>
    public Size Inflate(float dw, float dh) => new(Width + dw, Height + dh);

    /// <inheritdoc/>
    public override string ToString() => $"{Width} \u00d7 {Height}";
}
