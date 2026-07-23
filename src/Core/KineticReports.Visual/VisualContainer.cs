namespace KineticReports.Visual;

/// <summary>
/// Represents a visual grouping element that owns child nodes.
/// </summary>
public sealed record VisualContainer : VisualElement
{
    /// <summary>
    /// Gets the ordered child elements.
    /// </summary>
    public IReadOnlyList<VisualElement> Children { get; init; } = [];
}
