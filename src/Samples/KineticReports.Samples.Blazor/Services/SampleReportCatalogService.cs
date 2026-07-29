namespace KineticReports.Samples.Blazor.Services;

using System.Text.Json;
using KineticReports.Core.Authoring.Compilation;
using KineticReports.Core.Authoring.Components;
using KineticReports.Core.Authoring.Documents;
using KineticReports.Core.Authoring.Serialization;
using KineticReports.Core.Definition;
using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;
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
    private const string EmbeddedDemoImagePlaceholder = "__EMBED_DEMO_IMAGE__";

    private const string InMemorySelection = "InMemory";
    private const string SqlLiteSelection = "SqlLite";
    private const string SqliteAliasSelection = "SQLite";

    private const string PocoProviderType = "Poco";
    private const string SqlLiteProviderType = "SqlLite";

    private const string InMemorySourceName = "DefaultInMemory";
    private const string SqlLiteSourceName = "SampleSqlLite";

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
            SourceType: "Code"),
        new SampleReportTemplateDescriptor(
            Id: "fluent-operations-executive-pack",
            Name: "Operations Executive Pack (fluent builder)",
            Description: "A multi-page operational report authored with the fluent ReportDesignerDocumentBuilder and compiled to ReportDefinition.",
            SourceType: "Fluent")
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
            "fluent-operations-executive-pack" => BuildOperationsExecutivePackFluentDefinition(),
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
        var definition = _reportDefinitionSerializer.Deserialize(json);
        return ApplySharedSampleStyles(ApplyEmbeddedImageSource(definition));
    }

    private ReportDefinition BuildOperationsExecutivePackDefinition()
    {
        var embeddedDemoImageSource = BuildEmbeddedDemoImageSourceKey();

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
            Styles = BuildSharedSampleStyles(),
            Layout = new ReportLayoutDefinition
            {
                PageHeader =
                [
                    TextItem("ops-title", "Operations Executive Pack", textStyleId: "text-title"),
                    TextItem("ops-subtitle", "Delivery milestones, active escalations, and renewal pipeline coverage.", textStyleId: "text-subtitle")
                ],
                Body =
                [
                    TextItem("ops-intro", "This operational brief is authored in code and then executed by the default Core pipeline.", textStyleId: "text-body"),
                    TextItem("ops-milestones-title", "Implementation Milestones", textStyleId: "text-section-title"),
                    TableItem(
                        id: "ops-milestones-table",
                        dataSourceId: "implementation-milestones",
                        regionStyleId: "table-region",
                        headerCellStyleId: "table-header-cell",
                        dataCellStyleId: "table-data-cell",
                        footerCellStyleId: "table-data-cell",
                        columns:
                        [
                            Column("Customer", "{Customer}", 180f, 2f),
                            Column("Workstream", "{Workstream}", 170f, 2f),
                            Column("Stage", "{Stage}", 110f),
                            Column("Go-Live", "{GoLiveDate}", 110f),
                            Column("Sponsor", "{ExecutiveSponsor}", 140f)
                        ]),
                    TextItem("ops-escalations-title", "Open Escalations", textStyleId: "text-section-title"),
                    TableItem(
                        id: "ops-escalations-table",
                        dataSourceId: "support-escalations",
                        regionStyleId: "table-region",
                        headerCellStyleId: "table-header-cell",
                        dataCellStyleId: "table-data-cell",
                        footerCellStyleId: "table-data-cell",
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
                    TextItem("ops-renewal-title", "Renewal Pipeline", textStyleId: "text-section-title"),
                    TextItem("ops-renewal-copy", "Page two focuses on upcoming contract renewals and measurable expansion potential for revenue planning.", textStyleId: "text-body"),
                    TableItem(
                        id: "ops-renewal-table",
                        dataSourceId: "renewal-pipeline",
                        regionStyleId: "table-region",
                        headerCellStyleId: "table-header-cell",
                        dataCellStyleId: "table-data-cell",
                        footerCellStyleId: "table-data-cell",
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
                        footerLabel: "Pipeline total"),
                    new ReportLayoutItemDefinition { Id = "ops-page-break-image", Kind = ReportLayoutItemKind.PageBreak },
                    TextItem("ops-image-title", "Product Showcase Image", textStyleId: "text-section-title"),
                    TextItem("ops-image-copy", "This third page demonstrates image rendering using the sample asset bundled with the Blazor app.", textStyleId: "text-body"),
                    new ReportLayoutItemDefinition
                    {
                        Id = "ops-demo-image",
                        Kind = ReportLayoutItemKind.Image,
                        SourceKey = "/images/demo-image.jpg"
                    },
                    TextItem("ops-image-embedded-title", "Embedded Image (Data URI)", textStyleId: "text-section-title"),
                    TextItem("ops-image-embedded-copy", "This second image uses ReportImageSourceKeyHelper to encode the file into a data URI before rendering.", textStyleId: "text-body"),
                    new ReportLayoutItemDefinition
                    {
                        Id = "ops-demo-image-embedded",
                        Kind = ReportLayoutItemKind.Image,
                        SourceKey = embeddedDemoImageSource
                    }
                ],
                PageFooter =
                [
                    TextItem("ops-footer-page", "Page {PageNumber}", textStyleId: "text-footer"),
                    TextItem("ops-footer-generated", "Generated {CurrentDate}", textStyleId: "text-footer")
                ]
            }
        };
    }

    private ReportDefinition BuildOperationsExecutivePackFluentDefinition()
    {
        var embeddedDemoImageSource = BuildEmbeddedDemoImageSourceKey();

        var implementationColumns = new List<ReportLayoutTableColumnDefinition>
        {
            Column("Customer", "{Customer}", 180f, 2f),
            Column("Workstream", "{Workstream}", 170f, 2f),
            Column("Stage", "{Stage}", 110f),
            Column("Go-Live", "{GoLiveDate}", 110f),
            Column("Sponsor", "{ExecutiveSponsor}", 140f)
        };

        var renewalColumns = new List<ReportLayoutTableColumnDefinition>
        {
            Column("Customer", "{Customer}", 180f, 2f),
            Column("Quarter", "{RenewalQuarter}", 110f),
            Column("ARR", "{ARR}", 110f),
            Column("Expansion", "{ExpansionPotential}", 120f),
            Column("Stage", "{Stage}", 150f, 2f)
        };

        var renewalAggregates = new List<ReportLayoutTableAggregateDefinition>
        {
            Aggregate(2, "{ARR}"),
            Aggregate(3, "{ExpansionPotential}")
        };

        var documentBuilder = ReportDesignerDocumentBuilder
            .Create("fluent-operations-executive-pack", "Operations Executive Pack (Fluent Builder)")
            .WithAuthor("KineticReports Samples")
            .WithDescription("A multi-page operational report authored with the fluent component builder and compiled into the canonical report definition.")
            .AddDataSource(CreateDataSource("implementation-milestones", "Implementation Milestones"))
            .AddDataSource(CreateDataSource("renewal-pipeline", "Renewal Pipeline"));

        foreach (var style in BuildSharedSampleStyles())
            documentBuilder.AddStyle(style);

        documentBuilder
            .AddComponent("ops-header", ReportComponentType.Header, header =>
            {
                header
                    .AddChild("ops-title", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Operations Executive Pack")
                            .WithProperty("TextStyleId", "text-title"))
                    .AddChild("ops-subtitle", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Delivery milestones, renewal readiness, and product showcase coverage.")
                            .WithProperty("TextStyleId", "text-subtitle"));
            })
            .AddComponent("ops-intro", ReportComponentType.Text, text =>
                text.WithBinding("Text", "This fluent-builder sample compiles component metadata into canonical layout items before execution.")
                    .WithProperty("TextStyleId", "text-body"))
            .AddComponent("ops-milestones-title", ReportComponentType.Text, text =>
                text.WithBinding("Text", "Implementation Milestones")
                    .WithProperty("TextStyleId", "text-section-title"))
            .AddComponent("ops-milestones-table", ReportComponentType.Table, table =>
                table.BoundToDataSource("implementation-milestones")
                    .WithProperty("ColumnsJson", JsonSerializer.Serialize(implementationColumns))
                    .WithProperty("RegionStyleId", "table-region")
                    .WithProperty("HeaderCellStyleId", "table-header-cell")
                    .WithProperty("DataCellStyleId", "table-data-cell")
                    .WithProperty("FooterCellStyleId", "table-data-cell"))
            .AddComponent("ops-page-break", ReportComponentType.PageBreak)
            .AddComponent("ops-renewal-title", ReportComponentType.Text, text =>
                text.WithBinding("Text", "Renewal Pipeline")
                    .WithProperty("TextStyleId", "text-section-title"))
            .AddComponent("ops-renewal-table", ReportComponentType.Table, table =>
                table.BoundToDataSource("renewal-pipeline")
                    .WithProperty("ColumnsJson", JsonSerializer.Serialize(renewalColumns))
                    .WithProperty("FooterAggregatesJson", JsonSerializer.Serialize(renewalAggregates))
                    .WithProperty("FooterLabel", "Pipeline total")
                    .WithProperty("RegionStyleId", "table-region")
                    .WithProperty("HeaderCellStyleId", "table-header-cell")
                    .WithProperty("DataCellStyleId", "table-data-cell")
                    .WithProperty("FooterCellStyleId", "table-data-cell"))
            .AddComponent("ops-page-break-image", ReportComponentType.PageBreak)
            .AddComponent("ops-image-title", ReportComponentType.Text, text =>
                text.WithBinding("Text", "Product Showcase Image")
                    .WithProperty("TextStyleId", "text-section-title"))
            .AddComponent("ops-demo-image", ReportComponentType.Image, image =>
                image.WithBinding("SourceKey", "/images/demo-image.jpg"))
            .AddComponent("ops-image-embedded-title", ReportComponentType.Text, text =>
                text.WithBinding("Text", "Embedded Image (Data URI)")
                    .WithProperty("TextStyleId", "text-section-title"))
            .AddComponent("ops-demo-image-embedded", ReportComponentType.Image, image =>
                image.WithBinding("SourceKey", embeddedDemoImageSource))
            .AddComponent("ops-footer", ReportComponentType.Footer, footer =>
            {
                footer
                    .AddChild("ops-footer-page", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Page {PageNumber}")
                            .WithProperty("TextStyleId", "text-footer"))
                    .AddChild("ops-footer-generated", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Generated {CurrentDate}")
                            .WithProperty("TextStyleId", "text-footer"));
            });

        var compiler = new DefaultDesignerDocumentCompiler();
        var definition = compiler.Compile(documentBuilder.Build());
        return ApplySharedSampleStyles(definition);
    }

    private string BuildEmbeddedDemoImageSourceKey()
    {
        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var demoImagePath = Path.Combine(webRoot, "images", "demo-image.jpg");
        if (!File.Exists(demoImagePath))
            return "/images/demo-image.jpg";

        return ReportImageSourceKeyHelper.CreateEmbeddedSourceKeyFromFile(demoImagePath);
    }

    private ReportDefinition ApplyEmbeddedImageSource(ReportDefinition definition)
    {
        if (definition.Layout is null || definition.Layout.Body.Count == 0)
            return definition;

        var embeddedSource = BuildEmbeddedDemoImageSourceKey();
        var body = definition.Layout.Body
            .Select(item => item.SourceKey == EmbeddedDemoImagePlaceholder
                ? item with { SourceKey = embeddedSource }
                : item)
            .ToList();

        return definition with
        {
            Layout = definition.Layout with
            {
                Body = body
            }
        };
    }

    private static ReportDefinition ApplySharedSampleStyles(ReportDefinition definition)
    {
        return definition with
        {
            Styles = BuildSharedSampleStyles()
        };
    }

    private static IReadOnlyList<StyleDefinition> BuildSharedSampleStyles()
    {
        return
        [
            new StyleDefinition
            {
                Id = "text-body",
                Name = "Body Text",
                Typography = new Typography
                {
                    Family = "Segoe UI",
                    Size = 11f,
                    LineHeight = 1.35f,
                    Color = new Color(255, 37, 50, 65)
                }
            },
            new StyleDefinition
            {
                Id = "text-title",
                Name = "Title Text",
                BasedOn = "text-body",
                Typography = new Typography
                {
                    Size = 22f,
                    Weight = FontWeight.Bold
                }
            },
            new StyleDefinition
            {
                Id = "text-subtitle",
                Name = "Subtitle Text",
                BasedOn = "text-body",
                Typography = new Typography
                {
                    Size = 12f
                }
            },
            new StyleDefinition
            {
                Id = "text-section-title",
                Name = "Section Title Text",
                BasedOn = "text-body",
                Typography = new Typography
                {
                    Size = 14f,
                    Weight = FontWeight.SemiBold
                }
            },
            new StyleDefinition
            {
                Id = "table-region",
                Name = "Table Region",
                Padding = new Thickness(0f, 4f, 0f, 10f)
            },
            new StyleDefinition
            {
                Id = "table-header-cell",
                Name = "Table Header Cell",
                BasedOn = "text-body",
                Typography = new Typography
                {
                    Weight = FontWeight.SemiBold,
                    Color = Color.White
                },
                Background = new Color(255, 32, 92, 156)
            },
            new StyleDefinition
            {
                Id = "table-data-cell",
                Name = "Table Data Cell",
                BasedOn = "text-body",
                Typography = new Typography
                {
                    Size = 10f
                }
            },
            new StyleDefinition
            {
                Id = "text-footer",
                Name = "Footer Text",
                BasedOn = "text-body",
                Typography = new Typography
                {
                    Size = 10f
                }
            }
        ];
    }

    private static DataSourceDefinition CreateDataSource(string id, string name)
    {
        return new DataSourceDefinition
        {
            Id = id,
            Name = name,
            ProviderType = PocoProviderType,
            SourceName = InMemorySourceName,
            Properties = new Dictionary<string, string>()
        };
    }

    private static ReportDefinition ApplyProvider(ReportDefinition definition, string providerType)
    {
        var usesSqlLite = string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase);

        var updatedDataSources = definition.DataSources
            .Select(source => source with
            {
                ProviderType = usesSqlLite ? SqlLiteProviderType : PocoProviderType,
                SourceName = usesSqlLite ? SqlLiteSourceName : InMemorySourceName,
                Properties = BuildDataSourceProperties(source.Id, usesSqlLite)
            })
            .ToList();

        return definition with
        {
            Name = $"{definition.Name} ({GetProviderDisplayName(providerType)})",
            DataSources = updatedDataSources
        };
    }

    private static IReadOnlyDictionary<string, string> BuildDataSourceProperties(string dataSourceId, bool usesSqlLite)
    {
        if (usesSqlLite)
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
        if (string.Equals(providerType, SqlLiteSelection, StringComparison.OrdinalIgnoreCase)
            || string.Equals(providerType, SqliteAliasSelection, StringComparison.OrdinalIgnoreCase)
            || string.Equals(providerType, "SqlServer", StringComparison.OrdinalIgnoreCase))
            return SqlLiteProviderType;

        return InMemorySelection;
    }

    private static string GetProviderDisplayName(string providerType)
    {
        return string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(providerType, SqliteAliasSelection, StringComparison.OrdinalIgnoreCase)
            ? "SQLite"
            : "In-Memory";
    }

    private static ReportLayoutItemDefinition TextItem(
        string id,
        string text,
        string? textStyleId = null,
        string? blockStyleId = null)
        => new()
        {
            Id = id,
            Kind = ReportLayoutItemKind.Text,
            Text = text,
            TextStyleId = textStyleId,
            BlockStyleId = blockStyleId
        };

    private static ReportLayoutItemDefinition TableItem(
        string id,
        string dataSourceId,
        IReadOnlyList<ReportLayoutTableColumnDefinition> columns,
        string? regionStyleId = null,
        string? tableStyleId = null,
        string? headerCellStyleId = null,
        string? dataCellStyleId = null,
        string? footerCellStyleId = null,
        IReadOnlyList<ReportLayoutTableAggregateDefinition>? footerAggregates = null,
        string? footerLabel = "Total",
        int footerLabelColumnIndex = 0)
        => new()
        {
            Id = id,
            Kind = ReportLayoutItemKind.Table,
            DataSourceId = dataSourceId,
            RegionStyleId = regionStyleId,
            TableStyleId = tableStyleId,
            HeaderCellStyleId = headerCellStyleId,
            DataCellStyleId = dataCellStyleId,
            FooterCellStyleId = footerCellStyleId,
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
