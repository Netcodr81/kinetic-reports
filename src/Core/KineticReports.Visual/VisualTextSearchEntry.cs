namespace KineticReports.Visual;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents one searchable visual text entry.
/// </summary>
public sealed record VisualTextSearchEntry
{
    /// <summary>
    /// Gets the one-based page number containing this text.
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Gets the owning visual layer name.
    /// </summary>
    public required string LayerName { get; init; }

    /// <summary>
    /// Gets the source visual element id.
    /// </summary>
    public required string ElementId { get; init; }

    /// <summary>
    /// Gets the text bounds in page-local DIPs.
    /// </summary>
    public required Rect Bounds { get; init; }

    /// <summary>
    /// Gets the searchable text.
    /// </summary>
    public required string Text { get; init; }
}
