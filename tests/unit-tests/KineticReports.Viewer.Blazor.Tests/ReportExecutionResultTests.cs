namespace KineticReports.Viewer.Blazor.Tests;

public class ReportExecutionResultTests
{
    [Fact]
    public void ReportExecutionResult_WithSuccessful_CreatesValidInstance()
    {
        // Arrange & Act
        var result = new ReportExecutionResult
        {
            Success = true,
            ExecutedAt = DateTime.UtcNow,
            HtmlContent = "<html><body>ok</body></html>",
            Trace = ["[Render] ok"]
        };

        // Assert
        result.Success.ShouldBeTrue();
        result.HtmlContent.ShouldContain("<html>");
        result.Trace.Count.ShouldBe(1);
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
        result.HtmlContent.ShouldBeEmpty();
        result.Trace.ShouldBeEmpty();
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
