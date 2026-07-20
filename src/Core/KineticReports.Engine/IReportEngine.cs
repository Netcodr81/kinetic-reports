namespace KineticReports.Engine;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Layout;

/// <summary>
/// Orchestrates the full report pipeline: data resolution, expression evaluation,
/// logical tree construction, and the layout pipeline.
/// </summary>
public interface IReportEngine
{
    /// <summary>
    /// Executes the report pipeline and returns the immutable <see cref="LayoutTree"/>.
    /// </summary>
    /// <param name="definition">The immutable report definition.</param>
    /// <param name="parameters">
    /// Runtime parameter values whose keys match the IDs declared in
    /// <paramref name="definition"/>. Pass an empty dictionary when the report
    /// has no parameters.
    /// </param>
    /// <param name="measureContext">Font metrics and image-resolution services.</param>
    /// <param name="layoutOptions">
    /// Page layout configuration. Uses <see cref="LayoutOptions"/> defaults
    /// when <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The completed, immutable <see cref="LayoutTree"/>.</returns>
    Task<LayoutTree> RunAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        IMeasureContext measureContext,
        LayoutOptions? layoutOptions = null,
        CancellationToken cancellationToken = default);
}
