namespace KineticReports.Core.Tests.Definition;

using Shouldly;
using KineticReports.Core.Definition;
using KineticReports.Core.Styling;

public class ReportDefinitionTests
{
    [Fact]
    public void ReportDefinition_WithRequiredFields_IsCreated()
    {
        var report = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "Invoice",
            Name = "Invoice Report"
        };
        report.SchemaVersion.ShouldBe("1.0");
        report.Id.ShouldBe("Invoice");
        report.Name.ShouldBe("Invoice Report");
    }

    [Fact]
    public void ReportDefinition_DefaultValues_AreSet()
    {
        var report = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "Test",
            Name = "Test Report"
        };
        report.Author.ShouldBeNull();
        report.Description.ShouldBeNull();
        report.Metadata.Count.ShouldBe(0);
        report.Parameters.Count.ShouldBe(0);
        report.DataSources.Count.ShouldBe(0);
        report.Styles.Count.ShouldBe(0);
    }

    [Fact]
    public void ReportDefinition_WithOptionalFields()
    {
        var report = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "Invoice",
            Name = "Invoice Report",
            Author = "John Doe",
            Description = "Standard invoice template",
            Metadata = new Dictionary<string, object> { { "Version", 1 } },
            Parameters = new[] {
                new ParameterDefinition { Id = "Date", Name = "Date", Type = ParameterType.DateTime }
            },
            DataSources = new[] {
                new DataSourceDefinition { Id = "Db", Name = "Main", ProviderType = "SqlServer" }
            },
            Styles = new[] {
                new StyleDefinition { Id = "Heading", Name = "Heading Style" }
            }
        };
        report.Author.ShouldBe("John Doe");
        report.Description.ShouldBe("Standard invoice template");
        report.Metadata["Version"].ShouldBe(1);
        report.Parameters.Count.ShouldBe(1);
        report.DataSources.Count.ShouldBe(1);
        report.Styles.Count.ShouldBe(1);
    }

    [Fact]
    public void ReportDefinition_IsImmutable()
    {
        var original = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "Test",
            Name = "Test"
        };
        var modified = original with { Name = "Modified" };
        original.Name.ShouldBe("Test");
        modified.Name.ShouldBe("Modified");
    }
}
