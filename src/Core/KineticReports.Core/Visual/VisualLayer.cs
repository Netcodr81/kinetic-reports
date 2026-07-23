namespace KineticReports.Core.Visual;

/// <summary>
/// Represents an ordered logical layer of visual elements.
/// </summary>
public sealed record VisualLayer
{
    /// <summary>
    /// Gets the layer name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the ordered elements in this layer.
    /// </summary>
    public IReadOnlyList<VisualElement> Elements { get; init; } = [];
}
