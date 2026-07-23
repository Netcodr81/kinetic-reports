namespace KineticReports.Core.Tests.Data;

using KineticReports.Core.Data;

public class PocoDataProviderTests
{
    [Fact]
    public async Task ExecuteAsync_WithPocoDataset_ReturnsExpectedSchemaAndRows()
    {
        var source = new[]
        {
            new SalesRow(1, "Contoso", 1250.50m),
            new SalesRow(2, "Fabrikam", 875.00m)
        };

        var provider = new PocoDataProvider("sales", source.Cast<object?>());

        var result = await provider.ExecuteAsync(new QueryRequest
        {
            DatasetName = "sales",
            QueryText = string.Empty,
            Parameters = new Dictionary<string, object?>()
        });

        result.Schema.Count.ShouldBe(4);
        result.Schema[0].Name.ShouldBe(nameof(SalesRow.Id));
        result.Schema[1].Name.ShouldBe(nameof(SalesRow.Customer));
        result.Schema[2].Name.ShouldBe(nameof(SalesRow.Region));
        result.Schema[3].Name.ShouldBe(nameof(SalesRow.Amount));

        result.RowCount.ShouldBe(2);
        result.Rows[0].Values[0].ShouldBe(1);
        result.Rows[0].Values[1].ShouldBe("Contoso");
        result.Rows[0].Values[2].ShouldBe(string.Empty);
        result.Rows[0].Values[3].ShouldBe(1250.50m);
        result.Rows[1].Values[0].ShouldBe(2);
        result.Rows[1].Values[1].ShouldBe("Fabrikam");
        result.Rows[1].Values[2].ShouldBe(string.Empty);
        result.Rows[1].Values[3].ShouldBe(875.00m);
    }

    [Fact]
    public async Task ExecuteAsync_WithInlineAggregatedData_UsesRequestParameterObjects()
    {
        var aggregated = new object?[]
        {
            new { Source = "OrdersApi", Region = "North", Total = 10 },
            new Dictionary<string, object?>
            {
                ["Source"] = "BillingDb",
                ["Region"] = "South",
                ["Total"] = 4,
                ["DistinctCustomers"] = 3
            },
            42
        };

        var provider = new PocoDataProvider();

        var result = await provider.ExecuteAsync(new QueryRequest
        {
            DatasetName = "aggregated",
            QueryText = string.Empty,
            Parameters = new Dictionary<string, object?>
            {
                [PocoDataProvider.InlineDataParameterName] = aggregated
            }
        });

        result.RowCount.ShouldBe(3);
        result.Schema.Select(column => column.Name).ShouldContain("Source");
        result.Schema.Select(column => column.Name).ShouldContain("Region");
        result.Schema.Select(column => column.Name).ShouldContain("Total");
        result.Schema.Select(column => column.Name).ShouldContain("DistinctCustomers");
        result.Schema.Select(column => column.Name).ShouldContain("Value");

        // Third row came from scalar input.
        var valueColumn = result.Schema.First(column => column.Name == "Value");
        result.Rows[2].Values[valueColumn.Ordinal].ShouldBe(42);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownDataset_ReturnsEmptyResult()
    {
        var provider = new PocoDataProvider();

        var result = await provider.ExecuteAsync(new QueryRequest
        {
            DatasetName = "missing",
            QueryText = string.Empty,
            Parameters = new Dictionary<string, object?>()
        });

        result.RowCount.ShouldBe(0);
        result.Schema.Count.ShouldBe(0);
        result.Rows.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithLinqExpressionAgainstSource_AppliesFilteringProjectionAndAggregation()
    {
        var source = new[]
        {
            new SalesRow(1, "Contoso", "North", 100m),
            new SalesRow(2, "Fabrikam", "North", 50m),
            new SalesRow(3, "Tailspin", "South", 20m),
        };

        var provider = new PocoDataProvider();

        var result = await provider.ExecuteAsync<SalesRow, RegionAggregate>(
            source,
            query => query
                .Where(row => row.Region == "North")
                .GroupBy(row => row.Region)
                .Select(group => new RegionAggregate
                {
                    Region = group.Key,
                    Total = group.Sum(item => item.Amount),
                    Count = group.Count(),
                }));

        result.RowCount.ShouldBe(1);
        result.Schema.Select(column => column.Name).ShouldContain(nameof(RegionAggregate.Region));
        result.Schema.Select(column => column.Name).ShouldContain(nameof(RegionAggregate.Total));
        result.Schema.Select(column => column.Name).ShouldContain(nameof(RegionAggregate.Count));

        result.Rows[0].Values[0].ShouldBe("North");
        result.Rows[0].Values[1].ShouldBe(150m);
        result.Rows[0].Values[2].ShouldBe(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithLinqExpressionAgainstRegisteredDataset_UsesNamedSource()
    {
        var source = new[]
        {
            new SalesRow(1, "Contoso", "North", 100m),
            new SalesRow(2, "Fabrikam", "South", 35m),
            new SalesRow(3, "Tailspin", "South", 55m),
        };

        var provider = new PocoDataProvider("sales", source.Cast<object?>());

        var result = await provider.ExecuteAsync<SalesRow, object>(
            "sales",
            query => query
                .Where(row => row.Region == "South")
                .Select(row => new
                {
                    row.Customer,
                    row.Amount
                })
                .Cast<object>());

        result.RowCount.ShouldBe(2);
        result.Schema.Select(column => column.Name).ShouldContain("Customer");
        result.Schema.Select(column => column.Name).ShouldContain("Amount");
    }

    [Fact]
    public async Task ExecuteAsync_ThroughILinqDataProviderAbstraction_ExecutesQuery()
    {
        ILinqDataProvider provider = new PocoDataProvider();
        var source = new[]
        {
            new SalesRow(1, "Contoso", "North", 100m),
            new SalesRow(2, "Fabrikam", "South", 35m),
            new SalesRow(3, "Tailspin", "South", 55m),
        };

        var result = await provider.ExecuteAsync<SalesRow, object>(
            source,
            query => query
                .Where(row => row.Region == "South")
                .Select(row => new
                {
                    row.Customer,
                    row.Amount
                })
                .Cast<object>());

        result.RowCount.ShouldBe(2);
        result.Schema.Select(column => column.Name).ShouldContain("Customer");
        result.Schema.Select(column => column.Name).ShouldContain("Amount");
    }

    private sealed record SalesRow(int Id, string Customer, string Region, decimal Amount)
    {
        public SalesRow(int id, string customer, decimal amount)
            : this(id, customer, string.Empty, amount)
        {
        }
    }

    private sealed record RegionAggregate
    {
        public required string Region { get; init; }

        public required decimal Total { get; init; }

        public required int Count { get; init; }
    }
}
