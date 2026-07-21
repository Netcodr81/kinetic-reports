namespace KineticReports.Rendering.Tests.Fakes;

using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;

/// <summary>
/// Deterministic fake text layout for Rendering unit tests.
/// charWidth = fontSize × 0.6, lineHeight = fontSize × 1.2, ascent = fontSize × 0.8
/// </summary>
public sealed class FakeTextLayout : ITextLayout
{
    public Size MeasureText(string text, ResolvedStyle style, float maxWidth)
        => new Size(text.Length * style.FontSize * 0.6f, style.FontSize * 1.2f);

    public IReadOnlyList<TextRun> ShapeText(string text, ResolvedStyle style, Rect bounds)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        float ascent = style.FontSize * 0.8f;
        float lineHeight = style.FontSize * style.LineHeight;
        return [new TextRun
        {
            Text = text,
            BaselineOrigin = new Point(bounds.X + style.Padding.Left, bounds.Y + style.Padding.Top + ascent),
            Bounds = new Rect(bounds.X + style.Padding.Left, bounds.Y + style.Padding.Top,
                text.Length * style.FontSize * 0.6f, lineHeight),
            Style = style
        }];
    }

    public float GetAscent(FontDescriptor descriptor) => descriptor.Size * 0.8f;
    public float GetDescent(FontDescriptor descriptor) => descriptor.Size * 0.2f;
    public float GetLineGap(FontDescriptor descriptor) => 0f;
}
