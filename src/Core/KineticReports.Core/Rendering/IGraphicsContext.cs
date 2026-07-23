namespace KineticReports.Core.Rendering;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

/// <summary>
/// Backend-agnostic drawing surface consumed by <see cref="ReportDocumentRenderer"/>.
/// Each method accepts page-relative coordinates in Device Independent Pixels (DIPs).
/// Implementations translate these calls into native graphics commands
/// (SkiaSharp, HTML Canvas, Direct2D, etc.).
/// </summary>
public interface IGraphicsContext
{
    // -------------------------------------------------------------------------
    // Filled shapes
    // -------------------------------------------------------------------------

    /// <summary>Fills a rectangle with the given color.</summary>
    void FillRectangle(Rect bounds, Color color);

    /// <summary>Fills an ellipse inscribed within <paramref name="bounds"/> with the given color.</summary>
    void FillEllipse(Rect bounds, Color color);

    // -------------------------------------------------------------------------
    // Stroked shapes
    // -------------------------------------------------------------------------

    /// <summary>Draws the outline of a rectangle.</summary>
    void StrokeRectangle(Rect bounds, Color color, float strokeWidth);

    /// <summary>Draws the outline of an ellipse inscribed within <paramref name="bounds"/>.</summary>
    void StrokeEllipse(Rect bounds, Color color, float strokeWidth);

    /// <summary>Draws a straight line segment.</summary>
    void DrawLine(Point from, Point to, Color color, float strokeWidth);

    // -------------------------------------------------------------------------
    // Complex geometry
    // -------------------------------------------------------------------------

    /// <summary>
    /// Draws a vector path. Fill and stroke are taken from the path geometry's own properties.
    /// </summary>
    void DrawPath(PathGeometry path);

    // -------------------------------------------------------------------------
    // Text
    // -------------------------------------------------------------------------

    /// <summary>
    /// Draws a single pre-shaped text run. The baseline origin and style are
    /// already embedded in the <see cref="TextRun"/>.
    /// </summary>
    void DrawText(TextRun run);

    // -------------------------------------------------------------------------
    // Images
    // -------------------------------------------------------------------------

    /// <summary>
    /// Draws a decoded image within <paramref name="destinationBounds"/>,
    /// scaled according to <paramref name="stretch"/>.
    /// </summary>
    void DrawImage(Rect destinationBounds, ImageReference image, ImageStretch stretch);

    // -------------------------------------------------------------------------
    // Clip / opacity stack
    // -------------------------------------------------------------------------

    /// <summary>
    /// Pushes a rectangular clip region. All subsequent drawing is clipped to
    /// the intersection of <paramref name="clip"/> and any previously active clips.
    /// Must be balanced with a matching <see cref="PopClip"/>.
    /// </summary>
    void PushClip(Rect clip);

    /// <summary>Pops the most recently pushed clip region.</summary>
    void PopClip();

    /// <summary>
    /// Pushes an opacity layer. All subsequent drawing is composited at
    /// <paramref name="opacity"/> (0 = fully transparent, 1 = fully opaque).
    /// Must be balanced with a matching <see cref="PopOpacity"/>.
    /// </summary>
    void PushOpacity(float opacity);

    /// <summary>Pops the most recently pushed opacity layer.</summary>
    void PopOpacity();
}
