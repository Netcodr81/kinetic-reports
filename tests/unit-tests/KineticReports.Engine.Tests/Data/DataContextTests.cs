namespace KineticReports.Engine.Tests.Data;

using KineticReports.Engine.Data;

public class DataContextTests
{
    [Fact]
    public void GetRows_WithUnknownId_ReturnsEmptyList()
    {
        var sut = new DataContext
        {
            DataSources = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>>()
        };
        sut.GetRows("nonexistent").ShouldBeEmpty();
    }

    [Fact]
    public void GetRows_WithKnownId_ReturnsRegisteredRows()
    {
        var rows = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["Id"] = 1 },
            new Dictionary<string, object?> { ["Id"] = 2 }
        };

        var sut = new DataContext
        {
            DataSources = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>>
            {
                ["orders"] = rows
            }
        };

        var result = sut.GetRows("orders");
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void DataSources_IsReadOnly()
    {
        var sut = new DataContext { DataSources = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>>() };
        sut.DataSources.ShouldBeAssignableTo<IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>>>();
    }
}
