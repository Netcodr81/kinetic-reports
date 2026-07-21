using KineticReports.Core.Definition;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Layout;
using KineticReports.Viewer.Blazor.Services;

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
        public Task<ReportLayout> RunAsync(
            ReportDefinition definition,
            IReadOnlyDictionary<string, object?> parameters,
            ILayoutSizingContext layoutSizingContext,
            LayoutOptions? layoutOptions = null,
            CancellationToken cancellationToken = default)
        {
            var page = new PageElement
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

            return Task.FromResult(new ReportLayout { Pages = [page] });
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidDefinition_ReturnsLayoutTree()
    {
        // Arrange
        var engine = new MockReportEngine();
        var exporter = new HtmlExporter();
        var TextLayout = new MockTextLayout();
        var service = new LocalReportService(engine, exporter, TextLayout);

        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-1",
            Name = "TestReport"
        };
        var parameters = new Dictionary<string, object?>();

        // Act
        var result = await service.ExecuteAsync(definition, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Pages.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullDefinition_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = new MockReportEngine();
        var exporter = new HtmlExporter();
        var TextLayout = new MockTextLayout();
        var service = new LocalReportService(engine, exporter, TextLayout);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.ExecuteAsync(null!, new Dictionary<string, object?>()));
    }

    [Fact]
    public async Task ExecuteAsync_WithNullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = new MockReportEngine();
        var exporter = new HtmlExporter();
        var TextLayout = new MockTextLayout();
        var service = new LocalReportService(engine, exporter, TextLayout);

        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-2",
            Name = "TestReport"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.ExecuteAsync(definition, null!));
    }

    [Fact]
    public async Task ExportHtmlAsync_WithNullLayoutTree_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = new MockReportEngine();
        var exporter = new HtmlExporter();
        var TextLayout = new MockTextLayout();
        var service = new LocalReportService(engine, exporter, TextLayout);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.ExportHtmlAsync(null!));
    }

    [Fact]
    public async Task ExportHtmlAsync_WithValidLayoutTree_ReturnsHtmlString()
    {
        // Arrange
        var engine = new MockReportEngine();
        var exporter = new HtmlExporter();
        var TextLayout = new MockTextLayout();
        var service = new LocalReportService(engine, exporter, TextLayout);

        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-3",
            Name = "TestReport"
        };
        var ReportLayout = await service.ExecuteAsync(definition, new Dictionary<string, object?>());
        ReportLayout.ShouldNotBeNull();

        // Act
        var html = await service.ExportHtmlAsync(ReportLayout);

        // Assert
        html.ShouldNotBeEmpty();
        html.ShouldContain("<!DOCTYPE html");
        html.ShouldContain("<html");
        html.ShouldContain("</html>");
    }
}
