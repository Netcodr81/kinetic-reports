namespace KineticReports.Viewer.Blazor.Tests.Services;

using KineticReports.Samples.Blazor.Reports;

public class SampleReportsExpressionTests
{
    [Fact]
    public void CreateExpressionReport_ReturnsExpectedDefinition()
    {
        // Act
        var report = SampleReports.CreateExpressionReport();

        // Assert
        report.ShouldNotBeNull();
        report.Id.ShouldBe("expression-report-sqlite");
        report.Name.ShouldBe("Expression Evaluation Report (SQLite)");
        report.Description.ShouldNotBeNull();
        report.Description.ShouldContain("{FieldName}");

        report.DataSources.Count.ShouldBe(1);
        report.DataSources[0].Id.ShouldBe("expression-demo");
        report.DataSources[0].ProviderType.ShouldBe("SqlLite");
    }

    [Fact]
    public void GetAllSampleReports_IncludesExpressionReport()
    {
        // Act
        var reports = SampleReports.GetAllSampleReports();

        // Assert
        reports.ShouldContain(r => r.Id == "expression-report-sqlite");
    }
}
