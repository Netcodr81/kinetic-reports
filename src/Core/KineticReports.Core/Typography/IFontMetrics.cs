namespace KineticReports.Core.Typography;

using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;

/// <summary>
/// Provides font measurement services used during the Measure pass.
/// Implementations are backed by a specific font engine (e.g. SkiaSharp, DirectWrite).
/// </summary>
/// <remarks>
/// Font metrics are centralized (ADR-015) so that all renderers share identical
/// measurement results, ensuring deterministic and pixel-consistent layout.
/// </remarks>
public interface IFontMetrics
{
    /// <summary>
    /// Measures the bounding box of <paramref name="text"/> when rendered with
    /// the specified <paramref name="style"/>, constrained to
    /// <paramref name="maxWidth"/> DIPs.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="style">The resolved style containing font properties.</param>
    /// <param name="maxWidth">
    /// The maximum available width in DIPs.
    /// Pass <see cref="float.PositiveInfinity"/> for unconstrained measurement.
    /// </param>
    /// <returns>The measured bounding size in DIPs.</returns>
    Size MeasureText(string text, ResolvedStyle style, float maxWidth);

    /// <summary>
    /// Returns the ascent of the font described by <paramref name="descriptor"/> in DIPs.
    /// The ascent is the distance from the baseline to the top of the tallest glyph.
    /// </summary>
    /// <param name="descriptor">The font face to query.</param>
    /// <returns>The ascent in DIPs.</returns>
    float GetAscent(FontDescriptor descriptor);

    /// <summary>
    /// Returns the descent of the font described by <paramref name="descriptor"/> in DIPs.
    /// The descent is the distance from the baseline downward to the lowest descender.
    /// </summary>
    /// <param name="descriptor">The font face to query.</param>
    /// <returns>The descent in DIPs (a positive value).</returns>
    float GetDescent(FontDescriptor descriptor);

    /// <summary>
    /// Returns the recommended line gap for the font described by
    /// <paramref name="descriptor"/> in DIPs.
    /// </summary>
    /// <param name="descriptor">The font face to query.</param>
    /// <returns>The line gap in DIPs.</returns>
    float GetLineGap(FontDescriptor descriptor);
}
