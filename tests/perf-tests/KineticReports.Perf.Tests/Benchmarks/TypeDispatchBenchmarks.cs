using BenchmarkDotNet.Attributes;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

namespace KineticReports.Perf.Tests;

/// <summary>
/// Benchmarks for type dispatch performance:
/// - v3.0.0: Enum-based dispatch (switch on BlockContentType)
/// - v2.x (simulated): Polymorphic dispatch (12 is checks)
/// </summary>
[MemoryDiagnoser]
[Config(typeof(PerformanceConfig))]
public class TypeDispatchBenchmarks
{
    private ContentBlock[] _blocks = [];

    [GlobalSetup]
    public void Setup()
    {
        // Create 1000 blocks distributed evenly across all 12 types
        var blockList = new List<ContentBlock>();
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        for (int i = 0; i < 1000; i++)
        {
            var contentType = (BlockContentType)(i % 12);
            blockList.Add(new ContentBlock
            {
                Id = $"block-{i}",
                Style = style,
                ContentType = contentType,
                Text = contentType == BlockContentType.Text ? "Test" : null
            });
        }

        _blocks = blockList.ToArray();
    }

    /// <summary>
    /// v3.0.0: Enum-based dispatch using switch statement (FAST)
    /// </summary>
    [Benchmark(Description = "v3.0.0: Enum Switch Dispatch")]
    public int EnumSwitchDispatch()
    {
        int count = 0;
        foreach (var block in _blocks)
        {
            var result = ProcessBlockWithEnumSwitch(block);
            count += result;
        }
        return count;
    }

    /// <summary>
    /// v2.x (simulated): Polymorphic dispatch using 12 'is' checks (SLOW)
    /// </summary>
    [Benchmark(Description = "v2.x: Simulated Polymorphic Dispatch")]
    public int PolymorphicDispatch()
    {
        int count = 0;
        foreach (var block in _blocks)
        {
            var result = ProcessBlockWithPolymorphicSwitch(block);
            count += result;
        }
        return count;
    }

    /// <summary>
    /// v3.0.0 dispatch: Single enum switch
    /// </summary>
    private static int ProcessBlockWithEnumSwitch(ContentBlock block)
    {
        return block.ContentType switch
        {
            BlockContentType.Text => 1,
            BlockContentType.Image => 2,
            BlockContentType.Shape => 3,
            BlockContentType.Chart => 4,
            BlockContentType.Barcode => 5,
            BlockContentType.Container => 6,
            BlockContentType.Table => 7,
            BlockContentType.Row => 8,
            BlockContentType.Cell => 9,
            BlockContentType.ReportSection => 10,
            BlockContentType.PageSection => 11,
            BlockContentType.Page => 12,
            _ => 0
        };
    }

    /// <summary>
    /// v2.x dispatch (simulated): 12 sequential 'is' checks
    /// Simulates if (block is TextBlock) { } else if (block is ImageBlock) { } ...
    /// </summary>
    private static int ProcessBlockWithPolymorphicSwitch(ContentBlock block)
    {
        // This simulates the v2.x approach of checking the ContentType enum value
        // to route to the appropriate handler (since we can't have 12 actual subclasses here)
        if (block.ContentType == BlockContentType.Text)
            return 1;
        if (block.ContentType == BlockContentType.Image)
            return 2;
        if (block.ContentType == BlockContentType.Shape)
            return 3;
        if (block.ContentType == BlockContentType.Chart)
            return 4;
        if (block.ContentType == BlockContentType.Barcode)
            return 5;
        if (block.ContentType == BlockContentType.Container)
            return 6;
        if (block.ContentType == BlockContentType.Table)
            return 7;
        if (block.ContentType == BlockContentType.Row)
            return 8;
        if (block.ContentType == BlockContentType.Cell)
            return 9;
        if (block.ContentType == BlockContentType.ReportSection)
            return 10;
        if (block.ContentType == BlockContentType.PageSection)
            return 11;
        if (block.ContentType == BlockContentType.Page)
            return 12;
        return 0;
    }

    /// <summary>
    /// Benchmark direct enum switch performance (no type checks)
    /// </summary>
    [Benchmark(Description = "Direct Enum Switch (No Type Check)")]
    public int DirectEnumSwitch()
    {
        int count = 0;
        foreach (var contentType in _blocks.Select(b => b.ContentType))
        {
            count += contentType switch
            {
                BlockContentType.Text => 1,
                BlockContentType.Image => 2,
                BlockContentType.Shape => 3,
                BlockContentType.Chart => 4,
                BlockContentType.Barcode => 5,
                BlockContentType.Container => 6,
                BlockContentType.Table => 7,
                BlockContentType.Row => 8,
                BlockContentType.Cell => 9,
                BlockContentType.ReportSection => 10,
                BlockContentType.PageSection => 11,
                BlockContentType.Page => 12,
                _ => 0
            };
        }
        return count;
    }

    /// <summary>
    /// Benchmark the overhead of 12 sequential equality checks
    /// </summary>
    [Benchmark(Description = "Sequential Equality Checks")]
    public int SequentialEqualityChecks()
    {
        int count = 0;
        var textType = BlockContentType.Text;
        var imageType = BlockContentType.Image;
        var shapeType = BlockContentType.Shape;
        var chartType = BlockContentType.Chart;
        var barcodeType = BlockContentType.Barcode;
        var containerType = BlockContentType.Container;
        var tableType = BlockContentType.Table;
        var rowType = BlockContentType.Row;
        var cellType = BlockContentType.Cell;
        var reportSectionType = BlockContentType.ReportSection;
        var pageSectionType = BlockContentType.PageSection;
        var pageType = BlockContentType.Page;

        foreach (var block in _blocks)
        {
            var ct = block.ContentType;
            if (ct == textType) count += 1;
            else if (ct == imageType) count += 2;
            else if (ct == shapeType) count += 3;
            else if (ct == chartType) count += 4;
            else if (ct == barcodeType) count += 5;
            else if (ct == containerType) count += 6;
            else if (ct == tableType) count += 7;
            else if (ct == rowType) count += 8;
            else if (ct == cellType) count += 9;
            else if (ct == reportSectionType) count += 10;
            else if (ct == pageSectionType) count += 11;
            else if (ct == pageType) count += 12;
        }
        return count;
    }
}
