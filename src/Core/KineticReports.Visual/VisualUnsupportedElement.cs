namespace KineticReports.Visual;

/// <summary>
/// Represents a layout block that has no first-class visual mapping in the current phase.
/// </summary>
public sealed record VisualUnsupportedElement : VisualElement
{
    /// <summary>
    /// Gets the source layout block type name.
    /// </summary>
    public required string SourceType { get; init; }

    /// <summary>
    /// Gets a human-readable reason for the fallback.
    /// </summary>
    public required string Reason { get; init; }
}
