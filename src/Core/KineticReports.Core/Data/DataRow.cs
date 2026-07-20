namespace KineticReports.Core.Data;

/// <summary>
/// Represents a single row returned from a data provider query.
/// </summary>
public sealed record DataRow
{
    /// <summary>Gets the column values in ordinal order.</summary>
    public required IReadOnlyList<object?> Values { get; init; }

    /// <summary>Gets the value at the specified ordinal position.</summary>
    public object? this[int ordinal] => ordinal >= 0 && ordinal < Values.Count ? Values[ordinal] : null;

    /// <summary>Gets the value for the specified column name (case-insensitive).</summary>
    public object? GetValue(IReadOnlyList<ColumnSchema> schema, string columnName)
    {
        var column = schema.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        return column == null ? null : this[column.Ordinal];
    }
}
