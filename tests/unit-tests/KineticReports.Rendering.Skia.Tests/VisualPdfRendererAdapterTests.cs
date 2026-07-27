namespace KineticReports.Rendering.Skia.Tests;

using System.Text;
using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering.Skia;
using KineticReports.Core.Visual;

public class VisualPdfRendererAdapterTests
{
    [Fact]
    public async Task RenderAsync_WithVisualDocument_WritesPdfArtifact()
    {
        var skiaRenderer = new VisualSkiaRenderer();
        IVisualRenderer renderer = new VisualPdfRendererAdapter(skiaRenderer);

        var document = new VisualDocument
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
                            Elements =
                            [
                                new VisualText
                                {
                                    Id = "text-1",
                                    Bounds = new Rect(36f, 48f, 180f, 24f),
                                    Text = "PDF"
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        await using var output = new MemoryStream();
        await renderer.RenderAsync(document, output);

        var bytes = output.ToArray();
        bytes.Length.ShouldBeGreaterThan(100);
        Encoding.ASCII.GetString(bytes, 0, 4).ShouldBe("%PDF");
        renderer.Format.FormatId.ShouldBe("pdf");
    }
}
