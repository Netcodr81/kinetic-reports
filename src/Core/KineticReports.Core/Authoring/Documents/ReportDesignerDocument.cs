namespace KineticReports.Core.Authoring.Documents;

using KineticReports.Core.Authoring.Components;
using KineticReports.Core.Definition;
using KineticReports.Core.Styling;

/// <summary>
/// Portable drag-and-drop authoring document that can be serialized to JSON.
/// </summary>
public sealed record ReportDesignerDocument
{
    /// <summary>Gets or inits the designer schema version (for example "1.0").</summary>
    public required string SchemaVersion { get; init; }

    /// <summary>Gets or inits the stable unique report id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the report display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets or inits the report description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or inits the report author.</summary>
    public string? Author { get; init; }

    /// <summary>Gets or inits report parameters used at runtime.</summary>
    public IReadOnlyList<ParameterDefinition> Parameters { get; init; } = [];

    /// <summary>Gets or inits data source definitions used by components.</summary>
    public IReadOnlyList<DataSourceDefinition> DataSources { get; init; } = [];

    /// <summary>Gets or inits reusable style definitions.</summary>
    public IReadOnlyList<StyleDefinition> Styles { get; init; } = [];

    /// <summary>Gets or inits root-level components composing the report.</summary>
    public IReadOnlyList<ReportComponentDefinition> Components { get; init; } = [];
}
