namespace KineticReports.Samples.Blazor.Services;

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
    /// Creates a sample designer document with representative components.
    /// </summary>
    ReportDesignerDocument CreateSampleDocument();

    /// <summary>
    /// Generates authoring JSON from a sample document.
    /// </summary>
    string GenerateSampleJson();

    /// <summary>
    /// Compiles a sample designer document into a runtime definition.
    /// </summary>
    ReportDefinition CompileSampleDefinition();

    /// <summary>
    /// Saves the provided designer document as JSON.
    /// </summary>
    Task SaveDocumentJsonAsync(ReportDesignerDocument document, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a designer document JSON file and compiles it into a runtime definition.
    /// </summary>
    Task<ReportDefinition> LoadAndCompileAsync(string filePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IAuthoringJsonWorkflowService"/>.
/// </summary>
public sealed class AuthoringJsonWorkflowService : IAuthoringJsonWorkflowService
{
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
    public ReportDesignerDocument CreateSampleDocument()
    {
        return ReportDesignerDocumentBuilder
            .Create("sample-authoring-report", "Sample Authoring Report")
            .WithAuthor("KineticReports Sample")
            .WithDescription("Code-first authoring JSON workflow sample.")
            .AddDataSource(new DataSourceDefinition
            {
                Id = "sales-orders",
                Name = "Sales Orders",
                ProviderType = "SampleInMemory",
                Properties = new Dictionary<string, string>()
            })
            .AddComponent("page-root", ReportComponentType.Page, page =>
                page.WithPlacement(0f, 0f, 816f, 1056f)
                    .AddChild("report-header", ReportComponentType.Text, text =>
                        text.Named("Report Header")
                            .WithBinding("Text", "Sales Summary")
                            .WithPlacement(24f, 24f, 520f, 36f))
                    .AddChild("sales-table", ReportComponentType.Table, table =>
                        table.BoundToDataSource("sales-orders")
                            .WithPlacement(24f, 72f, 768f, 360f)))
            .Build();
    }

    /// <inheritdoc/>
    public string GenerateSampleJson()
    {
        return _documentSerializer.Serialize(CreateSampleDocument());
    }

    /// <inheritdoc/>
    public ReportDefinition CompileSampleDefinition()
    {
        return _compilationService.Compile(CreateSampleDocument());
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
}