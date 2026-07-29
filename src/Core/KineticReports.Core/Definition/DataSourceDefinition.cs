namespace KineticReports.Core.Definition;

/// <summary>
/// Describes a data source referenced by the report definition.
/// The runtime uses <see cref="SourceName"/> to locate the registered provider instance.
/// </summary>
public sealed record DataSourceDefinition
{
    /// <summary>Gets or inits the unique identifier of this data source.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the human-readable display name of this data source.</summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or inits the provider type identifier (e.g. "SqlServer", "SqlLite", "Poco").
    /// This value documents the expected provider category for this data source.
    /// </summary>
    public required string ProviderType { get; init; }

    /// <summary>
    /// Gets or inits the required registered provider source name.
    /// This value must match a configured named provider registration.
    /// </summary>
    public required string SourceName { get; init; }

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
