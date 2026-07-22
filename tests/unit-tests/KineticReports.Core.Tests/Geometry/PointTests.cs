namespace KineticReports.Core.Tests.Geometry;

using Shouldly;
using KineticReports.Core.Geometry;

public class PointTests
{
    [Fact]
    public void Zero_ReturnsOriginPoint()
    {
        Point.Zero.X.ShouldBe(0f);
        Point.Zero.Y.ShouldBe(0f);
    }

    [Fact]
    public void Constructor_SetsCoordinates()
    {
        var point = new Point(10.5f, 20.3f);
        point.X.ShouldBe(10.5f);
        point.Y.ShouldBe(20.3f);
    }

    [Theory]
    [InlineData(0f, 0f, 5f, 3f, 5f, 3f)]
    [InlineData(10f, 20f, 5f, -3f, 15f, 17f)]
    [InlineData(5f, 5f, -5f, -5f, 0f, 0f)]
    public void Translate_WithDelta_ReturnsTranslatedPoint(
        float startX, float startY, float dx, float dy, float expectedX, float expectedY)
    {
        var point = new Point(startX, startY);
        var result = point.Translate(dx, dy);
        result.X.ShouldBe(expectedX);
        result.Y.ShouldBe(expectedY);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var point = new Point(10f, 20f);
        point.ToString().ShouldBe("(10, 20)");
    }

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        var point1 = new Point(5f, 10f);
        var point2 = new Point(5f, 10f);
        (point1 == point2).ShouldBeTrue();
        point1.ShouldBe(point2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ReturnsFalse()
    {
        var point1 = new Point(5f, 10f);
        var point2 = new Point(5f, 11f);
        (point1 == point2).ShouldBeFalse();
        point1.ShouldNotBe(point2);
    }
}
