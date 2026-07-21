using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Rendering.Skia;
using KineticReports.Viewer.Web.Endpoints;
using KineticReports.Viewer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Register core report engine components
builder.Services
    .AddSingleton<ITextLayout, SkiaTextLayout>()
    .AddSingleton<IReportEngine, ReportEngine>();

// Register export implementations
builder.Services
    .AddSingleton<IHtmlExporter, HtmlExporter>();

// Register report orchestration
builder.Services
    .AddSingleton<IReportExecutor, ReportExecutor>()
    .AddSingleton<IReportStore, MemoryReportStore>();

var app = builder.Build();

// Configure HTTP pipeline
app.UseHttpsRedirection();

// Map report endpoints
app.MapReportEndpoints();

app.Run();
