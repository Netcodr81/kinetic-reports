namespace KineticReports.Core.Authoring.Compilation;

using KineticReports.Core.Authoring.Components;

/// <summary>
/// Maps authoring component types to canonical runtime-oriented component hints.
/// </summary>
internal static class ComponentCompilationMapper
{
    /// <summary>
    /// Compiles a component tree into canonical metadata records.
    /// </summary>
    public static IReadOnlyList<CompiledComponentMetadata> Compile(IReadOnlyList<ReportComponentDefinition> components)
    {
        ArgumentNullException.ThrowIfNull(components);
        return components.Select(CompileComponent).ToList();
    }

    /// <summary>
    /// Produces a summary count keyed by canonical kind.
    /// </summary>
    public static IReadOnlyDictionary<string, int> BuildCanonicalSummary(IReadOnlyList<CompiledComponentMetadata> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        return Flatten(components)
            .GroupBy(component => component.CanonicalKind)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Builds an alias map where the source type differs from the canonical type.
    /// </summary>
    public static IReadOnlyDictionary<string, string> BuildAliasMap(IReadOnlyList<CompiledComponentMetadata> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        return Flatten(components)
            .Where(component => !string.Equals(component.SourceType, component.CanonicalType, StringComparison.Ordinal))
            .GroupBy(component => component.SourceType)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.First().CanonicalType,
                StringComparer.Ordinal);
    }

    private static CompiledComponentMetadata CompileComponent(ReportComponentDefinition component)
    {
        var normalized = GetNormalization(component.Type);

        var properties = new Dictionary<string, string>(component.Properties, StringComparer.Ordinal);
        foreach (var pair in normalized.PresetProperties)
            properties[pair.Key] = pair.Value;

        var boundFields = new Dictionary<string, string>(component.BoundFields, StringComparer.Ordinal);
        foreach (var pair in normalized.PresetBindings)
            boundFields[pair.Key] = pair.Value;

        var children = component.Children
            .Select(CompileComponent)
            .ToList();

        return new CompiledComponentMetadata
        {
            Id = component.Id,
            Name = component.Name,
            SourceType = component.Type.ToString(),
            CanonicalType = normalized.CanonicalType,
            CanonicalKind = normalized.CanonicalKind,
            DataSourceId = component.DataSourceId,
            RepeatPath = component.RepeatPath,
            Placement = component.Placement,
            BoundFields = boundFields,
            Properties = properties,
            Children = children,
        };
    }

    private static IEnumerable<CompiledComponentMetadata> Flatten(IEnumerable<CompiledComponentMetadata> components)
    {
        foreach (var component in components)
        {
            yield return component;

            foreach (var child in Flatten(component.Children))
                yield return child;
        }
    }

    private static ComponentNormalization GetNormalization(ReportComponentType type)
    {
        return type switch
        {
            // Structure
            ReportComponentType.Page => new ComponentNormalization("Page", "Structure"),
            ReportComponentType.Section => new ComponentNormalization("Section", "Structure"),
            ReportComponentType.Panel => new ComponentNormalization("Panel", "Structure"),
            ReportComponentType.Group => new ComponentNormalization("Group", "Structure"),
            ReportComponentType.List => new ComponentNormalization(
                "Group",
                "Structure",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Group),
                    ["support.repeatMode"] = "List"
                }),
            ReportComponentType.Table => new ComponentNormalization("Table", "Structure"),

