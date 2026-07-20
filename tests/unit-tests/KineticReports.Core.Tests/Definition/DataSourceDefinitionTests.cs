namespace KineticReports.Core.Tests.Definition;

using Shouldly;
using KineticReports.Core.Definition;

public class DataSourceDefinitionTests
{
    [Fact]
    public void DataSourceDefinition_WithRequiredFields_IsCreated()
    {
        var ds = new DataSourceDefinition
        {
            Id = "MainDb",
            Name = "Main Database",
            ProviderType = "SqlServer"
        };
        ds.Id.ShouldBe("MainDb");
        ds.Name.ShouldBe("Main Database");
        ds.ProviderType.ShouldBe("SqlServer");
    }

    [Fact]
    public void DataSourceDefinition_PropertiesDefault_ToEmptyDictionary()
    {
        var ds = new DataSourceDefinition
        {
            Id = "TestDs",
            Name = "Test",
            ProviderType = "Rest"
        };
        ds.Properties.Count.ShouldBe(0);
    }

    [Fact]
    public void DataSourceDefinition_WithProperties()
    {
        var props = new Dictionary<string, string>
        {
            { "ConnectionString", "Server=localhost;Database=MyDb;" },
            { "Timeout", "30" }
        };
        var ds = new DataSourceDefinition
        {
            Id = "Db",
            Name = "Database",
            ProviderType = "SqlServer",
            Properties = props
        };
        ds.Properties["ConnectionString"].ShouldBe("Server=localhost;Database=MyDb;");
        ds.Properties["Timeout"].ShouldBe("30");
    }
}
