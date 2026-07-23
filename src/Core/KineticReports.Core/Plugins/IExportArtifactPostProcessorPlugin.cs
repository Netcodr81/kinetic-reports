namespace KineticReports.Core.Plugins;

/// <summary>
/// Plugin seam that post-processes exported artifacts after exporter execution.
/// </summary>
public interface IExportArtifactPostProcessorPlugin : IPlugin
{
    /// <summary>
    /// Gets execution order for deterministic processing.
    /// Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Post-processes an exported artifact.
    /// </summary>
    /// <param name="formatId">Final format ID selected for export.</param>
    /// <param name="artifact">Artifact bytes to transform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<byte[]> ProcessArtifactAsync(
        string formatId,
        byte[] artifact,
        CancellationToken cancellationToken = default);
}
