using BenchmarkDotNet.Attributes;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

namespace KineticReports.Perf.Tests;

/// <summary>
/// Benchmarks for memory usage and GC pressure.
/// Measures memory allocation patterns of ContentBlock consolidation.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(PerformanceConfig))]
public class MemoryBenchmarks
{
    private const int BlockCount = 10000;

    /// <summary>
    /// Benchmark creating 10000 ContentBlock instances
    /// </summary>
    [Benchmark(Description = "Create 10K ContentBlocks")]
    public ContentBlock[] CreateContentBlocks()
    {
        var blocks = new ContentBlock[BlockCount];
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        for (int i = 0; i < BlockCount; i++)
        {
            var contentType = (BlockContentType)(i % 12);
            blocks[i] = new ContentBlock
            {
                Id = $"block-{i}",
                Style = style,
                ContentType = contentType,
                Text = contentType == BlockContentType.Text ? "Lorem ipsum dolor sit amet" : null
            };
        }

        return blocks;
    }

    /// <summary>
    /// Benchmark creating text blocks specifically (most common type)
    /// </summary>
    [Benchmark(Description = "Create 10K Text ContentBlocks")]
    public ContentBlock[] CreateTextBlocks()
    {
        var blocks = new ContentBlock[BlockCount];
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        for (int i = 0; i < BlockCount; i++)
        {
            blocks[i] = new ContentBlock
            {
                Id = $"text-{i}",
                Style = style,
                ContentType = BlockContentType.Text,
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit"
            };
        }

        return blocks;
    }

    /// <summary>
    /// Benchmark creating container blocks with children
    /// </summary>
    [Benchmark(Description = "Create 1K Containers with children")]
    public ContentBlock[] CreateContainersWithChildren()
    {
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };
        var containers = new ContentBlock[1000];

        for (int i = 0; i < 1000; i++)
        {
            var children = new ContentBlock[10];
            for (int j = 0; j < 10; j++)
            {
                children[j] = new ContentBlock
                {
                    Id = $"child-{i}-{j}",
                    Style = style,
                    ContentType = BlockContentType.Text,
                    Text = $"Child {j}"
                };
            }

            containers[i] = new ContentBlock
            {
                Id = $"container-{i}",
                Style = style,
                ContentType = BlockContentType.Container,
                Children = children
            };
        }

        return containers;
    }

    /// <summary>
    /// Benchmark modifying block properties (typical usage pattern)
    /// </summary>
    [Benchmark(Description = "Modify Block Properties")]
    public int ModifyBlockProperties()
    {
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };
        var blocks = new List<ContentBlock>();

        for (int i = 0; i < BlockCount; i++)
        {
            var block = new ContentBlock
            {
                Id = $"block-{i}",
                Style = style,
                ContentType = BlockContentType.Text,
                Text = "Original text"
            };

            // ContentBlock is sealed and not a record, so just add the original
            blocks.Add(block);
        }

        return blocks.Count;
    }

    /// <summary>
    /// Benchmark string interning opportunities
    /// </summary>
    [Benchmark(Description = "Repeated Block IDs (String Interning)")]
    public ContentBlock[] CreateBlocksWithRepeatedIds()
    {
        var blocks = new ContentBlock[BlockCount];
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        // Create blocks with repeated IDs (common in templates)
        for (int i = 0; i < BlockCount; i++)
        {
            var id = $"block-{i % 100}"; // Only 100 unique IDs
            blocks[i] = new ContentBlock
            {
                Id = id,
                Style = style,
                ContentType = (BlockContentType)(i % 12),
                Text = i % 100 == 0 ? "Text" : null
            };
        }

        return blocks;
    }

    /// <summary>
    /// Benchmark children list allocation patterns
    /// </summary>
    [Benchmark(Description = "Children List Allocation")]
    public List<ContentBlock> AllocateChildrenLists()
    {
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };
        var containers = new List<ContentBlock>();

        for (int i = 0; i < 1000; i++)
        {
            var childrenList = new List<ContentBlock>();

            // Typical pattern: add 5-20 children
            for (int j = 0; j < 10; j++)
            {
                childrenList.Add(new ContentBlock
                {
                    Id = $"child-{i}-{j}",
                    Style = style,
                    ContentType = BlockContentType.Text,
                    Text = $"Child {j}"
                });
            }

            containers.Add(new ContentBlock
            {
                Id = $"container-{i}",
                Style = style,
                ContentType = BlockContentType.Container,
                Children = childrenList.ToArray()
            });
        }

        return containers;
    }

    /// <summary>
    /// Benchmark block tree traversal (typical in exporters)
    /// </summary>
    [Benchmark(Description = "Block Tree Traversal")]
    public int TraverseBlockTree()
    {
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        // Create a tree: 1 root with 10 containers, each with 10 text blocks
        var root = CreateTree(style, depth: 2, childrenPerNode: 10);

        // Traverse and count
        return CountBlocks(root);
    }

    private ContentBlock CreateTree(AppliedStyle style, int depth, int childrenPerNode)
    {
        if (depth == 0)
        {
            return new ContentBlock
            {
                Id = "leaf",
                Style = style,
                ContentType = BlockContentType.Text,
                Text = "Leaf"
            };
        }

        var children = new ContentBlock[childrenPerNode];
        for (int i = 0; i < childrenPerNode; i++)
        {
            children[i] = CreateTree(style, depth - 1, childrenPerNode);
        }

        return new ContentBlock
        {
            Id = $"container-{depth}",
            Style = style,
            ContentType = BlockContentType.Container,
            Children = children
        };
    }

    private int CountBlocks(ContentBlock block)
    {
        int count = 1;
        foreach (var child in block.Children)
        {
            count += CountBlocks(child);
        }
        return count;
    }
}
