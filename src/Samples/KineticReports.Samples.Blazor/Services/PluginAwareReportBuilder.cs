namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Layout;
using KineticReports.Engine.Building;
using KineticReports.Engine.Data;
using KineticReports.Engine.Expressions;
using KineticReports.Plugins;

/// <summary>
/// Decorates an <see cref="IReportBuilder"/> and applies loaded
/// <see cref="IReportBandsPostProcessorPlugin"/> instances before layout.
/// </summary>
internal sealed class PluginAwareReportBuilder : IReportBuilder
{
    private readonly SampleReportBuilder _inner;
    private readonly IPluginService _pluginService;
    private readonly IPluginExecutionTraceStore _traceStore;

    public PluginAwareReportBuilder(
        SampleReportBuilder inner,
        IPluginService pluginService,
        IPluginExecutionTraceStore traceStore)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _pluginService = pluginService ?? throw new ArgumentNullException(nameof(pluginService));
        _traceStore = traceStore ?? throw new ArgumentNullException(nameof(traceStore));
    }

    /// <inheritdoc/>
    public IReadOnlyList<BandElement> Build(DataContext dataContext, IExpressionEvaluator evaluator)
    {
        IReadOnlyList<BandElement> bands = _inner.Build(dataContext, evaluator);

        var postProcessors = _pluginService.LoadedPlugins
            .OfType<IReportBandsPostProcessorPlugin>()
            .OrderBy(p => p.Order)
            .ThenBy(p => p.Id, StringComparer.Ordinal);

        _traceStore.Add($"[Bands] Initial band count: {bands.Count}");

        foreach (var postProcessor in postProcessors)
        {
            var before = bands.Count;
            bands = postProcessor.ProcessBands(bands);
            _traceStore.Add($"[Bands] {postProcessor.Id} (Order {postProcessor.Order}) transformed {before} -> {bands.Count} band(s)");
        }

        return bands;
    }
}
