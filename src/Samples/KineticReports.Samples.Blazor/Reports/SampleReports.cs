namespace KineticReports.Samples.Blazor.Reports;

using KineticReports.Core.Definition;

/// <summary>
/// Generates sample reports demonstrating different layout components and styles.
/// </summary>
public static class SampleReports
{
    private const string SqlLiteProviderType = "SqlLite";
    private const string InMemoryProviderType = "SampleInMemory";
    private const string SampleConnectionString = "Data Source=|DataDirectory|\\sample-reports.db";

    /// <summary>
    /// A simple text-based report demonstrating basic layout.
    /// </summary>
    public static ReportDefinition CreateSimpleTextReport()
        => CreateSimpleTextReport(SqlLiteProviderType);

    private static ReportDefinition CreateSimpleTextReport(string providerType)
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = BuildReportId("simple-text-report", providerType),
            Name = BuildReportName("Simple Text Report", providerType),
            Description = "Basic layout with text elements and styling",
            Parameters = [],
            DataSources =
            [
                CreateDataSource("quote-daily", "Daily Quote", providerType)
            ],
            Styles = []
        };
    }

    /// <summary>
    /// A report demonstrating tables and data grids.
    /// </summary>
    public static ReportDefinition CreateTableReport()
        => CreateTableReport(SqlLiteProviderType);

    private static ReportDefinition CreateTableReport(string providerType)
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = BuildReportId("table-report", providerType),
            Name = BuildReportName("Sales Data Table", providerType),
            Description = "Demonstrates table layout with columns and rows for tabular data",
            Parameters = [],
            DataSources =
            [
                CreateDataSource("sales-orders", "Sales Orders", providerType)
            ],
            Styles = []
        };
    }

    /// <summary>
    /// A report demonstrating banded layout (repeating sections).
    /// </summary>
    public static ReportDefinition CreateBandedReport()
        => CreateBandedReport(SqlLiteProviderType);

    private static ReportDefinition CreateBandedReport(string providerType)
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = BuildReportId("banded-report", providerType),
            Name = BuildReportName("Customer Report with Grouping", providerType),
            Description = "Shows banded layout with repeating sections for hierarchical data",
            Parameters = [],
            DataSources =
            [
                CreateDataSource("customer-activity", "Customer Activity", providerType)
            ],
            Styles = []
        };
    }

    /// <summary>
    /// A multi-section report demonstrating report sections and styling.
    /// </summary>
    public static ReportDefinition CreateMultiSectionReport()
        => CreateMultiSectionReport(SqlLiteProviderType);

    private static ReportDefinition CreateMultiSectionReport(string providerType)
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = BuildReportId("multi-section-report", providerType),
            Name = BuildReportName("Company Report with Multiple Sections", providerType),
            Description = "Demonstrates multiple sections with grouping and pagination",
            Parameters = [],
            DataSources =
            [
                CreateDataSource("kpi-summary", "KPI Summary", providerType),
                CreateDataSource("regional-performance", "Regional Performance", providerType)
            ],
            Styles = []
        };
    }

    /// <summary>
    /// A styled report demonstrating the style system.
    /// </summary>
    public static ReportDefinition CreateStyledReport()
        => CreateStyledReport(SqlLiteProviderType);

    private static ReportDefinition CreateStyledReport(string providerType)
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = BuildReportId("styled-report", providerType),
            Name = BuildReportName("Styled Report Example", providerType),
            Description = "Showcases typography, colors, borders, and the style cascade system",
            Parameters = [],
            DataSources =
            [
                CreateDataSource("style-showcase", "Style Showcase Data", providerType)
            ],
            Styles = []
        };
    }

    /// <summary>
    /// A report demonstrating expression evaluation against row fields and parameters.
    /// </summary>
    public static ReportDefinition CreateExpressionReport()
        => CreateExpressionReport(SqlLiteProviderType);

    private static ReportDefinition CreateExpressionReport(string providerType)
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = BuildReportId("expression-report", providerType),
            Name = BuildReportName("Expression Evaluation Report", providerType),
            Description = "Shows how {FieldName} expressions resolve from the current row and parameters",
            Parameters = [],
            DataSources =
            [
                CreateDataSource("expression-demo", "Expression Demo Rows", providerType)
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
            CreateSimpleTextReport(SqlLiteProviderType),
            CreateSimpleTextReport(InMemoryProviderType),
            CreateTableReport(SqlLiteProviderType),
            CreateTableReport(InMemoryProviderType),
            CreateBandedReport(SqlLiteProviderType),
            CreateBandedReport(InMemoryProviderType),
            CreateMultiSectionReport(SqlLiteProviderType),
            CreateMultiSectionReport(InMemoryProviderType),
            CreateStyledReport(SqlLiteProviderType),
            CreateStyledReport(InMemoryProviderType),
            CreateExpressionReport(SqlLiteProviderType),
            CreateExpressionReport(InMemoryProviderType)
        ];
    }

    private static DataSourceDefinition CreateDataSource(string id, string name, string providerType)
    {
        var properties = new Dictionary<string, string>();

        if (string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase))
        {
            properties["ConnectionString"] = SampleConnectionString;
            properties["Query"] = BuildQuery(id);
        }

        return new DataSourceDefinition
        {
            Id = id,
            Name = name,
            ProviderType = providerType,
            Properties = properties
        };
    }

    private static string BuildReportId(string baseId, string providerType)
        => $"{baseId}-{GetProviderSuffix(providerType)}";

    private static string BuildReportName(string baseName, string providerType)
        => $"{baseName} ({GetProviderLabel(providerType)})";

    private static string GetProviderSuffix(string providerType)
        => string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase)
            ? "sqlite"
            : "inmemory";

    private static string GetProviderLabel(string providerType)
        => string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase)
            ? "SQLite"
            : "In-Memory";

    private static string BuildQuery(string dataSourceId) => dataSourceId switch
    {
        "quote-daily" => """
            SELECT
                heading AS Heading,
                quote AS Quote,
                generated_on AS GeneratedOn,
                author AS Author
            FROM quote_daily
            """,

        "sales-orders" => """
            SELECT
                order_id AS OrderId,
                region AS Region,
                sales_person AS SalesPerson,
                amount AS Amount,
                status AS Status
            FROM sales_orders
            ORDER BY order_id
            """,

        "customer-activity" => """
            SELECT
                customer_name AS CustomerName,
                tier AS Tier,
                last_invoice AS LastInvoice,
                balance AS Balance,
                assigned_rep AS AssignedRep
            FROM customer_activity
            ORDER BY customer_name
            """,

        "kpi-summary" => """
            SELECT
                metric AS Metric,
                value AS Value,
                trend AS Trend
            FROM kpi_summary
            ORDER BY metric
            """,

        "regional-performance" => """
            SELECT
                region AS Region,
                quota AS Quota,
                actual AS Actual,
                attainment AS Attainment
            FROM regional_performance
            ORDER BY region
            """,

        "style-showcase" => """
            SELECT
                label AS Label,
                preview AS Preview
            FROM style_showcase
            ORDER BY label
            """,

        "expression-demo" => """
            SELECT
                customer_name AS CustomerName,
                tier AS Tier,
                balance AS Balance,
                assigned_rep AS AssignedRep
            FROM expression_demo
            ORDER BY customer_name
            """,

        _ => "SELECT 'Unknown data source' AS Message"
    };
}
