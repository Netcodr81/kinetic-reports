namespace KineticReports.Samples.Blazor.Services;

using System.Net;
using System.Text;
using KineticReports.Authoring.Compilation;
using KineticReports.Authoring.Components;
using KineticReports.Authoring.Documents;
using KineticReports.Authoring.Serialization;
using KineticReports.Core.Definition;

/// <summary>
/// Demonstrates an end-to-end authoring JSON workflow in the sample app.
/// </summary>
public interface IAuthoringJsonWorkflowService
{
    /// <summary>
    /// Gets available authored sample reports that demonstrate component usage.
    /// </summary>
    IReadOnlyList<AuthoringSampleReportDescriptor> GetSampleReports();

    /// <summary>
    /// Gets available data provider options for authored sample generation.
    /// </summary>
    IReadOnlyList<AuthoringDataProviderDescriptor> GetProviderOptions();

    /// <summary>
    /// Creates a sample designer document with representative components.
    /// </summary>
    ReportDesignerDocument CreateSampleDocument();

    /// <summary>
    /// Creates a sample designer document for a specific authored sample id.
    /// </summary>
    /// <param name="sampleId">The authored sample id.</param>
    /// <returns>The sample designer document.</returns>
    ReportDesignerDocument CreateSampleDocument(string sampleId);

    /// <summary>
    /// Creates a sample designer document for a specific authored sample id and provider.
    /// </summary>
    /// <param name="sampleId">The authored sample id.</param>
    /// <param name="providerType">The provider type (for example, SampleInMemory or SqlLite).</param>
    /// <returns>The sample designer document.</returns>
    ReportDesignerDocument CreateSampleDocument(string sampleId, string providerType);

    /// <summary>
    /// Generates authoring JSON from a sample document.
    /// </summary>
    string GenerateSampleJson();

    /// <summary>
    /// Generates authored-preview HTML from the specified sample and provider.
    /// </summary>
    /// <param name="sampleId">The authored sample id.</param>
    /// <param name="providerType">The provider type (for example, SampleInMemory or SqlLite).</param>
    /// <returns>An HTML preview representing the authored component tree.</returns>
    string GenerateAuthoringPreviewHtml(string sampleId, string providerType);

    /// <summary>
    /// Generates authoring JSON from the specified sample document.
    /// </summary>
    /// <param name="sampleId">The authored sample id.</param>
    /// <returns>The serialized JSON payload.</returns>
    string GenerateSampleJson(string sampleId);

    /// <summary>
    /// Generates authoring JSON from the specified sample and provider.
    /// </summary>
    /// <param name="sampleId">The authored sample id.</param>
    /// <param name="providerType">The provider type (for example, SampleInMemory or SqlLite).</param>
    /// <returns>The serialized JSON payload.</returns>
    string GenerateSampleJson(string sampleId, string providerType);

    /// <summary>
    /// Compiles a sample designer document into a runtime definition.
    /// </summary>
    ReportDefinition CompileSampleDefinition();

    /// <summary>
    /// Compiles the specified authored sample into a runtime definition.
    /// </summary>
    /// <param name="sampleId">The authored sample id.</param>
    /// <returns>The compiled runtime definition.</returns>
    ReportDefinition CompileSampleDefinition(string sampleId);

    /// <summary>
    /// Compiles the specified authored sample and provider into a runtime definition.
    /// </summary>
    /// <param name="sampleId">The authored sample id.</param>
    /// <param name="providerType">The provider type (for example, SampleInMemory or SqlLite).</param>
    /// <returns>The compiled runtime definition.</returns>
    ReportDefinition CompileSampleDefinition(string sampleId, string providerType);

    /// <summary>
    /// Saves the provided designer document as JSON.
    /// </summary>
    Task SaveDocumentJsonAsync(ReportDesignerDocument document, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a designer document JSON file and compiles it into a runtime definition.
    /// </summary>
    Task<ReportDefinition> LoadAndCompileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compiles a designer document JSON payload into a runtime definition.
    /// </summary>
    /// <param name="designerDocumentJson">Serialized designer document JSON.</param>
    /// <returns>The compiled runtime definition.</returns>
    ReportDefinition CompileFromJson(string designerDocumentJson);
}

/// <summary>
/// Default implementation of <see cref="IAuthoringJsonWorkflowService"/>.
/// </summary>
public sealed class AuthoringJsonWorkflowService : IAuthoringJsonWorkflowService
{
    private const string SqlLiteProviderType = "SqlLite";
    private const string InMemoryProviderType = "SampleInMemory";
    private const string SampleConnectionString = "Data Source=|DataDirectory|\\sample-reports.db";
    private const string AllComponentsSampleId = "all-components-showcase";
    private const string AllComponentsMultiPageSampleId = "all-components-multipage";
    private const string StructureComponentsSampleId = "structure-components-showcase";
    private const string ContentComponentsSampleId = "content-components-showcase";
    private const string SupportingComponentsSampleId = "supporting-components-showcase";
    private const string ComponentSamplePrefix = "component-";
    private const string DataSourceId = "sales-orders";

    private static readonly IReadOnlyList<AuthoringSampleReportDescriptor> ComponentTypeSampleReports =
        Enum.GetValues<ReportComponentType>()
            .Select(CreateComponentTypeSampleDescriptor)
            .ToList();

    private static readonly IReadOnlyList<AuthoringSampleReportDescriptor> SampleReports =
    [
        new AuthoringSampleReportDescriptor(
            AllComponentsSampleId,
            "All Components Showcase",
            "Demonstrates structure, content, and supporting components in one authored report, including placeholder metadata for QR."),
        new AuthoringSampleReportDescriptor(
            AllComponentsMultiPageSampleId,
            "All Components Multi-Page Showcase",
            "Demonstrates all components in a multi-page authored report with repeated header and footer bands."),
        new AuthoringSampleReportDescriptor(
            StructureComponentsSampleId,
            "Structure Components Showcase",
            "Focused authored example for Page, Section, Panel, Group, List, and Table composition."),
        new AuthoringSampleReportDescriptor(
            ContentComponentsSampleId,
            "Content Components Showcase",
            "Focused authored example for Text, Image, Chart, Line, Rectangle, Ellipse, and Barcode/QR placeholder."),
        new AuthoringSampleReportDescriptor(
            SupportingComponentsSampleId,
            "Supporting Components Showcase",
            "Focused authored example for Spacer, Divider, PageBreak, tokens, Header, Footer, and Background."),
        ..ComponentTypeSampleReports
    ];

    private static readonly IReadOnlyList<AuthoringDataProviderDescriptor> ProviderOptions =
    [
        new AuthoringDataProviderDescriptor(
            InMemoryProviderType,
            "In-Memory",
            "Uses deterministic in-memory sample rows from SampleDataResolver."),
        new AuthoringDataProviderDescriptor(
            SqlLiteProviderType,
            "SQLite",
            "Uses the seeded sample SQLite database with query-based data sources.")
    ];

