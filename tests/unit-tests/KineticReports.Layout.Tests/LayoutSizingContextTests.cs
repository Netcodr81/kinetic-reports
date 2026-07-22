namespace KineticReports.Layout.Tests;

using KineticReports.Core.Geometry;
using KineticReports.Layout.Tests.Fakes;

public class MeasureContextTests
{
    [Fact]
    public void Constructor_SetsTextLayout()
    {
        var metrics = new FakeTextLayout();
        var sut = new LayoutSizingContext(metrics);
        sut.TextLayout.ShouldBeSameAs(metrics);
    }

    [Fact]
    public void ResolveImageSize_WithNoResolver_ReturnsNull()
    {
        var sut = new LayoutSizingContext(new FakeTextLayout());
        var result = sut.ResolveImageSize("logo.png");
        result.ShouldBeNull();
    }

    [Fact]
    public void ResolveImageSize_WithResolver_InvokesDelegate()
    {
        var expectedSize = new Size(200f, 100f);
        var sut = new LayoutSizingContext(new FakeTextLayout(), key => expectedSize);
        var result = sut.ResolveImageSize("logo.png");
        result.ShouldBe(expectedSize);
    }

    [Fact]
    public void ResolveImageSize_WithResolver_PassesKeyToDelegate()
    {
        string? capturedKey = null;
        var sut = new LayoutSizingContext(new FakeTextLayout(), key =>
        {
            capturedKey = key;
            return null;
        });
        sut.ResolveImageSize("banner.jpg");
        capturedKey.ShouldBe("banner.jpg");
    }
}
