namespace KineticReports.Core.Visual;

/// <summary>
/// Represents one page of a <see cref="VisualDocument"/>.
/// </summary>
public sealed record VisualPage
{
    /// <summary>
    /// Gets the one-based page number.
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Gets the page width in DIPs.
    /// </summary>
    public required float Width { get; init; }

    /// <summary>
    /// Gets the page height in DIPs.
    /// </summary>
    public required float Height { get; init; }

    /// <summary>
    /// Gets the ordered page layers.
    /// </summary>
    public IReadOnlyList<VisualLayer> Layers { get; init; } = [];

    /// <summary>
    /// Gets optional diagnostic metadata for this page.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
