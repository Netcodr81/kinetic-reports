namespace KineticReports.Core.Definition;

/// <summary>
/// Defines whether a plugin is enabled for a specific report definition.
/// </summary>
public sealed record ReportPluginToggleDefinition
{
    /// <summary>
    /// Gets or inits the plugin identifier.
    /// </summary>
    public required string PluginId { get; init; }

    /// <summary>
    /// Gets or inits whether the plugin is enabled for this report.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
