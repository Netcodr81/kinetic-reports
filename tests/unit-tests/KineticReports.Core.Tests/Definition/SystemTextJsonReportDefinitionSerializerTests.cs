using System.Text.Json;
using KineticReports.Core.Authoring.Serialization;

namespace KineticReports.Core.Tests.Definition;

public class SystemTextJsonReportDefinitionSerializerTests
{
    private readonly SystemTextJsonReportDefinitionSerializer _sut = new();

    [Fact]
    public void Deserialize_WithDuplicateRootKey_ThrowsJsonException()
    {
        var json = """
        {
          "schemaVersion": "1.0",
          "id": "report-1",
          "id": "report-2",
          "name": "Sample"
        }
        """;

        var error = Should.Throw<JsonException>(() => _sut.Deserialize(json));
        error.Message.ShouldContain("Duplicate JSON key 'id'");
    }

    [Fact]
    public void Deserialize_WithDuplicateNestedKey_ThrowsJsonException()
    {
        var json = """
        {
          "schemaVersion": "1.0",
          "id": "report-1",
          "name": "Sample",
          "layout": {
            "pageHeader": [],
            "body": [],
            "body": [],
            "pageFooter": []
          }
        }
        """;

        var error = Should.Throw<JsonException>(() => _sut.Deserialize(json));
        error.Message.ShouldContain("Duplicate JSON key 'body'");
        error.Message.ShouldContain("$.layout");
    }

    [Fact]
    public void Deserialize_WithValidUniqueKeys_ReturnsDefinition()
    {
        var json = """
        {
          "schemaVersion": "1.0",
          "id": "report-1",
          "name": "Sample",
          "dataSources": [],
          "parameters": [],
          "styles": []
        }
        """;

        var definition = _sut.Deserialize(json);

        definition.ShouldNotBeNull();
        definition.Id.ShouldBe("report-1");
    }
}
