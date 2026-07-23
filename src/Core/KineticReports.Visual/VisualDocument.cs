namespace KineticReports.Visual;

/// <summary>
/// Represents an immutable visual scene graph document ready for renderer consumption.
/// </summary>
public sealed record VisualDocument
{
    /// <summary>
    /// Gets the schema version of the visual document payload.
    /// </summary>
    public string SchemaVersion { get; init; } = "1.0";

    /// <summary>
    /// Gets the ordered pages that make up this visual document.
    /// </summary>
    public IReadOnlyList<VisualPage> Pages { get; init; } = [];

    /// <summary>
    /// Gets optional diagnostic metadata for this document.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
