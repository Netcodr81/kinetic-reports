namespace KineticReports.Core.Definition;

using KineticReports.Core.Styling;

/// <summary>
/// The top-level, canonical representation of a report.
/// This is the immutable input to the layout engine during execution.
/// </summary>
/// <remarks>
/// All producers (designer, fluent builder) emit a <see cref="ReportDefinition"/>.
/// All consumers (engine, viewer, exporters) receive a <see cref="ReportDefinition"/>.
/// The definition must not be mutated once execution begins.
/// </remarks>
public sealed record ReportDefinition
{
    /// <summary>Gets or inits the JSON schema version string (e.g. "1.0").</summary>
    public required string SchemaVersion { get; init; }

    /// <summary>Gets or inits the stable unique identifier of this report.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the human-readable display name of this report.</summary>
    public required string Name { get; init; }

    /// <summary>Gets or inits the name of the report author.</summary>
    public string? Author { get; init; }

    /// <summary>Gets or inits a description of the report's purpose.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or inits an arbitrary metadata bag for extensibility.
    /// Keys and values are defined by producers and consumers.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } =
        new Dictionary<string, object>();

    /// <summary>Gets or inits the parameter definitions declared by this report.</summary>
    public IReadOnlyList<ParameterDefinition> Parameters { get; init; } = [];

    /// <summary>Gets or inits the data source definitions referenced by this report.</summary>
    public IReadOnlyList<DataSourceDefinition> DataSources { get; init; } = [];

    /// <summary>Gets or inits the named style definitions used by elements in this report.</summary>
    public IReadOnlyList<StyleDefinition> Styles { get; init; } = [];
}
