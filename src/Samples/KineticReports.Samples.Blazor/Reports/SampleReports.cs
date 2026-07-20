namespace KineticReports.Samples.Blazor.Reports;

using KineticReports.Core.Definition;

/// <summary>
/// Generates sample reports demonstrating different layout components and styles.
/// </summary>
public static class SampleReports
{
    /// <summary>
    /// A simple text-based report demonstrating basic layout.
    /// </summary>
    public static ReportDefinition CreateSimpleTextReport()
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "simple-text-report",
            Name = "Simple Text Report",
            Description = "Basic layout with text elements and styling",
            Parameters = [],
            DataSources = [],
            Styles = []
        };
    }

    /// <summary>
    /// A report demonstrating tables and data grids.
    /// </summary>
    public static ReportDefinition CreateTableReport()
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "table-report",
            Name = "Sales Data Table",
            Description = "Demonstrates table layout with columns and rows for tabular data",
            Parameters = [],
            DataSources = [],
            Styles = []
        };
    }

    /// <summary>
    /// A report demonstrating banded layout (repeating sections).
    /// </summary>
    public static ReportDefinition CreateBandedReport()
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "banded-report",
            Name = "Customer Report with Grouping",
            Description = "Shows banded layout with repeating sections for hierarchical data",
            Parameters = [],
            DataSources = [],
            Styles = []
        };
    }

    /// <summary>
    /// A multi-section report demonstrating report sections and styling.
    /// </summary>
    public static ReportDefinition CreateMultiSectionReport()
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "multi-section-report",
            Name = "Company Report with Multiple Sections",
            Description = "Demonstrates multiple sections with grouping and pagination",
            Parameters = [],
            DataSources = [],
            Styles = []
        };
    }

    /// <summary>
    /// A styled report demonstrating the style system.
    /// </summary>
    public static ReportDefinition CreateStyledReport()
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "styled-report",
            Name = "Styled Report Example",
            Description = "Showcases typography, colors, borders, and the style cascade system",
            Parameters = [],
            DataSources = [],
            Styles = []
        };
    }

    /// <summary>
    /// Gets all available sample reports.
    /// </summary>
    public static IReadOnlyList<ReportDefinition> GetAllSampleReports()
    {
        return new[]
        {
            CreateSimpleTextReport(),
            CreateTableReport(),
            CreateBandedReport(),
            CreateMultiSectionReport(),
            CreateStyledReport()
        };
    }
}
