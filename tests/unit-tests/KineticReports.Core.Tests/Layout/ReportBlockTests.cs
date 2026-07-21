namespace KineticReports.Core.Tests.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;

public class ReportBlockTests
{
    [Fact]
    public void LayoutSize_WithPadding_IncludesInsetsInDesiredHeight()
    {
        var child = new FixedSizeElement
        {
            Id = "child-1",
            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f },
            FixedDesiredSize = new Size(50f, 10f)
        };

        var block = new DetailBlock
        {
            Id = "band-1",
            Style = new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Thickness(1f, 2f, 3f, 4f)
            },
            Children = [child]
        };

        block.LayoutSize(new Size(100f, 200f), new TestLayoutSizingContext());

        // Child height (10) + top/bottom padding (2+4)
        block.DesiredSize.Height.ShouldBe(16f);
    }

    [Fact]
    public void Arrange_WithPadding_OffsetsChildrenByInsets()
    {
        var child = new FixedSizeElement
        {
            Id = "child-2",
            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 12f },
            FixedDesiredSize = new Size(40f, 8f)
        };

        var block = new DetailBlock
        {
            Id = "band-2",
            Style = new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Thickness(5f, 6f, 7f, 8f)
            },
            Children = [child]
        };

        block.LayoutSize(new Size(100f, 200f), new TestLayoutSizingContext());
        block.Arrange(new Rect(10f, 20f, 100f, block.DesiredSize.Height));

        child.Bounds.X.ShouldBe(15f);
        child.Bounds.Y.ShouldBe(26f);
        child.Bounds.Width.ShouldBe(88f);
    }

    private sealed class FixedSizeElement : LayoutElement
    {
        public required Size FixedDesiredSize { get; init; }

        public override LayoutElementType ElementType => LayoutElementType.Container;

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
