namespace KineticReports.Core.Tests.Geometry;

using Shouldly;
using KineticReports.Core.Geometry;

public class ThicknessTests
{
    [Fact]
    public void Zero_ReturnsZeroThickness()
    {
        Thickness.Zero.Left.ShouldBe(0f);
        Thickness.Zero.Top.ShouldBe(0f);
        Thickness.Zero.Right.ShouldBe(0f);
        Thickness.Zero.Bottom.ShouldBe(0f);
    }

    [Fact]
    public void UniformConstructor_AppliesValueToAllSides()
    {
        var thickness = new Thickness(10f);
        thickness.Left.ShouldBe(10f);
        thickness.Top.ShouldBe(10f);
        thickness.Right.ShouldBe(10f);
        thickness.Bottom.ShouldBe(10f);
    }

    [Fact]
    public void HorizontalVerticalConstructor_SetsSymmetric()
    {
        var thickness = new Thickness(5f, 10f);
        thickness.Left.ShouldBe(5f);
        thickness.Top.ShouldBe(10f);
        thickness.Right.ShouldBe(5f);
        thickness.Bottom.ShouldBe(10f);
    }

    [Fact]
    public void FourValueConstructor_SetindividualSides()
    {
        var thickness = new Thickness(1f, 2f, 3f, 4f);
        thickness.Left.ShouldBe(1f);
        thickness.Top.ShouldBe(2f);
        thickness.Right.ShouldBe(3f);
        thickness.Bottom.ShouldBe(4f);
    }

    [Fact]
    public void Horizontal_ReturnsSumOfLeftAndRight()
    {
        var thickness = new Thickness(5f, 10f, 7f, 10f);
        thickness.Horizontal.ShouldBe(12f);
    }

    [Fact]
    public void Vertical_ReturnsSumOfTopAndBottom()
    {
        var thickness = new Thickness(5f, 10f, 7f, 15f);
        thickness.Vertical.ShouldBe(25f);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var thickness = new Thickness(1f, 2f, 3f, 4f);
        thickness.ToString().ShouldBe("L=1 T=2 R=3 B=4");
    }
}
