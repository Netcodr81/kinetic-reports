namespace KineticReports.Core.Authoring.Compilation;

using KineticReports.Core.Authoring.Documents;
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

        var compiledComponents = ComponentCompilationMapper.Compile(document.Components);
        var canonicalSummary = ComponentCompilationMapper.BuildCanonicalSummary(compiledComponents);
        var aliasMap = ComponentCompilationMapper.BuildAliasMap(compiledComponents);

        var metadata = new Dictionary<string, object>
        {
            ["authoring.schemaVersion"] = document.SchemaVersion,
            ["authoring.componentCount"] = document.Components.Count,
            ["authoring.componentTypes"] = componentSummary,
            ["authoring.componentCanonicalTypes"] = canonicalSummary,
            ["authoring.componentAliases"] = aliasMap,
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
            Layout = ComponentCompilationMapper.BuildLayout(compiledComponents),
        };
    }
}
