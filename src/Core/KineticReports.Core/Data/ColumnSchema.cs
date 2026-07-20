namespace KineticReports.Core.Data;

/// <summary>
/// Describes the schema of a column returned by a data provider query.
/// </summary>
public sealed record ColumnSchema
{
    /// <summary>Gets the column name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the CLR type of the column values.</summary>
    public required Type ClrType { get; init; }

    /// <summary>Gets a value indicating whether the column can contain null values.</summary>
    public bool IsNullable { get; init; }

    /// <summary>Gets the ordinal (zero-based column index) in the result set.</summary>
    public required int Ordinal { get; init; }

    /// <summary>Gets the optional human-readable display name for the column.</summary>
    public string? DisplayName { get; init; }
}
