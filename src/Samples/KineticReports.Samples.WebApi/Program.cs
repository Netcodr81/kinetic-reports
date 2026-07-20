using KineticReports.Export.Html;
using KineticReports.Core.Typography;
using KineticReports.Rendering.Skia;
using KineticReports.Viewer.Web.Services;

// ============================================================================
// KineticReports Sample Web API
// ============================================================================
// This sample demonstrates a complete Web API for report generation:
// 
// Endpoints:
// - GET    /health                   - Health check
// - POST   /api/reports/execute      - Execute a report and cache result (via MapReportEndpoints)
// - GET    /api/reports/{id}/html    - Export cached report to HTML
//
// To test:
//   curl http://localhost:5000/health
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// Add services
// SkiaFontMetrics is used for text measurement during layout
builder.Services.AddScoped<SkiaFontMetrics>(sp => new SkiaFontMetrics());

// HtmlExporter exports layout trees to HTML format
builder.Services.AddScoped<HtmlExporter>();

// IReportExecutor orchestrates the full report execution pipeline
// (See KineticReports.Viewer.Web project for full setup with IDataResolver, IExpressionEvaluator, etc.)
builder.Services.AddScoped<IReportExecutor>(sp =>
    new ReportExecutor(
        null!,  // Would need IReportEngine here
        sp.GetRequiredService<SkiaFontMetrics>()));

// In-memory report cache with 1-hour TTL
builder.Services.AddSingleton<IReportStore>(sp =>
    new MemoryReportStore(TimeSpan.FromHours(1)));

var app = builder.Build();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "KineticReports API",
    timestamp = DateTime.UtcNow
}))
    .WithName("Health");

// To enable the full report execution and export endpoints, 
// uncomment the following line (requires full IReportEngine setup):
// app.MapReportEndpoints();

// Run the application
app.Run();

// ============================================================================
// Sample Usage Notes
// ============================================================================
//
// This sample provides:
// 1. Dependency Injection setup for core KineticReports components
// 2. In-memory caching of executed reports (1-hour TTL by default)
// 3. HTML export capability
// 4. Basic health check endpoint
//
// To extend this sample to full report functionality:
// 1. Implement IDataResolver to handle data source resolution
// 2. Implement IExpressionEvaluator to handle field expressions
// 3. Create IReportEngine with all dependencies
// 4. Uncomment app.MapReportEndpoints() to enable report endpoints
//
// To further extend:
// - Add PDF/Excel export endpoints
// - Integrate with a database for report storage
// - Add authentication and authorization
// - Implement real data providers (SQL Server, REST APIs, etc.)
// - Add report scheduling
// - Add audit logging
// - Deploy to cloud (Azure, AWS, Google Cloud)
//
// For production use:
// - Use distributed caching (Redis) instead of in-memory
// - Add rate limiting and request throttling
// - Implement comprehensive error handling
// - Add observability (logging, tracing, metrics)
// - Secure API endpoints with authentication
// - Use connection pooling for databases
// ============================================================================
