namespace KineticReports.Core.Tests.Layout;

using Shouldly;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

public class ReportLayoutTests
{
    [Fact]
    public void LayoutTree_WithPages_CountsCorrectly()
    {
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };
        var pages = new[]
        {
            new PageBlock { Id = "Page1", Style = style, PageNumber = 1, PageWidth = 612f, PageHeight = 792f },
            new PageBlock { Id = "Page2", Style = style, PageNumber = 2, PageWidth = 612f, PageHeight = 792f }
        };
        var tree = new ReportLayout { Pages = pages };
        tree.PageCount.ShouldBe(2);
    }

    [Fact]
    public void LayoutTree_WithNoPages_HasZeroCount()
    {
        var tree = new ReportLayout { Pages = new PageBlock[0] };
        tree.PageCount.ShouldBe(0);
    }

    [Fact]
    public void LayoutTree_PagesAreReadOnly()
    {
        var style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };
        var page = new PageBlock { Id = "Page1", Style = style, PageWidth = 612f, PageHeight = 792f };
        var tree = new ReportLayout { Pages = new[] { page } };
        tree.Pages.ShouldBeAssignableTo<IReadOnlyList<PageBlock>>();
    }
}
