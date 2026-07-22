namespace KineticReports.Core.Tests.Styling;

using Shouldly;
using KineticReports.Core.Styling;

public class ColorTests
{
    [Fact]
    public void Transparent_ReturnsFullyTransparentColor()
    {
        ((int)Color.Transparent.A).ShouldBe(0);
        ((int)Color.Transparent.R).ShouldBe(0);
        ((int)Color.Transparent.G).ShouldBe(0);
        ((int)Color.Transparent.B).ShouldBe(0);
    }

    [Fact]
    public void Black_ReturnsOpaqueBlack()
    {
        ((int)Color.Black.A).ShouldBe(255);
        ((int)Color.Black.R).ShouldBe(0);
        ((int)Color.Black.G).ShouldBe(0);
        ((int)Color.Black.B).ShouldBe(0);
    }

    [Fact]
    public void White_ReturnsOpaqueWhite()
    {
        ((int)Color.White.A).ShouldBe(255);
        ((int)Color.White.R).ShouldBe(255);
        ((int)Color.White.G).ShouldBe(255);
        ((int)Color.White.B).ShouldBe(255);
    }

    [Fact]
    public void Constructor_SetsARGBChannels()
    {
        var color = new Color(200, 50, 100, 150);
        ((int)color.A).ShouldBe(200);
        ((int)color.R).ShouldBe(50);
        ((int)color.G).ShouldBe(100);
        ((int)color.B).ShouldBe(150);
    }

    [Fact]
    public void FromRgb_CreatesOpaqueColor()
    {
        var color = Color.FromRgb(100, 150, 200);
        ((int)color.A).ShouldBe(255);
        ((int)color.R).ShouldBe(100);
        ((int)color.G).ShouldBe(150);
        ((int)color.B).ShouldBe(200);
    }

    [Fact]
    public void FromArgb_UnpacksPackedValue()
    {
        var packed = 0xFF6432FFU; // A=255, R=100, G=50, B=255
        var color = Color.FromArgb(packed);
        ((int)color.A).ShouldBe(255);
        ((int)color.R).ShouldBe(100);
        ((int)color.G).ShouldBe(50);
        ((int)color.B).ShouldBe(255);
    }

    [Fact]
    public void ToArgb_PacksToUint()
    {
        var color = new Color(255, 100, 50, 200);
        var packed = color.ToArgb();
        packed.ShouldBe(0xFF6432C8U);
    }

    [Fact]
    public void ToString_ReturnsHexFormat()
    {
        var color = new Color(255, 100, 50, 200);
        color.ToString().ShouldBe("#FF6432C8");
    }

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        var color1 = new Color(255, 100, 50, 200);
        var color2 = new Color(255, 100, 50, 200);
        color1.ShouldBe(color2);
    }
}
