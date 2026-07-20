namespace KineticReports.Rendering.Skia;

using System.Text;
using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;
using SkiaSharp;

/// <summary>
/// Production implementation of <see cref="IFontMetrics"/> backed by SkiaSharp.
/// This is the single font metrics provider that must be shared between the layout
/// engine and the renderer to guarantee text runs fit exactly in their measured bounds
/// (ADR-015).
/// </summary>
public sealed class SkiaFontMetrics : IFontMetrics
{
    /// <inheritdoc/>
    public Size MeasureText(string text, ResolvedStyle style, float maxWidth)
    {
        if (string.IsNullOrEmpty(text))
            return new Size(0f, style.FontSize * style.LineHeight);

        using var font = CreateFont(style);
        float lineHeight = style.FontSize * style.LineHeight;
        float totalWidth = font.MeasureText(text);

        if (maxWidth <= 0f || maxWidth >= float.PositiveInfinity || totalWidth <= maxWidth)
            return new Size(totalWidth, lineHeight);

        // Word-wrap: count how many lines are needed.
        int lines = CountWrappedLines(text, font, maxWidth);
        return new Size(maxWidth, lines * lineHeight);
    }

    /// <inheritdoc/>
    public IReadOnlyList<TextRun> ShapeText(string text, ResolvedStyle style, Rect bounds)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        using var font = CreateFont(style);
        float lineHeight = style.FontSize * style.LineHeight;
        float ascent = -font.Metrics.Ascent; // SkiaSharp ascent is negative above baseline

        float contentX = bounds.X + style.Padding.Left;
        float contentY = bounds.Y + style.Padding.Top;
        float contentWidth = bounds.Width - style.Padding.Horizontal;

        if (contentWidth <= 0f)
            contentWidth = bounds.Width;

        float totalWidth = font.MeasureText(text);

        if (totalWidth <= contentWidth)
        {
            return [new TextRun
            {
                Text = text,
                BaselineOrigin = new Point(contentX, contentY + ascent),
                Bounds = new Rect(contentX, contentY, totalWidth, lineHeight),
                Style = style
            }];
        }

        // Word-wrap
        return WrapText(text, style, font, contentX, contentY, contentWidth, lineHeight, ascent);
    }

    /// <inheritdoc/>
    public float GetAscent(FontDescriptor descriptor)
    {
        using var font = CreateFontFromDescriptor(descriptor);
        return -font.Metrics.Ascent;
    }

    /// <inheritdoc/>
    public float GetDescent(FontDescriptor descriptor)
    {
        using var font = CreateFontFromDescriptor(descriptor);
        return font.Metrics.Descent;
    }

    /// <inheritdoc/>
    public float GetLineGap(FontDescriptor descriptor)
    {
        using var font = CreateFontFromDescriptor(descriptor);
        return font.Metrics.Leading;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static IReadOnlyList<TextRun> WrapText(
        string text, ResolvedStyle style, SKFont font,
        float x, float y, float maxWidth, float lineHeight, float ascent)
    {
        var runs = new List<TextRun>();
        var words = text.Split(' ');
        var line = new StringBuilder();
        float currentY = y;

        void FlushLine()
        {
            if (line.Length == 0) return;
            string lineText = line.ToString();
            float lineWidth = font.MeasureText(lineText);
            runs.Add(new TextRun
            {
                Text = lineText,
                BaselineOrigin = new Point(x, currentY + ascent),
                Bounds = new Rect(x, currentY, lineWidth, lineHeight),
                Style = style
            });
            line.Clear();
            currentY += lineHeight;
        }

        foreach (var word in words)
        {
            string candidate = line.Length == 0 ? word : line + " " + word;
            if (font.MeasureText(candidate) > maxWidth && line.Length > 0)
            {
                FlushLine();
                line.Append(word);
            }
            else
            {
                if (line.Length > 0) line.Append(' ');
                line.Append(word);
            }
        }

        FlushLine();
        return runs;
    }

    private static int CountWrappedLines(string text, SKFont font, float maxWidth)
    {
        var words = text.Split(' ');
        var line = new StringBuilder();
        int lines = 0;

        foreach (var word in words)
        {
            string candidate = line.Length == 0 ? word : line + " " + word;
            if (font.MeasureText(candidate) > maxWidth && line.Length > 0)
            {
                lines++;
                line.Clear();
                line.Append(word);
            }
            else
            {
                if (line.Length > 0) line.Append(' ');
                line.Append(word);
            }
        }

        if (line.Length > 0) lines++;
        return Math.Max(lines, 1);
    }

    internal static SKFont CreateFont(ResolvedStyle style)
    {
        var skStyle = new SKFontStyle(
            (SKFontStyleWeight)style.FontWeight,
            SKFontStyleWidth.Normal,
            style.FontStyle == FontStyleValue.Italic
                ? SKFontStyleSlant.Italic
                : SKFontStyleSlant.Upright);

        var typeface = SKTypeface.FromFamilyName(style.FontFamily, skStyle)
                       ?? SKTypeface.Default;

        return new SKFont(typeface, style.FontSize);
    }

    private static SKFont CreateFontFromDescriptor(FontDescriptor descriptor)
    {
        var skStyle = new SKFontStyle(
            (SKFontStyleWeight)descriptor.Weight,
            SKFontStyleWidth.Normal,
            descriptor.Style == FontStyleValue.Italic
                ? SKFontStyleSlant.Italic
                : SKFontStyleSlant.Upright);

        var typeface = SKTypeface.FromFamilyName(descriptor.Family, skStyle)
                       ?? SKTypeface.Default;

        return new SKFont(typeface, descriptor.Size);
    }
}
