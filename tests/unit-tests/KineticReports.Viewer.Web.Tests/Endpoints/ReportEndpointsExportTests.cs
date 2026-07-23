using System.Text;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Export.Html;
using KineticReports.Plugins;
using KineticReports.Rendering.Skia;
using KineticReports.Visual;
using KineticReports.Viewer.Web.Endpoints;
using KineticReports.Viewer.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace KineticReports.Viewer.Web.Tests;

public class ReportEndpointsExportTests
{
    [Fact]
    public async Task GetAvailableFormats_ReturnsRegisteredFormats()
    {
        await using var app = await BuildAppAsync("Legacy");
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/formats");

        response.IsSuccessStatusCode.ShouldBeTrue();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"formatId\":\"html\"");
        json.ShouldContain("\"formatId\":\"pdf\"");
        json.ShouldContain("\"mimeType\":\"text/html\"");
        json.ShouldContain("\"mimeType\":\"application/pdf\"");
    }

    [Theory]
    [InlineData("Legacy")]
    [InlineData("Visual")]
    public async Task ExportHtml_WithExistingOperationId_ReturnsHtmlForBothPipelineModes(string pipelineMode)
    {
        await using var app = await BuildAppAsync(pipelineMode);
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/op-html/html");

        response.IsSuccessStatusCode.ShouldBeTrue();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/html");

        var html = await response.Content.ReadAsStringAsync();
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html");
    }

    [Theory]
    [InlineData("Legacy")]
    [InlineData("Visual")]
    public async Task ExportPdf_WithExistingOperationId_ReturnsPdfForBothPipelineModes(string pipelineMode)
    {
        await using var app = await BuildAppAsync(pipelineMode);
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/op-pdf/pdf");

        response.IsSuccessStatusCode.ShouldBeTrue();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/pdf");

        var payload = await response.Content.ReadAsByteArrayAsync();
        payload.Length.ShouldBeGreaterThan(100);
        Encoding.ASCII.GetString(payload, 0, 4).ShouldBe("%PDF");
    }

    [Theory]
    [InlineData("Legacy", "html", "text/html", "<!DOCTYPE html>")]
    [InlineData("Visual", "html", "text/html", "<!DOCTYPE html>")]
    [InlineData("Legacy", "pdf", "application/pdf", "%PDF")]
    [InlineData("Visual", "pdf", "application/pdf", "%PDF")]
    public async Task ExportByFormat_WithSupportedFormat_ReturnsExpectedArtifact(
        string pipelineMode,
        string formatId,
        string expectedMediaType,
        string expectedPrefix)
    {
        await using var app = await BuildAppAsync(pipelineMode);
        var client = app.GetTestClient();

        var response = await client.GetAsync($"/api/reports/op-generic/export/{formatId}");

        response.IsSuccessStatusCode.ShouldBeTrue();
        response.Content.Headers.ContentType?.MediaType.ShouldBe(expectedMediaType);

        var payload = await response.Content.ReadAsByteArrayAsync();
        payload.Length.ShouldBeGreaterThan(20);
        Encoding.ASCII.GetString(payload, 0, expectedPrefix.Length).ShouldBe(expectedPrefix);
    }

    [Fact]
    public async Task HitTest_WithHitPoint_ReturnsHitElement()
    {
        await using var app = await BuildAppAsync("Legacy");
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/op-hit/hit-test?pageNumber=1&x=24&y=24");

        response.IsSuccessStatusCode.ShouldBeTrue();
        var json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"hit\":true");
        json.ShouldContain("\"elementId\":\"text-1\"");
        json.ShouldContain("\"layerName\":\"body\"");
    }

    [Fact]
    public async Task HitTest_WithMissPoint_ReturnsMissPayload()
    {
        await using var app = await BuildAppAsync("Legacy");
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/op-hit/hit-test?pageNumber=1&x=500&y=500");

        response.IsSuccessStatusCode.ShouldBeTrue();
        var json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"hit\":false");
        json.ShouldContain("\"pageNumber\":1");
    }

    [Fact]
    public async Task HitTest_WithUnknownOperation_ReturnsNotFound()
    {
        await using var app = await BuildAppAsync("Legacy");
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/unknown/hit-test?pageNumber=1&x=24&y=24");

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchText_WithQuery_ReturnsOrderedMatches()
    {
        await using var app = await BuildAppAsync("Legacy");
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/op-hit/search?query=hit");

        response.IsSuccessStatusCode.ShouldBeTrue();
        var json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"query\":\"hit\"");
        json.ShouldContain("\"matchCount\":2");
        json.ShouldContain("\"elementId\":\"text-1\"");
        json.ShouldContain("\"elementId\":\"text-2\"");
    }

    [Fact]
    public async Task SearchText_WithPageFilter_ReturnsScopedMatches()
    {
        await using var app = await BuildAppAsync("Legacy");
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/op-hit/search?query=hit&pageNumber=1");

        response.IsSuccessStatusCode.ShouldBeTrue();
        var json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"pageNumber\":1");
        json.ShouldContain("\"matchCount\":2");
    }

    [Fact]
    public async Task SearchText_WithUnknownOperation_ReturnsNotFound()
    {
        await using var app = await BuildAppAsync("Legacy");
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/reports/unknown/search?query=hit");

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    private static async Task<WebApplication> BuildAppAsync(string pipelineMode)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services
            .AddOptions<RenderingPipelineOptions>()
            .Configure(options => options.PipelineMode = pipelineMode);

        builder.Services
            .AddSingleton<IHtmlExporter, HtmlExporter>()
            .AddSingleton<IVisualHtmlExporter, VisualHtmlExporter>()
            .AddSingleton<IVisualDocumentBuilder, DefaultVisualDocumentBuilder>()
            .AddSingleton<VisualSkiaRenderer>()
            .AddSingleton<IReportDocumentExporter, HtmlReportDocumentExporter>()
            .AddSingleton<IReportDocumentExporter, PdfReportDocumentExporter>()
            .AddSingleton<IReportDocumentExporterRegistry, ReportDocumentExporterRegistry>()
            .AddSingleton<IReportStore, MemoryReportStore>()
            .AddSingleton<VisualHitTestIndexBuilder>()
            .AddSingleton<VisualTextSearchIndexBuilder>()
            .AddSingleton<IReportHitTestService, ReportHitTestService>()
            .AddSingleton<IReportTextSearchService, ReportTextSearchService>()
            .AddSingleton<IReportExecutor, StubReportExecutor>();

        var app = builder.Build();
        app.MapReportEndpoints();
        await app.StartAsync();

        var seededDocument = CreateReportDocument();
        var store = app.Services.GetRequiredService<IReportStore>();
        await store.SaveAsync("op-html", seededDocument);
        await store.SaveAsync("op-pdf", seededDocument);
        await store.SaveAsync("op-generic", seededDocument);
        await store.SaveAsync("op-hit", seededDocument);

        return app;
    }

    private static ReportDocument CreateReportDocument()
    {
        var style = new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f
        };

        var page = new PageBlock
        {
            Id = "page-1",
            Style = style,
            PageWidth = 612f,
            PageHeight = 792f,
            PageNumber = 1,
            Children =
            [
                new KineticReports.Core.Layout.TextBlock
                {
                    Id = "text-1",
                    Style = style,
                    Text = "Hit me"
                },
                new KineticReports.Core.Layout.TextBlock
                {
                    Id = "text-2",
                    Style = style,
                    Text = "Second hit"
                }
            ]
        };

        ((KineticReports.Core.Layout.TextBlock)page.Children[0]).Arrange(new KineticReports.Core.Geometry.Rect(20f, 20f, 120f, 24f));
        ((KineticReports.Core.Layout.TextBlock)page.Children[1]).Arrange(new KineticReports.Core.Geometry.Rect(20f, 60f, 180f, 24f));

        page.Arrange(new KineticReports.Core.Geometry.Rect(0f, 0f, 612f, 792f));

        return new ReportDocument
        {
            Pages = [page]
        };
    }

    private sealed class StubReportExecutor : IReportExecutor
    {
        public Task<ReportDocument> ExecuteAsync(
            KineticReports.Core.Definition.ReportDefinition definition,
            IReadOnlyDictionary<string, object?> parameters,
            CancellationToken ct = default)
        {
            return Task.FromResult(CreateReportDocument());
        }
    }
}
