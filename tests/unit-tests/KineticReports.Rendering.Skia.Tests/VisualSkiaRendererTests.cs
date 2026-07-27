namespace KineticReports.Rendering.Skia.Tests;

using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering.Skia;
using KineticReports.Core.Visual;

public class VisualSkiaRendererTests
{
    [Fact]
    public async Task RenderPdfAsync_WithSinglePageVisualDocument_WritesPdfBytes()
    {
        var document = CreateVisualDocument(1);
        var renderer = new VisualSkiaRenderer();

        using var output = new MemoryStream();
        await renderer.RenderPdfAsync(document, output);

        output.Length.ShouldBeGreaterThan(100);
        output.Position = 0;
        using var reader = new StreamReader(output, leaveOpen: true);
        var chars = new char[4];
        _ = reader.Read(chars, 0, chars.Length);
        var header = new string(chars);
        header.ShouldBe("%PDF");
    }

    [Fact]
    public async Task RenderPdfAsync_WithMultiplePages_ProducesLargerArtifact()
    {
        var onePageDocument = CreateVisualDocument(1);
        var threePageDocument = CreateVisualDocument(3);
        var renderer = new VisualSkiaRenderer();

        using var singleOutput = new MemoryStream();
        await renderer.RenderPdfAsync(onePageDocument, singleOutput);

        using var multiOutput = new MemoryStream();
        await renderer.RenderPdfAsync(threePageDocument, multiOutput);

        multiOutput.Length.ShouldBeGreaterThan(singleOutput.Length);
    }

    [Fact]
    public async Task RenderPdfAsync_WithVisualImageUsingFilePathSource_RendersWithoutError()
    {
        var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO8B2eQAAAAASUVORK5CYII=";
        var pngBytes = Convert.FromBase64String(pngBase64);
        var tempFile = Path.Combine(Path.GetTempPath(), $"kr-image-{Guid.NewGuid():N}.png");
        await File.WriteAllBytesAsync(tempFile, pngBytes);

        try
        {
            var document = new VisualDocument
            {
                Pages =
                [
                    new VisualPage
                    {
                        PageNumber = 1,
                        Width = 300f,
                        Height = 200f,
                        Layers =
                        [
                            new VisualLayer
                            {
                                Name = "body",
                                Elements =
                                [
                                    new VisualImage
                                    {
                                        Id = "img-1",
                                        Bounds = new Rect(20f, 20f, 80f, 80f),
                                        SourceKey = tempFile,
                                        Stretch = VisualImageStretch.Uniform
                                    }
                                ]
                            }
                        ]
                    }
                ]
            };

            var renderer = new VisualSkiaRenderer();

            using var output = new MemoryStream();
            await renderer.RenderPdfAsync(document, output);

            output.Length.ShouldBeGreaterThan(100);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static VisualDocument CreateVisualDocument(int pageCount)
    {
        var pages = new List<VisualPage>();

        for (var i = 1; i <= pageCount; i++)
        {
            pages.Add(new VisualPage
            {
                PageNumber = i,
                Width = 612f,
                Height = 792f,
                Layers =
                [
                    new VisualLayer
                    {
                        Name = "body",
                        Elements =
                        [
                            new VisualShape
                            {
                                Id = $"shape-{i}",
                                Bounds = new Rect(36f, 40f, 200f, 80f),
                                Kind = VisualShapeKind.Rectangle,
                                FillColorHex = "#FFF3F6FC",
                                StrokeColorHex = "#FF334466",
                                StrokeWidth = 1.5f
                            },
                            new VisualText
                            {
                                Id = $"text-{i}",
                                Bounds = new Rect(48f, 64f, 180f, 24f),
                                Text = $"Visual Page {i}",
                                FontFamily = "Arial",
                                FontSize = 14f,
                                TextColorHex = "#FF1F2A3D"
                            },
                            new VisualTablePlaceholder
                            {
                                Id = $"table-{i}",
                                Bounds = new Rect(36f, 150f, 280f, 120f),
                                RowCount = 4,
                                ColumnCount = 3
                            }
                        ]
                    }
                ]
            });
        }

        return new VisualDocument
        {
            Pages = pages
        };
    }
}
