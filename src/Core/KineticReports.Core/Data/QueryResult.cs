namespace KineticReports.Core.Data;

/// <summary>
/// Represents the result of executing a data provider query.
/// </summary>
public sealed record QueryResult
{
    /// <summary>Gets the schema (column definitions) of the result set.</summary>
    public required IReadOnlyList<ColumnSchema> Schema { get; init; }

    /// <summary>Gets the rows returned by the query.</summary>
    public required IReadOnlyList<DataRow> Rows { get; init; }

    /// <summary>Gets the total number of rows in the result (may differ from Rows.Count if streaming/paginated).</summary>
    public int RowCount { get; init; }

    /// <summary>Gets diagnostic information about the query execution.</summary>
    public string? DiagnosticsMessage { get; init; }

    /// <summary>Gets the execution time in milliseconds.</summary>
    public long ExecutionTimeMs { get; init; }
}
