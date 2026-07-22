namespace KineticReports.Authoring.Components;

/// <summary>
/// Default component catalog containing the built-in authoring components.
/// </summary>
public sealed class DefaultReportComponentCatalog : IReportComponentCatalog
{
    private static readonly IReadOnlyList<ReportComponentDescriptor> Components =
    [
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Text,
            DisplayName = "Text",
            Category = "Basic",
            Description = "Renders static or expression-driven text.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Table,
            DisplayName = "Table",
            Category = "Data",
            Description = "Renders tabular data with columns and rows.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Image,
            DisplayName = "Image",
            Category = "Basic",
            Description = "Renders an image from a bound source.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Barcode,
            DisplayName = "Barcode",
            Category = "Data",
            Description = "Renders a barcode from a bound value.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Group,
            DisplayName = "Group",
            Category = "Structure",
            Description = "Creates a repeated grouping scope for child components.",
            SupportsChildren = true,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Section,
            DisplayName = "Section",
            Category = "Structure",
            Description = "Defines a visual section boundary for child components.",
            SupportsChildren = true,
        },
    ];

    /// <inheritdoc/>
    public IReadOnlyList<ReportComponentDescriptor> GetComponents() => Components;
}
