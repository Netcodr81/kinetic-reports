namespace KineticReports.Core.Tests.Rendering;

using Shouldly;
using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;

public class TextRunTests
{
    [Fact]
    public void TextRun_WithRequiredProperties_IsCreated()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f };
        var run = new TextRun
        {
            Text = "Hello",
            BaselineOrigin = new Point(10f, 20f),
            Bounds = new Rect(10f, 8f, 30f, 16f),
            Style = style
        };
        run.Text.ShouldBe("Hello");
        run.BaselineOrigin.X.ShouldBe(10f);
        run.BaselineOrigin.Y.ShouldBe(20f);
        run.Style.FontFamily.ShouldBe("Arial");
    }

    [Fact]
    public void TextRun_IsRightToLeft_DefaultsFalse()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f };
        var run = new TextRun
        {
            Text = "Test",
            BaselineOrigin = Point.Zero,
            Bounds = Rect.Empty,
            Style = style
        };
        run.IsRightToLeft.ShouldBeFalse();
    }

    [Fact]
    public void TextRun_WithRightToLeft_IsSet()
    {
        var style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f };
        var run = new TextRun
        {
            Text = "مرحبا",
            BaselineOrigin = Point.Zero,
            Bounds = new Rect(0, 0, 50, 16),
            Style = style,
            IsRightToLeft = true
        };
        run.IsRightToLeft.ShouldBeTrue();
    }
}
