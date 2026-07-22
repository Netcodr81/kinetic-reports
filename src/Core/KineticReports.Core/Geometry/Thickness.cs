namespace KineticReports.Core.Geometry;

/// <summary>
/// Represents a set of four thickness values (left, top, right, bottom) used for
/// margins, padding, and border widths. All values are in device-independent pixels (DIPs).
/// </summary>
/// <param name="Left">The left thickness in DIPs.</param>
/// <param name="Top">The top thickness in DIPs.</param>
/// <param name="Right">The right thickness in DIPs.</param>
/// <param name="Bottom">The bottom thickness in DIPs.</param>
public readonly record struct Thickness(float Left, float Top, float Right, float Bottom)
{
    /// <summary>Gets a zero-thickness value (all sides are 0).</summary>
    public static readonly Thickness Zero = new(0f, 0f, 0f, 0f);

    /// <summary>
    /// Creates a uniform thickness where all four sides have the same value.
    /// </summary>
    /// <param name="uniform">The value applied to all four sides.</param>
    public Thickness(float uniform) : this(uniform, uniform, uniform, uniform) { }

    /// <summary>
    /// Creates a thickness with symmetric horizontal and vertical values.
    /// </summary>
    /// <param name="horizontal">Applied to the left and right sides.</param>
    /// <param name="vertical">Applied to the top and bottom sides.</param>
    public Thickness(float horizontal, float vertical)
        : this(horizontal, vertical, horizontal, vertical) { }

    /// <summary>Gets the total horizontal thickness (Left + Right).</summary>
    public float Horizontal => Left + Right;

    /// <summary>Gets the total vertical thickness (Top + Bottom).</summary>
    public float Vertical => Top + Bottom;

    /// <inheritdoc/>
    public override string ToString() => $"L={Left} T={Top} R={Right} B={Bottom}";
}
