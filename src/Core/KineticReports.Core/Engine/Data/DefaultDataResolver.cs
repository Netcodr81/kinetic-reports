using KineticReports.Core.Definition;

namespace KineticReports.Core.Engine.Data;

/// <summary>
/// Default implementation of <see cref="IDataResolver"/> that returns empty result sets.
/// Override or replace this with a real data resolver that connects to your backend
/// (SQL Server, REST API, in-memory, etc.).
/// </summary>
public sealed class DefaultDataResolver : IDataResolver
{
    /// <summary>
    /// Resolves a data source by returning an empty list of rows.
    /// </summary>
    /// <remarks>
    /// This is a placeholder implementation. To use real data:
    /// 1. Implement this interface with your data access logic
    /// 2. Or use SqlServerDataProvider if you have a SQL Server data source
    /// 3. Or create a custom resolver for your data backend
    /// </remarks>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ResolveAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        // Return an empty list for now - your implementation should fetch real data
        IReadOnlyList<IReadOnlyDictionary<string, object?>> empty = Array.Empty<IReadOnlyDictionary<string, object?>>();
        return Task.FromResult(empty);
    }
}
