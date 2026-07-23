namespace KineticReports.Core.Engine.Data;

using KineticReports.Core.Definition;

/// <summary>
/// Resolves a <see cref="DataSourceDefinition"/> into typed data rows.
/// Implement this interface to connect the engine to any data backend
/// (SQL, REST, in-memory, etc.).
/// </summary>
public interface IDataResolver
{
    /// <summary>
    /// Asynchronously fetches all rows for the specified data source.
    /// </summary>
    /// <param name="definition">The data source configuration from the report definition.</param>
    /// <param name="parameters">
    /// The resolved report parameters, available for use in parameterised queries.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An ordered list of rows, where each row is a name-to-value map.
    /// Returns an empty list when the data source yields no rows.
    /// </returns>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ResolveAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
