namespace KineticReports.Export.Html.Tests;

using System.Text;
using KineticReports.Core.Geometry;
using KineticReports.Export.Html;
using KineticReports.Visual;

public class VisualHtmlExporterTests
{
    [Fact]
    public async Task ExportAsync_WithVisualDocument_ProducesSemanticWrappersAndModelJson()
    {
        var visualDocument = new VisualDocument
        {
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 816f,
                    Height = 1056f,
                    Layers =
                    [
                        new VisualLayer
                        {
                            Name = "header",
                            Elements =
                            [
                                new VisualText
                                {
                                    Id = "header-text-1",
                                    Bounds = new Rect(20f, 10f, 240f, 24f),
                                    Text = "Header"
                                }
                            ]
                        },
                        new VisualLayer
                        {
                            Name = "body",
                            Elements =
                            [
                                new VisualTablePlaceholder
                                {
                                    Id = "table-ph-1",
                                    Bounds = new Rect(20f, 80f, 420f, 120f),
                                    RowCount = 3,
                                    ColumnCount = 4
                                },
                                new VisualUnsupportedElement
                                {
                                    Id = "unsupported-1",
                                    Bounds = new Rect(20f, 220f, 300f, 40f),
                                    SourceType = "ChartBlock",
                                    Reason = "Chart visual primitive not mapped in phase 1."
                                }
                            ]
                        },
                        new VisualLayer
                        {
                            Name = "footer",
                            Elements =
                            [
                                new VisualText
                                {
                                    Id = "footer-text-1",
                                    Bounds = new Rect(20f, 1020f, 260f, 20f),
                                    Text = "Footer"
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var exporter = new VisualHtmlExporter();

        using var output = new MemoryStream();
        await exporter.ExportAsync(visualDocument, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("id=\"report-document\"");
        html.ShouldContain("id=\"report-document-model\"");
        html.ShouldContain("page-header-region");
        html.ShouldContain("page-body-region");
        html.ShouldContain("page-footer-region");
        html.ShouldContain("visual-table-placeholder");
        html.ShouldContain("visual-unsupported");
        html.ShouldContain("schemaVersion");
    }

    [Fact]
    public async Task ExportAsync_WithNestedContainer_UsesRelativeChildPositioning()
    {
        var visualDocument = new VisualDocument
        {
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 400f,
                    Height = 300f,
                    Layers =
                    [
                        new VisualLayer
                        {
                            Name = "body",
                            Elements =
                            [
                                new VisualContainer
                                {
                                    Id = "container-1",
                                    Bounds = new Rect(100f, 120f, 200f, 120f),
                                    Children =
                                    [
                                        new VisualText
                                        {
                                            Id = "child-text-1",
                                            Bounds = new Rect(130f, 150f, 80f, 20f),
                                            Text = "Inside"
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var exporter = new VisualHtmlExporter();

        using var output = new MemoryStream();
        await exporter.ExportAsync(visualDocument, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("id=\"child-text-1\"");
        html.ShouldContain("left: 30.0px; top: 30.0px;");
    }

    [Fact]
    public async Task ExportAsync_WithClippedElement_EmitsOverflowHiddenStyle()
    {
        var clipped = new VisualText
        {
            Id = "clip-text-1",
            Bounds = new Rect(20f, 20f, 120f, 24f),
            Text = "Clipped",
            Clips =
            [
                new VisualClip
                {
                    Bounds = new Rect(20f, 20f, 80f, 20f)
                }
            ]
        };

        var visualDocument = new VisualDocument
        {
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 400f,
                    Height = 300f,
                    Layers =
                    [
                        new VisualLayer
                        {
                            Name = "body",
                            Elements = [clipped]
                        }
                    ]
                }
            ]
        };

        var exporter = new VisualHtmlExporter();

        using var output = new MemoryStream();
        await exporter.ExportAsync(visualDocument, output);

        var html = Encoding.UTF8.GetString(output.ToArray());
        html.ShouldContain("id=\"clip-text-1\"");
        html.ShouldContain("overflow: hidden;");
    }
}
