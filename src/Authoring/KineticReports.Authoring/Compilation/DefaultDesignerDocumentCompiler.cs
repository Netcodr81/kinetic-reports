namespace KineticReports.Authoring.Compilation;

using KineticReports.Authoring.Documents;
using KineticReports.Core.Definition;

/// <summary>
/// Default implementation that maps designer metadata to <see cref="ReportDefinition"/>.
/// </summary>
public sealed class DefaultDesignerDocumentCompiler : IDesignerDocumentCompiler
{
    /// <inheritdoc/>
    public ReportDefinition Compile(ReportDesignerDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var componentSummary = document.Components
            .GroupBy(component => component.Type)
            .ToDictionary(group => group.Key.ToString(), group => group.Count());

        var metadata = new Dictionary<string, object>
        {
            ["authoring.schemaVersion"] = document.SchemaVersion,
            ["authoring.componentCount"] = document.Components.Count,
            ["authoring.componentTypes"] = componentSummary,
        };

        return new ReportDefinition
        {
            SchemaVersion = document.SchemaVersion,
            Id = document.Id,
            Name = document.Name,
            Author = document.Author,
            Description = document.Description,
            Metadata = metadata,
            Parameters = document.Parameters,
            DataSources = document.DataSources,
            Styles = document.Styles,
        };
    }
}
