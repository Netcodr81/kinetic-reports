namespace KineticReports.Core.Rendering;

/// <summary>
/// Controls which file format <see cref="IRenderer"/> implementations write to the output stream.
/// </summary>
public enum RenderFormat
{
    /// <summary>
    /// Portable Network Graphics (PNG). Single-page; one PNG file per rendered page.
    /// Suitable for previews, thumbnails, and testing.
    /// </summary>
    Png,

    /// <summary>
    /// Portable Document Format (PDF). Multi-page; all pages are written as a single PDF document.
    /// Suitable for print and archival output.
    /// </summary>
    Pdf,
}

/// <summary>
/// Configuration for a single render pass.
/// All measurements are in Device Independent Pixels (1/96 inch) unless noted.
/// </summary>
public sealed record RenderOptions
{
    /// <summary>
    /// Gets the output resolution in dots per inch.
    /// Default is 96 DPI (1:1 DIP-to-pixel mapping).
    /// Use 300+ for print-quality raster output.
    /// </summary>
    public float Dpi { get; init; } = 96f;

    /// <summary>
    /// Gets the page background color drawn before all content.
    /// Default is <see cref="Styling.Color.White"/>.
    /// </summary>
    public Styling.Color Background { get; init; } = Core.Styling.Color.White;

    /// <summary>
    /// Gets the output format written to the render stream.
    /// Default is <see cref="RenderFormat.Pdf"/>.
    /// </summary>
    public RenderFormat Format { get; init; } = RenderFormat.Pdf;
}
