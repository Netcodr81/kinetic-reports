namespace KineticReports.Visual.Tests;

using KineticReports.Core.Geometry;
using Shouldly;
using Xunit;

public class VisualHitTestIndexBuilderTests
{
    [Fact]
    public void Build_WithNullDocument_ThrowsArgumentNullException()
    {
        var sut = new VisualHitTestIndexBuilder();

        Should.Throw<ArgumentNullException>(() => sut.Build(null!));
    }

    [Fact]
    public void HitTest_WithOverlappingElements_ReturnsTopmostElement()
    {
        var visualDocument = new VisualDocument
        {
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 200f,
                    Height = 200f,
                    Layers =
                    [
                        new VisualLayer
                        {
                            Name = "body",
                            Elements =
                            [
                                new VisualShape
                                {
                                    Id = "bottom",
                                    Bounds = new Rect(10f, 10f, 100f, 100f),
                                    Kind = VisualShapeKind.Rectangle
                                },
                                new VisualShape
                                {
                                    Id = "top",
                                    Bounds = new Rect(20f, 20f, 100f, 100f),
                                    Kind = VisualShapeKind.Rectangle
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var sut = new VisualHitTestIndexBuilder();

        var index = sut.Build(visualDocument);
        var hit = index.HitTest(1, new Point(30f, 30f));

        hit.ShouldNotBeNull();
        hit.Element.Id.ShouldBe("top");
        hit.LayerName.ShouldBe("body");
    }

    [Fact]
    public void HitTest_WithNestedContainer_ReturnsTopmostChild()
    {
        var visualDocument = new VisualDocument
        {
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 400f,
                    Height = 300f,
                    Layers =
                    [
                        new VisualLayer
                        {
                            Name = "body",
                            Elements =
                            [
                                new VisualContainer
                                {
                                    Id = "container-1",
                                    Bounds = new Rect(40f, 40f, 180f, 120f),
                                    Children =
                                    [
                                        new VisualText
                                        {
                                            Id = "child-text-1",
                                            Bounds = new Rect(60f, 60f, 80f, 20f),
                                            Text = "inside"
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var sut = new VisualHitTestIndexBuilder();

        var index = sut.Build(visualDocument);
        var hit = index.HitTest(1, new Point(65f, 65f));

        hit.ShouldNotBeNull();
        hit.Element.Id.ShouldBe("child-text-1");
    }

    [Fact]
    public void HitTest_WithNoElementAtPoint_ReturnsNull()
    {
        var visualDocument = new VisualDocument
        {
            Pages =
            [
                new VisualPage
                {
                    PageNumber = 1,
                    Width = 200f,
                    Height = 200f,
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
                                    Bounds = new Rect(10f, 10f, 50f, 20f),
                                    Text = "sample"
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var sut = new VisualHitTestIndexBuilder();

        var index = sut.Build(visualDocument);
        var hit = index.HitTest(1, new Point(180f, 180f));

        hit.ShouldBeNull();
    }
}
