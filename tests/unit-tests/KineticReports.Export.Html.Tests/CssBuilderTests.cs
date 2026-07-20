namespace KineticReports.Export.Html.Tests;

using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;

public class CssBuilderTests
{
    [Fact]
    public void BuildStyle_WithDefaultStyle_IncludesBasicCss()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f };
        var css = BuildStyle(style);

        css.ShouldContain("padding:");
        css.ShouldContain("margin:");
    }

    [Fact]
    public void BuildStyle_WithFontFamily_IncludesFontFamily()
    {
        var style = new ResolvedStyle { FontFamily = "Times New Roman", FontSize = 12f };
        var css = BuildStyle(style);

        css.ShouldContain("font-family: Times New Roman;");
    }

    [Fact]
    public void BuildStyle_WithFontSize_IncludesFontSize()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 14f };
        var css = BuildStyle(style);

        css.ShouldContain("font-size: 14.0pt;");
    }

    [Fact]
    public void BuildStyle_WithBoldWeight_IncludesWeightAttribute()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f, FontWeight = FontWeight.Bold };
        var css = BuildStyle(style);

        css.ShouldContain("font-weight: 700;");
    }

    [Fact]
    public void BuildStyle_WithItalicStyle_IncludesFontStyle()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f, FontStyle = FontStyleValue.Italic };
        var css = BuildStyle(style);

        css.ShouldContain("font-style: italic;");
    }

    [Fact]
    public void BuildStyle_WithCustomTextColor_IncludesColorCss()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f, TextColor = Color.FromRgb(255, 0, 0) };
        var css = BuildStyle(style);

        css.ShouldContain("color: rgba(255, 0, 0,");
    }

    [Fact]
    public void BuildStyle_WithBackgroundColor_IncludesBackgroundCss()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f, Background = Color.FromRgb(0, 255, 0) };
        var css = BuildStyle(style);

        css.ShouldContain("background-color: rgba(0, 255, 0,");
    }

    [Fact]
    public void BuildStyle_WithOpacity_IncludesOpacityCss()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f, Opacity = 0.5f };
        var css = BuildStyle(style);

        css.ShouldContain("opacity: 0.50;");
    }

    [Fact]
    public void BuildStyle_WithPadding_IncludesPaddingCss()
    {
        var style = new ResolvedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f,
            Padding = new Thickness(10, 20, 30, 40)
        };
        var css = BuildStyle(style);

        css.ShouldContain("padding: 20.0px 30.0px 40.0px 10.0px;");
    }

    [Fact]
    public void BuildStyle_WithBorder_IncludesBorderCss()
    {
        var borderSide = new BorderSide(2f, Color.FromRgb(0, 0, 0), BorderLineStyle.Solid);
        var border = new Border { Top = borderSide };
        var style = new ResolvedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f,
            Border = border
        };
        var css = BuildStyle(style);

        css.ShouldContain("border: 2.0px solid");
    }

    [Fact]
    public void BuildStyle_WithCenterAlignment_IncludesTextAlignCss()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f, TextAlignment = TextAlignment.Center };
        var css = BuildStyle(style);

        css.ShouldContain("text-align: center;");
    }

    // -----

    private static string BuildStyle(ResolvedStyle style) => CssBuilder.BuildStyle(style);
}
