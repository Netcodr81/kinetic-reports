namespace KineticReports.Rendering.Skia.Tests;

using System.Text;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Visual;
using KineticReports.Viewer.Web.Services;
using Microsoft.Extensions.Options;
using PdfRenderingPipelineOptions = KineticReports.Viewer.Web.Services.RenderingPipelineOptions;

public class PdfReportDocumentExporterTests
{
    [Fact]
    public async Task ExportAsync_WithLegacyMode_DoesNotUseVisualBuilderAndReturnsPdf()
    {
        var reportDocument = CreateReportDocument();
        var visualBuilder = new TrackingVisualDocumentBuilder(CreateVisualDocument());
        var visualRenderer = new VisualSkiaRenderer();
        var options = Options.Create(new PdfRenderingPipelineOptions { PipelineMode = "Legacy" });
        var sut = new PdfReportDocumentExporter(visualBuilder, visualRenderer, options);

        var artifact = await sut.ExportAsync(reportDocument);

        visualBuilder.BuildCallCount.ShouldBe(0);
        artifact.Length.ShouldBeGreaterThan(100);
        Encoding.ASCII.GetString(artifact, 0, 4).ShouldBe("%PDF");
    }

    [Fact]
    public async Task ExportAsync_WithVisualMode_UsesVisualBuilderAndReturnsPdf()
    {
        var reportDocument = CreateReportDocument();
        var visualBuilder = new TrackingVisualDocumentBuilder(CreateVisualDocument());
        var visualRenderer = new VisualSkiaRenderer();
        var options = Options.Create(new PdfRenderingPipelineOptions { PipelineMode = "Visual" });
        var sut = new PdfReportDocumentExporter(visualBuilder, visualRenderer, options);

        var artifact = await sut.ExportAsync(reportDocument);

        visualBuilder.BuildCallCount.ShouldBe(1);
        artifact.Length.ShouldBeGreaterThan(100);
        Encoding.ASCII.GetString(artifact, 0, 4).ShouldBe("%PDF");
    }

    private static ReportDocument CreateReportDocument()
    {
        return new ReportDocument
        {
            Pages =
            [
                new PageBlock
                {
                    Id = "page-1",
                    Style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f },
                    PageNumber = 1,
                    PageWidth = 612f,
                    PageHeight = 792f,
                    Children = []
                }
            ]
        };
    }

    private static VisualDocument CreateVisualDocument()
    {
        return new VisualDocument
        {
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 612f,
                    Height = 792f,
                    Layers =
                    [
                        new VisualLayer
                        {
                            Name = "body",
                            Elements = []
                        }
                    ]
                }
            ]
        };
    }

    private sealed class TrackingVisualDocumentBuilder : IVisualDocumentBuilder
    {
        private readonly VisualDocument _visualDocument;

        public TrackingVisualDocumentBuilder(VisualDocument visualDocument)
        {
            _visualDocument = visualDocument;
        }

        public int BuildCallCount { get; private set; }

        public VisualDocument Build(ReportDocument reportDocument)
        {
            BuildCallCount++;
            return _visualDocument;
        }
    }
}
