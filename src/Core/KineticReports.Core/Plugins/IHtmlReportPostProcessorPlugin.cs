namespace KineticReports.Plugins;

/// <summary>
/// Optional plugin capability that post-processes exported HTML.
/// This runs after report execution and HTML export, and before the final
/// HTML is returned to the caller.
/// </summary>
public interface IHtmlReportPostProcessorPlugin : IPlugin
{
    /// <summary>
    /// Gets execution order for deterministic post-processing.
    /// Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Applies plugin-specific transformations to exported HTML.
    /// </summary>
    /// <param name="html">The exported HTML to transform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transformed HTML.</returns>
    ValueTask<string> ProcessHtmlAsync(string html, CancellationToken cancellationToken = default);
}
