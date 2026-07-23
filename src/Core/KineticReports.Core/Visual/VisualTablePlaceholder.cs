namespace KineticReports.Visual;

/// <summary>
/// Represents a table-like visual element placeholder for phase-1 mapping.
/// </summary>
public sealed record VisualTablePlaceholder : VisualElement
{
    /// <summary>
    /// Gets the optional source data identifier.
    /// </summary>
    public string? DataSourceId { get; init; }

    /// <summary>
    /// Gets the optional estimated row count.
    /// </summary>
    public int? RowCount { get; init; }

    /// <summary>
    /// Gets the optional estimated column count.
    /// </summary>
    public int? ColumnCount { get; init; }
}
