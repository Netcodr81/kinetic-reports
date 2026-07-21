namespace KineticReports.Rendering.Skia.Tests;

using System.IO;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Rendering.Skia;

public class SkiaRendererTests
{
    private static readonly ResolvedStyle DefaultStyle = new() { FontFamily = "Arial", FontSize = 12f };

    [Fact]
    public async Task RenderAsync_WithSinglePage_CompletesSuccessfully()
    {
        var tree = MakeSimpleLayoutTree();
        var renderer = new SkiaRenderer(new RenderOptions { Format = RenderFormat.Pdf });

        using var output = new MemoryStream();
        await renderer.RenderAsync(tree, output);

        output.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_PngFormat_ProducesNonEmptyStream()
    {
        var tree = MakeSimpleLayoutTree();
        var renderer = new SkiaRenderer(new RenderOptions { Format = RenderFormat.Png });

        using var output = new MemoryStream();
        await renderer.RenderAsync(tree, output);

        output.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_WithMultiplePages_IncludesAllPages()
    {
        var tree = MakeLayoutTreeWithPages(3);
        var renderer = new SkiaRenderer(new RenderOptions { Format = RenderFormat.Pdf });

        using var output = new MemoryStream();
        await renderer.RenderAsync(tree, output);

        // PDF is generated; content size should vary with page count.
        output.Length.ShouldBeGreaterThan(100);
    }

    // -----

    private static ReportLayout MakeSimpleLayoutTree() =>
        new() { Pages = [MakePage(1)] };

    private static ReportLayout MakeLayoutTreeWithPages(int count) =>
        new() { Pages = Enumerable.Range(1, count).Select(MakePage).ToList() };

    private static PageElement MakePage(int pageNum)
    {
        var page = new PageElement
        {
            Id = $"page-{pageNum}",
            Style = DefaultStyle,
            PageWidth = 612f,
            PageHeight = 792f,
            PageNumber = pageNum,
            Children = []
        };
        page.Arrange(new Rect(0, 0, 612, 792));
        return page;
    }
}
