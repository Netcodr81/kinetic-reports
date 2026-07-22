namespace KineticReports.Core.Data;

/// <summary>
/// Defines the contract for a data provider that retrieves query results for report execution.
/// Implementations are backend-agnostic and can target SQL Server, PostgreSQL, REST APIs, etc.
/// </summary>
public interface IDataProvider
{
    /// <summary>Gets the user-friendly name of this provider (e.g., "SQL Server", "PostgreSQL").</summary>
    string Name { get; }

    /// <summary>
    /// Executes a query against the data source asynchronously.
    /// </summary>
    /// <param name="request">The query request containing dataset name, query text, and parameters.</param>
    /// <param name="cancellationToken">A cancellation token to allow operation cancellation.</param>
    /// <returns>A task that yields the query result with schema and rows.</returns>
    /// <remarks>
    /// Implementations MUST use parameterized queries to prevent SQL injection.
    /// Large result sets SHOULD be streamed when practical.
    /// </remarks>
    Task<QueryResult> ExecuteAsync(QueryRequest request, CancellationToken cancellationToken = default);
}
