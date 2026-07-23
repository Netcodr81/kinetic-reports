using KineticReports.Samples.Blazor.Components;
using KineticReports.Samples.Blazor.Services;
using KineticReports.Authoring.DependencyInjection;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Engine.Building;
using KineticReports.Engine.Data;
using KineticReports.Engine.Expressions;
using KineticReports.Export.Html;
using KineticReports.Layout;
using KineticReports.Plugins;
using KineticReports.Rendering.Skia;
using KineticReports.Viewer.Blazor;
using KineticReports.Visual;
using SampleRenderingPipelineOptions = KineticReports.Samples.Blazor.Services.RenderingPipelineOptions;

// ============================================================================
// KineticReports Blazor Sample Application (Interactive Server)
// ============================================================================
// This sample demonstrates:
// 1. Loading and managing plugins via IPluginService
// 2. Displaying sample reports with different components
// 3. Interactive Blazor components (Server-side rendering)
// 4. Report rendering with preview capability
//
// To run: dotnet run
// To build: dotnet build
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

SampleSqlLiteDatabaseInitializer.EnsureSeeded(builder.Environment.ContentRootPath);

if (args.Any(arg => string.Equals(arg, "--seed-sqlite", StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine("SQLite sample database seeded.");
    return;
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register authoring contracts/services for JSON-first and drag-and-drop flows.
builder.Services.AddKineticReportsAuthoring();

// Register custom services
builder.Services
    .AddOptions<SampleRenderingPipelineOptions>()
    .Bind(builder.Configuration.GetSection(SampleRenderingPipelineOptions.SectionName));

builder.Services.AddScoped<IPluginService, PluginService>();
builder.Services.AddScoped<IPluginExecutionTraceStore, PluginExecutionTraceStore>();
builder.Services.AddScoped<IAuthoringJsonWorkflowService, AuthoringJsonWorkflowService>();
builder.Services.AddSingleton<IAuthoredReportCatalogService, AuthoredReportCatalogService>();

// Register report execution services
builder.Services.AddScoped<ITextLayout, SkiaTextLayout>();

// Register report engine dependencies
builder.Services.AddScoped<IExpressionEvaluator, LiteralEvaluator>();
builder.Services.AddScoped<IDataResolver, SampleDataResolver>();
builder.Services.AddScoped<SampleReportBuilder>();
builder.Services.AddScoped<IReportBuilder, PluginAwareReportBuilder>();
builder.Services.AddScoped<ILayoutEngine, LayoutEngine>();

// Register report engine and exporters
builder.Services.AddScoped<IReportEngine, ReportEngine>();
builder.Services.AddScoped<IHtmlExporter, HtmlExporter>();
builder.Services.AddScoped<IVisualHtmlExporter, VisualHtmlExporter>();
builder.Services.AddScoped<IVisualDocumentBuilder, DefaultVisualDocumentBuilder>();
builder.Services.AddKineticReportsViewerBlazor(options =>
{
    options.PipelineMode = builder.Configuration[$"{SampleRenderingPipelineOptions.SectionName}:PipelineMode"]
        ?? "Legacy";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
