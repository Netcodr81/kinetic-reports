namespace KineticReports.Core.Plugins;

using KineticReports.Core.Layout;

/// <summary>
/// Optional plugin capability that post-processes report blocks.
/// This runs after <c>IReportBuilder.Build(...)</c> and before the layout engine executes.
/// </summary>
public interface IReportBlocksPostProcessorPlugin : IPlugin
{
    /// <summary>
    /// Gets execution order for deterministic post-processing.
    /// Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Applies plugin-specific transformations to report blocks before layout.
    /// </summary>
    /// <param name="blocks">The report blocks produced by the report builder.</param>
    /// <returns>The transformed report blocks.</returns>
    IReadOnlyList<ReportBlock> ProcessBlocks(IReadOnlyList<ReportBlock> blocks);
}
