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

    /// <summary>
    /// Gets optional PDF watermark overlay options.
    /// When specified, PDF pages are rendered with a centered watermark.
    /// </summary>
    public PdfWatermarkOptions? PdfWatermark { get; init; }
}

/// <summary>
/// Options for rendering a text watermark overlay on PDF pages.
/// </summary>
public sealed record PdfWatermarkOptions
{
    /// <summary>
    /// Gets the watermark text to draw.
    /// </summary>
    public string Text { get; init; } = "KineticReports";

    /// <summary>
    /// Gets watermark opacity in the range [0.0, 1.0].
    /// </summary>
    public float Opacity { get; init; } = 0.08f;

    /// <summary>
    /// Gets clockwise rotation in degrees.
    /// </summary>
    public float RotationDegrees { get; init; } = -30f;

    /// <summary>
    /// Gets text size in DIPs.
    /// </summary>
    public float FontSize { get; init; } = 72f;

    /// <summary>
    /// Gets watermark text color.
    /// </summary>
    public Styling.Color Color { get; init; } = Core.Styling.Color.Black;
}
