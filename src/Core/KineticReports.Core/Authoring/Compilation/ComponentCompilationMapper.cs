namespace KineticReports.Core.Authoring.Compilation;

using KineticReports.Core.Authoring.Components;
using KineticReports.Core.Definition;

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

    /// <summary>
    /// Builds a canonical layout definition from compiled components.
    /// </summary>
    public static ReportLayoutDefinition BuildLayout(IReadOnlyList<CompiledComponentMetadata> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        var header = new List<ReportLayoutItemDefinition>();
        var body = new List<ReportLayoutItemDefinition>();
        var footer = new List<ReportLayoutItemDefinition>();

        foreach (var component in components)
            AppendLayoutItems(component, body, header, footer, ReportLayoutTarget.Body);

        return new ReportLayoutDefinition
        {
            PageHeader = header,
            Body = body,
            PageFooter = footer
        };
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

    private static void AppendLayoutItems(
        CompiledComponentMetadata component,
        List<ReportLayoutItemDefinition> body,
        List<ReportLayoutItemDefinition> header,
        List<ReportLayoutItemDefinition> footer,
        ReportLayoutTarget currentTarget)
    {
        if (HasSupportRole(component, "Header"))
        {
            foreach (var child in component.Children)
                AppendLayoutItems(child, body, header, footer, ReportLayoutTarget.Header);

            return;
        }

        if (HasSupportRole(component, "Footer"))
        {
            foreach (var child in component.Children)
                AppendLayoutItems(child, body, header, footer, ReportLayoutTarget.Footer);

            return;
        }

        var target = currentTarget switch
        {
            ReportLayoutTarget.Header => header,
            ReportLayoutTarget.Footer => footer,
            _ => body
        };

        if (ForcesPageBreak(component))
        {
            target.Add(new ReportLayoutItemDefinition
            {
                Id = component.Id,
                Kind = ReportLayoutItemKind.PageBreak
            });

            return;
        }

        if (string.Equals(component.CanonicalType, "Text", StringComparison.OrdinalIgnoreCase))
        {
            var text = TryGetBinding(component, "Text")
                ?? TryGetProperty(component, "Text")
                ?? component.Name
                ?? component.Id;

            target.Add(new ReportLayoutItemDefinition
            {
                Id = component.Id,
                Kind = ReportLayoutItemKind.Text,
                Text = NormalizeTokens(text)
            });

            return;
        }

        if (string.Equals(component.CanonicalType, "Table", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(component.DataSourceId))
        {
            var columnsJson = TryGetProperty(component, "ColumnsJson");
            if (!string.IsNullOrWhiteSpace(columnsJson))
            {
                var columns = global::System.Text.Json.JsonSerializer.Deserialize<IReadOnlyList<ReportLayoutTableColumnDefinition>>(columnsJson);
                var footerAggregatesJson = TryGetProperty(component, "FooterAggregatesJson");
                var footerAggregates = string.IsNullOrWhiteSpace(footerAggregatesJson)
                    ? Array.Empty<ReportLayoutTableAggregateDefinition>()
                    : global::System.Text.Json.JsonSerializer.Deserialize<IReadOnlyList<ReportLayoutTableAggregateDefinition>>(footerAggregatesJson)
                        ?? Array.Empty<ReportLayoutTableAggregateDefinition>();

                if (columns is not null && columns.Count > 0)
                {
                    target.Add(new ReportLayoutItemDefinition
                    {
                        Id = component.Id,
                        Kind = ReportLayoutItemKind.Table,
                        DataSourceId = component.DataSourceId,
                        Columns = columns,
                        FooterAggregates = footerAggregates,
                        FooterLabel = TryGetProperty(component, "FooterLabel") ?? "Total",
                        FooterLabelColumnIndex = TryParseInt(TryGetProperty(component, "FooterLabelColumnIndex")) ?? 0,
                        GroupByExpression = NormalizeNullableTokens(TryGetProperty(component, "GroupByExpression")),
                        IncludeHeader = TryParseBool(TryGetProperty(component, "IncludeHeader")) ?? true,
                        RepeatHeaders = TryParseBool(TryGetProperty(component, "RepeatHeaders")) ?? true
                    });

                    return;
                }
            }
        }

        foreach (var child in component.Children)
            AppendLayoutItems(child, body, header, footer, currentTarget);
    }

    private static bool HasSupportRole(CompiledComponentMetadata component, string role)
    {
        var configuredRole = TryGetProperty(component, "support.role");
        return string.Equals(configuredRole, role, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ForcesPageBreak(CompiledComponentMetadata component)
    {
        return string.Equals(TryGetProperty(component, "pagination.forcePageBreakBefore"), "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(component.SourceType, "PageBreak", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryGetBinding(CompiledComponentMetadata component, string key)
    {
        return component.BoundFields.FirstOrDefault(pair => string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase)).Value;
    }

    private static string? TryGetProperty(CompiledComponentMetadata component, string key)
    {
        return component.Properties.FirstOrDefault(pair => string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase)).Value;
    }

    private static int? TryParseInt(string? value)
        => int.TryParse(value, out var parsed) ? parsed : null;

    private static bool? TryParseBool(string? value)
        => bool.TryParse(value, out var parsed) ? parsed : null;

    private static string NormalizeTokens(string expression)
        => expression.Replace("{{", "{", StringComparison.Ordinal).Replace("}}", "}", StringComparison.Ordinal);

    private static string? NormalizeNullableTokens(string? expression)
        => string.IsNullOrWhiteSpace(expression) ? null : NormalizeTokens(expression);

    private enum ReportLayoutTarget
    {
        Body,
        Header,
        Footer
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