    private readonly IReportDesignerDocumentSerializer _documentSerializer;
    private readonly IDesignerDocumentCompilationService _compilationService;

    /// <summary>
    /// Initializes a new <see cref="AuthoringJsonWorkflowService"/>.
    /// </summary>
    public AuthoringJsonWorkflowService(
        IReportDesignerDocumentSerializer documentSerializer,
        IDesignerDocumentCompilationService compilationService)
    {
        _documentSerializer = documentSerializer ?? throw new ArgumentNullException(nameof(documentSerializer));
        _compilationService = compilationService ?? throw new ArgumentNullException(nameof(compilationService));
    }

    /// <inheritdoc/>
    public IReadOnlyList<AuthoringSampleReportDescriptor> GetSampleReports() => SampleReports;

    /// <inheritdoc/>
    public IReadOnlyList<AuthoringDataProviderDescriptor> GetProviderOptions() => ProviderOptions;

    /// <inheritdoc/>
    public ReportDesignerDocument CreateSampleDocument()
    {
        return CreateSampleDocument(AllComponentsSampleId, InMemoryProviderType);
    }

    /// <inheritdoc/>
    public ReportDesignerDocument CreateSampleDocument(string sampleId)
    {
        return CreateSampleDocument(sampleId, InMemoryProviderType);
    }

    /// <inheritdoc/>
    public ReportDesignerDocument CreateSampleDocument(string sampleId, string providerType)
    {
        if (string.IsNullOrWhiteSpace(sampleId))
            throw new ArgumentException("Sample id must be provided.", nameof(sampleId));

        var normalizedProviderType = NormalizeProviderType(providerType);

        if (string.Equals(sampleId, AllComponentsSampleId, StringComparison.OrdinalIgnoreCase))
            return CreateAllComponentsSampleDocument(normalizedProviderType);

        if (string.Equals(sampleId, AllComponentsMultiPageSampleId, StringComparison.OrdinalIgnoreCase))
            return CreateAllComponentsMultiPageSampleDocument(normalizedProviderType);

        if (string.Equals(sampleId, StructureComponentsSampleId, StringComparison.OrdinalIgnoreCase))
            return CreateStructureComponentsSampleDocument(normalizedProviderType);

        if (string.Equals(sampleId, ContentComponentsSampleId, StringComparison.OrdinalIgnoreCase))
            return CreateContentComponentsSampleDocument(normalizedProviderType);

        if (string.Equals(sampleId, SupportingComponentsSampleId, StringComparison.OrdinalIgnoreCase))
            return CreateSupportingComponentsSampleDocument(normalizedProviderType);

        if (TryGetComponentTypeFromSampleId(sampleId, out var componentType))
            return CreateSingleComponentSampleDocument(componentType, normalizedProviderType);

        throw new ArgumentException($"Unknown sample id '{sampleId}'.", nameof(sampleId));
    }

    private static string NormalizeProviderType(string providerType)
    {
        if (string.IsNullOrWhiteSpace(providerType))
            return InMemoryProviderType;

        if (string.Equals(providerType, InMemoryProviderType, StringComparison.OrdinalIgnoreCase))
            return InMemoryProviderType;

        if (string.Equals(providerType, SqlLiteProviderType, StringComparison.OrdinalIgnoreCase))
            return SqlLiteProviderType;

        throw new ArgumentException(
            $"Unsupported provider type '{providerType}'. Expected '{InMemoryProviderType}' or '{SqlLiteProviderType}'.",
            nameof(providerType));
    }

    private static AuthoringSampleReportDescriptor CreateComponentTypeSampleDescriptor(ReportComponentType componentType)
    {
        var sampleId = GetComponentSampleId(componentType);
        var name = $"{componentType} Component Sample";
        var description = $"Single-component authored sample focused on {componentType}.";

        if (componentType is ReportComponentType.Barcode)
            description += " Includes QR placeholder metadata example.";

        return new AuthoringSampleReportDescriptor(sampleId, name, description);
    }

    private static string GetComponentSampleId(ReportComponentType componentType)
        => $"{ComponentSamplePrefix}{componentType.ToString().ToLowerInvariant()}";

