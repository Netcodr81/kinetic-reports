namespace KineticReports.Authoring.Documents;

using KineticReports.Authoring.Components;
using KineticReports.Core.Definition;
using KineticReports.Core.Styling;

/// <summary>
/// Fluent code-first builder for <see cref="ReportDesignerDocument"/>.
/// </summary>
public sealed class ReportDesignerDocumentBuilder
{
    private string _schemaVersion = "1.0";
    private string? _id;
    private string? _name;
    private string? _description;
    private string? _author;
    private readonly List<ParameterDefinition> _parameters = [];
    private readonly List<DataSourceDefinition> _dataSources = [];
    private readonly List<StyleDefinition> _styles = [];
    private readonly List<ReportComponentDefinition> _components = [];

    /// <summary>
    /// Creates a new builder with required id and name.
    /// </summary>
    public static ReportDesignerDocumentBuilder Create(string id, string name)
    {
        return new ReportDesignerDocumentBuilder()
            .WithId(id)
            .WithName(name);
    }

    /// <summary>
    /// Sets the document schema version.
    /// </summary>
    public ReportDesignerDocumentBuilder WithSchemaVersion(string schemaVersion)
    {
        if (string.IsNullOrWhiteSpace(schemaVersion))
            throw new ArgumentException("Schema version must be provided.", nameof(schemaVersion));

        _schemaVersion = schemaVersion;
        return this;
    }

    /// <summary>
    /// Sets the document id.
    /// </summary>
    public ReportDesignerDocumentBuilder WithId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id must be provided.", nameof(id));

        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the document display name.
    /// </summary>
    public ReportDesignerDocumentBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be provided.", nameof(name));

        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the optional description.
    /// </summary>
    public ReportDesignerDocumentBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the optional author.
    /// </summary>
    public ReportDesignerDocumentBuilder WithAuthor(string? author)
    {
        _author = author;
        return this;
    }

    /// <summary>
    /// Adds a parameter definition.
    /// </summary>
    public ReportDesignerDocumentBuilder AddParameter(ParameterDefinition parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        _parameters.Add(parameter);
        return this;
    }

    /// <summary>
    /// Adds a data source definition.
    /// </summary>
    public ReportDesignerDocumentBuilder AddDataSource(DataSourceDefinition dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSources.Add(dataSource);
        return this;
    }

    /// <summary>
    /// Adds a style definition.
    /// </summary>
    public ReportDesignerDocumentBuilder AddStyle(StyleDefinition style)
    {
        ArgumentNullException.ThrowIfNull(style);
        _styles.Add(style);
        return this;
    }

    /// <summary>
    /// Adds a root-level component definition.
    /// </summary>
    public ReportDesignerDocumentBuilder AddComponent(ReportComponentDefinition component)
    {
        ArgumentNullException.ThrowIfNull(component);
        _components.Add(component);
        return this;
    }

    /// <summary>
    /// Adds a root-level component using a fluent component builder.
    /// </summary>
    public ReportDesignerDocumentBuilder AddComponent(
        string id,
        ReportComponentType type,
        Action<ReportComponentDefinitionBuilder>? configure = null)
    {
        var builder = new ReportComponentDefinitionBuilder(id, type);
        configure?.Invoke(builder);
        return AddComponent(builder.Build());
    }

    /// <summary>
    /// Builds the immutable designer document.
    /// </summary>
    public ReportDesignerDocument Build()
    {
        if (string.IsNullOrWhiteSpace(_id))
            throw new InvalidOperationException("Document id is required. Call WithId(...).");

        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Document name is required. Call WithName(...).");

        return new ReportDesignerDocument
        {
            SchemaVersion = _schemaVersion,
            Id = _id,
            Name = _name,
            Description = _description,
            Author = _author,
            Parameters = _parameters.ToList(),
            DataSources = _dataSources.ToList(),
            Styles = _styles.ToList(),
            Components = _components.ToList(),
        };
    }
}

/// <summary>
/// Fluent code-first builder for <see cref="ReportComponentDefinition"/>.
/// </summary>
public sealed class ReportComponentDefinitionBuilder
{
    private readonly string _id;
    private readonly ReportComponentType _type;
    private string? _name;
    private string? _dataSourceId;
    private string? _repeatPath;
    private readonly Dictionary<string, string> _bindings = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _properties = new(StringComparer.Ordinal);
    private ReportComponentPlacement _placement = new();
    private readonly List<ReportComponentDefinition> _children = [];

    /// <summary>
    /// Initializes a new instance of the component builder.
    /// </summary>
    public ReportComponentDefinitionBuilder(string id, ReportComponentType type)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Component id must be provided.", nameof(id));

        _id = id;
        _type = type;
    }

    /// <summary>
    /// Sets the optional component display name.
    /// </summary>
    public ReportComponentDefinitionBuilder Named(string? name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the optional data source binding id.
    /// </summary>
    public ReportComponentDefinitionBuilder BoundToDataSource(string? dataSourceId)
    {
        _dataSourceId = dataSourceId;
        return this;
    }

    /// <summary>
    /// Sets the optional repeat scope path.
    /// </summary>
    public ReportComponentDefinitionBuilder WithRepeatPath(string? repeatPath)
    {
        _repeatPath = repeatPath;
        return this;
    }

    /// <summary>
    /// Adds or replaces a binding expression.
    /// </summary>
    public ReportComponentDefinitionBuilder WithBinding(string key, string expression)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Binding key must be provided.", nameof(key));

        _bindings[key] = expression;
        return this;
    }

    /// <summary>
    /// Adds or replaces a custom property.
    /// </summary>
    public ReportComponentDefinitionBuilder WithProperty(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Property key must be provided.", nameof(key));

        _properties[key] = value;
        return this;
    }

    /// <summary>
    /// Sets absolute placement metadata.
    /// </summary>
    public ReportComponentDefinitionBuilder WithPlacement(float x, float y, float width, float height)
    {
        _placement = new ReportComponentPlacement
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
        };

        return this;
    }

    /// <summary>
    /// Adds a child component definition.
    /// </summary>
    public ReportComponentDefinitionBuilder AddChild(ReportComponentDefinition child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
        return this;
    }

    /// <summary>
    /// Adds a child component using a nested fluent builder.
    /// </summary>
    public ReportComponentDefinitionBuilder AddChild(
        string id,
        ReportComponentType type,
        Action<ReportComponentDefinitionBuilder>? configure = null)
    {
        var builder = new ReportComponentDefinitionBuilder(id, type);
        configure?.Invoke(builder);
        return AddChild(builder.Build());
    }

    /// <summary>
    /// Builds the immutable component definition.
    /// </summary>
    public ReportComponentDefinition Build()
    {
        return new ReportComponentDefinition
        {
            Id = _id,
            Type = _type,
            Name = _name,
            DataSourceId = _dataSourceId,
            RepeatPath = _repeatPath,
            Bindings = new Dictionary<string, string>(_bindings, StringComparer.Ordinal),
            Properties = new Dictionary<string, string>(_properties, StringComparer.Ordinal),
            Placement = _placement,
            Children = _children.ToList(),
        };
    }
}