namespace KineticReports.Core.Engine.Expressions;

/// <summary>
/// Provides contextual data available during expression evaluation,
/// including the current data row, report parameters, and pagination state.
/// </summary>
public sealed class ExpressionContext
{
    /// <summary>Gets the fields of the current data row.</summary>
    public required IReadOnlyDictionary<string, object?> CurrentRow { get; init; }

    /// <summary>Gets the resolved report parameters.</summary>
    public required IReadOnlyDictionary<string, object?> Parameters { get; init; }

    /// <summary>Gets the zero-based index of the current data row within its data source.</summary>
    public int RowIndex { get; init; }

    /// <summary>Gets the one-based current page number as determined during pagination.</summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the total page count when known; otherwise <see langword="null"/>.
    /// </summary>
    public int? TotalPages { get; init; }

    /// <summary>Returns an <see cref="ExpressionContext"/> with no data row or parameters.</summary>
    public static ExpressionContext Empty { get; } = new ExpressionContext
    {
        CurrentRow = new Dictionary<string, object?>(),
        Parameters = new Dictionary<string, object?>()
    };
}