            // Content
            ReportComponentType.Text => new ComponentNormalization("Text", "Content"),
            ReportComponentType.Image => new ComponentNormalization("Image", "Content"),
            ReportComponentType.Chart => new ComponentNormalization("Chart", "Content"),
            ReportComponentType.Line => new ComponentNormalization(
                "Rectangle",
                "Content",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Rectangle),
                    ["shape.kind"] = "Line"
                }),
            ReportComponentType.Rectangle => new ComponentNormalization(
                "Rectangle",
                "Content",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["shape.kind"] = "Rectangle"
                }),
            ReportComponentType.Ellipse => new ComponentNormalization(
                "Rectangle",
                "Content",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Rectangle),
                    ["shape.kind"] = "Ellipse"
                }),
            ReportComponentType.Barcode => new ComponentNormalization("Barcode", "Content"),

            // Supporting
            ReportComponentType.Spacer => new ComponentNormalization(
                "Panel",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Panel),
                    ["support.role"] = "Spacer"
                }),
            ReportComponentType.Divider => new ComponentNormalization(
                "Line",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Line),
                    ["shape.kind"] = "Line",
                    ["support.role"] = "Divider"
                }),
            ReportComponentType.PageBreak => new ComponentNormalization(
                "Panel",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Panel),
                    ["pagination.forcePageBreakBefore"] = "true"
                }),
            ReportComponentType.PageNumber => new ComponentNormalization(
                "Text",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Text),
                    ["token.kind"] = "PageNumber"
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Text"] = "{{PageNumber}}"
                }),
            ReportComponentType.TotalPages => new ComponentNormalization(
                "Text",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Text),
                    ["token.kind"] = "TotalPages"
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Text"] = "{{TotalPages}}"
                }),
            ReportComponentType.CurrentDate => new ComponentNormalization(
                "Text",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Text),
                    ["token.kind"] = "CurrentDate"
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Text"] = "{{CurrentDate}}"
                }),
            ReportComponentType.CurrentTime => new ComponentNormalization(
                "Text",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Text),
                    ["token.kind"] = "CurrentTime"
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Text"] = "{{CurrentTime}}"
                }),
            ReportComponentType.DocumentInfo => new ComponentNormalization(
                "Text",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Text),
                    ["token.kind"] = "DocumentInfo"
                }),
            ReportComponentType.Header => new ComponentNormalization(
                "Section",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Section),
                    ["support.role"] = "Header"
                }),
            ReportComponentType.Footer => new ComponentNormalization(
                "Section",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Section),
                    ["support.role"] = "Footer"
                }),
            ReportComponentType.Background => new ComponentNormalization(
                "Panel",
                "Supporting",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["support.aliasOf"] = nameof(ReportComponentType.Panel),
                    ["support.role"] = "Background"
                }),
            _ => new ComponentNormalization(type.ToString(), "Unknown"),
        };
    }

    private sealed class ComponentNormalization
    {
        public ComponentNormalization(
            string canonicalType,
            string canonicalKind,
            IReadOnlyDictionary<string, string>? presetProperties = null,
            IReadOnlyDictionary<string, string>? presetBindings = null)
        {
            CanonicalType = canonicalType;
            CanonicalKind = canonicalKind;
            PresetProperties = presetProperties ?? new Dictionary<string, string>(StringComparer.Ordinal);
            PresetBindings = presetBindings ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public string CanonicalType { get; }

        public string CanonicalKind { get; }

        public IReadOnlyDictionary<string, string> PresetProperties { get; }

        public IReadOnlyDictionary<string, string> PresetBindings { get; }
    }
}

/// <summary>
/// Canonical metadata emitted for a compiled authoring component.
/// </summary>
public sealed record CompiledComponentMetadata
{
    /// <summary>Gets or inits the source component id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the optional source display name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets or inits the source component type before normalization.</summary>
    public required string SourceType { get; init; }

    /// <summary>Gets or inits the canonical target type after normalization.</summary>
    public required string CanonicalType { get; init; }

    /// <summary>Gets or inits the canonical grouping (Structure, Content, Supporting).</summary>
    public required string CanonicalKind { get; init; }

    /// <summary>Gets or inits the optional data source id.</summary>
    public string? DataSourceId { get; init; }

    /// <summary>Gets or inits the optional repeat path.</summary>
    public string? RepeatPath { get; init; }

    /// <summary>Gets or inits component placement metadata.</summary>
    public required ReportComponentPlacement Placement { get; init; }

    /// <summary>Gets or inits merged expression-bound fields.</summary>
    public IReadOnlyDictionary<string, string> BoundFields { get; init; } = new Dictionary<string, string>();

    /// <summary>Gets or inits merged component properties.</summary>
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();

    /// <summary>Gets or inits normalized child components.</summary>
    public IReadOnlyList<CompiledComponentMetadata> Children { get; init; } = [];
}