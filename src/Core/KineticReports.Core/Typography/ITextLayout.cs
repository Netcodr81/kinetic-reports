namespace KineticReports.Core.Typography;

using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;

/// <summary>
/// Provides text layout services used during the layout passes.
/// Implementations are backed by a specific font engine (e.g. SkiaSharp, DirectWrite).
/// Text layout is centralized (ADR-015) so all renderers share identical measurements.
/// </summary>
public interface ITextLayout
{
    /// <summary>
    /// Measures the bounding box of <paramref name="text"/> constrained to <paramref name="maxWidth"/> DIPs.
    /// </summary>
    Size MeasureText(string text, ResolvedStyle style, float maxWidth);

    /// <summary>
    /// Shapes <paramref name="text"/> into word-wrapped <see cref="TextRun"/> objects
    /// positioned within <paramref name="bounds"/>, accounting for padding.
    /// Returns an empty list when <paramref name="text"/> is empty.
    /// </summary>
    IReadOnlyList<TextRun> ShapeText(string text, ResolvedStyle style, Rect bounds);

    /// <summary>Returns the ascent (baseline to cap-height) of the descriptor font in DIPs.</summary>
    float GetAscent(FontDescriptor descriptor);

    /// <summary>Returns the descent (baseline to descender bottom) of the descriptor font in DIPs.</summary>
    float GetDescent(FontDescriptor descriptor);

    /// <summary>Returns the recommended line gap of the descriptor font in DIPs.</summary>
    float GetLineGap(FontDescriptor descriptor);
}