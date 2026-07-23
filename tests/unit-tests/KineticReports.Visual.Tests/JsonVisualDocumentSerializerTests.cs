namespace KineticReports.Visual.Tests;

using System.Text;
using KineticReports.Core.Geometry;
using KineticReports.Core.Visual;
using Shouldly;
using Xunit;

public class JsonVisualDocumentSerializerTests
{
    [Fact]
    public void Serialize_WithValidDocument_IncludesSchemaVersion()
    {
        var document = CreateVisualDocument();
        IVisualDocumentSerializer serializer = new JsonVisualDocumentSerializer();

        var json = serializer.Serialize(document);

        json.ShouldContain("\"schemaVersion\":\"1.0\"");
    }

    [Fact]
    public void RoundTrip_WithBaselineElements_PreservesElementTypesAndCoreValues()
    {
        var document = CreateVisualDocument();
        IVisualDocumentSerializer serializer = new JsonVisualDocumentSerializer();

        var json = serializer.Serialize(document);
        var deserialized = serializer.Deserialize(json);

        deserialized.SchemaVersion.ShouldBe("1.0");
        deserialized.Pages.Count.ShouldBe(1);

        var page = deserialized.Pages[0];
        page.PageNumber.ShouldBe(1);
        page.Layers.Count.ShouldBe(1);

        var elements = page.Layers[0].Elements;
        elements.Count.ShouldBe(6);
        elements[0].ShouldBeOfType<VisualText>();
        elements[1].ShouldBeOfType<VisualImage>();
        elements[2].ShouldBeOfType<VisualShape>();
        elements[3].ShouldBeOfType<VisualTablePlaceholder>();
        elements[4].ShouldBeOfType<VisualUnsupportedElement>();
        elements[5].ShouldBeOfType<VisualContainer>();

        var text = (VisualText)elements[0];
        text.Text.ShouldBe("Hello Visual");
        text.FontFamily.ShouldBe("Segoe UI");
        text.Clips.Count.ShouldBe(1);
        text.Transforms.Count.ShouldBe(1);

        var container = (VisualContainer)elements[5];
        container.Children.Count.ShouldBe(1);
        container.Children[0].ShouldBeOfType<VisualText>();
    }

    [Fact]
    public void Deserialize_WithMissingOptionalFields_UsesDefaults()
    {
        const string json = """
            {
              "schemaVersion": "1.0",
              "pages": [
                {
                  "pageNumber": 1,
                  "width": 200,
                  "height": 120,
                  "layers": [
                    {
                      "name": "body",
                      "elements": [
                        {
                          "$type": "text",
                          "id": "text-1",
                          "bounds": { "x": 10, "y": 12, "width": 60, "height": 20 },
                          "text": "Minimal"
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """;

        IVisualDocumentSerializer serializer = new JsonVisualDocumentSerializer();

        var deserialized = serializer.Deserialize(json);

        deserialized.Metadata.Count.ShouldBe(0);
        var element = deserialized.Pages[0].Layers[0].Elements[0].ShouldBeOfType<VisualText>();
        element.FontFamily.ShouldBe("Arial");
        element.FontSize.ShouldBe(12f);
        element.Clips.Count.ShouldBe(0);
        element.Transforms.Count.ShouldBe(0);
        element.Metadata.Count.ShouldBe(0);
    }

    [Fact]
    public async Task SerializeAsyncAndDeserializeAsync_WithValidPayload_RoundTrips()
    {
        var document = CreateVisualDocument();
        IVisualDocumentSerializer serializer = new JsonVisualDocumentSerializer();

        await using var stream = new MemoryStream();
        await serializer.SerializeAsync(document, stream);
        stream.Position = 0;

        var deserialized = await serializer.DeserializeAsync(stream);

        deserialized.Pages.Count.ShouldBe(1);
        deserialized.Pages[0].Layers[0].Elements.Count.ShouldBe(6);
    }

    private static VisualDocument CreateVisualDocument()
    {
        return new VisualDocument
        {
            SchemaVersion = "1.0",
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source"] = "unit-test"
            },
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 612f,
                    Height = 792f,
                    Layers =
                    [
                        new VisualLayer
                        {
                            Name = "body",
                            Elements =
                            [
                                new VisualText
                                {
                                    Id = "text-1",
                                    Bounds = new Rect(20f, 24f, 140f, 28f),
                                    Text = "Hello Visual",
                                    FontFamily = "Segoe UI",
                                    FontSize = 14f,
                                    FontWeight = "Bold",
                                    TextColorHex = "#FF112233",
                                    Clips =
                                    [
                                        new VisualClip
                                        {
                                            Bounds = new Rect(20f, 24f, 120f, 24f)
                                        }
                                    ],
                                    Transforms =
                                    [
                                        new VisualTransform
                                        {
                                            OffsetX = 2f,
                                            OffsetY = 3f
                                        }
                                    ]
                                },
                                new VisualImage
                                {
                                    Id = "image-1",
                                    Bounds = new Rect(30f, 70f, 100f, 70f),
                                    SourceKey = "asset://logo",
                                    Stretch = VisualImageStretch.Fill
                                },
                                new VisualShape
                                {
                                    Id = "shape-1",
                                    Bounds = new Rect(150f, 70f, 80f, 80f),
                                    Kind = VisualShapeKind.Ellipse,
                                    FillColorHex = "#FF336699",
                                    StrokeColorHex = "#FF000000",
                                    StrokeWidth = 2f
                                },
                                new VisualTablePlaceholder
                                {
                                    Id = "table-1",
                                    Bounds = new Rect(20f, 170f, 220f, 120f),
                                    RowCount = 4,
                                    ColumnCount = 3
                                },
                                new VisualUnsupportedElement
                                {
                                    Id = "unsupported-1",
                                    Bounds = new Rect(260f, 170f, 160f, 60f),
                                    SourceType = "ChartBlock",
                                    Reason = "Chart not mapped"
                                },
                                new VisualContainer
                                {
                                    Id = "container-1",
                                    Bounds = new Rect(20f, 320f, 220f, 80f),
                                    Children =
                                    [
                                        new VisualText
                                        {
                                            Id = "child-text-1",
                                            Bounds = new Rect(32f, 340f, 100f, 20f),
                                            Text = "Inside"
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}
