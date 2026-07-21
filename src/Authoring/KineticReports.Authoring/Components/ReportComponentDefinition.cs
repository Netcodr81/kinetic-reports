namespace KineticReports.Authoring.Components;

/// <summary>
/// Defines one authoring component instance used by drag-and-drop report designers.
/// </summary>
public sealed record ReportComponentDefinition
{
    /// <summary>Gets or inits the unique component identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the component type.</summary>
    public ReportComponentType Type { get; init; }

    /// <summary>Gets or inits a display name for designer tooling.</summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or inits the data source id this component binds to.
    /// </summary>
    public string? DataSourceId { get; init; }

    /// <summary>
    /// Gets or inits the repeat scope path for grouping/repeating components.
    /// </summary>
    public string? RepeatPath { get; init; }

    /// <summary>
    /// Gets or inits expression bindings (for example, Text={CustomerName}).
    /// </summary>
    public IReadOnlyDictionary<string, string> Bindings { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or inits arbitrary component properties used by specific component types.
    /// </summary>
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or inits designer placement metadata.
    /// </summary>
    public ReportComponentPlacement Placement { get; init; } = new();

    /// <summary>
    /// Gets or inits child components nested beneath this component.
    /// </summary>
    public IReadOnlyList<ReportComponentDefinition> Children { get; init; } = [];
}
