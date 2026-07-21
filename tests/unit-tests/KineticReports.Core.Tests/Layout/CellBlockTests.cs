namespace KineticReports.Core.Tests.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;

public class CellBlockTests
{
    [Fact]
    public void LayoutSize_WithPaddingAndBorder_IncludesInsetsInDesiredHeight()
    {
        var child = new FixedSizeElement
        {
            Id = "child-1",
            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f },
            FixedDesiredSize = new Size(50f, 10f)
        };

        var cell = new CellBlock
        {
            Id = "cell-1",
            Style = new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Thickness(2f, 3f, 4f, 5f),
                Border = Border.Uniform(1f, Color.Black)
            },
            Children = [child]
        };

        cell.LayoutSize(new Size(100f, 200f), new TestLayoutSizingContext());

        // Child height (10) + top/bottom padding (3+5) + top/bottom border (1+1)
        cell.DesiredSize.Height.ShouldBe(20f);
    }

    private sealed class FixedSizeElement : LayoutBlock
    {
        public required Size FixedDesiredSize { get; init; }

        public override LayoutBlockType LayoutBlockType => LayoutBlockType.Container;

        public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
        {
            DesiredSize = FixedDesiredSize;
        }

        public override void Arrange(Rect finalRect)
        {
            Bounds = finalRect;
        }
    }

    private sealed class TestLayoutSizingContext : ILayoutSizingContext
    {
        public ITextLayout TextLayout { get; } = new StubTextLayout();

        public Size? ResolveImageSize(string imageKey) => null;
    }

    private sealed class StubTextLayout : ITextLayout
    {
        public Size MeasureText(string text, AppliedStyle style, float maxWidth) => new(0f, 0f);

        public IReadOnlyList<TextRun> ShapeText(string text, AppliedStyle style, Rect bounds) => [];

        public float GetAscent(FontDescriptor descriptor) => 0f;

        public float GetDescent(FontDescriptor descriptor) => 0f;

        public float GetLineGap(FontDescriptor descriptor) => 0f;
    }
}
