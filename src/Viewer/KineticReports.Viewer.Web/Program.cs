using KineticReports.Core.Typography;
using KineticReports.Viewer.Web.Endpoints;
using KineticReports.Viewer.Web.Services;
using KineticReports.Core.Plugins;
using KineticReports.Core.Engine.DependencyInjection;
using KineticReports.Core.Visual;
using KineticReports.Core.Export.Html;
using KineticReports.Core.Export.Document;
using KineticReports.Core.Rendering.Skia;

var builder = WebApplication.CreateBuilder(args);

// Register core report engine components
builder.Services
    .AddSingleton<ITextLayout, SkiaTextLayout>()
    .AddKineticReportsEngine(ServiceLifetime.Singleton);

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
