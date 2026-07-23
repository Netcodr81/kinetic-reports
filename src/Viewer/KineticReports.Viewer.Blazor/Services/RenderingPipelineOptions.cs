namespace KineticReports.Viewer.Blazor.Services;

/// <summary>
/// Configuration options for selecting the report rendering pipeline mode.
/// </summary>
public sealed class RenderingPipelineOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Rendering";

    /// <summary>
    /// Gets or sets the pipeline mode. Supported values: Legacy, Visual.
    /// </summary>
    public string PipelineMode { get; set; } = "Legacy";

    /// <summary>
    /// Gets a value indicating whether visual mode is requested.
    /// </summary>
    public bool UseVisualPipeline =>
        string.Equals(PipelineMode, "Visual", StringComparison.OrdinalIgnoreCase);
}
