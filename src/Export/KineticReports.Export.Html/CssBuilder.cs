namespace KineticReports.Export.Html;

using System.Text;
using KineticReports.Core.Styling;

/// <summary>
/// Converts <see cref="AppliedStyle"/> to inline CSS string.
/// </summary>
public sealed class CssBuilder
{
    /// <summary>
    /// Generates inline CSS from a resolved style.
    /// </summary>
    public static string BuildStyle(AppliedStyle style)
    {
        var css = new StringBuilder();

        // Typography
        if (!string.IsNullOrEmpty(style.FontFamily))
            css.Append($"font-family: {style.FontFamily};");

        if (style.FontSize > 0)
            css.Append($"font-size: {style.FontSize:F1}px;");

        if (style.FontWeight != FontWeight.Normal)
            css.Append($"font-weight: {FontWeightToCss(style.FontWeight)};");

        if (style.FontStyle != FontStyleValue.Normal)
            css.Append($"font-style: {FontStyleToCss(style.FontStyle)};");

        if (style.LineHeight > 0)
            css.Append($"line-height: {style.LineHeight:F1};");

        // Colors
        css.Append($"color: {ColorToCss(style.TextColor)};");

        if (style.Background != Color.Transparent)
            css.Append($"background-color: {ColorToCss(style.Background)};");

        // Spacing
        css.Append($"padding: {style.Padding.Top:F1}px {style.Padding.Right:F1}px {style.Padding.Bottom:F1}px {style.Padding.Left:F1}px;");
        css.Append($"margin: {style.Margin.Top:F1}px {style.Margin.Right:F1}px {style.Margin.Bottom:F1}px {style.Margin.Left:F1}px;");

        // Border
        if (style.Border != null)
            css.Append(BuildBorderStyle(style.Border));

        // Opacity
        if (style.Opacity < 1f)
            css.Append($"opacity: {style.Opacity:F2};");

        // Text alignment
        if (style.TextAlignment != TextAlignment.Left)
            css.Append($"text-align: {TextAlignmentToCss(style.TextAlignment)};");

        return css.ToString();
    }

    private static string FontWeightToCss(FontWeight weight) => weight switch
    {
        FontWeight.Thin => "100",
        FontWeight.ExtraLight => "200",
        FontWeight.Light => "300",
        FontWeight.Normal => "400",
        FontWeight.Medium => "500",
        FontWeight.SemiBold => "600",
        FontWeight.Bold => "700",
        FontWeight.ExtraBold => "800",
        FontWeight.Black => "900",
        _ => "400"
    };

    private static string FontStyleToCss(FontStyleValue style) => style switch
    {
        FontStyleValue.Normal => "normal",
        FontStyleValue.Italic => "italic",
        FontStyleValue.Oblique => "oblique",
        _ => "normal"
    };

    private static string ColorToCss(Color color) =>
        $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255f:F2})";

    private static string TextAlignmentToCss(TextAlignment align) => align switch
    {
        TextAlignment.Left => "left",
        TextAlignment.Center => "center",
        TextAlignment.Right => "right",
        TextAlignment.Justify => "justify",
        _ => "left"
    };

    private static string BuildBorderStyle(Border border)
    {
        // Use the top border side as representative (all sides may differ)
        if (border.Top is not { } top)
            return string.Empty;

        var styleStr = top.Style switch
        {
            BorderLineStyle.Solid => "solid",
            BorderLineStyle.Dashed => "dashed",
            BorderLineStyle.Dotted => "dotted",
            _ => "solid"
        };

        var w = top.Width > 0 ? top.Width : 1f;
        var c = ColorToCss(top.Color);

        return $"border: {w:F1}px {styleStr} {c};";
    }
}
