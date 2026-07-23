namespace KineticReports.Core.Engine.Data;

using KineticReports.Core.Definition;

/// <summary>
/// Holds all data resolved from every data source for a single report execution.
/// </summary>
public sealed class DataContext
{
    /// <summary>
    /// Gets or inits the report definition being executed, when available.
    /// </summary>
    public ReportDefinition? Definition { get; init; }

    /// <summary>
    /// Gets the resolved rows keyed by data-source ID.
    /// Each value is an ordered list of rows; each row maps field name to value.
    /// </summary>
    public required IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>> DataSources { get; init; }

    /// <summary>
    /// Returns the rows for the specified data-source ID, or an empty list when
    /// the data source is not present in this context.
    /// </summary>
    /// <param name="dataSourceId">The data-source ID to look up.</param>
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> GetRows(string dataSourceId)
        => DataSources.TryGetValue(dataSourceId, out var rows) ? rows : [];
}
