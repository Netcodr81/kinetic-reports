using BenchmarkDotNet.Attributes;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;

namespace KineticReports.Perf.Tests;

/// <summary>
/// Benchmarks for renderer element dispatch performance.
/// Measures the cost of rendering dispatch in ReportDocumentRenderer and HtmlExporter.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(PerformanceConfig))]
public class RendererDispatchBenchmarks
{
    private ContentBlock[] _mixedBlocks = [];
    private int _dispatchCount = 0;

    [GlobalSetup]
    public void Setup()
    {
        // Create 100 blocks with mixed types (simulates typical report)
        var blockList = new List<ContentBlock>();
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        // Create typical report structure: header, content, footer
        for (int i = 0; i < 100; i++)
        {
            var contentType = (BlockContentType)(i % 12);
            blockList.Add(new ContentBlock
            {
                Id = $"block-{i}",
                Style = style,
                ContentType = contentType,
                Text = contentType == BlockContentType.Text ? $"Text content {i}" : null,
                SourceKey = contentType == BlockContentType.Image ? $"image-{i}.png" : null,
                Children = contentType is BlockContentType.Container or BlockContentType.Table or BlockContentType.Row
                    ? new[] { new ContentBlock { Id = $"child-{i}", Style = style, ContentType = BlockContentType.Text } }
                    : []
            });
        }

        _mixedBlocks = blockList.ToArray();
    }

    /// <summary>
    /// Benchmark renderer element dispatch: 100 mixed block types
    /// </summary>
    [Benchmark(Description = "Renderer Dispatch (100 mixed blocks)")]
    public int RendererDispatch()
    {
        _dispatchCount = 0;
        foreach (var block in _mixedBlocks)
        {
            DispatchRendererElement(block);
        }
        return _dispatchCount;
    }

    /// <summary>
    /// Simulate the ReportDocumentRenderer.RenderElement dispatch pattern
    /// </summary>
    private void DispatchRendererElement(LayoutBlock block)
    {
        if (block is ContentBlock content)
        {
            switch (content.ContentType)
            {
                case BlockContentType.Text:
                    _dispatchCount += 1;
                    break;
                case BlockContentType.Image:
                    _dispatchCount += 2;
                    break;
                case BlockContentType.Shape:
                    _dispatchCount += 3;
                    break;
                case BlockContentType.Chart:
                    _dispatchCount += 4;
                    break;
                case BlockContentType.Barcode:
                    _dispatchCount += 5;
                    break;
                case BlockContentType.Container:
                    _dispatchCount += 6;
                    break;
                case BlockContentType.Table:
                    _dispatchCount += 7;
                    break;
                case BlockContentType.Row:
                    _dispatchCount += 8;
                    break;
                case BlockContentType.Cell:
                    _dispatchCount += 9;
                    break;
                case BlockContentType.ReportSection:
                    _dispatchCount += 10;
                    break;
                case BlockContentType.PageSection:
                    _dispatchCount += 11;
                    break;
                case BlockContentType.Page:
                    _dispatchCount += 12;
                    break;
            }
        }
    }

    /// <summary>
    /// Benchmark the cost of children iteration in containers
    /// </summary>
    [Benchmark(Description = "Container Children Iteration")]
    public int IterateContainerChildren()
    {
        int count = 0;
        foreach (var block in _mixedBlocks)
        {
            if (block.ContentType is BlockContentType.Container or BlockContentType.Table or BlockContentType.Row)
            {
                foreach (var child in block.Children)
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// Benchmark accessing type-specific properties
    /// </summary>
    [Benchmark(Description = "Type-Specific Property Access")]
    public string AccessTypeSpecificProperties()
    {
        var result = string.Empty;
        foreach (var block in _mixedBlocks)
        {
            switch (block.ContentType)
            {
                case BlockContentType.Text:
                    result += block.Text ?? string.Empty;
                    break;
                case BlockContentType.Image:
                    result += block.SourceKey ?? string.Empty;
                    break;
                case BlockContentType.Shape:
                    result += block.Kind.ToString();
                    break;
                case BlockContentType.Barcode:
                    result += block.Symbology ?? string.Empty;
                    break;
                case BlockContentType.Cell:
                    result += block.ColumnIndex.ToString();
                    break;
                default:
                    result += "default";
                    break;
            }
        }
        return result;
    }

    /// <summary>
    /// Benchmark typical renderer workload: dispatch + property access + recursion
    /// </summary>
    [Benchmark(Description = "Realistic Renderer Workload")]
    public int RealisticRendererWorkload()
    {
        return ProcessBlocks(_mixedBlocks);
    }

    private int ProcessBlocks(IReadOnlyList<ContentBlock> blocks)
    {
        int count = 0;
        foreach (var block in blocks)
        {
            count += ProcessBlock(block);
        }
        return count;
    }

    private int ProcessBlock(ContentBlock block)
    {
        int count = 1; // Count the dispatch

        // Type-specific processing
        switch (block.ContentType)
        {
            case BlockContentType.Text:
                if (!string.IsNullOrEmpty(block.Text))
                    count += block.Text.Length;
                break;
            case BlockContentType.Image:
                if (!string.IsNullOrEmpty(block.SourceKey))
                    count += 10;
                break;
            case BlockContentType.Shape:
                count += (int)block.StrokeWidth;
                break;
            case BlockContentType.Container:
            case BlockContentType.Table:
            case BlockContentType.Row:
                count += ProcessBlocks(block.Children);
                break;
        }

        return count;
    }
}
