namespace KineticReports.Core.Engine.Data;

using KineticReports.Core.Definition;

/// <summary>
/// Resolves a specific subset of data-source definitions.
/// </summary>
/// <remarks>
/// Multiple <see cref="IDataSourceResolver"/> implementations can be registered.
/// The composite engine resolver selects the first implementation that reports
/// <see cref="CanResolve"/> for a given data source.
/// </remarks>
public interface IDataSourceResolver
{
    /// <summary>
    /// Determines whether this resolver can handle the provided data source definition.
    /// </summary>
    /// <param name="definition">Data source definition.</param>
    /// <returns><see langword="true"/> when this resolver can resolve the definition.</returns>
    bool CanResolve(DataSourceDefinition definition);

    /// <summary>
    /// Resolves the provided data source into rows.
    /// </summary>
    /// <param name="definition">Data source definition.</param>
    /// <param name="parameters">Resolved report parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved rows.</returns>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ResolveAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
