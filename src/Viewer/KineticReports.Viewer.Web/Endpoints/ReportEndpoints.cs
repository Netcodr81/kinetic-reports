namespace KineticReports.Viewer.Web.Endpoints;

using System.Text.Json;
using KineticReports.Core.Definition;
using KineticReports.Plugins;
using KineticReports.Viewer.Web.Models;
using KineticReports.Viewer.Web.Services;
using Microsoft.AspNetCore.Mvc;

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

        group.MapGet("/formats", GetAvailableFormats)
            .WithName("GetAvailableFormats")
            .WithSummary("Get available export formats")
            .Produces<IReadOnlyList<ExportFormatDescriptor>>(StatusCodes.Status200OK);

        group.MapGet("/{operationId}/html", ExportHtml)
            .WithName("ExportHtml")
            .WithSummary("Export report as HTML")
            .Produces<byte[]>(StatusCodes.Status200OK, "text/html");

        group.MapGet("/{operationId}/pdf", ExportPdf)
            .WithName("ExportPdf")
            .WithSummary("Export report as PDF")
            .Produces<byte[]>(StatusCodes.Status200OK, "application/pdf");

        group.MapGet("/{operationId}/export/{formatId}", ExportByFormat)
            .WithName("ExportByFormat")
            .WithSummary("Export report by format id")
            .Produces<byte[]>(StatusCodes.Status200OK);

        group.MapGet("/{operationId}/hit-test", HitTest)
            .WithName("HitTest")
            .WithSummary("Hit-test a page-local point against a report visual tree")
            .Produces<ReportHitTestResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{operationId}/search", SearchText)
            .WithName("SearchText")
            .WithSummary("Search text within a report visual tree")
            .Produces<ReportTextSearchResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// GET /api/reports/formats — List available export formats.
    /// </summary>
    private static IResult GetAvailableFormats(
        [FromServices] IReportDocumentExporterRegistry exporterRegistry)
    {
        var formats = exporterRegistry.GetAvailableFormats();
        return Results.Ok(formats);
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

            var ReportDocument = await executor.ExecuteAsync(definition, request.Parameters, ct);
            var operationId = Guid.NewGuid().ToString("N");

            await store.SaveAsync(operationId, ReportDocument, ct);

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
        [FromServices] IReportStore store,
        [FromServices] IReportDocumentExporterRegistry exporterRegistry,
        CancellationToken ct)
    {
        return await ExportWithFormatAsync(operationId, "html", store, exporterRegistry, ct);
    }

    /// <summary>
    /// GET /api/reports/{operationId}/pdf — Export report layout as PDF.
    /// </summary>
    private static async Task<IResult> ExportPdf(
        string operationId,
        [FromServices] IReportStore store,
        [FromServices] IReportDocumentExporterRegistry exporterRegistry,
        CancellationToken ct)
    {
        return await ExportWithFormatAsync(operationId, "pdf", store, exporterRegistry, ct);
    }

    /// <summary>
    /// GET /api/reports/{operationId}/export/{formatId} — Export report layout as the requested format.
    /// </summary>
    private static Task<IResult> ExportByFormat(
        string operationId,
        string formatId,
        [FromServices] IReportStore store,
        [FromServices] IReportDocumentExporterRegistry exporterRegistry,
        CancellationToken ct)
    {
        return ExportWithFormatAsync(operationId, formatId, store, exporterRegistry, ct);
    }

    private static async Task<IResult> ExportWithFormatAsync(
        string operationId,
        string formatId,
        IReportStore store,
        IReportDocumentExporterRegistry exporterRegistry,
        CancellationToken ct)
    {
        var reportDocument = await store.RetrieveAsync(operationId, ct);
        if (reportDocument == null)
            return Results.NotFound($"Report execution '{operationId}' not found");

        if (!exporterRegistry.TryGetExporter(formatId, out var exporter) || exporter is null)
            return Results.NotFound($"Export format '{formatId}' is not available.");

        var artifact = await exporter.ExportAsync(reportDocument, ct);
        var mimeType = string.Equals(exporter.Format.MimeType, "text/html", StringComparison.OrdinalIgnoreCase)
            ? "text/html; charset=utf-8"
            : exporter.Format.MimeType;

        return Results.File(artifact, mimeType, enableRangeProcessing: false);
    }

    /// <summary>
    /// GET /api/reports/{operationId}/hit-test?pageNumber=1&amp;x=10&amp;y=20
    /// </summary>
    private static async Task<IResult> HitTest(
        string operationId,
        int pageNumber,
        float x,
        float y,
        [FromServices] IReportHitTestService hitTestService,
        CancellationToken ct)
    {
        if (pageNumber <= 0)
            return Results.BadRequest("pageNumber must be greater than zero.");

        var (found, response) = await hitTestService.HitTestAsync(operationId, pageNumber, x, y, ct).ConfigureAwait(false);
        if (!found)
            return Results.NotFound($"Report execution '{operationId}' not found");

        return Results.Ok(response);
    }

    /// <summary>
    /// GET /api/reports/{operationId}/search?query=abc&amp;pageNumber=1
    /// </summary>
    private static async Task<IResult> SearchText(
        string operationId,
        string query,
        int? pageNumber,
        [FromServices] IReportTextSearchService searchService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Results.BadRequest("query is required.");

        if (pageNumber.HasValue && pageNumber.Value <= 0)
            return Results.BadRequest("pageNumber must be greater than zero.");

        var (found, response) = await searchService.SearchAsync(operationId, query, pageNumber, ct).ConfigureAwait(false);
        if (!found)
            return Results.NotFound($"Report execution '{operationId}' not found");

        return Results.Ok(response);
    }
}
