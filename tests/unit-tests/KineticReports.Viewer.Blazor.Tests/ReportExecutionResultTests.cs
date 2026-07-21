using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

namespace KineticReports.Viewer.Blazor.Tests;

public class ReportExecutionResultTests
{
    [Fact]
    public void ReportExecutionResult_WithSuccessful_CreatesValidInstance()
    {
        // Arrange & Act
        var page = new PageElement
        {
            Id = "page-1",
            PageWidth = 800,
            PageHeight = 600,
            PageNumber = 1,
            Header = null,
            Footer = null,
            Children = [],
            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f }
        };
        var ReportLayout = new ReportLayout { Pages = [page] };

        var result = new ReportExecutionResult
        {
            Success = true,
            ExecutedAt = DateTime.UtcNow,
            ReportLayout = ReportLayout
        };

        // Assert
        result.Success.ShouldBeTrue();
        result.ReportLayout.ShouldNotBeNull();
        result.ReportLayout.Pages.Count.ShouldBe(1);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void ReportExecutionResult_WithError_CreatesValidInstance()
    {
        // Arrange & Act
        var result = new ReportExecutionResult
        {
            Success = false,
            ExecutedAt = DateTime.UtcNow,
            ErrorMessage = "Report execution failed"
        };

        // Assert
        result.Success.ShouldBeFalse();
        result.ReportLayout.ShouldBeNull();
        result.ErrorMessage.ShouldBe("Report execution failed");
    }

    [Fact]
    public void ReportExecutionResult_RequiresSuccessAndExecutedAt()
    {
        // This test verifies that Success and ExecutedAt are required properties
        // Attempting to create without them would be a compiler error

        var utcNow = DateTime.UtcNow;

        var result = new ReportExecutionResult
        {
            Success = true,
            ExecutedAt = utcNow
        };

        result.Success.ShouldBeTrue();
        result.ExecutedAt.ShouldBe(utcNow);
    }
}
