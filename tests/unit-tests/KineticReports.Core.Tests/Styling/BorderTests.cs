namespace KineticReports.Core.Tests.Styling;

using Shouldly;
using KineticReports.Core.Styling;

public class BorderTests
{
    [Fact]
    public void Uniform_CreatesSymmetricBorder()
    {
        var border = Border.Uniform(2f, Color.Black);
        border.Top?.Width.ShouldBe(2f);
        border.Right?.Width.ShouldBe(2f);
        border.Bottom?.Width.ShouldBe(2f);
        border.Left?.Width.ShouldBe(2f);
        border.Top?.Color.ShouldBe(Color.Black);
        border.Top?.Style.ShouldBe(BorderLineStyle.Solid);
    }

    [Fact]
    public void Uniform_WithCustomStyle_AppliesStyleToAll()
    {
        var border = Border.Uniform(1f, Color.White, BorderLineStyle.Dashed);
        border.Top?.Style.ShouldBe(BorderLineStyle.Dashed);
        border.Right?.Style.ShouldBe(BorderLineStyle.Dashed);
        border.Bottom?.Style.ShouldBe(BorderLineStyle.Dashed);
        border.Left?.Style.ShouldBe(BorderLineStyle.Dashed);
    }

    [Fact]
    public void BorderWithoutCornerRadius_HasZeroCornerRadius()
    {
        var border = Border.Uniform(1f, Color.Black);
        border.CornerRadius.ShouldBe(0f);
    }

    [Fact]
    public void BorderWithAsymmetricSides_AllowsIndividualConfig()
    {
        var border = new Border
        {
            Top = new BorderSide(2f, Color.FromRgb(255, 0, 0)),     // Red
            Right = new BorderSide(1f, Color.FromRgb(0, 0, 255)),   // Blue
            Bottom = new BorderSide(3f, Color.FromRgb(0, 255, 0)),  // Green
            Left = new BorderSide(1f, Color.Black)
        };
        border.Top?.Width.ShouldBe(2f);
        border.Right?.Width.ShouldBe(1f);
        border.Bottom?.Width.ShouldBe(3f);
        border.Left?.Width.ShouldBe(1f);
    }
}
