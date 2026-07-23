namespace KineticReports.Core.Tests.Engine.Building;

using KineticReports.Core.Engine.Building;
using KineticReports.Core.Engine.Data;
using KineticReports.Core.Engine.Expressions;
using KineticReports.Core.Layout;
using KineticReports.Core.Plugins;
using KineticReports.Core.Styling;

public class DefaultReportBuilderTests
{
    [Fact]
    public void Build_WithNoConfiguredSteps_ReturnsEmptyList()
    {
        var sut = new DefaultReportBuilder();

        var result = sut.Build(CreateDataContext(), new LiteralEvaluator());

        result.ShouldBeEmpty();
    }

    [Fact]
    public void AddTextRegion_WithDefaults_AddsDetailBlockWithText()
    {
        var sut = new DefaultReportBuilder()
            .AddTextRegion("header-1", "Sales Summary", BlockType.ReportHeader);

        var result = sut.Build(CreateDataContext(), new LiteralEvaluator());

        result.Count.ShouldBe(1);
        var block = result[0];
        block.Kind.ShouldBe(BlockType.ReportHeader);
        block.Id.ShouldBe("header-1");

        var text = block.Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>();
        text.Text.ShouldBe("Sales Summary");
    }

    [Fact]
    public void ForEachRowText_WithFieldExpression_BindsRowValues()
    {
        var sut = new DefaultReportBuilder()
            .ForEachRowText("orders", "order-row", "{CustomerName}");

        var result = sut.Build(CreateDataContext(
        [
            Row(("CustomerName", "Contoso")),
            Row(("CustomerName", "Fabrikam"))
        ]), new LiteralEvaluator());

        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe("order-row-1");
        result[1].Id.ShouldBe("order-row-2");

        result[0].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Contoso");
        result[1].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Fabrikam");
    }

    [Fact]
    public void AddDataSourceTable_WithRows_CreatesHeaderAndDataRows()
    {
        var sut = new DefaultReportBuilder()
            .AddDataSourceTable(
                dataSourceId: "orders",
                regionId: "orders-region",
                tableId: "orders-table",
                columns:
                [
                    new DefaultReportBuilder.DataSourceTableColumn
                    {
                        Header = "Customer",
                        ValueExpression = "{CustomerName}",
                        Column = new TableColumn { MinWidth = 50f, Grow = 1f }
                    },
                    new DefaultReportBuilder.DataSourceTableColumn
                    {
                        Header = "Amount",
                        ValueExpression = "{Amount}",
                        Column = new TableColumn { Width = 90f, MinWidth = 90f, Grow = 0f }
                    }
                ]);

        var result = sut.Build(CreateDataContext(
        [
            Row(("CustomerName", "Contoso"), ("Amount", 123.45m)),
            Row(("CustomerName", "Fabrikam"), ("Amount", 456.78m))
        ]), new LiteralEvaluator());

        result.Count.ShouldBe(1);
        var block = result[0];
        block.Id.ShouldBe("orders-region");

        var table = block.Children.ShouldHaveSingleItem().ShouldBeOfType<TableBlock>();
        table.Id.ShouldBe("orders-table");
        table.Columns.Count.ShouldBe(2);
        table.Rows.Count.ShouldBe(3);

        table.Rows[0].RowType.ShouldBe(RowType.Header);
        table.Rows[0].Cells[0].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Customer");
        table.Rows[0].Cells[1].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Amount");

        table.Rows[1].RowType.ShouldBe(RowType.Data);
        table.Rows[1].Cells[0].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Contoso");
        table.Rows[1].Cells[1].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("123.45");

        table.Rows[2].RowType.ShouldBe(RowType.Data);
        table.Rows[2].Cells[0].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Fabrikam");
        table.Rows[2].Cells[1].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("456.78");
    }

    [Fact]
    public void ForEachRow_WithCustomFactory_AllowsAdvancedBlockComposition()
    {
        var detailStyle = new AppliedStyle { FontFamily = "Consolas", FontSize = 10f };

        var sut = new DefaultReportBuilder()
            .ForEachRow("orders", context =>
            {
                var text = context.EvaluateAsString("{CustomerName}");
                return ReportDsl.TextRegion(
                    id: $"line-{context.RowIndex + 1}",
                    blockType: BlockType.Detail,
                    blockStyle: detailStyle,
                    textStyle: detailStyle,
                    text: $"Row {context.RowIndex + 1}: {text}");
            });

        var result = sut.Build(CreateDataContext(
        [
            Row(("CustomerName", "Litware"))
        ]), new LiteralEvaluator());

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe("line-1");
        result[0].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Row 1: Litware");
    }

