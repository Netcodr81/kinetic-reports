using System.Text.Json;
using KineticReports.Core.Authoring.Serialization;

namespace KineticReports.Core.Tests.Definition;

public class SystemTextJsonReportDesignerDocumentSerializerTests
{
    private readonly SystemTextJsonReportDesignerDocumentSerializer _sut = new();

    [Fact]
    public void Deserialize_WithDuplicateRootKey_ThrowsJsonException()
    {
        var json = """
        {
          "schemaVersion": "1.0",
          "id": "designer-1",
          "id": "designer-2",
          "name": "Designer Report"
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
          "id": "designer-1",
          "name": "Designer Report",
          "components": [
            {
              "id": "comp-1",
              "type": "Text",
              "properties": {
                "text": "One",
                "text": "Two"
              }
            }
          ]
        }
        """;

        var error = Should.Throw<JsonException>(() => _sut.Deserialize(json));
        error.Message.ShouldContain("Duplicate JSON key 'text'");
        error.Message.ShouldContain("$.components[0].properties");
    }

    [Fact]
    public void Deserialize_WithValidUniqueKeys_ReturnsDesignerDocument()
    {
        var json = """
        {
          "schemaVersion": "1.0",
          "id": "designer-1",
          "name": "Designer Report",
          "components": []
        }
        """;

        var document = _sut.Deserialize(json);

        document.ShouldNotBeNull();
        document.Id.ShouldBe("designer-1");
        document.Components.ShouldBeEmpty();
    }
}