    private static bool TryGetComponentTypeFromSampleId(string sampleId, out ReportComponentType componentType)
    {
        componentType = default;

        if (!sampleId.StartsWith(ComponentSamplePrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var typeToken = sampleId[ComponentSamplePrefix.Length..];
        if (string.IsNullOrWhiteSpace(typeToken))
            return false;

        foreach (var type in Enum.GetValues<ReportComponentType>())
        {
            if (string.Equals(type.ToString(), typeToken, StringComparison.OrdinalIgnoreCase))
            {
                componentType = type;
                return true;
            }
        }

        return false;
    }

    private static ReportDesignerDocumentBuilder CreateBaseBuilder(
        string id,
        string name,
        string description,
        string providerType)
    {
        return ReportDesignerDocumentBuilder
            .Create(id, name)
            .WithAuthor("KineticReports Sample")
            .WithDescription(description)
            .AddDataSource(BuildDataSourceDefinition(providerType));
    }

    private static DataSourceDefinition BuildDataSourceDefinition(string providerType)
    {
        var normalizedProviderType = NormalizeProviderType(providerType);
        var properties = new Dictionary<string, string>(StringComparer.Ordinal);

        if (string.Equals(normalizedProviderType, SqlLiteProviderType, StringComparison.Ordinal))
        {
            properties["ConnectionString"] = SampleConnectionString;
            properties["Query"] = BuildQuery(DataSourceId);
        }

        return new DataSourceDefinition
        {
            Id = DataSourceId,
            Name = "Sales Orders",
            ProviderType = normalizedProviderType,
            Properties = properties,
        };
    }

    private static string BuildQuery(string dataSourceId) => dataSourceId switch
    {
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
        _ => "SELECT 'Unsupported data source for SQLite authoring sample' AS Message"
    };

    private static ReportDesignerDocument CreateAllComponentsSampleDocument(string providerType)
    {
        return CreateBaseBuilder(
                AllComponentsSampleId,
                "All Components Showcase",
                "Code-first authored report that demonstrates every built-in component type.",
                providerType)
            .AddComponent("page-root", ReportComponentType.Page, page =>
                page.WithPlacement(0f, 0f, 816f, 1056f)
                    .AddChild("report-background", ReportComponentType.Background, background =>
                        background.Named("Background Surface")
                            .WithPlacement(0f, 0f, 816f, 1056f)
                            .WithProperty("fill", "#f8f9fb"))
                    .AddChild("report-header", ReportComponentType.Header, header =>
                        header.Named("Header Region")
                            .WithPlacement(24f, 16f, 768f, 56f)
                            .AddChild("header-title", ReportComponentType.Text, title =>
                                title.Named("Header Title")
                                    .WithBinding("Text", "All Components Showcase")
                                    .WithPlacement(0f, 0f, 420f, 28f))
                            .AddChild("header-page-number", ReportComponentType.PageNumber, pageNumber =>
                                pageNumber.Named("Header Page Number")
                                    .WithPlacement(660f, 0f, 48f, 20f))
                            .AddChild("header-total-pages", ReportComponentType.TotalPages, totalPages =>
                                totalPages.Named("Header Total Pages")
                                    .WithPlacement(714f, 0f, 48f, 20f))
                            .AddChild("header-date", ReportComponentType.CurrentDate, currentDate =>
                                currentDate.Named("Header Current Date")
                                    .WithPlacement(560f, 28f, 100f, 20f))
                            .AddChild("header-time", ReportComponentType.CurrentTime, currentTime =>
                                currentTime.Named("Header Current Time")
                                    .WithPlacement(664f, 28f, 98f, 20f)))
                    .AddChild("body-section", ReportComponentType.Section, section =>
                        section.Named("Body Section")
                            .WithPlacement(24f, 80f, 768f, 880f)
                            .AddChild("content-panel", ReportComponentType.Panel, panel =>
                                panel.Named("Content Panel")
                                    .WithPlacement(0f, 0f, 768f, 840f)
                                    .AddChild("section-label", ReportComponentType.Text, text =>
                                        text.Named("Section Label")
                                            .WithBinding("Text", "Structure + Content + Supporting")
                                            .WithPlacement(0f, 0f, 520f, 24f))
                                    .AddChild("section-divider", ReportComponentType.Divider, divider =>
                                        divider.Named("Section Divider")
                                            .WithPlacement(0f, 28f, 768f, 2f))
                                    .AddChild("top-spacer", ReportComponentType.Spacer, spacer =>
                                        spacer.Named("Top Spacer")
                                            .WithPlacement(0f, 34f, 768f, 10f))
                                    .AddChild("line-shape", ReportComponentType.Line, line =>
                                        line.Named("Line Shape")
                                            .WithPlacement(0f, 52f, 240f, 2f))
                                    .AddChild("rectangle-shape", ReportComponentType.Rectangle, rectangle =>
                                        rectangle.Named("Rectangle Shape")
                                            .WithPlacement(252f, 44f, 120f, 28f)
                                            .WithProperty("fill", "#d8e8ff"))
                                    .AddChild("ellipse-shape", ReportComponentType.Ellipse, ellipse =>
                                        ellipse.Named("Ellipse Shape")
                                            .WithPlacement(384f, 44f, 120f, 28f)
                                            .WithProperty("fill", "#dff6dd"))
                                    .AddChild("image-logo", ReportComponentType.Image, image =>
                                        image.Named("Image Placeholder")
                                            .WithPlacement(516f, 40f, 120f, 50f)
                                            .WithBinding("Source", "https://example.invalid/brand/logo.png"))
                                    .AddChild("chart-sales", ReportComponentType.Chart, chart =>
                                        chart.Named("Chart Placeholder")
                                            .BoundToDataSource(DataSourceId)
                                            .WithPlacement(0f, 100f, 360f, 180f)
                                            .WithProperty("chart.type", "Bar")
                                            .WithProperty("chart.x", "Region")
                                            .WithProperty("chart.y", "Amount"))
                                    .AddChild("barcode-qr", ReportComponentType.Barcode, barcode =>
                                        barcode.Named("Barcode / QR Placeholder")
                                            .WithPlacement(372f, 100f, 180f, 80f)
                                            .WithBinding("Value", "ORDER-1001")
                                            .WithProperty("barcode.format", "QrCode")
                                            .WithProperty("placeholder.note", "QR renderer not implemented yet. Placeholder only."))
                                    .AddChild("barcode-note", ReportComponentType.Text, barcodeNote =>
                                        barcodeNote.Named("Barcode Note")
                                            .WithBinding("Text", "QR Code placeholder: format requested via property barcode.format=QrCode")
                                            .WithPlacement(372f, 186f, 360f, 34f))
                                    .AddChild("data-table", ReportComponentType.Table, table =>
                                        table.Named("Data Table")
                                            .BoundToDataSource(DataSourceId)
                                            .WithPlacement(0f, 292f, 768f, 210f))
                                    .AddChild("group-region", ReportComponentType.Group, group =>
                                        group.Named("Group Example")
                                            .BoundToDataSource(DataSourceId)
                                            .WithRepeatPath("Region")
                                            .WithPlacement(0f, 514f, 360f, 150f)
                                            .AddChild("group-label", ReportComponentType.Text, groupLabel =>
                                                groupLabel.Named("Group Label")
                                                    .WithBinding("Text", "Group by Region")
                                                    .WithPlacement(0f, 0f, 180f, 22f)))
                                    .AddChild("list-region", ReportComponentType.List, list =>
                                        list.Named("List Example")
                                            .BoundToDataSource(DataSourceId)
                                            .WithRepeatPath("Rows")
                                            .WithPlacement(372f, 514f, 360f, 150f)
                                            .AddChild("list-item", ReportComponentType.Text, listItem =>
                                                listItem.Named("List Item")
                                                    .WithBinding("Text", "{OrderId} - {Region} - {Amount}")
                                                    .WithPlacement(0f, 0f, 340f, 22f)))
                                    .AddChild("doc-info", ReportComponentType.DocumentInfo, docInfo =>
                                        docInfo.Named("Document Info Token")
                                            .WithPlacement(0f, 676f, 360f, 22f)
                                            .WithProperty("document.field", "Author"))
                                    .AddChild("forced-page-break", ReportComponentType.PageBreak, pageBreak =>
                                        pageBreak.Named("Forced Page Break")
                                            .WithPlacement(0f, 706f, 120f, 16f))
                                    .AddChild("post-break-note", ReportComponentType.Text, postBreakText =>
                                        postBreakText.Named("Post-Break Note")
                                            .WithBinding("Text", "This text is intended to render after a forced page break.")
                                            .WithPlacement(0f, 730f, 500f, 22f))))
                    .AddChild("report-footer", ReportComponentType.Footer, footer =>
                        footer.Named("Footer Region")
                            .WithPlacement(24f, 968f, 768f, 64f)
                            .AddChild("footer-divider", ReportComponentType.Divider, divider =>
                                divider.Named("Footer Divider")
                                    .WithPlacement(0f, 0f, 768f, 2f))
                            .AddChild("footer-text", ReportComponentType.Text, text =>
                                text.Named("Footer Text")
                                    .WithBinding("Text", "Generated by KineticReports Authoring Samples")
                                    .WithPlacement(0f, 12f, 420f, 20f))))
            .Build();
    }

    private static ReportDesignerDocument CreateStructureComponentsSampleDocument(string providerType)
    {
        var panel = new ReportComponentDefinitionBuilder("structure-panel", ReportComponentType.Panel)
            .Named("Container Panel")
            .WithPlacement(0f, 0f, 768f, 980f)
            .AddChild("panel-label", ReportComponentType.Text, label =>
                label.WithBinding("Text", "Panel hosting group, list, and table")
                    .WithPlacement(0f, 0f, 420f, 24f))
            .AddChild("structure-group", ReportComponentType.Group, group =>
                group.Named("Region Group")
                    .BoundToDataSource(DataSourceId)
                    .WithRepeatPath("Region")
                    .WithPlacement(0f, 36f, 360f, 180f)
                    .AddChild("group-header", ReportComponentType.Text, groupHeader =>
                        groupHeader.WithBinding("Text", "Group Header: {Region}")
                            .WithPlacement(0f, 0f, 300f, 22f)))
            .AddChild("structure-list", ReportComponentType.List, list =>
                list.Named("Order List")
                    .BoundToDataSource(DataSourceId)
                    .WithRepeatPath("Rows")
                    .WithPlacement(372f, 36f, 360f, 180f)
                    .AddChild("list-row", ReportComponentType.Text, item =>
                        item.WithBinding("Text", "{OrderId} | {Region} | {Amount}")
                            .WithPlacement(0f, 0f, 340f, 20f)))
            .AddChild("structure-table", ReportComponentType.Table, table =>
                table.Named("Orders Table")
                    .BoundToDataSource(DataSourceId)
                    .WithPlacement(0f, 232f, 732f, 220f))
            .Build();

        var section = new ReportComponentDefinitionBuilder("structure-section", ReportComponentType.Section)
            .Named("Main Section")
            .WithPlacement(24f, 24f, 768f, 1008f)
            .AddChild(panel)
            .Build();

        var page = new ReportComponentDefinitionBuilder("structure-page", ReportComponentType.Page)
            .WithPlacement(0f, 0f, 816f, 1056f)
            .AddChild(section)
            .Build();

        return CreateBaseBuilder(
                StructureComponentsSampleId,
                "Structure Components Showcase",
            "Authoring example focused on structure components.",
            providerType)
            .AddComponent(page)
            .Build();
    }

    private static ReportDesignerDocument CreateAllComponentsMultiPageSampleDocument(string providerType)
    {
        return CreateBaseBuilder(
                AllComponentsMultiPageSampleId,
                "All Components Multi-Page Showcase",
                "Comprehensive authored sample that includes all component types across multiple pages.",
                providerType)
            .AddComponent("all-blocks-page", ReportComponentType.Page, page =>
                page.WithPlacement(0f, 0f, 816f, 1056f)
                    .AddChild("all-blocks-background", ReportComponentType.Background, background =>
                        background.Named("Background Layer")
                            .WithPlacement(0f, 0f, 816f, 1056f)
                            .WithProperty("fill", "#f7f9fc"))
                    .AddChild("all-blocks-header", ReportComponentType.Header, header =>
                        header.Named("Repeated Header")
                            .WithPlacement(24f, 16f, 768f, 64f)
                            .AddChild("all-blocks-header-title", ReportComponentType.Text, text =>
                                text.WithBinding("Text", "All Block Types - Multi-Page")
                                    .WithPlacement(0f, 0f, 360f, 24f))
                            .AddChild("all-blocks-header-subtitle", ReportComponentType.Text, text =>
                                text.WithBinding("Text", "Header should repeat on every page")
                                    .WithPlacement(0f, 28f, 360f, 20f))
                            .AddChild("all-blocks-header-date", ReportComponentType.CurrentDate, date =>
                                date.WithPlacement(566f, 0f, 98f, 20f))
                            .AddChild("all-blocks-header-time", ReportComponentType.CurrentTime, time =>
                                time.WithPlacement(670f, 0f, 92f, 20f))
                            .AddChild("all-blocks-header-page", ReportComponentType.PageNumber, pageNumber =>
                                pageNumber.WithPlacement(690f, 28f, 30f, 20f))
                            .AddChild("all-blocks-header-total", ReportComponentType.TotalPages, totalPages =>
                                totalPages.WithPlacement(730f, 28f, 30f, 20f)))
                    .AddChild("all-blocks-section-one", ReportComponentType.Section, section =>
                        section.Named("Page 1 Content")
                            .WithPlacement(24f, 92f, 768f, 830f)
                            .AddChild("all-blocks-panel-one", ReportComponentType.Panel, panel =>
                                panel.Named("Primary Content Panel")
                                    .WithPlacement(0f, 0f, 768f, 820f)
                                    .AddChild("all-blocks-divider-top", ReportComponentType.Divider, divider =>
                                        divider.WithPlacement(0f, 0f, 768f, 2f))
                                    .AddChild("all-blocks-spacer-top", ReportComponentType.Spacer, spacer =>
                                        spacer.WithPlacement(0f, 8f, 768f, 12f))
                                    .AddChild("all-blocks-line", ReportComponentType.Line, line =>
                                        line.WithPlacement(0f, 28f, 220f, 2f))
                                    .AddChild("all-blocks-rectangle", ReportComponentType.Rectangle, rectangle =>
                                        rectangle.WithPlacement(230f, 20f, 120f, 40f)
                                            .WithProperty("fill", "#dce9ff"))
                                    .AddChild("all-blocks-ellipse", ReportComponentType.Ellipse, ellipse =>
                                        ellipse.WithPlacement(362f, 20f, 120f, 40f)
                                            .WithProperty("fill", "#e1f6dd"))
                                    .AddChild("all-blocks-image", ReportComponentType.Image, image =>
                                        image.WithPlacement(494f, 16f, 130f, 50f)
                                            .WithBinding("Source", "https://example.invalid/assets/multipage-logo.png"))
                                    .AddChild("all-blocks-chart", ReportComponentType.Chart, chart =>
                                        chart.BoundToDataSource(DataSourceId)
                                            .WithPlacement(0f, 76f, 360f, 180f)
                                            .WithProperty("chart.type", "Bar")
                                            .WithProperty("chart.x", "Region")
                                            .WithProperty("chart.y", "Amount"))
                                    .AddChild("all-blocks-barcode", ReportComponentType.Barcode, barcode =>
                                        barcode.WithPlacement(372f, 76f, 180f, 80f)
                                            .WithBinding("Value", "INV-7001")
                                            .WithProperty("barcode.format", "Code128"))
                                    .AddChild("all-blocks-qr", ReportComponentType.Barcode, qr =>
                                        qr.WithPlacement(560f, 76f, 180f, 80f)
                                            .WithBinding("Value", "https://contoso.invalid/report/INV-7001")
                                            .WithProperty("barcode.format", "QrCode")
                                            .WithProperty("placeholder.note", "QR placeholder metadata sample."))
                                    .AddChild("all-blocks-table", ReportComponentType.Table, table =>
                                        table.BoundToDataSource(DataSourceId)
                                            .WithPlacement(0f, 268f, 740f, 220f))
                                    .AddChild("all-blocks-group", ReportComponentType.Group, group =>
                                        group.BoundToDataSource(DataSourceId)
                                            .WithRepeatPath("Region")
                                            .WithPlacement(0f, 500f, 350f, 130f)
                                            .AddChild("all-blocks-group-text", ReportComponentType.Text, text =>
                                                text.WithBinding("Text", "Group: {Region}")
                                                    .WithPlacement(0f, 0f, 220f, 20f)))
                                    .AddChild("all-blocks-list", ReportComponentType.List, list =>
                                        list.BoundToDataSource(DataSourceId)
                                            .WithRepeatPath("Rows")
                                            .WithPlacement(370f, 500f, 370f, 130f)
                                            .AddChild("all-blocks-list-text", ReportComponentType.Text, text =>
                                                text.WithBinding("Text", "Order {OrderId} / {Region} / {Amount}")
                                                    .WithPlacement(0f, 0f, 360f, 20f)))
                                    .AddChild("all-blocks-document-info", ReportComponentType.DocumentInfo, docInfo =>
                                        docInfo.WithPlacement(0f, 642f, 380f, 22f)
                                            .WithProperty("document.field", "Name"))
                                    .AddChild("all-blocks-break-label", ReportComponentType.Text, text =>
                                        text.WithBinding("Text", "Forced page break before next content section")
                                            .WithPlacement(0f, 676f, 420f, 22f))
                                    .AddChild("all-blocks-page-break", ReportComponentType.PageBreak, pageBreak =>
                                        pageBreak.WithPlacement(0f, 702f, 160f, 16f))))
                    .AddChild("all-blocks-section-two", ReportComponentType.Section, section =>
                        section.Named("Page 2 Content")
                            .WithPlacement(24f, 944f, 768f, 180f)
                            .AddChild("all-blocks-panel-two", ReportComponentType.Panel, panel =>
                                panel.WithPlacement(0f, 0f, 768f, 160f)
                                    .AddChild("all-blocks-second-page-title", ReportComponentType.Text, text =>
                                        text.WithBinding("Text", "Second page content begins here")
                                            .WithPlacement(0f, 0f, 420f, 24f))
                                    .AddChild("all-blocks-second-page-note", ReportComponentType.Text, text =>
                                        text.WithBinding("Text", "Header and footer should repeat on this page.")
                                            .WithPlacement(0f, 30f, 480f, 22f))))
                    .AddChild("all-blocks-footer", ReportComponentType.Footer, footer =>
                        footer.Named("Repeated Footer")
                            .WithPlacement(24f, 964f, 768f, 60f)
                            .AddChild("all-blocks-footer-divider", ReportComponentType.Divider, divider =>
                                divider.WithPlacement(0f, 0f, 768f, 2f))
                            .AddChild("all-blocks-footer-text", ReportComponentType.Text, text =>
                                text.WithBinding("Text", "KineticReports Multi-Page Showcase Footer")
                                    .WithPlacement(0f, 12f, 440f, 20f))
                            .AddChild("all-blocks-footer-page", ReportComponentType.PageNumber, pageNumber =>
                                pageNumber.WithPlacement(692f, 12f, 30f, 20f))
                            .AddChild("all-blocks-footer-total", ReportComponentType.TotalPages, totalPages =>
                                totalPages.WithPlacement(730f, 12f, 30f, 20f))))
            .Build();
    }

    private static ReportDesignerDocument CreateContentComponentsSampleDocument(string providerType)
    {
        return CreateBaseBuilder(
                ContentComponentsSampleId,
                "Content Components Showcase",
            "Authoring example focused on content components and placeholder behavior.",
            providerType)
            .AddComponent("content-page", ReportComponentType.Page, page =>
                page.WithPlacement(0f, 0f, 816f, 1056f)
                    .AddChild("content-panel", ReportComponentType.Panel, panel =>
                        panel.WithPlacement(24f, 24f, 768f, 1008f)
                            .AddChild("content-title", ReportComponentType.Text, text =>
                                text.WithBinding("Text", "Content Components")
                                    .WithPlacement(0f, 0f, 240f, 28f))
                            .AddChild("content-image", ReportComponentType.Image, image =>
                                image.WithBinding("Source", "https://example.invalid/assets/logo.png")
                                    .WithPlacement(0f, 40f, 160f, 80f)
                                    .WithProperty("placeholder.note", "Image source is sample-only."))
                            .AddChild("content-chart", ReportComponentType.Chart, chart =>
                                chart.BoundToDataSource(DataSourceId)
                                    .WithPlacement(172f, 40f, 320f, 180f)
                                    .WithProperty("chart.type", "Bar")
                                    .WithProperty("chart.x", "Region")
                                    .WithProperty("chart.y", "Amount"))
                            .AddChild("content-line", ReportComponentType.Line, line =>
                                line.WithPlacement(504f, 40f, 240f, 2f))
                            .AddChild("content-rectangle", ReportComponentType.Rectangle, rectangle =>
                                rectangle.WithPlacement(504f, 54f, 112f, 64f)
                                    .WithProperty("fill", "#e6f0ff"))
                            .AddChild("content-ellipse", ReportComponentType.Ellipse, ellipse =>
                                ellipse.WithPlacement(632f, 54f, 112f, 64f)
                                    .WithProperty("fill", "#ebfaeb"))
                            .AddChild("content-barcode", ReportComponentType.Barcode, barcode =>
                                barcode.WithPlacement(0f, 232f, 200f, 88f)
                                    .WithBinding("Value", "ORDER-2002")
                                    .WithProperty("barcode.format", "Code128"))
                            .AddChild("content-qr", ReportComponentType.Barcode, qr =>
                                qr.WithPlacement(212f, 232f, 200f, 88f)
                                    .WithBinding("Value", "https://contoso.invalid/reports/2002")
                                    .WithProperty("barcode.format", "QrCode")
                                    .WithProperty("placeholder.note", "QR code renderer is not implemented yet; this is an authored placeholder."))
                            .AddChild("content-qr-note", ReportComponentType.Text, note =>
                                note.WithBinding("Text", "QR placeholder authored using Barcode component + barcode.format=QrCode")
                                    .WithPlacement(212f, 324f, 520f, 24f))))
            .Build();
    }

    private static ReportDesignerDocument CreateSupportingComponentsSampleDocument(string providerType)
    {
        return CreateBaseBuilder(
                SupportingComponentsSampleId,
                "Supporting Components Showcase",
                "Authoring example focused on supporting components and report tokens.",
                providerType)
            .AddComponent("support-page", ReportComponentType.Page, page =>
                page.WithPlacement(0f, 0f, 816f, 1056f)
                    .AddChild("support-background", ReportComponentType.Background, background =>
                        background.WithPlacement(0f, 0f, 816f, 1056f)
                            .WithProperty("fill", "#fcfcff"))
                    .AddChild("support-header", ReportComponentType.Header, header =>
                        header.WithPlacement(24f, 20f, 768f, 54f)
                            .AddChild("support-header-title", ReportComponentType.Text, title =>
                                title.WithBinding("Text", "Supporting Components")
                                    .WithPlacement(0f, 0f, 280f, 24f))
                            .AddChild("support-date", ReportComponentType.CurrentDate, date =>
                                date.WithPlacement(560f, 0f, 100f, 20f))
                            .AddChild("support-time", ReportComponentType.CurrentTime, time =>
                                time.WithPlacement(664f, 0f, 98f, 20f)))
                    .AddChild("support-body", ReportComponentType.Section, section =>
                        section.WithPlacement(24f, 84f, 768f, 860f)
                            .AddChild("support-divider-1", ReportComponentType.Divider, divider =>
                                divider.WithPlacement(0f, 0f, 768f, 2f))
                            .AddChild("support-spacer", ReportComponentType.Spacer, spacer =>
                                spacer.WithPlacement(0f, 8f, 768f, 20f))
                            .AddChild("support-doc-info", ReportComponentType.DocumentInfo, info =>
                                info.WithPlacement(0f, 36f, 360f, 22f)
                                    .WithProperty("document.field", "Name"))
                            .AddChild("support-page-break", ReportComponentType.PageBreak, pageBreak =>
                                pageBreak.WithPlacement(0f, 70f, 120f, 16f))
                            .AddChild("support-after-break", ReportComponentType.Text, note =>
                                note.WithPlacement(0f, 96f, 520f, 22f)
                                    .WithBinding("Text", "Content after page break marker."))
                            .AddChild("support-divider-2", ReportComponentType.Divider, divider =>
                                divider.WithPlacement(0f, 130f, 768f, 2f)))
                    .AddChild("support-footer", ReportComponentType.Footer, footer =>
                        footer.WithPlacement(24f, 956f, 768f, 62f)
                            .AddChild("support-page-number", ReportComponentType.PageNumber, pageNumber =>
                                pageNumber.WithPlacement(640f, 12f, 40f, 20f))
                            .AddChild("support-total-pages", ReportComponentType.TotalPages, totalPages =>
                                totalPages.WithPlacement(686f, 12f, 40f, 20f))
                            .AddChild("support-footer-label", ReportComponentType.Text, text =>
                                text.WithPlacement(0f, 12f, 420f, 20f)
                                    .WithBinding("Text", "Supporting component token examples."))))
            .Build();
    }

    private static ReportDesignerDocument CreateSingleComponentSampleDocument(
        ReportComponentType componentType,
        string providerType)
    {
        var sampleId = GetComponentSampleId(componentType);
        var sampleName = $"{componentType} Component Sample";
        var sampleDescription = $"Authoring report focused on the {componentType} component type.";

        var pageBuilder = new ReportComponentDefinitionBuilder("sample-page", ReportComponentType.Page)
            .Named("Sample Page")
            .WithPlacement(0f, 0f, 816f, 1056f)
            .AddChild("sample-title", ReportComponentType.Text, title =>
                title.Named("Sample Title")
                    .WithBinding("Text", sampleName)
                    .WithPlacement(24f, 24f, 560f, 28f))
            .AddChild("sample-subtitle", ReportComponentType.Text, subtitle =>
                subtitle.Named("Sample Subtitle")
                    .WithBinding("Text", $"Authoring showcase for component type: {componentType}")
                    .WithPlacement(24f, 56f, 740f, 22f));

        AddShowcaseComponent(pageBuilder, componentType);

        return CreateBaseBuilder(sampleId, sampleName, sampleDescription, providerType)
            .AddComponent(pageBuilder.Build())
            .Build();
    }

    private static void AddShowcaseComponent(
        ReportComponentDefinitionBuilder pageBuilder,
        ReportComponentType componentType)
    {
        switch (componentType)
        {
            case ReportComponentType.Page:
                pageBuilder
                    .Named("Page Showcase")
                    .WithProperty("showcase.component", "Page")
                    .AddChild("page-note", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "The page root itself is the showcased component.")
                            .WithPlacement(24f, 92f, 560f, 22f));
                break;

            case ReportComponentType.Section:
                pageBuilder.AddChild("showcase-section", ReportComponentType.Section, section =>
                    section.WithPlacement(24f, 92f, 740f, 180f)
                        .AddChild("section-text", ReportComponentType.Text, text =>
                            text.WithBinding("Text", "Section component hosting nested content.")
                                .WithPlacement(16f, 16f, 500f, 22f)));
                break;

            case ReportComponentType.Panel:
                pageBuilder.AddChild("showcase-panel", ReportComponentType.Panel, panel =>
                    panel.WithPlacement(24f, 92f, 740f, 180f)
                        .AddChild("panel-text", ReportComponentType.Text, text =>
                            text.WithBinding("Text", "Panel component used as a generic container.")
                                .WithPlacement(16f, 16f, 500f, 22f)));
                break;

            case ReportComponentType.Group:
                pageBuilder.AddChild("showcase-group", ReportComponentType.Group, group =>
                    group.BoundToDataSource(DataSourceId)
                        .WithRepeatPath("Region")
                        .WithPlacement(24f, 92f, 740f, 220f)
                        .AddChild("group-row", ReportComponentType.Text, text =>
                            text.WithBinding("Text", "Grouped row for {Region}")
                                .WithPlacement(0f, 0f, 420f, 22f)));
                break;

            case ReportComponentType.List:
                pageBuilder.AddChild("showcase-list", ReportComponentType.List, list =>
                    list.BoundToDataSource(DataSourceId)
                        .WithRepeatPath("Rows")
                        .WithPlacement(24f, 92f, 740f, 220f)
                        .AddChild("list-row", ReportComponentType.Text, text =>
                            text.WithBinding("Text", "List row: {OrderId} / {Region} / {Amount}")
                                .WithPlacement(0f, 0f, 500f, 22f)));
                break;

            case ReportComponentType.Table:
                pageBuilder.AddChild("showcase-table", ReportComponentType.Table, table =>
                    table.BoundToDataSource(DataSourceId)
                        .WithPlacement(24f, 92f, 740f, 260f)
                        .WithProperty("columns", "OrderId,Region,SalesPerson,Amount,Status"));
                break;

            case ReportComponentType.Text:
                pageBuilder.AddChild("showcase-text", ReportComponentType.Text, text =>
                    text.WithBinding("Text", "This is a text component with expression placeholders: {OrderId} {Region}")
                        .WithPlacement(24f, 92f, 740f, 22f));
                break;

            case ReportComponentType.Image:
                pageBuilder.AddChild("showcase-image", ReportComponentType.Image, image =>
                    image.WithBinding("Source", "https://example.invalid/assets/sample-image.png")
                        .WithPlacement(24f, 92f, 220f, 120f)
                        .WithProperty("placeholder.note", "Image path is placeholder-only in this sample."));
                break;

            case ReportComponentType.Chart:
                pageBuilder.AddChild("showcase-chart", ReportComponentType.Chart, chart =>
                    chart.BoundToDataSource(DataSourceId)
                        .WithPlacement(24f, 92f, 420f, 240f)
                        .WithProperty("chart.type", "Bar")
                        .WithProperty("chart.x", "Region")
                        .WithProperty("chart.y", "Amount"));
                break;

            case ReportComponentType.Line:
                pageBuilder.AddChild("showcase-line", ReportComponentType.Line, line =>
                    line.WithPlacement(24f, 110f, 420f, 2f));
                break;

            case ReportComponentType.Rectangle:
                pageBuilder.AddChild("showcase-rectangle", ReportComponentType.Rectangle, rectangle =>
                    rectangle.WithPlacement(24f, 92f, 220f, 120f)
                        .WithProperty("fill", "#d8e8ff"));
                break;

            case ReportComponentType.Ellipse:
                pageBuilder.AddChild("showcase-ellipse", ReportComponentType.Ellipse, ellipse =>
                    ellipse.WithPlacement(24f, 92f, 220f, 120f)
                        .WithProperty("fill", "#e4f7df"));
                break;

            case ReportComponentType.Barcode:
                pageBuilder
                    .AddChild("showcase-barcode", ReportComponentType.Barcode, barcode =>
                        barcode.WithPlacement(24f, 92f, 220f, 88f)
                            .WithBinding("Value", "ORDER-1001")
                            .WithProperty("barcode.format", "Code128"))
                    .AddChild("showcase-qr", ReportComponentType.Barcode, qr =>
                        qr.WithPlacement(256f, 92f, 220f, 88f)
                            .WithBinding("Value", "https://contoso.invalid/reports/1001")
                            .WithProperty("barcode.format", "QrCode")
                            .WithProperty("placeholder.note", "QR renderer not implemented yet. Placeholder authored for workflow validation."));
                break;

            case ReportComponentType.Spacer:
                pageBuilder
                    .AddChild("spacer-top-label", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Text above spacer")
                            .WithPlacement(24f, 92f, 220f, 22f))
                    .AddChild("showcase-spacer", ReportComponentType.Spacer, spacer =>
                        spacer.WithPlacement(24f, 120f, 320f, 44f))
                    .AddChild("spacer-bottom-label", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Text below spacer")
                            .WithPlacement(24f, 172f, 220f, 22f));
                break;

            case ReportComponentType.Divider:
                pageBuilder
                    .AddChild("divider-top-label", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Top content")
                            .WithPlacement(24f, 92f, 220f, 22f))
                    .AddChild("showcase-divider", ReportComponentType.Divider, divider =>
                        divider.WithPlacement(24f, 122f, 740f, 2f))
                    .AddChild("divider-bottom-label", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Bottom content")
                            .WithPlacement(24f, 132f, 220f, 22f));
                break;

            case ReportComponentType.PageBreak:
                pageBuilder
                    .AddChild("before-page-break", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Content before page break")
                            .WithPlacement(24f, 92f, 280f, 22f))
                    .AddChild("showcase-page-break", ReportComponentType.PageBreak, pageBreak =>
                        pageBreak.WithPlacement(24f, 122f, 120f, 16f))
                    .AddChild("after-page-break", ReportComponentType.Text, text =>
                        text.WithBinding("Text", "Content intended after page break")
                            .WithPlacement(24f, 148f, 300f, 22f));
                break;

            case ReportComponentType.PageNumber:
                pageBuilder.AddChild("showcase-page-number", ReportComponentType.PageNumber, pageNumber =>
                    pageNumber.WithPlacement(24f, 92f, 80f, 22f));
                break;

            case ReportComponentType.TotalPages:
                pageBuilder.AddChild("showcase-total-pages", ReportComponentType.TotalPages, totalPages =>
                    totalPages.WithPlacement(24f, 92f, 80f, 22f));
                break;

            case ReportComponentType.CurrentDate:
                pageBuilder.AddChild("showcase-current-date", ReportComponentType.CurrentDate, currentDate =>
                    currentDate.WithPlacement(24f, 92f, 140f, 22f));
                break;

            case ReportComponentType.CurrentTime:
                pageBuilder.AddChild("showcase-current-time", ReportComponentType.CurrentTime, currentTime =>
                    currentTime.WithPlacement(24f, 92f, 140f, 22f));
                break;

            case ReportComponentType.DocumentInfo:
                pageBuilder.AddChild("showcase-document-info", ReportComponentType.DocumentInfo, info =>
                    info.WithPlacement(24f, 92f, 220f, 22f)
                        .WithProperty("document.field", "Name"));
                break;

            case ReportComponentType.Header:
                pageBuilder.AddChild("showcase-header", ReportComponentType.Header, header =>
                    header.WithPlacement(24f, 92f, 740f, 64f)
                        .AddChild("header-content", ReportComponentType.Text, text =>
                            text.WithBinding("Text", "Header region content")
                                .WithPlacement(0f, 0f, 260f, 22f)));
                break;

            case ReportComponentType.Footer:
                pageBuilder.AddChild("showcase-footer", ReportComponentType.Footer, footer =>
                    footer.WithPlacement(24f, 900f, 740f, 64f)
                        .AddChild("footer-content", ReportComponentType.Text, text =>
                            text.WithBinding("Text", "Footer region content")
                                .WithPlacement(0f, 0f, 260f, 22f)));
                break;

            case ReportComponentType.Background:
                pageBuilder.AddChild("showcase-background", ReportComponentType.Background, background =>
                    background.WithPlacement(0f, 0f, 816f, 1056f)
                        .WithProperty("fill", "#f4f8ff"));
                break;

            default:
                pageBuilder.AddChild("showcase-unknown", ReportComponentType.Text, text =>
                    text.WithBinding("Text", $"No showcase implementation for {componentType}.")
                        .WithPlacement(24f, 92f, 560f, 22f));
                break;
        }
    }

    /// <inheritdoc/>
    public string GenerateSampleJson()
    {
        return GenerateSampleJson(AllComponentsSampleId, InMemoryProviderType);
    }

    /// <inheritdoc/>
    public string GenerateAuthoringPreviewHtml(string sampleId, string providerType)
    {
        var document = CreateSampleDocument(sampleId, providerType);
        return BuildAuthoringPreviewHtml(document, providerType);
    }

    /// <inheritdoc/>
    public string GenerateSampleJson(string sampleId)
    {
        return GenerateSampleJson(sampleId, InMemoryProviderType);
    }

    /// <inheritdoc/>
    public string GenerateSampleJson(string sampleId, string providerType)
    {
        return _documentSerializer.Serialize(CreateSampleDocument(sampleId, providerType));
    }

    /// <inheritdoc/>
    public ReportDefinition CompileSampleDefinition()
    {
        return CompileSampleDefinition(AllComponentsSampleId, InMemoryProviderType);
    }

    /// <inheritdoc/>
    public ReportDefinition CompileSampleDefinition(string sampleId)
    {
        return CompileSampleDefinition(sampleId, InMemoryProviderType);
    }

    /// <inheritdoc/>
    public ReportDefinition CompileSampleDefinition(string sampleId, string providerType)
    {
        return _compilationService.Compile(CreateSampleDocument(sampleId, providerType));
    }

    /// <inheritdoc/>
    public async Task SaveDocumentJsonAsync(
        ReportDesignerDocument document,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must be provided.", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = _documentSerializer.Serialize(document);
        await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ReportDefinition> LoadAndCompileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must be provided.", nameof(filePath));

        var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        return _compilationService.CompileFromJson(json);
    }

    /// <inheritdoc/>
    public ReportDefinition CompileFromJson(string designerDocumentJson)
    {
        if (string.IsNullOrWhiteSpace(designerDocumentJson))
            throw new ArgumentException("Designer document JSON must be provided.", nameof(designerDocumentJson));

        return _compilationService.CompileFromJson(designerDocumentJson);
    }

    private static string BuildAuthoringPreviewHtml(ReportDesignerDocument document, string providerType)
    {
        var sb = new StringBuilder();
        sb.Append("<html><head><style>");
        sb.Append("body{font-family:Segoe UI,Arial,sans-serif;background:#f2f4f8;margin:0;padding:16px;}");
        sb.Append(".meta{max-width:1080px;margin:0 auto 10px auto;padding:10px 12px;background:#fff;border:1px solid #d7deea;border-radius:8px;font-size:13px;color:#334;}");
        sb.Append(".canvas{position:relative; width:816px; height:1056px; margin:0 auto; background:#fff; border:1px solid #cfd8e6; box-shadow:0 8px 24px rgba(0,0,0,.08); overflow:hidden;}");
        sb.Append(".node{position:absolute; border:1px solid #8aa2c7; background:rgba(221,233,252,.35); box-sizing:border-box; overflow:hidden;}");
        sb.Append(".node .hdr{font-size:11px; line-height:14px; background:#2d4f87; color:#fff; padding:2px 6px; white-space:nowrap; text-overflow:ellipsis; overflow:hidden;}");
        sb.Append(".node .body{font-size:11px; color:#243249; padding:4px 6px; white-space:normal;}");
        sb.Append(".k-Text,.k-PageNumber,.k-TotalPages,.k-CurrentDate,.k-CurrentTime,.k-DocumentInfo{background:rgba(239,246,255,.9);border-color:#6e8fbf;}");
        sb.Append(".k-Table{background:rgba(231,249,243,.8);border-color:#5fa07f;}");
        sb.Append(".k-Chart,.k-Barcode,.k-Image{background:rgba(255,246,228,.8);border-color:#b48b4f;}");
        sb.Append(".k-Header,.k-Footer,.k-Background,.k-Section,.k-Panel,.k-Group,.k-List{background:rgba(241,237,255,.55);border-color:#7b64b9;}");
        sb.Append("</style></head><body>");

        sb.Append("<div class='meta'>");
        sb.Append($"<strong>{WebUtility.HtmlEncode(document.Name)}</strong><br/>");
        sb.Append($"Id: {WebUtility.HtmlEncode(document.Id)} | Provider: {WebUtility.HtmlEncode(providerType)} | Components: {document.Components.Count}");
        sb.Append("</div>");
        sb.Append("<div class='canvas'>");

        foreach (var component in document.Components)
            AppendComponent(component, sb, 0);

        sb.Append("</div></body></html>");
        return sb.ToString();
    }

    private static void AppendComponent(ReportComponentDefinition component, StringBuilder sb, int depth)
    {
        var left = component.Placement.X;
        var top = component.Placement.Y;
        var width = component.Placement.Width <= 0 ? 120f : component.Placement.Width;
        var height = component.Placement.Height <= 0 ? 40f : component.Placement.Height;

        var typeName = component.Type.ToString();
        var title = $"{typeName} - {component.Id}";
        var body = BuildComponentBody(component);

        var z = Math.Clamp(depth + 1, 1, 30);

        sb.Append($"<div class='node k-{WebUtility.HtmlEncode(typeName)}' style='left:{left:0.##}px;top:{top:0.##}px;width:{width:0.##}px;height:{height:0.##}px;z-index:{z};'>");
        sb.Append($"<div class='hdr'>{WebUtility.HtmlEncode(title)}</div>");
        if (!string.IsNullOrWhiteSpace(body))
            sb.Append($"<div class='body'>{body}</div>");

        foreach (var child in component.Children)
            AppendComponent(child, sb, depth + 1);

        sb.Append("</div>");
    }

    private static string BuildComponentBody(ReportComponentDefinition component)
    {
        var pieces = new List<string>();

        if (!string.IsNullOrWhiteSpace(component.Name))
            pieces.Add($"Name: {WebUtility.HtmlEncode(component.Name)}");

        if (!string.IsNullOrWhiteSpace(component.DataSourceId))
            pieces.Add($"DataSource: {WebUtility.HtmlEncode(component.DataSourceId)}");

        if (component.BoundFields.TryGetValue("Text", out var textBinding) && !string.IsNullOrWhiteSpace(textBinding))
            pieces.Add($"Text: {WebUtility.HtmlEncode(textBinding)}");

        if (component.BoundFields.TryGetValue("Value", out var valueBinding) && !string.IsNullOrWhiteSpace(valueBinding))
            pieces.Add($"Value: {WebUtility.HtmlEncode(valueBinding)}");

        if (component.Properties.TryGetValue("barcode.format", out var barcodeFormat) && !string.IsNullOrWhiteSpace(barcodeFormat))
            pieces.Add($"Format: {WebUtility.HtmlEncode(barcodeFormat)}");

        if (component.Properties.TryGetValue("placeholder.note", out var note) && !string.IsNullOrWhiteSpace(note))
            pieces.Add($"Note: {WebUtility.HtmlEncode(note)}");

        return string.Join("<br/>", pieces);
    }
}

/// <summary>
/// Describes an authored sample report available in the workflow service.
/// </summary>
/// <param name="Id">Stable sample identifier.</param>
/// <param name="Name">Display name shown in sample UI.</param>
/// <param name="Description">Short description of what the sample demonstrates.</param>
public sealed record AuthoringSampleReportDescriptor(string Id, string Name, string Description);

/// <summary>
/// Describes an available data provider option for authored sample generation.
/// </summary>
/// <param name="ProviderType">Stable provider type id used in DataSourceDefinition.ProviderType.</param>
/// <param name="DisplayName">Display name shown in sample UI.</param>
/// <param name="Description">Short description of provider behavior.</param>
public sealed record AuthoringDataProviderDescriptor(string ProviderType, string DisplayName, string Description);