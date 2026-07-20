namespace KineticReports.Core.Tests.Geometry;

using Shouldly;
using KineticReports.Core.Geometry;

public class SizeTests
{
    [Fact]
    public void Zero_ReturnsZeroSize()
    {
        Size.Zero.Width.ShouldBe(0f);
        Size.Zero.Height.ShouldBe(0f);
        Size.Zero.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Infinity_ReturnsInfinitySize()
    {
        Size.Infinity.Width.ShouldBe(float.PositiveInfinity);
        Size.Infinity.Height.ShouldBe(float.PositiveInfinity);
    }

    [Fact]
    public void Constructor_SetesDimensions()
    {
        var size = new Size(100f, 50f);
        size.Width.ShouldBe(100f);
        size.Height.ShouldBe(50f);
    }

    [Fact]
    public void IsEmpty_WithZeroDimensions_ReturnsTrue()
    {
        new Size(0f, 0f).IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void IsEmpty_WithNonZeroDimensions_ReturnsFalse()
    {
        new Size(0f, 10f).IsEmpty.ShouldBeFalse();
        new Size(10f, 0f).IsEmpty.ShouldBeFalse();
        new Size(10f, 20f).IsEmpty.ShouldBeFalse();
        new Size(0.1f, 0.1f).IsEmpty.ShouldBeFalse();
    }

    [Theory]
    [InlineData(100f, 50f, 10f, 5f, 110f, 55f)]
    [InlineData(0f, 0f, 10f, 10f, 10f, 10f)]
    public void Inflate_WithPositiveDelta_InflatesSize(
        float width, float height, float dw, float dh, float expectedW, float expectedH)
    {
        var size = new Size(width, height);
        var result = size.Inflate(dw, dh);
        result.Width.ShouldBe(expectedW);
        result.Height.ShouldBe(expectedH);
    }

    [Fact]
    public void Inflate_WithNegativeDelta_DeflatesSize()
    {
        var size = new Size(100f, 50f);
        var result = size.Inflate(-10f, -5f);
        result.Width.ShouldBe(90f);
        result.Height.ShouldBe(45f);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var size = new Size(100f, 50f);
        size.ToString().ShouldBe("100 × 50");
    }
}
