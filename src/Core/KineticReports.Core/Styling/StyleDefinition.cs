namespace KineticReports.Core.Styling;

using KineticReports.Core.Geometry;

/// <summary>
/// Defines a named, reusable style that can be referenced by report elements.
/// Properties that are <see langword="null"/> are inherited from the parent or theme.
/// </summary>
public sealed record StyleDefinition
{
    /// <summary>Gets or inits the unique identifier for this style.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the human-readable display name.</summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or inits the <see cref="Id"/> of the parent style this style inherits from.
    /// </summary>
    public string? BasedOn { get; init; }

    /// <summary>Gets or inits the typographic properties for this style.</summary>
    public Typography? Typography { get; init; }

    /// <summary>Gets or inits the border definition for this style.</summary>
    public Border? Border { get; init; }

    /// <summary>Gets or inits the background fill color.</summary>
    public Color? Background { get; init; }

    /// <summary>Gets or inits the margin (space outside the element border) in DIPs.</summary>
    public Thickness? Margin { get; init; }

    /// <summary>Gets or inits the padding (space inside the element border) in DIPs.</summary>
    public Thickness? Padding { get; init; }

    /// <summary>Gets or inits the element opacity in the range [0.0, 1.0].</summary>
    public float? Opacity { get; init; }

    /// <summary>Gets or inits the overflow behavior for content that exceeds the element bounds.</summary>
    public Overflow? Overflow { get; init; }
}
