namespace KineticReports.Core.Authoring.Components;

/// <summary>
/// Describes one component type available to a report designer toolbox.
/// </summary>
public sealed record ReportComponentDescriptor
{
    /// <summary>Gets or inits the component type identifier.</summary>
    public required ReportComponentType Type { get; init; }

    /// <summary>Gets or inits the display name shown in toolbox UIs.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Gets or inits the component category shown in toolbox grouping.</summary>
    public required string Category { get; init; }

    /// <summary>Gets or inits a concise description of what the component does.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or inits a value indicating whether this component can contain children.</summary>
    public bool SupportsChildren { get; init; }
}
