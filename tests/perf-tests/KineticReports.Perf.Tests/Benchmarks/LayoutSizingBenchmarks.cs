using BenchmarkDotNet.Attributes;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;

namespace KineticReports.Perf.Tests;

/// <summary>
/// Benchmarks for layout engine performance.
/// Measures the cost of type dispatch in LayoutSize() and Arrange() methods.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(PerformanceConfig))]
public class LayoutSizingBenchmarks
{
    private MockLayoutSizingContext _context = new();
    private ContentBlock _rootBlock = null!;
    private ContentBlock[] _flatBlocks = [];

    [GlobalSetup]
    public void Setup()
    {
        _context = new MockLayoutSizingContext();

        // Create a nested block hierarchy (3 levels deep)
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        var level2Blocks = new List<ContentBlock>();
        for (int i = 0; i < 10; i++)
        {
            level2Blocks.Add(new ContentBlock
            {
                Id = $"level2-{i}",
                Style = style,
                ContentType = BlockContentType.Text,
                Text = $"Child {i}"
            });
        }

        var level1Blocks = new List<ContentBlock>();
        for (int i = 0; i < 5; i++)
        {
            level1Blocks.Add(new ContentBlock
            {
                Id = $"level1-{i}",
                Style = style,
                ContentType = BlockContentType.Container,
                Children = level2Blocks.ToArray()
            });
        }

        _rootBlock = new ContentBlock
        {
            Id = "root",
            Style = style,
            ContentType = BlockContentType.Container,
            Children = level1Blocks.ToArray()
        };

        // Also create a flat array for simpler benchmarks
        var flatList = new List<ContentBlock>();
        for (int i = 0; i < 100; i++)
        {
            var contentType = (BlockContentType)(i % 12);
            flatList.Add(new ContentBlock
            {
                Id = $"block-{i}",
                Style = style,
                ContentType = contentType,
                Text = contentType == BlockContentType.Text ? $"Text {i}" : null
            });
        }
        _flatBlocks = flatList.ToArray();
    }

    /// <summary>
    /// Benchmark LayoutSize dispatch on flat array of 100 blocks
    /// </summary>
    [Benchmark(Description = "LayoutSize Dispatch (100 blocks)")]
    public Size LayoutSizeDispatch()
    {
        var totalSize = new Size(0, 0);
        foreach (var block in _flatBlocks)
        {
            block.LayoutSize(new Size(100, 100), _context);
            totalSize = new Size(totalSize.Width + block.DesiredSize.Width, totalSize.Height + block.DesiredSize.Height);
        }
        return totalSize;
    }

    /// <summary>
    /// Benchmark LayoutSize on nested block hierarchy
    /// </summary>
    [Benchmark(Description = "LayoutSize Nested Hierarchy")]
    public Size LayoutSizeNested()
    {
        _rootBlock.LayoutSize(new Size(400, 600), _context);
        return _rootBlock.DesiredSize;
    }

    /// <summary>
    /// Benchmark Arrange dispatch on flat array
    /// </summary>
    [Benchmark(Description = "Arrange Dispatch (100 blocks)")]
    public int ArrangeDispatch()
    {
        int count = 0;
        var rect = new Rect(0, 0, 100, 100);
        foreach (var block in _flatBlocks)
        {
            block.LayoutSize(new Size(100, 100), _context);
            block.Arrange(rect);
            count++;
        }
        return count;
    }

    /// <summary>
    /// Benchmark container children measurement
    /// </summary>
    [Benchmark(Description = "Container Children Measurement")]
    public Size MeasureContainerChildren()
    {
        var totalSize = new Size(0, 0);
        foreach (var block in _flatBlocks)
        {
            if (block.ContentType is BlockContentType.Container)
            {
                foreach (var child in block.Children)
                {
                    child.LayoutSize(new Size(100, 100), _context);
                    totalSize = new Size(totalSize.Width + child.DesiredSize.Width, totalSize.Height + child.DesiredSize.Height);
                }
            }
        }
        return totalSize;
    }

    /// <summary>
    /// Measure full layout pipeline: LayoutSize + Arrange
    /// </summary>
    [Benchmark(Description = "Full Layout Pipeline")]
    public Rect FullLayoutPipeline()
    {
        var rect = new Rect(0, 0, 400, 600);
        _rootBlock.LayoutSize(new Size(400, 600), _context);
        _rootBlock.Arrange(rect);
        return _rootBlock.Bounds;
    }

    /// <summary>
    /// Mock implementation of ILayoutSizingContext for testing
    /// </summary>
    private sealed class MockLayoutSizingContext : ILayoutSizingContext
    {
        public ITextLayout TextLayout => new MockTextLayout();
        public Size? ResolveImageSize(string imageKey) => new(100, 100);
    }

    private sealed class MockTextLayout : ITextLayout
    {
        public Size MeasureText(string text, AppliedStyle style, float maxWidth) => new(100, 20);

        public IReadOnlyList<TextRun> ShapeText(string text, AppliedStyle style, Rect bounds)
        {
            // Return an empty list - not needed for benchmark measurements
            return [];
        }

        public float GetAscent(FontDescriptor descriptor) => 16f;
        public float GetDescent(FontDescriptor descriptor) => 4f;
        public float GetLineGap(FontDescriptor descriptor) => 0f;
    }
}
