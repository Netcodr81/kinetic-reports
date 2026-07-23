namespace KineticReports.Core.Rendering.Skia;

using System.Text;
using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;
using SkiaSharp;

/// <summary>
/// SkiaSharp-backed <see cref="ITextLayout"/> implementation.
/// </summary>
public sealed class SkiaTextLayout : ITextLayout
{
    /// <inheritdoc/>
    public Size MeasureText(string text, AppliedStyle style, float maxWidth)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Size.Zero;
        }

        using var font = CreateFont(style);
        var lineHeight = GetLineHeight(style, font);
        var lines = WrapLines(text, font, maxWidth);

        var width = 0f;
        foreach (var line in lines)
        {
            width = Math.Max(width, font.MeasureText(line));
        }

        return new Size(width, lineHeight * lines.Count);
    }

    /// <inheritdoc/>
    public IReadOnlyList<TextRun> ShapeText(string text, AppliedStyle style, Rect bounds)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        using var font = CreateFont(style);
        var metrics = font.Metrics;
        var ascent = -metrics.Ascent;
        var lineHeight = GetLineHeight(style, font);

        var contentX = bounds.X + style.Padding.Left;
        var contentY = bounds.Y + style.Padding.Top;
        var contentWidth = Math.Max(0f, bounds.Width - style.Padding.Horizontal);

        var lines = WrapLines(text, font, contentWidth);
        var runs = new List<TextRun>(lines.Count);

        var y = contentY;
        foreach (var line in lines)
        {
            var lineWidth = Math.Min(contentWidth, font.MeasureText(line));
            var baselineY = y + ascent;

            runs.Add(new TextRun
            {
                Text = line,
                BaselineOrigin = new Point(contentX, baselineY),
                Bounds = new Rect(contentX, y, lineWidth, lineHeight),
                Style = style,
                IsRightToLeft = false
            });

            y += lineHeight;
        }

        return runs;
    }

    /// <inheritdoc/>
    public float GetAscent(FontDescriptor descriptor)
    {
        using var font = CreateFont(descriptor);
        return -font.Metrics.Ascent;
    }

    /// <inheritdoc/>
    public float GetDescent(FontDescriptor descriptor)
    {
        using var font = CreateFont(descriptor);
        return font.Metrics.Descent;
    }

    /// <inheritdoc/>
    public float GetLineGap(FontDescriptor descriptor)
    {
        using var font = CreateFont(descriptor);
        return font.Metrics.Leading;
    }

    internal static SKFont CreateFont(AppliedStyle style)
    {
        var typeface = SKTypeface.FromFamilyName(
            style.FontFamily,
            new SKFontStyle(ToSkiaWeight(style.FontWeight), SKFontStyleWidth.Normal, ToSkiaSlant(style.FontStyle)));

        return new SKFont(typeface, style.FontSize);
    }

    private static SKFont CreateFont(FontDescriptor descriptor)
    {
        var typeface = SKTypeface.FromFamilyName(
            descriptor.Family,
            new SKFontStyle(ToSkiaWeight(descriptor.Weight), SKFontStyleWidth.Normal, ToSkiaSlant(descriptor.Style)));

        return new SKFont(typeface, descriptor.Size);
    }

    private static float GetLineHeight(AppliedStyle style, SKFont font)
    {
        var metrics = font.Metrics;
        var natural = (-metrics.Ascent) + metrics.Descent + metrics.Leading;
        var requested = style.FontSize * style.LineHeight;
        return Math.Max(natural, requested);
    }

    private static List<string> WrapLines(string text, SKFont font, float maxWidth)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var paragraphs = normalized.Split('\n');
        var lines = new List<string>();

        var isConstrained = !(float.IsInfinity(maxWidth) || maxWidth <= 0f);
        if (!isConstrained)
        {
            foreach (var paragraph in paragraphs)
            {
                lines.Add(paragraph);
            }

            return lines.Count == 0 ? [string.Empty] : lines;
        }

        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                lines.Add(string.Empty);
                continue;
            }

            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var current = new StringBuilder();

            foreach (var word in words)
            {
                var candidate = current.Length == 0 ? word : $"{current} {word}";

                if (font.MeasureText(candidate) <= maxWidth)
                {
                    current.Clear();
                    current.Append(candidate);
                    continue;
                }

                if (current.Length > 0)
                {
                    lines.Add(current.ToString());
                    current.Clear();
                }

                if (font.MeasureText(word) <= maxWidth)
                {
                    current.Append(word);
                    continue;
                }

                foreach (var chunk in BreakWord(word, font, maxWidth))
                {
                    if (font.MeasureText(chunk) <= maxWidth)
                    {
                        lines.Add(chunk);
                    }
                }
            }

            if (current.Length > 0)
            {
                lines.Add(current.ToString());
            }
        }

        return lines.Count == 0 ? [string.Empty] : lines;
    }

    private static IEnumerable<string> BreakWord(string word, SKFont font, float maxWidth)
    {
        var start = 0;

        while (start < word.Length)
        {
            var length = 1;

            while (start + length <= word.Length && font.MeasureText(word.Substring(start, length)) <= maxWidth)
            {
                length++;
            }

            var actualLength = Math.Max(1, length - 1);
            yield return word.Substring(start, actualLength);
            start += actualLength;
        }
    }

    private static SKFontStyleWeight ToSkiaWeight(FontWeight weight) => (SKFontStyleWeight)(int)weight;

    private static SKFontStyleSlant ToSkiaSlant(FontStyleValue style) => style switch
    {
        FontStyleValue.Italic => SKFontStyleSlant.Italic,
        FontStyleValue.Oblique => SKFontStyleSlant.Oblique,
        _ => SKFontStyleSlant.Upright
    };
}