namespace KineticReports.Core.Tests.Layout;

using Shouldly;
using KineticReports.Core.Layout;

public class LayoutBlockTypeTests
{
    [Fact]
    public void AllElementTypes_AreDefinedInEnum()
    {
        LayoutBlockType.Page.ShouldBe(LayoutBlockType.Page);
        LayoutBlockType.Section.ShouldBe(LayoutBlockType.Section);
        LayoutBlockType.Band.ShouldBe(LayoutBlockType.Band);
        LayoutBlockType.Container.ShouldBe(LayoutBlockType.Container);
        LayoutBlockType.Text.ShouldBe(LayoutBlockType.Text);
        LayoutBlockType.Table.ShouldBe(LayoutBlockType.Table);
        LayoutBlockType.Row.ShouldBe(LayoutBlockType.Row);
        LayoutBlockType.Cell.ShouldBe(LayoutBlockType.Cell);
        LayoutBlockType.Image.ShouldBe(LayoutBlockType.Image);
        LayoutBlockType.Shape.ShouldBe(LayoutBlockType.Shape);
        LayoutBlockType.Chart.ShouldBe(LayoutBlockType.Chart);
        LayoutBlockType.Barcode.ShouldBe(LayoutBlockType.Barcode);
    }

    [Fact]
    public void ElementTypeCount_Is12()
    {
        var values = System.Enum.GetValues<LayoutBlockType>();
        values.Length.ShouldBe(12);
    }
}
