namespace KineticReports.Plugins;

using KineticReports.Core.Layout;

/// <summary>
/// Optional plugin capability that post-processes report bands.
/// This runs after <c>IReportBuilder.Build(...)</c> and before the layout engine executes.
/// </summary>
public interface IReportBandsPostProcessorPlugin : IPlugin
{
    /// <summary>
    /// Gets execution order for deterministic post-processing.
    /// Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Applies plugin-specific transformations to report bands before layout.
    /// </summary>
    /// <param name="bands">The report bands produced by the report builder.</param>
    /// <returns>The transformed report bands.</returns>
    IReadOnlyList<BandElement> ProcessBands(IReadOnlyList<BandElement> bands);
}
