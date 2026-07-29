using KineticReports.Core.Export.Document;

namespace KineticReports.Rendering.Skia.Tests;

using System.Text;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

public class PdfReportDocumentExporterTests
{
    [Fact]
    public async Task ExportAsync_WithReportDocumentContent_ReturnsPdf()
    {
        var reportDocument = CreateReportDocument();
        var sut = new PdfReportDocumentExporter();

        var artifact = await sut.ExportAsync(reportDocument);

        artifact.Length.ShouldBeGreaterThan(100);
        Encoding.ASCII.GetString(artifact, 0, 4).ShouldBe("%PDF");
    }

    [Fact]
    public async Task ExportAsync_WithEmbeddedContentBlockImage_ReturnsPdf()
    {
        var reportDocument = new ReportDocument
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
                    Children =
                    [
                        new ContentBlock
                        {
                            Id = "img-1",
                            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f },
                            ContentType = BlockContentType.Image,
                            SourceKey = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZkLsAAAAASUVORK5CYII="
                        }
                    ]
                }
            ]
        };

        var sut = new PdfReportDocumentExporter();

        var artifact = await sut.ExportAsync(reportDocument);

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
                    Children =
                    [
                        new TextBlock
                        {
                            Id = "text-1",
                            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f },
                            Text = "Hello PDF"
                        }
                    ]
                }
            ]
        };
    }
}
