namespace KineticReports.Core.Definition;

/// <summary>
/// Canonical layout definition stored on a <see cref="ReportDefinition"/>.
/// </summary>
public sealed record ReportLayoutDefinition
{
    /// <summary>Gets or inits page header items repeated across pages.</summary>
    public IReadOnlyList<ReportLayoutItemDefinition> PageHeader { get; init; } = [];

    /// <summary>Gets or inits body items rendered in order.</summary>
    public IReadOnlyList<ReportLayoutItemDefinition> Body { get; init; } = [];

    /// <summary>Gets or inits page footer items repeated across pages.</summary>
    public IReadOnlyList<ReportLayoutItemDefinition> PageFooter { get; init; } = [];
}

/// <summary>
/// One report layout item interpreted by the default Core builder.
/// </summary>
public sealed record ReportLayoutItemDefinition
{
    /// <summary>Gets or inits the stable item id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the item kind.</summary>
    public required ReportLayoutItemKind Kind { get; init; }

    /// <summary>Gets or inits text content for text items.</summary>
    public string? Text { get; init; }

    /// <summary>Gets or inits the bound data source id for table items.</summary>
    public string? DataSourceId { get; init; }

    /// <summary>Gets or inits the table columns for table items.</summary>
    public IReadOnlyList<ReportLayoutTableColumnDefinition> Columns { get; init; } = [];

    /// <summary>Gets or inits optional table footer aggregates.</summary>
    public IReadOnlyList<ReportLayoutTableAggregateDefinition> FooterAggregates { get; init; } = [];

    /// <summary>Gets or inits optional table group expression.</summary>
    public string? GroupByExpression { get; init; }

    /// <summary>Gets or inits whether the table should include a header row.</summary>
    public bool IncludeHeader { get; init; } = true;

    /// <summary>Gets or inits whether the table should repeat its headers.</summary>
    public bool RepeatHeaders { get; init; } = true;

    /// <summary>Gets or inits the footer label.</summary>
    public string? FooterLabel { get; init; } = "Total";

    /// <summary>Gets or inits the zero-based footer label column index.</summary>
    public int FooterLabelColumnIndex { get; init; }
}

/// <summary>
/// Supported canonical layout item kinds.
/// </summary>
public enum ReportLayoutItemKind
{
    /// <summary>Plain text region.</summary>
    Text,

    /// <summary>Data-backed table.</summary>
    Table,

    /// <summary>Explicit page break.</summary>
    PageBreak,
}

/// <summary>
/// Canonical table column definition for report layouts.
/// </summary>
public sealed record ReportLayoutTableColumnDefinition
{
    /// <summary>Gets or inits the display header.</summary>
    public required string Header { get; init; }

    /// <summary>Gets or inits the value expression.</summary>
    public required string ValueExpression { get; init; }

    /// <summary>Gets or inits the minimum width in DIPs.</summary>
    public float MinWidth { get; init; } = 40f;

    /// <summary>Gets or inits the relative growth factor.</summary>
    public float Grow { get; init; } = 1f;
}

/// <summary>
/// Canonical footer aggregate definition for report layout tables.
/// </summary>
public sealed record ReportLayoutTableAggregateDefinition
{
    /// <summary>Gets or inits the target column index.</summary>
    public required int ColumnIndex { get; init; }

    /// <summary>Gets or inits the aggregate value expression.</summary>
    public required string ValueExpression { get; init; }

    /// <summary>Gets or inits the aggregate kind name.</summary>
    public string Kind { get; init; } = "Sum";

    /// <summary>Gets or inits an optional display format string.</summary>
    public string? FormatString { get; init; }
}