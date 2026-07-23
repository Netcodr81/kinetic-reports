namespace KineticReports.Core.Visual;

/// <summary>
/// Represents one hit-testable element entry.
/// </summary>
public sealed record VisualHitTestEntry
{
    /// <summary>
    /// Gets the page number containing the element.
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Gets the owning visual layer name.
    /// </summary>
    public required string LayerName { get; init; }

    /// <summary>
    /// Gets the visual element represented by this entry.
    /// </summary>
    public required VisualElement Element { get; init; }
}
