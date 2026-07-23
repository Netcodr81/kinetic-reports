namespace KineticReports.Core.Authoring.Components;

/// <summary>
/// Default component catalog containing the built-in authoring components.
/// </summary>
public sealed class DefaultReportComponentCatalog : IReportComponentCatalog
{
    private static readonly IReadOnlyList<ReportComponentDescriptor> Components =
    [
        // -----------------------------------------------------------------
        // Structure
        // -----------------------------------------------------------------

        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Page,
            DisplayName = "Page",
            Category = "Structure",
            Description = "Defines the page canvas root.",
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
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Panel,
            DisplayName = "Panel",
            Category = "Structure",
            Description = "Groups child components in a generic container.",
            SupportsChildren = true,
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
            Type = ReportComponentType.List,
            DisplayName = "List",
            Category = "Structure",
            Description = "Repeats child components for each bound record.",
            SupportsChildren = true,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Table,
            DisplayName = "Table",
            Category = "Structure",
            Description = "Renders tabular data with columns and rows.",
            SupportsChildren = false,
        },

        // -----------------------------------------------------------------
        // Content
        // -----------------------------------------------------------------

        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Text,
            DisplayName = "Text",
            Category = "Content",
            Description = "Renders static or expression-driven text.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Image,
            DisplayName = "Image",
            Category = "Content",
            Description = "Renders an image from a bound source.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Chart,
            DisplayName = "Chart",
            Category = "Content",
            Description = "Renders a chart using a configured chart type.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Line,
            DisplayName = "Line",
            Category = "Content",
            Description = "Renders a line shape.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Rectangle,
            DisplayName = "Rectangle",
            Category = "Content",
            Description = "Renders a rectangle shape.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Ellipse,
            DisplayName = "Ellipse",
            Category = "Content",
            Description = "Renders an ellipse shape.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Barcode,
            DisplayName = "Barcode",
            Category = "Content",
            Description = "Renders a barcode or QR code from a bound value.",
            SupportsChildren = false,
        },

        // -----------------------------------------------------------------
        // Supporting
        // -----------------------------------------------------------------

        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Spacer,
            DisplayName = "Spacer",
            Category = "Supporting",
            Description = "Adds flexible spacing between components.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Divider,
            DisplayName = "Divider",
            Category = "Supporting",
            Description = "Adds a simple visual separator.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.PageBreak,
            DisplayName = "Page Break",
            Category = "Supporting",
            Description = "Forces following content onto a new page.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.PageNumber,
            DisplayName = "Page Number",
            Category = "Supporting",
            Description = "Renders the current page number.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.TotalPages,
            DisplayName = "Total Pages",
            Category = "Supporting",
            Description = "Renders the total number of pages.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.CurrentDate,
            DisplayName = "Current Date",
            Category = "Supporting",
            Description = "Renders the current date.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.CurrentTime,
            DisplayName = "Current Time",
            Category = "Supporting",
            Description = "Renders the current time.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.DocumentInfo,
            DisplayName = "Document Info",
            Category = "Supporting",
            Description = "Renders report metadata values.",
            SupportsChildren = false,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Header,
            DisplayName = "Header",
            Category = "Supporting",
            Description = "Defines content for a header area.",
            SupportsChildren = true,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Footer,
            DisplayName = "Footer",
            Category = "Supporting",
            Description = "Defines content for a footer area.",
            SupportsChildren = true,
        },
        new ReportComponentDescriptor
        {
            Type = ReportComponentType.Background,
            DisplayName = "Background",
            Category = "Supporting",
            Description = "Defines a background region behind content.",
            SupportsChildren = true,
        },
    ];

    /// <inheritdoc/>
    public IReadOnlyList<ReportComponentDescriptor> GetComponents() => Components;
}