    [Fact]
    public void AddDataSourceTable_WithGroupingAndFooterAggregates_AddsGroupHeadersAndFooter()
    {
        var sut = new DefaultReportBuilder()
            .AddDataSourceTable(
                dataSourceId: "orders",
                regionId: "orders-region",
                tableId: "orders-table",
                columns:
                [
                    new DefaultReportBuilder.DataSourceTableColumn
                    {
                        Header = "Region",
                        ValueExpression = "{Region}",
                        Column = new TableColumn { MinWidth = 80f, Grow = 1f }
                    },
                    new DefaultReportBuilder.DataSourceTableColumn
                    {
                        Header = "Amount",
                        ValueExpression = "{Amount}",
                        Column = new TableColumn { Width = 90f, MinWidth = 90f, Grow = 0f }
                    }
                ],
                groupByExpression: "{Region}",
                footerAggregates:
                [
                    new DefaultReportBuilder.TableAggregateDefinition
                    {
                        ColumnIndex = 1,
                        ValueExpression = "{Amount}",
                        Kind = DefaultReportBuilder.TableAggregateKind.Sum,
                        FormatString = "0.00"
                    }
                ],
                footerLabel: "Grand Total");

        var result = sut.Build(CreateDataContext(
        [
            Row(("Region", "East"), ("Amount", 100m)),
            Row(("Region", "East"), ("Amount", 50m)),
            Row(("Region", "West"), ("Amount", 200m))
        ]), new LiteralEvaluator());

        var table = result.ShouldHaveSingleItem()
            .Children.ShouldHaveSingleItem()
            .ShouldBeOfType<TableBlock>();

        table.Rows.Count.ShouldBe(7);

        var firstGroupHeader = table.Rows[1];
        firstGroupHeader.RowType.ShouldBe(RowType.Data);
        firstGroupHeader.Cells[0].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Group: East");

        var secondGroupHeader = table.Rows[4];
        secondGroupHeader.Cells[0].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Group: West");

        var footer = table.Rows[^1];
        footer.RowType.ShouldBe(RowType.Footer);
        footer.Cells[0].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("Grand Total");
        footer.Cells[1].Children.ShouldHaveSingleItem().ShouldBeOfType<TextBlock>().Text.ShouldBe("350.00");
    }

    [Fact]
    public void Build_WithLoadedPostProcessorPlugins_AppliesPluginTransformations()
    {
        var pluginManager = new FakePluginManager(
        [
            new MarkerPostProcessorPlugin("plugin-a", order: 20),
            new MarkerPostProcessorPlugin("plugin-b", order: 10)
        ]);

        var sut = new DefaultReportBuilder(pluginManager)
            .AddTextRegion("detail-1", "Line", BlockType.Detail);

        var result = sut.Build(CreateDataContext(), new LiteralEvaluator());

        result.Count.ShouldBe(3);
        result[0].Id.ShouldBe("detail-1");
        result[1].Id.ShouldBe("plugin-b-marker");
        result[2].Id.ShouldBe("plugin-a-marker");
    }

    private static DataContext CreateDataContext(
        IReadOnlyList<IReadOnlyDictionary<string, object?>>? rows = null)
    {
        var dataRows = rows ?? [];

        return new DataContext
        {
            DataSources = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>>(StringComparer.Ordinal)
            {
                ["orders"] = dataRows
            }
        };
    }

    private static IReadOnlyDictionary<string, object?> Row(params (string Key, object? Value)[] values)
    {
        var row = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in values)
            row[key] = value;

        return row;
    }

    private sealed class FakePluginManager : IPluginManager
    {
        public FakePluginManager(IReadOnlyList<IPlugin> loadedPlugins)
        {
            LoadedPlugins = loadedPlugins;
        }

        public IReadOnlyList<IPlugin> LoadedPlugins { get; }

        public Task DiscoverAndLoadPluginsAsync(string pluginDirectory, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IPlugin> LoadPluginAsync(string assemblyPath, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public IPlugin? GetPluginById(string pluginId)
            => LoadedPlugins.FirstOrDefault(plugin => string.Equals(plugin.Id, pluginId, StringComparison.Ordinal));
    }

    private sealed class MarkerPostProcessorPlugin : IReportBlocksPostProcessorPlugin
    {
        public MarkerPostProcessorPlugin(string id, int order)
        {
            Id = id;
            Order = order;
        }

        public string Id { get; }

        public string Name => Id;

        public string Version => "1.0.0";

        public string? Author => null;

        public string? Description => null;

        public int Order { get; }

        public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UnloadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public IReadOnlyList<ReportBlock> ProcessBlocks(IReadOnlyList<ReportBlock> blocks)
        {
            var transformed = blocks.ToList();
            transformed.Add(ReportBlockFactory.CreatePageBreak(
                $"{Id}-marker",
                new AppliedStyle { FontFamily = "Arial", FontSize = 11f }));
            return transformed;
        }
    }
}
