namespace KineticReports.Core.Layout;

/// <summary>
/// Represents immutable metadata associated with a <see cref="ReportDocument"/>.
/// </summary>
public sealed record ReportDocumentMetadata
{
    /// <summary>
    /// Gets or inits the report identifier that produced this document.
    /// </summary>
    public string? ReportId { get; init; }

    /// <summary>
    /// Gets or inits the report display name that produced this document.
    /// </summary>
    public string? ReportName { get; init; }

    /// <summary>
    /// Gets or inits the preferred file name stem for exports (without extension).
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets or inits additional metadata values.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Properties { get; init; } = new Dictionary<string, object?>();
}
