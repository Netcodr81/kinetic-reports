namespace KineticReports.Layout.Tests;

using KineticReports.Core.Geometry;
using KineticReports.Core.LayoutEngine;

public class LayoutOptionsTests
{
    [Fact]
    public void DefaultPageWidth_Is816()
    {
        var sut = new LayoutOptions();
        sut.PageWidth.ShouldBe(816f);
    }

    [Fact]
    public void DefaultPageHeight_Is1056()
    {
        var sut = new LayoutOptions();
        sut.PageHeight.ShouldBe(1056f);
    }

    [Fact]
    public void DefaultPageMargins_AreOneInchOnAllSides()
    {
        var sut = new LayoutOptions();
        sut.PageMargins.Left.ShouldBe(96f);
        sut.PageMargins.Top.ShouldBe(96f);
        sut.PageMargins.Right.ShouldBe(96f);
        sut.PageMargins.Bottom.ShouldBe(96f);
    }

    [Fact]
    public void PageWidth_CanBeOverridden()
    {
        var sut = new LayoutOptions { PageWidth = 595f };
        sut.PageWidth.ShouldBe(595f);
    }

    [Fact]
    public void PageHeight_CanBeOverridden()
    {
        var sut = new LayoutOptions { PageHeight = 842f };
        sut.PageHeight.ShouldBe(842f);
    }

    [Fact]
    public void PageMargins_CanBeOverridden()
    {
        var margins = new Thickness(48f);
        var sut = new LayoutOptions { PageMargins = margins };
        sut.PageMargins.ShouldBe(margins);
    }
}
