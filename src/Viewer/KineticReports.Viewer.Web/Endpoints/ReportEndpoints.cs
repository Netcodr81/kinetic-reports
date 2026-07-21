namespace KineticReports.Viewer.Web.Endpoints;

using System.Text.Json;
using KineticReports.Core.Definition;
using KineticReports.Export.Html;
using KineticReports.Viewer.Web.Models;
using KineticReports.Viewer.Web.Services;

/// <summary>
/// Maps report execution and export endpoints.
/// </summary>
public static class ReportEndpoints
{
    /// <summary>
    /// Registers all report endpoints with the app.
    /// </summary>
    public static WebApplication MapReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports")
            .WithName("Reports");

        group.MapPost("/execute", ExecuteReport)
            .WithName("ExecuteReport")
            .WithSummary("Execute a report and get the report layout")
            .Produces<ExecuteReportResponse>(StatusCodes.Status200OK);

        group.MapGet("/{operationId}/html", ExportHtml)
            .WithName("ExportHtml")
            .WithSummary("Export report as HTML")
            .Produces<byte[]>(StatusCodes.Status200OK, "text/html");

        return app;
    }

    /// <summary>
    /// POST /api/reports/execute — Run a report and store result in session.
    /// </summary>
    private static async Task<IResult> ExecuteReport(
        ExecuteReportRequest request,
        IReportExecutor executor,
        IReportStore store,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Definition))
            return Results.BadRequest("Definition is required");

        try
        {
            var definition = JsonSerializer.Deserialize<ReportDefinition>(request.Definition)
                ?? throw new InvalidOperationException("Failed to deserialize report definition");

            var ReportLayout = await executor.ExecuteAsync(definition, request.Parameters, ct);
            var operationId = Guid.NewGuid().ToString("N");

            await store.SaveAsync(operationId, ReportLayout, ct);

            var response = new ExecuteReportResponse
            {
                OperationId = operationId,
                Status = "Success",
                ExecutedAt = DateTime.UtcNow
            };

            return Results.Ok(response);
        }
        catch (JsonException ex)
        {
            return Results.BadRequest($"Invalid report definition: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Report execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// GET /api/reports/{operationId}/html — Export report layout as HTML.
    /// </summary>
    private static async Task<IResult> ExportHtml(
        string operationId,
        IReportStore store,
        IHtmlExporter exporter,
        CancellationToken ct)
    {
        var ReportLayout = await store.RetrieveAsync(operationId, ct);
        if (ReportLayout == null)
            return Results.NotFound($"Report execution '{operationId}' not found");

        var stream = new MemoryStream();
        await exporter.ExportAsync(ReportLayout, stream, ct);
        stream.Position = 0;

        return Results.Stream(stream, "text/html; charset=utf-8");
    }
}
