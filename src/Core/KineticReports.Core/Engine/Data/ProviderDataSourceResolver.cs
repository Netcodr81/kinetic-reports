namespace KineticReports.Core.Engine.Data;

using KineticReports.Core.Data;
using KineticReports.Core.Definition;

/// <summary>
/// Resolves data sources by dispatching to a named <see cref="IDataProvider"/>.
/// </summary>
public sealed class ProviderDataSourceResolver : IDataSourceResolver
{
    private const string QueryProperty = "Query";

    private readonly IReadOnlyList<INamedDataProvider> _providers;

    /// <summary>
    /// Initializes a new <see cref="ProviderDataSourceResolver"/>.
    /// </summary>
    /// <param name="providers">Named provider registrations.</param>
    public ProviderDataSourceResolver(IEnumerable<INamedDataProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        _providers = providers.ToList();
    }

    /// <inheritdoc/>
    public bool CanResolve(DataSourceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return FindProvider(definition.ProviderType) is not null;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ResolveAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(parameters);

        var provider = FindProvider(definition.ProviderType);
        if (provider is null)
        {
            throw new InvalidOperationException(
                $"No IDataProvider is registered for provider type '{definition.ProviderType}'.");
        }

        var queryText = ResolveQueryText(definition);

        var queryResult = await provider.ExecuteAsync(
            new QueryRequest
            {
                DatasetName = definition.Id,
                QueryText = queryText,
                Parameters = parameters
            },
            cancellationToken).ConfigureAwait(false);

        return ConvertRows(queryResult);
    }

    private IDataProvider? FindProvider(string providerType)
    {
        if (string.IsNullOrWhiteSpace(providerType))
            return null;

        return _providers
            .FirstOrDefault(candidate => string.Equals(candidate.ProviderType, providerType, StringComparison.OrdinalIgnoreCase))
            ?.Provider;
    }

    private static string ResolveQueryText(DataSourceDefinition definition)
    {
        if (definition.Properties.TryGetValue(QueryProperty, out var queryText)
            && !string.IsNullOrWhiteSpace(queryText))
        {
            return queryText;
        }

        // POCO provider supports resolving by dataset name when query text is not needed.
        return definition.Id;
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, object?>> ConvertRows(QueryResult queryResult)
    {
        var rows = new List<IReadOnlyDictionary<string, object?>>(queryResult.Rows.Count);

        foreach (var row in queryResult.Rows)
        {
            var values = new Dictionary<string, object?>(queryResult.Schema.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var column in queryResult.Schema)
            {
                values[column.Name] = row[column.Ordinal];
            }

            rows.Add(values);
        }

        return rows;
    }
}
