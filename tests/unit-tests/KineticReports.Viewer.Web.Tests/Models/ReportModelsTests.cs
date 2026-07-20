using KineticReports.Viewer.Web.Models;

namespace KineticReports.Viewer.Web.Tests;

public class ReportModelsTests
{
    [Fact]
    public void ExecuteReportRequest_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var request = new ExecuteReportRequest
        {
            Definition = @"{ ""name"": ""test"" }",
            Parameters = new Dictionary<string, object?> { { "param1", "value1" } }
        };

        // Assert
        request.Definition.ShouldNotBeEmpty();
        request.Parameters.Count.ShouldBe(1);
    }

    [Fact]
    public void ExecuteReportRequest_DefaultParameters_IsEmpty()
    {
        // Arrange & Act
        var request = new ExecuteReportRequest
        {
            Definition = @"{ ""name"": ""test"" }"
        };

        // Assert
        request.Parameters.ShouldNotBeNull();
        request.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ExecuteReportResponse_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var response = new ExecuteReportResponse
        {
            OperationId = "op-123",
            Status = "Success",
            ExecutedAt = DateTime.UtcNow
        };

        // Assert
        response.OperationId.ShouldBe("op-123");
        response.Status.ShouldBe("Success");
        response.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void ExecuteReportResponse_WithErrorMessage_IncludesMessage()
    {
        // Arrange & Act
        var response = new ExecuteReportResponse
        {
            OperationId = "op-456",
            Status = "Failed",
            ErrorMessage = "Report execution failed: Invalid definition"
        };

        // Assert
        response.ErrorMessage.ShouldBe("Report execution failed: Invalid definition");
    }
}
