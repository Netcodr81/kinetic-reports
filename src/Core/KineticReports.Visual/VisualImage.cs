namespace KineticReports.Visual;

/// <summary>
/// Represents an image drawing primitive.
/// </summary>
public sealed record VisualImage : VisualElement
{
    /// <summary>
    /// Gets the source key or URI for this image.
    /// </summary>
    public required string SourceKey { get; init; }

    /// <summary>
    /// Gets the image stretch behavior.
    /// </summary>
    public VisualImageStretch Stretch { get; init; } = VisualImageStretch.Uniform;
}

/// <summary>
/// Defines supported image scaling strategies.
/// </summary>
public enum VisualImageStretch
{
    /// <summary>
    /// No scaling is applied.
    /// </summary>
    None,

    /// <summary>
    /// Uniformly scales content to fit within bounds.
    /// </summary>
    Uniform,

    /// <summary>
    /// Uniformly scales content to fill bounds.
    /// </summary>
    UniformToFill,

    /// <summary>
    /// Stretches content independently on both axes.
    /// </summary>
    Fill
}
