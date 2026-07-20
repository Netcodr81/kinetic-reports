namespace KineticReports.Layout.Tests;

using KineticReports.Core.Geometry;
using KineticReports.Layout.Tests.Fakes;

public class MeasureContextTests
{
    [Fact]
    public void Constructor_SetsFontMetrics()
    {
        var metrics = new FakeFontMetrics();
        var sut = new MeasureContext(metrics);
        sut.FontMetrics.ShouldBeSameAs(metrics);
    }

    [Fact]
    public void ResolveImageSize_WithNoResolver_ReturnsNull()
    {
        var sut = new MeasureContext(new FakeFontMetrics());
        var result = sut.ResolveImageSize("logo.png");
        result.ShouldBeNull();
    }

    [Fact]
    public void ResolveImageSize_WithResolver_InvokesDelegate()
    {
        var expectedSize = new Size(200f, 100f);
        var sut = new MeasureContext(new FakeFontMetrics(), key => expectedSize);
        var result = sut.ResolveImageSize("logo.png");
        result.ShouldBe(expectedSize);
    }

    [Fact]
    public void ResolveImageSize_WithResolver_PassesKeyToDelegate()
    {
        string? capturedKey = null;
        var sut = new MeasureContext(new FakeFontMetrics(), key =>
        {
            capturedKey = key;
            return null;
        });
        sut.ResolveImageSize("banner.jpg");
        capturedKey.ShouldBe("banner.jpg");
    }
}
