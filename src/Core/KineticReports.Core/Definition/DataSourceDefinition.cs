namespace KineticReports.Core.Definition;

/// <summary>
/// Describes a data source referenced by the report definition.
/// The runtime uses <see cref="ProviderType"/> to locate and instantiate
/// the appropriate data provider plugin.
/// </summary>
public sealed record DataSourceDefinition
{
    /// <summary>Gets or inits the unique identifier of this data source.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the human-readable display name of this data source.</summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or inits the provider type identifier (e.g. "SqlServer", "Rest", "Csv").
    /// This value is matched against registered <c>IDataProvider</c> implementations.
    /// </summary>
    public required string ProviderType { get; init; }

    /// <summary>
    /// Gets or inits provider-specific connection properties as key-value pairs.
    /// </summary>
    /// <remarks>
    /// Sensitive values such as passwords and API tokens must be supplied at
    /// runtime via a secrets provider and must not be stored in the report definition.
    /// </remarks>
    public IReadOnlyDictionary<string, string> Properties { get; init; } =
        new Dictionary<string, string>();
}
