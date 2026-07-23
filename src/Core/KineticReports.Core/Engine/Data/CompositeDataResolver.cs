namespace KineticReports.Core.Engine.Data;

using KineticReports.Core.Definition;

/// <summary>
/// Default engine-level <see cref="IDataResolver"/> that delegates to registered
/// <see cref="IDataSourceResolver"/> implementations.
/// </summary>
public sealed class CompositeDataResolver : IDataResolver
{
    private readonly IReadOnlyList<IDataSourceResolver> _resolvers;

    /// <summary>
    /// Initializes a new <see cref="CompositeDataResolver"/>.
    /// </summary>
    /// <param name="resolvers">Ordered resolver contributions.</param>
    public CompositeDataResolver(IEnumerable<IDataSourceResolver> resolvers)
    {
        ArgumentNullException.ThrowIfNull(resolvers);
        _resolvers = resolvers.ToList();
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ResolveAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(parameters);

        var resolver = _resolvers.FirstOrDefault(candidate => candidate.CanResolve(definition));
        if (resolver is null)
        {
            throw new InvalidOperationException(
                $"No data resolver is registered for provider type '{definition.ProviderType}' on data source '{definition.Id}'.");
        }

        return resolver.ResolveAsync(definition, parameters, cancellationToken);
    }
}
