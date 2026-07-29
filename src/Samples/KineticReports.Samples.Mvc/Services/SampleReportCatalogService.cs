namespace KineticReports.Samples.Mvc.Services;

using System.Text.Json;
using KineticReports.Core.Authoring.Compilation;
using KineticReports.Core.Authoring.Components;
using KineticReports.Core.Authoring.Documents;
using KineticReports.Core.Authoring.Serialization;
using KineticReports.Core.Definition;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
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

    private static readonly IReadOnlyList<BarcodeCatalogEntry> BarcodeCatalogEntries =
    [
        new(BarcodeSymbology.QrCode, "QR Code", "https://kineticreports.dev"),
        new(BarcodeSymbology.MicroQr, "Micro QR", "MQR-42"),
        new(BarcodeSymbology.Code128, "Code 128", "SO-2026-001"),
        new(BarcodeSymbology.Code39, "Code 39", "INV-4472"),
        new(BarcodeSymbology.Ean13, "EAN-13", "5901234123457"),
        new(BarcodeSymbology.Ean8, "EAN-8", "55123457"),
        new(BarcodeSymbology.UpcA, "UPC-A", "036000291452"),
        new(BarcodeSymbology.UpcE, "UPC-E", "01234565"),
        new(BarcodeSymbology.Itf, "ITF (Interleaved 2 of 5)", "12345670"),
        new(BarcodeSymbology.Pdf417, "PDF417", "PO-4472|Dock-07|Gate-C"),
        new(BarcodeSymbology.DataMatrix, "Data Matrix", "SN:KR-2026-00091"),
        new(BarcodeSymbology.Codabar, "Codabar", "A40156B")
    ];

    private static readonly IReadOnlyList<ChartCatalogEntry> ChartCatalogEntries =
    [
        new(
            ChartTypeName.BarVertical,
            "Vertical Bar Chart",
            "BAR_VERTICAL",
            new ChartSeriesData
            {
                Points =
                [
                    new ChartDataPoint { Label = "North", Value = 46 },
                    new ChartDataPoint { Label = "South", Value = 38 },
                    new ChartDataPoint { Label = "East", Value = 57 },
                    new ChartDataPoint { Label = "West", Value = 42 }
                ],
                Options = new ChartOptions
                {
                    XAxisLabel = "Region",
                    YAxisLabel = "Revenue (USD M)",
                    ShowAxes = true,
                    ShowGridLines = true,
                    ShowTicks = true,
                    ShowTickLabels = true,
                    YAxisTickCount = 6,
                    AxisLineWidth = 1.2f,
                    GridLineWidth = 0.8f,
                    LabelFontSize = 9f,
                    BarGapRatio = 0.22f,
                    ShowLegend = true,
                    LegendPosition = PieLegendPosition.Right,
                    LegendFontSize = 9f,
                    LegendMarkerSize = 10f
                }
            }),
        new(
            ChartTypeName.BarHorizontal,
            "Horizontal Bar Chart",
            "BAR_HORIZONTAL",
            new ChartSeriesData
            {
                Points =
                [
                    new ChartDataPoint { Label = "Support", Value = 72 },
                    new ChartDataPoint { Label = "Delivery", Value = 61 },
                    new ChartDataPoint { Label = "Sales", Value = 88 },
                    new ChartDataPoint { Label = "Finance", Value = 54 }
                ],
                Options = new ChartOptions
                {
                    XAxisLabel = "SLA Compliance (%)",
                    YAxisLabel = "Team",
                    ShowAxes = true,
                    ShowGridLines = true,
                    ShowTicks = true,
                    ShowTickLabels = true,
                    XAxisTickCount = 6,
                    YAxisTickCount = 6,
                    LabelFontSize = 9f,
                    BarGapRatio = 0.25f,
                    MinValue = 0,
                    MaxValue = 100,
                    ShowLegend = true,
                    LegendPosition = PieLegendPosition.Right,
                    LegendFontSize = 9f,
                    LegendMarkerSize = 10f
                }
            }),
        new(
            ChartTypeName.Line,
            "Line Chart",
            "LINE",
            new ChartSeriesData
            {
                Points =
                [
                    new ChartDataPoint { Label = "Jan", Value = 18 },
                    new ChartDataPoint { Label = "Feb", Value = 22 },
                    new ChartDataPoint { Label = "Mar", Value = 27 },
                    new ChartDataPoint { Label = "Apr", Value = 25 },
                    new ChartDataPoint { Label = "May", Value = 31 },
                    new ChartDataPoint { Label = "Jun", Value = 36 }
                ],
                Options = new ChartOptions
                {
                    XAxisLabel = "Month",
                    YAxisLabel = "Pipeline ($M)",
                    ShowAxes = true,
                    ShowGridLines = true,
                    ShowTicks = true,
                    ShowTickLabels = true,
                    YAxisTickCount = 5,
                    LineWidth = 2.4f,
                    ShowMarkers = true,
                    LabelFontSize = 9f,
                    LineColor = new Color(255, 37, 99, 235),
                    ShowLegend = true,
                    LegendPosition = PieLegendPosition.Right,
                    LegendFontSize = 9f,
                    LegendMarkerSize = 10f
                }
            }),
        new(
            ChartTypeName.Pie,
            "Pie Chart",
            "PIE",
            new ChartSeriesData
            {
                Points =
                [
                    new ChartDataPoint { Label = "Enterprise", Value = 44 },
                    new ChartDataPoint { Label = "Mid-Market", Value = 31 },
                    new ChartDataPoint { Label = "SMB", Value = 25 }
                ],
                Options = new ChartOptions
                {
                    ShowAxes = false,
                    ShowGridLines = false,
                    ShowTicks = false,
                    ShowTickLabels = true,
                    LabelFontSize = 9f,
                    PieLabelColor = new Color(255, 31, 41, 55),
                    PieLabelFontWeight = FontWeight.SemiBold,
                    ShowLegend = true,
                    LegendPosition = PieLegendPosition.Right,
                    LegendFontSize = 9f,
                    LegendMarkerSize = 10f,
                    LegendTextColor = new Color(255, 17, 24, 39)
                }
            })
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
                    },
                    .. BuildBarcodeCatalogLayoutItems("ops"),
                    .. BuildChartCatalogLayoutItems("ops")
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
            .AddComponent("ops-barcode-page-break", ReportComponentType.PageBreak)
            .AddComponent("ops-barcodes-title", ReportComponentType.Text, text =>
                text.WithBinding("Text", "Barcode Catalog")
                    .WithProperty("TextStyleId", "text-section-title"))
            .AddComponent("ops-barcodes-copy", ReportComponentType.Text, text =>
                text.WithBinding("Text", "All supported barcode types are shown below with a label and sample value.")
                    .WithProperty("TextStyleId", "text-body"));

        foreach (var entry in BarcodeCatalogEntries)
        {
            var slug = BarcodeSymbologyName.ToIdentifier(entry.Symbology).ToLowerInvariant();
            var labelText = $"{entry.Label} ({BarcodeSymbologyName.ToIdentifier(entry.Symbology)})";

            documentBuilder
                .AddComponent($"ops-barcodes-{slug}-label", ReportComponentType.Text, text =>
                    text.WithBinding("Text", labelText)
                        .WithProperty("TextStyleId", "text-body"))
                .AddComponent($"ops-barcodes-{slug}", ReportComponentType.Barcode, barcode =>
                    barcode.WithBinding("Value", entry.Value)
                        .WithProperty("SymbologyType", entry.Symbology.ToString())
                        .WithProperty("Symbology", BarcodeSymbologyName.ToIdentifier(entry.Symbology))
                        .WithProperty("ShowText", entry.ShowText ? "true" : "false")
                        .WithProperty("BarcodeStyleId", "text-body"));
        }

        documentBuilder
            .AddComponent("ops-chart-page-break", ReportComponentType.PageBreak)
            .AddComponent("ops-charts-title", ReportComponentType.Text, text =>
                text.WithBinding("Text", "Chart Catalog")
                    .WithProperty("TextStyleId", "text-section-title"))
            .AddComponent("ops-charts-copy", ReportComponentType.Text, text =>
                text.WithBinding("Text", "Built-in chart types are shown below with sample datasets.")
                    .WithProperty("TextStyleId", "text-body"));

        foreach (var entry in ChartCatalogEntries)
        {
            var slug = entry.Identifier.ToLowerInvariant();

            documentBuilder
                .AddComponent($"ops-charts-{slug}-label", ReportComponentType.Text, text =>
                    text.WithBinding("Text", $"{entry.Label} ({entry.Identifier})")
                        .WithProperty("TextStyleId", "text-body"))
                .AddComponent($"ops-charts-{slug}", ReportComponentType.Chart, chart =>
                    chart.WithProperty("ChartType", entry.Identifier)
                        .WithProperty("ChartTypeValue", entry.ChartType.ToString())
                        .WithProperty("ChartDataJson", JsonSerializer.Serialize(entry.Data))
                        .WithProperty("ChartStyleId", "text-body"));
        }

        documentBuilder
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

    private static IReadOnlyList<ReportLayoutItemDefinition> BuildBarcodeCatalogLayoutItems(string prefix)
    {
        var items = new List<ReportLayoutItemDefinition>
        {
            new()
            {
                Id = $"{prefix}-barcode-page-break",
                Kind = ReportLayoutItemKind.PageBreak
            },
            TextItem($"{prefix}-barcodes-title", "Barcode Catalog", textStyleId: "text-section-title"),
            TextItem($"{prefix}-barcodes-copy", "All supported barcode types are shown below with a label and sample value.", textStyleId: "text-body")
        };

        foreach (var entry in BarcodeCatalogEntries)
        {
            var identifier = BarcodeSymbologyName.ToIdentifier(entry.Symbology);
            var slug = identifier.ToLowerInvariant();
            items.Add(TextItem(
                id: $"{prefix}-barcodes-{slug}-label",
                text: $"{entry.Label} ({identifier})",
                textStyleId: "text-body"));
            items.Add(new ReportLayoutItemDefinition
            {
                Id = $"{prefix}-barcodes-{slug}",
                Kind = ReportLayoutItemKind.Barcode,
                SymbologyType = entry.Symbology,
                Symbology = identifier,
                Value = entry.Value,
                ShowText = entry.ShowText,
                BarcodeStyleId = "text-body"
            });
        }

        return items;
    }

    private static IReadOnlyList<ReportLayoutItemDefinition> BuildChartCatalogLayoutItems(string prefix)
    {
        var items = new List<ReportLayoutItemDefinition>
        {
            new()
            {
                Id = $"{prefix}-chart-page-break",
                Kind = ReportLayoutItemKind.PageBreak
            },
            TextItem($"{prefix}-charts-title", "Chart Catalog", textStyleId: "text-section-title"),
            TextItem($"{prefix}-charts-copy", "Built-in chart types are shown below with sample datasets.", textStyleId: "text-body")
        };

        foreach (var entry in ChartCatalogEntries)
        {
            var slug = entry.Identifier.ToLowerInvariant();

            items.Add(TextItem(
                id: $"{prefix}-charts-{slug}-label",
                text: $"{entry.Label} ({entry.Identifier})",
                textStyleId: "text-body"));

            items.Add(new ReportLayoutItemDefinition
            {
                Id = $"{prefix}-charts-{slug}",
                Kind = ReportLayoutItemKind.Chart,
                ChartTypeValue = entry.ChartType,
                ChartType = entry.Identifier,
                ChartDataJson = JsonSerializer.Serialize(entry.Data),
                ChartStyleId = "text-body"
            });
        }

        return items;
    }

    private sealed record BarcodeCatalogEntry(BarcodeSymbology Symbology, string Label, string Value, bool ShowText = true);

    private sealed record ChartCatalogEntry(ChartTypeName ChartType, string Label, string Identifier, ChartSeriesData Data);
}
