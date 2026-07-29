namespace KineticReports.Engine.Tests;

using KineticReports.Core.Definition;
using KineticReports.Core.Engine;
using KineticReports.Core.Engine.Expressions;
using KineticReports.Core.LayoutEngine;
using KineticReports.Engine.Tests.Fakes;

public class ReportEngineTests
{
    private static readonly ReportDefinition SimpleDefinition = new()
    {
        SchemaVersion = "1.0",
        Id = "test-report",
        Name = "Test Report"
    };

    private readonly MeasureContextFake _measureContext = new();
    private readonly LayoutOptions _options = new();

    // -------------------------------------------------------------------------
    // Basic execution
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RunAsync_WithNoDataSources_ReturnsReportDocument()
    {
        var sut = BuildEngine();
        var tree = await sut.RunAsync(SimpleDefinition, new Dictionary<string, object?>(), _measureContext, _options);
        tree.ShouldNotBeNull();
    }

    [Fact]
    public async Task RunAsync_WithNoDataSources_ProducesAtLeastOnePage()
    {
        var sut = BuildEngine();
        var tree = await sut.RunAsync(SimpleDefinition, new Dictionary<string, object?>(), _measureContext, _options);
        tree.PageCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task RunAsync_UsesDefaultLayoutOptions_WhenNullPassed()
    {
        var sut = BuildEngine();
        var tree = await sut.RunAsync(SimpleDefinition, new Dictionary<string, object?>(), _measureContext, layoutOptions: null);
        tree.Pages[0].PageWidth.ShouldBe(new LayoutOptions().PageWidth);
    }

    [Fact]
    public async Task RunAsync_PopulatesReportDocumentMetadata_FromDefinition()
    {
        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "quarterly-report",
            Name = "Quarterly Business Review",
            Metadata = new Dictionary<string, object>
            {
                ["department"] = "Sales",
                ["year"] = 2026
            }
        };

        var sut = BuildEngine();
        var tree = await sut.RunAsync(definition, new Dictionary<string, object?>(), _measureContext, _options);

        tree.Metadata.ReportId.ShouldBe("quarterly-report");
        tree.Metadata.ReportName.ShouldBe("Quarterly Business Review");
        tree.Metadata.FileName.ShouldBe("Quarterly Business Review");
        tree.Metadata.Properties["department"].ShouldBe("Sales");
        tree.Metadata.Properties["year"].ShouldBe(2026);
    }

    // -------------------------------------------------------------------------
    // Data source resolution
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RunAsync_CallsDataResolverForEachDataSource()
    {
        var resolver = new StubDataResolver();
        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "rpt",
            Name = "Test",
            DataSources = [
                new DataSourceDefinition { Id = "ds1", Name = "Source 1", ProviderType = "test", SourceName = "TestSourceOne" },
                new DataSourceDefinition { Id = "ds2", Name = "Source 2", ProviderType = "test", SourceName = "TestSourceTwo" }
            ]
        };

        var sut = BuildEngine(resolver: resolver);
        await sut.RunAsync(definition, new Dictionary<string, object?>(), _measureContext, _options);

        resolver.CallCount.ShouldBe(2);
    }

    // -------------------------------------------------------------------------
    // Cancellation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RunAsync_WithCancelledToken_ThrowsOperationCancelledException()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var definition = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "rpt",
            Name = "Test",
            DataSources = [new DataSourceDefinition { Id = "ds1", Name = "S1", ProviderType = "test", SourceName = "TestSourceOne" }]
        };

        var sut = BuildEngine();
        await Should.ThrowAsync<OperationCanceledException>(() =>
            sut.RunAsync(definition, new Dictionary<string, object?>(), _measureContext, _options, cts.Token));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ReportEngine BuildEngine(
        StubDataResolver? resolver = null,
        StubReportBuilder? bandsBuilder = null)
    {
        return new ReportEngine(
            resolver ?? new StubDataResolver(),
            new LiteralEvaluator(),
            bandsBuilder ?? new StubReportBuilder(),
            new KineticReports.Core.LayoutEngine.LayoutEngine());
    }

    private sealed class MeasureContextFake : KineticReports.Core.Layout.ILayoutSizingContext
    {
        public KineticReports.Core.Typography.ITextLayout TextLayout { get; } = new FakeTextLayout();
        public KineticReports.Core.Geometry.Size? ResolveImageSize(string imageKey) => null;
    }
}
