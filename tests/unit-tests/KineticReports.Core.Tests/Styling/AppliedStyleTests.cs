namespace KineticReports.Core.Tests.Styling;

using Shouldly;
using KineticReports.Core.Styling;

public class ResolvedStyleTests
{
    [Fact]
    public void DefaultValues_AreSet()
    {
        var style = new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f
        };
        style.FontWeight.ShouldBe(FontWeight.Normal);
        style.FontStyle.ShouldBe(FontStyleValue.Normal);
        style.LineHeight.ShouldBe(1.2f);
        style.LetterSpacing.ShouldBe(0f);
        style.TextColor.ShouldBe(Color.Black);
        style.TextAlignment.ShouldBe(TextAlignment.Left);
        style.VerticalAlignment.ShouldBe(VerticalAlignment.Top);
        style.TextDecoration.ShouldBe(TextDecoration.None);
        style.Background.ShouldBe(Color.Transparent);
        style.Opacity.ShouldBe(1f);
        style.Overflow.ShouldBe(Overflow.Hidden);
    }

    [Fact]
    public void CustomValues_ArePreserved()
    {
        var style = new AppliedStyle
        {
            FontFamily = "Inter",
            FontSize = 16f,
            FontWeight = FontWeight.Bold,
            FontStyle = FontStyleValue.Italic,
            LineHeight = 1.5f,
            TextColor = Color.White,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Middle,
            Background = Color.Black,
            Opacity = 0.9f
        };
        style.FontFamily.ShouldBe("Inter");
        style.FontSize.ShouldBe(16f);
        style.FontWeight.ShouldBe(FontWeight.Bold);
        style.FontStyle.ShouldBe(FontStyleValue.Italic);
        style.LineHeight.ShouldBe(1.5f);
        style.TextColor.ShouldBe(Color.White);
        style.TextAlignment.ShouldBe(TextAlignment.Center);
        style.VerticalAlignment.ShouldBe(VerticalAlignment.Middle);
        style.Background.ShouldBe(Color.Black);
        style.Opacity.ShouldBe(0.9f);
    }

    [Fact]
    public void IsImmutable_AsRecord()
    {
        var style1 = new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f
        };
        var style2 = style1 with { FontSize = 14f };
        style1.FontSize.ShouldBe(12f);
        style2.FontSize.ShouldBe(14f);
        (style1 == style2).ShouldBeFalse();
    }
}
