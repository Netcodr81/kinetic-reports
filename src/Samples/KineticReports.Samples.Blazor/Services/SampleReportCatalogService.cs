namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Authoring.Serialization;
using KineticReports.Core.Definition;
using Microsoft.AspNetCore.Hosting;

/// <summary>
/// Provides report definitions for the sample app from JSON files and code-built templates.
/// </summary>
public interface ISampleReportCatalogService
{
    IReadOnlyList<SampleReportTemplateDescriptor> GetTemplates();

    Task<ReportDefinition> LoadAsync(string templateId, string providerType, CancellationToken cancellationToken = default);

    Task<string> LoadJsonAsync(string templateId, string providerType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Describes a report template available in the sample app.
/// </summary>
public sealed record SampleReportTemplateDescriptor(
    string Id,
    string Name,
    string Description,
    string SourceType);

internal sealed class SampleReportCatalogService : ISampleReportCatalogService
{
    private const string InMemoryProviderType = "InMemory";
    private const string SqlLiteProviderType = "SqlLite";
    private const string SqliteAliasProviderType = "SQLite";

    private static readonly IReadOnlyList<SampleReportTemplateDescriptor> Templates =
    [
        new SampleReportTemplateDescriptor(
            Id: "json-quarterly-business-review",
            Name: "Quarterly Business Review (JSON file)",
            Description: "A multi-page leadership report loaded directly as a canonical ReportDefinition JSON file.",
            SourceType: "JSON"),
        new SampleReportTemplateDescriptor(
            Id: "code-operations-executive-pack",
            Name: "Operations Executive Pack (code)",
            Description: "A multi-page operational report built directly in code as a canonical ReportDefinition.",
            SourceType: "Code")
    ];

    private readonly IWebHostEnvironment _environment;
    private readonly IReportDefinitionSerializer _reportDefinitionSerializer;

    public SampleReportCatalogService(
        IWebHostEnvironment environment,
        IReportDefinitionSerializer reportDefinitionSerializer)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _reportDefinitionSerializer = reportDefinitionSerializer ?? throw new ArgumentNullException(nameof(reportDefinitionSerializer));
    }

    public IReadOnlyList<SampleReportTemplateDescriptor> GetTemplates() => Templates;

    public async Task<ReportDefinition> LoadAsync(string templateId, string providerType, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedProviderType = NormalizeProviderType(providerType);

        var definition = templateId switch
        {
            "json-quarterly-business-review" => await LoadJsonTemplateAsync(cancellationToken).ConfigureAwait(false),
            "code-operations-executive-pack" => BuildOperationsExecutivePackDefinition(),
            _ => throw new InvalidOperationException($"Unknown report template '{templateId}'.")
        };

        return ApplyProvider(definition, normalizedProviderType);
    }

    public async Task<string> LoadJsonAsync(string templateId, string providerType, CancellationToken cancellationToken = default)
    {
        var definition = await LoadAsync(templateId, providerType, cancellationToken).ConfigureAwait(false);
        return _reportDefinitionSerializer.Serialize(definition);
    }

    private async Task<ReportDefinition> LoadJsonTemplateAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(_environment.ContentRootPath, "Reports", "Definitions", "quarterly-business-review.report.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Report JSON file was not found: {path}");

        var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        return _reportDefinitionSerializer.Deserialize(json);
    }

