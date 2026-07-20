namespace KineticReports.Core.Tests.Layout;

using Shouldly;
using KineticReports.Core.Layout;

public class LayoutElementTypeTests
{
    [Fact]
    public void AllElementTypes_AreDefinedInEnum()
    {
        LayoutElementType.Page.ShouldBe(LayoutElementType.Page);
        LayoutElementType.Section.ShouldBe(LayoutElementType.Section);
        LayoutElementType.Band.ShouldBe(LayoutElementType.Band);
        LayoutElementType.Container.ShouldBe(LayoutElementType.Container);
        LayoutElementType.Text.ShouldBe(LayoutElementType.Text);
        LayoutElementType.Table.ShouldBe(LayoutElementType.Table);
        LayoutElementType.Row.ShouldBe(LayoutElementType.Row);
        LayoutElementType.Cell.ShouldBe(LayoutElementType.Cell);
        LayoutElementType.Image.ShouldBe(LayoutElementType.Image);
        LayoutElementType.Shape.ShouldBe(LayoutElementType.Shape);
        LayoutElementType.Chart.ShouldBe(LayoutElementType.Chart);
        LayoutElementType.Barcode.ShouldBe(LayoutElementType.Barcode);
    }

    [Fact]
    public void ElementTypeCount_Is12()
    {
        var values = System.Enum.GetValues<LayoutElementType>();
        values.Length.ShouldBe(12);
    }
}
