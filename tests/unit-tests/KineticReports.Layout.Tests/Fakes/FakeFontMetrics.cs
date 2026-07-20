namespace KineticReports.Layout.Tests.Fakes;

using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;

/// <summary>
/// A deterministic, dependency-free <see cref="IFontMetrics"/> for layout tests.
/// charWidth = fontSize * 0.6, lineHeight = fontSize * 1.2, ascent = fontSize * 0.8
/// </summary>
public sealed class FakeFontMetrics : IFontMetrics
{
    public Size MeasureText(string text, ResolvedStyle style, float maxWidth)
    {
        float charWidth = style.FontSize * 0.6f;
        float lineHeight = style.FontSize * 1.2f;
        float fullWidth = text.Length * charWidth;

        if (maxWidth <= 0f || maxWidth == float.PositiveInfinity || fullWidth <= maxWidth)
            return new Size(fullWidth, lineHeight);

        float charsPerLine = maxWidth / charWidth;
        int lines = (int)Math.Ceiling(text.Length / charsPerLine);
        return new Size(maxWidth, lines * lineHeight);
    }

    public IReadOnlyList<TextRun> ShapeText(string text, ResolvedStyle style, Rect bounds)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        float ascent = style.FontSize * 0.8f;
        float lineHeight = style.FontSize * style.LineHeight;
        float contentX = bounds.X + style.Padding.Left;
        float contentY = bounds.Y + style.Padding.Top;
        float contentWidth = bounds.Width - style.Padding.Horizontal;
        float charWidth = style.FontSize * 0.6f;
        float fullWidth = text.Length * charWidth;

        if (contentWidth <= 0f || fullWidth <= contentWidth)
        {
            return [new TextRun
            {
                Text = text,
                BaselineOrigin = new Point(contentX, contentY + ascent),
                Bounds = new Rect(contentX, contentY, Math.Min(fullWidth, bounds.Width), lineHeight),
                Style = style
            }];
        }

        int charsPerLine = Math.Max(1, (int)(contentWidth / charWidth));
        var runs = new List<TextRun>();
        float y = contentY;
        for (int i = 0; i < text.Length; i += charsPerLine)
        {
            int len = Math.Min(charsPerLine, text.Length - i);
            string lineText = text.Substring(i, len);
            runs.Add(new TextRun
            {
                Text = lineText,
                BaselineOrigin = new Point(contentX, y + ascent),
                Bounds = new Rect(contentX, y, len * charWidth, lineHeight),
                Style = style
            });
            y += lineHeight;
        }
        return runs;
    }

    public float GetAscent(FontDescriptor descriptor) => descriptor.Size * 0.8f;
    public float GetDescent(FontDescriptor descriptor) => descriptor.Size * 0.2f;
    public float GetLineGap(FontDescriptor descriptor) => 0f;
}