    private static ReportDefinition BuildOperationsExecutivePackDefinition()
    {
        return new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "code-operations-executive-pack",
            Name = "Operations Executive Pack",
            Author = "KineticReports Samples",
            Description = "A multi-page operational report built directly in code and executed through Core.",
            Parameters = [],
            DataSources =
            [
                CreateDataSource("implementation-milestones", "Implementation Milestones"),
                CreateDataSource("support-escalations", "Open Escalations"),
                CreateDataSource("renewal-pipeline", "Renewal Pipeline")
            ],
            Styles = [

            ],
            Layout = new ReportLayoutDefinition
            {
                PageHeader =
                [
                    TextItem("ops-title", "Operations Executive Pack"),
                    TextItem("ops-subtitle", "Delivery milestones, active escalations, and renewal pipeline coverage.")
                ],
                Body =
                [
                    TextItem("ops-intro", "This operational brief is authored in code and then executed by the default Core pipeline."),
                    TextItem("ops-milestones-title", "Implementation Milestones"),
                    TableItem(
                        id: "ops-milestones-table",
                        dataSourceId: "implementation-milestones",
                        columns:
                        [
                            Column("Customer", "{Customer}", 180f, 2f),
                            Column("Workstream", "{Workstream}", 170f, 2f),
                            Column("Stage", "{Stage}", 110f),
                            Column("Go-Live", "{GoLiveDate}", 110f),
                            Column("Sponsor", "{ExecutiveSponsor}", 140f)
                        ]),
                    TextItem("ops-escalations-title", "Open Escalations"),
                    TableItem(
                        id: "ops-escalations-table",
                        dataSourceId: "support-escalations",
                        columns:
                        [
                            Column("Ticket", "{Ticket}", 90f),
                            Column("Customer", "{Customer}", 170f, 2f),
                            Column("Severity", "{Severity}", 90f),
                            Column("Owner", "{Owner}", 130f),
                            Column("Days Open", "{DaysOpen}", 90f),
                            Column("Status", "{Status}", 180f, 2f)
                        ]),
                    new ReportLayoutItemDefinition { Id = "ops-page-break", Kind = ReportLayoutItemKind.PageBreak },
                    TextItem("ops-renewal-title", "Renewal Pipeline"),
                    TextItem("ops-renewal-copy", "Page two focuses on upcoming contract renewals and measurable expansion potential for revenue planning."),
                    TableItem(
                        id: "ops-renewal-table",
                        dataSourceId: "renewal-pipeline",
                        columns:
                        [
                            Column("Customer", "{Customer}", 180f, 2f),
                            Column("Quarter", "{RenewalQuarter}", 110f),
                            Column("ARR", "{ARR}", 110f),
                            Column("Expansion", "{ExpansionPotential}", 120f),
                            Column("Stage", "{Stage}", 150f, 2f)
                        ],
                        footerAggregates:
                        [
                            Aggregate(2, "{ARR}"),
                            Aggregate(3, "{ExpansionPotential}")
                        ],
                        footerLabel: "Pipeline total")
                ],
                PageFooter =
                [
                    TextItem("ops-footer-page", "Page {PageNumber}"),
                    TextItem("ops-footer-generated", "Generated {CurrentDate}")
                ]
            }
        };
    }

    private static DataSourceDefinition CreateDataSource(string id, string name)
    {
        return new DataSourceDefinition
        {
            Id = id,
            Name = name,
            ProviderType = InMemoryProviderType,
            Properties = new Dictionary<string, string>()
        };
    }

    private static ReportDefinition ApplyProvider(ReportDefinition definition, string providerType)
    {
        var updatedDataSources = definition.DataSources
            .Select(source => source with
            {
                ProviderType = providerType,
                Properties = BuildDataSourceProperties(source.Id, providerType)
            })
            .ToList();

        return definition with
        {
            Name = $"{definition.Name} ({GetProviderDisplayName(providerType)})",
            DataSources = updatedDataSources
        };
    }

    private static IReadOnlyDictionary<string, string> BuildDataSourceProperties(string dataSourceId, string providerType)
    {
        if (string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(providerType, SqliteAliasProviderType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(providerType, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Query"] = BuildSqlLiteQuery(dataSourceId)
            };
        }

        return new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static string BuildSqlLiteQuery(string dataSourceId) => dataSourceId switch
    {
        "regional-performance" => """
            SELECT Region, Revenue, Target, Variance, Accounts
            FROM RegionalPerformance
            ORDER BY Revenue DESC;
            """,
        "top-accounts" => """
            SELECT Customer, Segment, AnnualRevenue, RenewalDate, Owner
            FROM TopAccounts
            ORDER BY AnnualRevenue DESC;
            """,
        "monthly-trend" => """
            SELECT [Month], Billings, GrossMargin, NewLogos
            FROM MonthlyTrend
            ORDER BY MonthOrder;
            """,
        "sales-orders-detail" => """
            SELECT OrderNumber, Customer, Region, OrderDate, Amount, Status
            FROM SalesOrdersDetail
            ORDER BY OrderDate, OrderNumber;
            """,
        "implementation-milestones" => """
            SELECT Customer, Workstream, Stage, GoLiveDate, ExecutiveSponsor
            FROM ImplementationMilestones
            ORDER BY GoLiveDate, Customer;
            """,
        "support-escalations" => """
            SELECT Ticket, Customer, Severity, Owner, DaysOpen, Status
            FROM SupportEscalations
            ORDER BY DaysOpen DESC, Ticket;
            """,
        "renewal-pipeline" => """
            SELECT Customer, RenewalQuarter, ARR, ExpansionPotential, Stage
            FROM RenewalPipeline
            ORDER BY RenewalQuarter, Customer;
            """,
        _ => throw new InvalidOperationException($"No SQLite query mapping is defined for data source '{dataSourceId}'.")
    };

    private static string NormalizeProviderType(string providerType)
    {
        if (string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(providerType, SqliteAliasProviderType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(providerType, "SqlServer", StringComparison.OrdinalIgnoreCase))
            return SqlLiteProviderType;

        return InMemoryProviderType;
    }

    private static string GetProviderDisplayName(string providerType)
    {
        return string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(providerType, SqliteAliasProviderType, StringComparison.OrdinalIgnoreCase)
            ? "SQLite"
            : "In-Memory";
    }

    private static ReportLayoutItemDefinition TextItem(string id, string text)
        => new() { Id = id, Kind = ReportLayoutItemKind.Text, Text = text };

    private static ReportLayoutItemDefinition TableItem(
        string id,
        string dataSourceId,
        IReadOnlyList<ReportLayoutTableColumnDefinition> columns,
        IReadOnlyList<ReportLayoutTableAggregateDefinition>? footerAggregates = null,
        string? footerLabel = "Total",
        int footerLabelColumnIndex = 0)
        => new()
        {
            Id = id,
            Kind = ReportLayoutItemKind.Table,
            DataSourceId = dataSourceId,
            Columns = columns,
            FooterAggregates = footerAggregates ?? [],
            FooterLabel = footerLabel,
            FooterLabelColumnIndex = footerLabelColumnIndex
        };

    private static ReportLayoutTableColumnDefinition Column(string header, string valueExpression, float minWidth, float grow = 1f)
        => new() { Header = header, ValueExpression = valueExpression, MinWidth = minWidth, Grow = grow };

    private static ReportLayoutTableAggregateDefinition Aggregate(int columnIndex, string valueExpression)
        => new() { ColumnIndex = columnIndex, ValueExpression = valueExpression, Kind = "Sum", FormatString = "0.##" };
}
