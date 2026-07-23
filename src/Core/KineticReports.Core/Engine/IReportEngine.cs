namespace KineticReports.Core.Engine;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.LayoutEngine;

/// <summary>
/// Orchestrates the full report pipeline: data resolution, expression evaluation,
/// report content-region construction, and the layout pipeline.
/// </summary>
public interface IReportEngine
{
    /// <summary>
    /// Executes the report pipeline with default runtime inputs and returns the immutable
    /// <see cref="ReportDocument"/>.
    /// </summary>
    /// <param name="definition">The immutable report definition.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The completed, immutable <see cref="ReportDocument"/>.</returns>
    /// <remarks>
    /// This convenience overload is intended for the common flow where callers only need
    /// to pass a <see cref="ReportDefinition"/> and receive a <see cref="ReportDocument"/>.
    /// It uses default values for:
    /// <list type="bullet">
    /// <item><description>Parameters: empty dictionary.</description></item>
    /// <item><description>Layout options: <see cref="LayoutOptions"/> defaults.</description></item>
    /// <item><description>Layout sizing context: created from the engine's registered text layout service.</description></item>
    /// </list>
    /// </remarks>
    Task<ReportDocument> RunAsync(
        ReportDefinition definition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the report pipeline and returns the immutable <see cref="ReportDocument"/>.
    /// </summary>
    /// <param name="definition">The immutable report definition.</param>
    /// <param name="parameters">
    /// Runtime parameter values whose keys match the IDs declared in
    /// <paramref name="definition"/>. Pass an empty dictionary when the report
    /// has no parameters.
    /// </param>
    /// <param name="layoutSizingContext">Text layout and image-resolution services.</param>
    /// <param name="layoutOptions">
    /// Page layout configuration. Uses <see cref="LayoutOptions"/> defaults
    /// when <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The completed, immutable <see cref="ReportDocument"/>.</returns>
    Task<ReportDocument> RunAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        ILayoutSizingContext layoutSizingContext,
        LayoutOptions? layoutOptions = null,
        CancellationToken cancellationToken = default);
}
