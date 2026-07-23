using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Plugins;
using KineticReports.Rendering.Skia;
using KineticReports.Visual;
using KineticReports.Viewer.Web.Endpoints;
using KineticReports.Viewer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<RenderingPipelineOptions>()
    .Bind(builder.Configuration.GetSection(RenderingPipelineOptions.SectionName));

// Register core report engine components
builder.Services
    .AddSingleton<ITextLayout, SkiaTextLayout>()
    .AddSingleton<IReportEngine, ReportEngine>();

// Register export implementations
builder.Services
    .AddSingleton<IHtmlExporter, HtmlExporter>()
    .AddSingleton<IVisualHtmlExporter, VisualHtmlExporter>()
    .AddSingleton<IVisualDocumentBuilder, DefaultVisualDocumentBuilder>()
    .AddSingleton<VisualSkiaRenderer>()
    .AddSingleton<IVisualRenderer, VisualHtmlRendererAdapter>()
    .AddSingleton<IVisualRenderer, VisualPdfRendererAdapter>()
    .AddSingleton<IReportDocumentExporter, HtmlReportDocumentExporter>()
    .AddSingleton<IReportDocumentExporter, PdfReportDocumentExporter>()
    .AddSingleton<IReportDocumentExporterRegistry, ReportDocumentExporterRegistry>();

// Register report orchestration
builder.Services
    .AddSingleton<IReportExecutor, ReportExecutor>()
    .AddSingleton<IReportStore, MemoryReportStore>()
    .AddSingleton<VisualHitTestIndexBuilder>()
    .AddSingleton<VisualTextSearchIndexBuilder>()
    .AddSingleton<IReportHitTestService, ReportHitTestService>()
    .AddSingleton<IReportTextSearchService, ReportTextSearchService>();

var app = builder.Build();

// Configure HTTP pipeline
app.UseHttpsRedirection();

// Map report endpoints
app.MapReportEndpoints();

app.Run();
