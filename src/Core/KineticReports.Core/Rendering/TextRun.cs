namespace KineticReports.Core.Rendering;

using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;

/// <summary>
/// Represents a single shaped and positioned run of text ready for rendering.
/// Text runs are produced by the layout engine after word-wrapping and glyph shaping,
/// and are consumed directly by renderers. Renderers must not perform any text
/// measurement or layout of their own.
/// </summary>
public sealed record TextRun
{
    /// <summary>Gets the text content of this run.</summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the baseline origin of this run in page-relative coordinates (DIPs).
    /// </summary>
    public required Point BaselineOrigin { get; init; }

    /// <summary>Gets the bounding box of this run in page-relative coordinates (DIPs).</summary>
    public required Rect Bounds { get; init; }

    /// <summary>Gets the fully resolved style used to draw this run.</summary>
    public required ResolvedStyle Style { get; init; }

    /// <summary>
    /// Gets a value indicating whether this run is rendered right-to-left
    /// (e.g. Arabic, Hebrew).
    /// </summary>
    public bool IsRightToLeft { get; init; }
}
