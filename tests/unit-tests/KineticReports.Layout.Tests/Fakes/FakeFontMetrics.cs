namespace KineticReports.Layout.Tests.Fakes;

using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;

/// <summary>
/// A deterministic, dependency-free <see cref="IFontMetrics"/> implementation
/// for use in unit tests. Returns fixed proportional measurements:
/// <list type="bullet">
///   <item><description>Text width  = character count × fontSize × 0.6</description></item>
///   <item><description>Text height = fontSize × 1.2 (one line)</description></item>
///   <item><description>Ascent      = fontSize × 0.8</description></item>
///   <item><description>Descent     = fontSize × 0.2</description></item>
///   <item><description>Line gap    = 0</description></item>
/// </list>
/// </summary>
public sealed class FakeFontMetrics : IFontMetrics
{
    /// <inheritdoc/>
    public Size MeasureText(string text, ResolvedStyle style, float maxWidth)
    {
        float charWidth = style.FontSize * 0.6f;
        float lineHeight = style.FontSize * 1.2f;
        float fullWidth = text.Length * charWidth;

        if (maxWidth <= 0f || maxWidth == float.PositiveInfinity || fullWidth <= maxWidth)
            return new Size(fullWidth, lineHeight);

        // Simple word-wrap: count lines needed.
        float charsPerLine = maxWidth / charWidth;
        int lines = (int)Math.Ceiling(text.Length / charsPerLine);
        return new Size(maxWidth, lines * lineHeight);
    }

    /// <inheritdoc/>
    public float GetAscent(FontDescriptor descriptor) => descriptor.Size * 0.8f;

    /// <inheritdoc/>
    public float GetDescent(FontDescriptor descriptor) => descriptor.Size * 0.2f;

    /// <inheritdoc/>
    public float GetLineGap(FontDescriptor descriptor) => 0f;
}
