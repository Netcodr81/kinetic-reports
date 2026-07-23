using KineticReports.Core.Definition;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Layout;
using KineticReports.Visual;
using KineticReports.Viewer.Blazor.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace KineticReports.Viewer.Blazor.Tests;

public class LocalReportServiceTests
{
    private sealed class MockTextLayout : ITextLayout
    {
        public Size MeasureText(string text, AppliedStyle style, float maxWidth)
        {
            return new Size(text.Length * 8f, 12f);
        }

        public IReadOnlyList<TextRun> ShapeText(string text, AppliedStyle style, Rect bounds)
        {
            return
            [
                new TextRun
                {
                    Text = text,
                    BaselineOrigin = new Point(bounds.X, bounds.Y + 10),
                    Bounds = bounds,
                    Style = style,
                    IsRightToLeft = false
                }
            ];
        }

        public float GetAscent(FontDescriptor descriptor) => 10f;
        public float GetDescent(FontDescriptor descriptor) => 2f;
        public float GetLineGap(FontDescriptor descriptor) => 2f;
    }

    private sealed class MockReportEngine : IReportEngine
    {
        public Task<ReportDocument> RunAsync(
            ReportDefinition definition,
            IReadOnlyDictionary<string, object?> parameters,
            ILayoutSizingContext layoutSizingContext,
            LayoutOptions? layoutOptions = null,
            CancellationToken cancellationToken = default)
        {
            var page = new PageBlock
            {
                Id = "page-1",
                PageWidth = 800,
                PageHeight = 600,
                PageNumber = 1,
                Header = null,
                Footer = null,
                Children = [],
                Style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f }
            };

            return Task.FromResult(new ReportDocument { Pages = [page] });
        }
    }

    private static LocalReportService CreateService(RenderingPipelineOptions? options = null)
    {
        return new LocalReportService(
            new MockReportEngine(),
            new HtmlExporter(),
            new VisualHtmlExporter(),
            new DefaultVisualDocumentBuilder(),
            new VisualHitTestIndexBuilder(),
            new VisualTextSearchIndexBuilder(),
            new MockTextLayout(),
            Options.Create(options ?? new RenderingPipelineOptions()),
            NullLogger<LocalReportService>.Instance);
    }

    [Fact]
    public async Task RenderHtmlAsync_WithValidDefinition_ReturnsHtml()
    {
        // Arrange
        var service = CreateService();

        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-1",
            Name = "TestReport"
        };
        var parameters = new Dictionary<string, object?>();

        // Act
        var html = await service.RenderHtmlAsync(definition, parameters);

        // Assert
        html.ShouldContain("<!DOCTYPE html");
        html.ShouldContain("<html");
        html.ShouldContain("</html>");
    }

    [Fact]
    public async Task RenderHtmlAsync_WithNullDefinition_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.RenderHtmlAsync(null!, new Dictionary<string, object?>()));
    }

    [Fact]
    public async Task RenderHtmlAsync_WithNullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-2",
            Name = "TestReport"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.RenderHtmlAsync(definition, null!));
    }

    [Fact]
    public async Task ExportAsync_WithUnsupportedFormat_ThrowsNotSupportedException()
    {
        // Arrange
        var service = CreateService();

        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-3a",
            Name = "TestReport"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await service.ExportAsync(definition, "pdf", new Dictionary<string, object?>()));
    }

    [Fact]
    public async Task ExportAsync_WithHtmlFormat_ReturnsHtmlBytes()
    {
        // Arrange
        var service = CreateService();

        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-3",
            Name = "TestReport"
        };

        // Act
        var result = await service.ExportAsync(definition, "html", new Dictionary<string, object?>());
        var html = System.Text.Encoding.UTF8.GetString(result.Content);

        // Assert
        result.FormatId.ShouldBe("html");
        result.MimeType.ShouldBe("text/html");
        result.FileExtension.ShouldBe("html");
        html.ShouldNotBeEmpty();
        html.ShouldContain("<!DOCTYPE html");
    }

    [Fact]
    public async Task HitTestAsync_WithEmptyDocument_ReturnsMiss()
    {
        // Arrange
        var service = CreateService(new RenderingPipelineOptions { PipelineMode = "Visual" });
        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-hit-1",
            Name = "HitTestReport"
        };

        // Act
        var result = await service.HitTestAsync(definition, new Dictionary<string, object?>(), 1, 100f, 100f);

        // Assert
        result.Hit.ShouldBeFalse();
        result.PageNumber.ShouldBe(1);
    }

    [Fact]
    public async Task SearchTextAsync_WithEmptyDocument_ReturnsNoMatches()
    {
        // Arrange
        var service = CreateService(new RenderingPipelineOptions { PipelineMode = "Visual" });
        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-search-1",
            Name = "SearchReport"
        };

        // Act
        var result = await service.SearchTextAsync(definition, new Dictionary<string, object?>(), "foo");

        // Assert
        result.Query.ShouldBe("foo");
        result.Matches.ShouldBeEmpty();
    }
}
