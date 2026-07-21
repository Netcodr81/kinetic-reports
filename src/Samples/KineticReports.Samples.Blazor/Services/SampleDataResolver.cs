namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Definition;
using KineticReports.Engine.Data;

/// <summary>
/// Provides deterministic in-memory data for the Blazor sample reports.
/// </summary>
internal sealed class SampleDataResolver : IDataResolver
{
    private const string SampleProviderType = "SampleInMemory";

    /// <inheritdoc/>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ResolveAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.Equals(definition.ProviderType, SampleProviderType, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(CreateRows(
                new Dictionary<string, object?>
                {
                    ["Message"] = $"Unsupported provider type '{definition.ProviderType}'.",
                    ["DataSourceId"] = definition.Id
                }));
        }

        var rows = definition.Id switch
        {
            "quote-daily" => CreateRows(
                new Dictionary<string, object?>
                {
                    ["Heading"] = "Daily Operations Snapshot",
                    ["Quote"] = "Reliable reports start with deterministic data.",
                    ["GeneratedOn"] = new DateTime(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc),
                    ["Author"] = "KineticReports Samples"
                }),

            "sales-orders" => CreateRows(
                new Dictionary<string, object?>
                {
                    ["OrderId"] = "SO-1001",
                    ["Region"] = "North",
                    ["SalesPerson"] = "Alicia",
                    ["Amount"] = 12450.75m,
                    ["Status"] = "Complete"
                },
                new Dictionary<string, object?>
                {
                    ["OrderId"] = "SO-1002",
                    ["Region"] = "West",
                    ["SalesPerson"] = "Marcus",
                    ["Amount"] = 8975.40m,
                    ["Status"] = "Shipped"
                },
                new Dictionary<string, object?>
                {
                    ["OrderId"] = "SO-1003",
                    ["Region"] = "South",
                    ["SalesPerson"] = "Priya",
                    ["Amount"] = 15320.00m,
                    ["Status"] = "Pending"
                }),

            "customer-activity" => CreateRows(
                new Dictionary<string, object?>
                {
                    ["CustomerName"] = "Acme Manufacturing",
                    ["Tier"] = "Gold",
                    ["LastInvoice"] = "INV-4421",
                    ["Balance"] = 1400.00m,
                    ["AssignedRep"] = "Noah"
                },
                new Dictionary<string, object?>
                {
                    ["CustomerName"] = "Bright Retail",
                    ["Tier"] = "Silver",
                    ["LastInvoice"] = "INV-4428",
                    ["Balance"] = 0.00m,
                    ["AssignedRep"] = "Avery"
                },
                new Dictionary<string, object?>
                {
                    ["CustomerName"] = "Contour Labs",
                    ["Tier"] = "Platinum",
                    ["LastInvoice"] = "INV-4432",
                    ["Balance"] = 510.25m,
                    ["AssignedRep"] = "Iris"
                }),

            "kpi-summary" => CreateRows(
                new Dictionary<string, object?>
                {
                    ["Metric"] = "Monthly Revenue",
                    ["Value"] = "$348,210",
                    ["Trend"] = "+8.2%"
                },
                new Dictionary<string, object?>
                {
                    ["Metric"] = "Gross Margin",
                    ["Value"] = "41.8%",
                    ["Trend"] = "+1.1%"
                },
                new Dictionary<string, object?>
                {
                    ["Metric"] = "Open Projects",
                    ["Value"] = "27",
                    ["Trend"] = "-2"
                }),

            "regional-performance" => CreateRows(
                new Dictionary<string, object?>
                {
                    ["Region"] = "North",
                    ["Quota"] = "$120,000",
                    ["Actual"] = "$128,500",
                    ["Attainment"] = "107%"
                },
                new Dictionary<string, object?>
                {
                    ["Region"] = "South",
                    ["Quota"] = "$95,000",
                    ["Actual"] = "$92,750",
                    ["Attainment"] = "98%"
                },
                new Dictionary<string, object?>
                {
                    ["Region"] = "West",
                    ["Quota"] = "$105,000",
                    ["Actual"] = "$110,400",
                    ["Attainment"] = "105%"
                }),

            "style-showcase" => CreateRows(
                new Dictionary<string, object?>
                {
                    ["Label"] = "Primary Heading",
                    ["Preview"] = "Bold text with spacing and high contrast"
                },
                new Dictionary<string, object?>
                {
                    ["Label"] = "Body Content",
                    ["Preview"] = "Readable default styling for long-form text"
                },
                new Dictionary<string, object?>
                {
                    ["Label"] = "Accent Note",
                    ["Preview"] = "Emphasized content for callouts"
                }),

            _ => CreateRows(
                new Dictionary<string, object?>
                {
                    ["Message"] = $"No sample rows defined for data source '{definition.Id}'."
                })
        };

        return Task.FromResult(rows);
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, object?>> CreateRows(
        params IReadOnlyDictionary<string, object?>[] rows)
    {
        return rows;
    }
}
