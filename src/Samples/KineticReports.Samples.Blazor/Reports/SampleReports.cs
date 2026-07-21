namespace KineticReports.Samples.Blazor.Reports;

using KineticReports.Core.Definition;

/// <summary>
/// Generates sample reports demonstrating different layout components and styles.
/// </summary>
public static class SampleReports
{
    private const string SampleProviderType = "SampleInMemory";

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
            DataSources =
            [
                CreateDataSource("quote-daily", "Daily Quote")
            ],
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
            DataSources =
            [
                CreateDataSource("sales-orders", "Sales Orders")
            ],
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
            DataSources =
            [
                CreateDataSource("customer-activity", "Customer Activity")
            ],
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
            DataSources =
            [
                CreateDataSource("kpi-summary", "KPI Summary"),
                CreateDataSource("regional-performance", "Regional Performance")
            ],
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
            DataSources =
            [
                CreateDataSource("style-showcase", "Style Showcase Data")
            ],
            Styles = []
        };
    }

    /// <summary>
    /// A report demonstrating expression evaluation against row fields and parameters.
    /// </summary>
    public static ReportDefinition CreateExpressionReport()
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "expression-report",
            Name = "Expression Evaluation Report",
            Description = "Shows how {FieldName} expressions resolve from the current row and parameters",
            Parameters = [],
            DataSources =
            [
                CreateDataSource("expression-demo", "Expression Demo Rows")
            ],
            Styles = []
        };
    }

    /// <summary>
    /// Gets all available sample reports.
    /// </summary>
    public static IReadOnlyList<ReportDefinition> GetAllSampleReports()
    {
        return
        [
            CreateSimpleTextReport(),
            CreateTableReport(),
            CreateBandedReport(),
            CreateMultiSectionReport(),
            CreateStyledReport(),
            CreateExpressionReport()
        ];
    }

    private static DataSourceDefinition CreateDataSource(string id, string name)
    {
        return new DataSourceDefinition
        {
            Id = id,
            Name = name,
            ProviderType = SampleProviderType,
            Properties = new Dictionary<string, string>()
        };
    }
}
