namespace KineticReports.Engine.Tests;

using KineticReports.Core.Definition;
using KineticReports.Engine.Expressions;
using KineticReports.Engine.Tests.Fakes;
using KineticReports.Layout;

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
    public async Task RunAsync_WithNoDataSources_ReturnsReportLayout()
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
                new DataSourceDefinition { Id = "ds1", Name = "Source 1", ProviderType = "test" },
                new DataSourceDefinition { Id = "ds2", Name = "Source 2", ProviderType = "test" }
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
            DataSources = [new DataSourceDefinition { Id = "ds1", Name = "S1", ProviderType = "test" }]
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
            new KineticReports.Layout.LayoutEngine());
    }

    private sealed class MeasureContextFake : KineticReports.Core.Layout.ILayoutSizingContext
    {
        public KineticReports.Core.Typography.IFontMetrics FontMetrics { get; } = new FakeFontMetrics();
        public KineticReports.Core.Geometry.Size? ResolveImageSize(string imageKey) => null;
    }
}
