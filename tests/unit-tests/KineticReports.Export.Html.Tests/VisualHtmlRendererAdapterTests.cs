namespace KineticReports.Export.Html.Tests;

using System.Text;
using KineticReports.Core.Geometry;
using KineticReports.Core.Visual;
using KineticReports.Visual;

public class VisualHtmlRendererAdapterTests
{
    [Fact]
    public async Task RenderAsync_WithVisualDocument_WritesHtmlArtifact()
    {
        var exporter = new VisualHtmlExporter();
        IVisualRenderer renderer = new VisualHtmlRendererAdapter(exporter);

        var document = new VisualDocument
        {
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 200f,
                    Height = 120f,
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
                                    Bounds = new Rect(10f, 12f, 60f, 20f),
                                    Text = "Hello"
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        await using var output = new MemoryStream();
        await renderer.RenderAsync(document, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("id=\"text-1\"");
        renderer.Format.FormatId.ShouldBe("html");
    }
}
