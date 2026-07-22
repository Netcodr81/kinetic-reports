namespace KineticReports.Core.Tests.Geometry;

using Shouldly;
using KineticReports.Core.Geometry;

public class RectTests
{
    [Fact]
    public void Empty_ReturnsZeroRect()
    {
        Rect.Empty.X.ShouldBe(0f);
        Rect.Empty.Y.ShouldBe(0f);
        Rect.Empty.Width.ShouldBe(0f);
        Rect.Empty.Height.ShouldBe(0f);
        Rect.Empty.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_SetsCoordinatesAndDimensions()
    {
        var rect = new Rect(10f, 20f, 100f, 50f);
        rect.X.ShouldBe(10f);
        rect.Y.ShouldBe(20f);
        rect.Width.ShouldBe(100f);
        rect.Height.ShouldBe(50f);
    }

    [Fact]
    public void Right_ReturnsXPlusWidth()
    {
        var rect = new Rect(10f, 20f, 100f, 50f);
        rect.Right.ShouldBe(110f);
    }

    [Fact]
    public void Bottom_ReturnsYPlusHeight()
    {
        var rect = new Rect(10f, 20f, 100f, 50f);
        rect.Bottom.ShouldBe(70f);
    }

    [Fact]
    public void TopLeft_ReturnsTopLeftCornerPoint()
    {
        var rect = new Rect(10f, 20f, 100f, 50f);
        var corner = rect.TopLeft;
        corner.X.ShouldBe(10f);
        corner.Y.ShouldBe(20f);
    }

    [Fact]
    public void Size_ReturnsDimensionsAsSize()
    {
        var rect = new Rect(10f, 20f, 100f, 50f);
        var size = rect.Size;
        size.Width.ShouldBe(100f);
        size.Height.ShouldBe(50f);
    }

    [Fact]
    public void IsEmpty_WithZeroDimensions_ReturnsTrue()
    {
        new Rect(0f, 0f, 0f, 0f).IsEmpty.ShouldBeTrue();
        new Rect(10f, 10f, 0f, 50f).IsEmpty.ShouldBeTrue();
        new Rect(10f, 10f, 100f, 0f).IsEmpty.ShouldBeTrue();
        new Rect(10f, 10f, 100f, -5f).IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void IsEmpty_WithPositiveDimensions_ReturnsFalse()
    {
        new Rect(10f, 20f, 100f, 50f).IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void FromPoints_CreatesRectFromCorners()
    {
        var topLeft = new Point(10f, 20f);
        var bottomRight = new Point(110f, 70f);
        var rect = Rect.FromPoints(topLeft, bottomRight);
        rect.X.ShouldBe(10f);
        rect.Y.ShouldBe(20f);
        rect.Width.ShouldBe(100f);
        rect.Height.ShouldBe(50f);
    }

    [Fact]
    public void Deflate_ReducesSizeAndShrinksInward()
    {
        var rect = new Rect(10f, 20f, 100f, 80f);
        var thickness = new Thickness(5f, 10f, 5f, 10f);
        var result = rect.Deflate(thickness);
        result.X.ShouldBe(15f);
        result.Y.ShouldBe(30f);
        result.Width.ShouldBe(90f);
        result.Height.ShouldBe(60f);
    }

    [Fact]
    public void Inflate_IncreasesSize()
    {
        var rect = new Rect(10f, 20f, 100f, 80f);
        var thickness = new Thickness(5f);
        var result = rect.Inflate(thickness);
        result.X.ShouldBe(5f);
        result.Y.ShouldBe(15f);
        result.Width.ShouldBe(110f);
        result.Height.ShouldBe(90f);
    }

    [Theory]
    [InlineData(50f, 40f, true)]  // Inside
    [InlineData(10f, 20f, true)]  // On top-left corner
    [InlineData(110f, 70f, true)] // On bottom-right corner
    [InlineData(9f, 40f, false)]  // Left of rect
    [InlineData(111f, 40f, false)] // Right of rect
    [InlineData(50f, 19f, false)]  // Above rect
    [InlineData(50f, 101f, false)] // Below rect
    public void Contains_DetectsPointInclusion(float px, float py, bool expected)
    {
        var rect = new Rect(10f, 20f, 100f, 80f);
        var point = new Point(px, py);
        rect.Contains(point).ShouldBe(expected);
    }
}
