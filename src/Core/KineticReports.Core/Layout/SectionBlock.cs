namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a horizontal section within a <see cref="PageBlock"/>
/// (page header, page body, or page footer).
/// Children are stacked vertically in document order.
/// </summary>
public sealed class SectionBlock : LayoutBlock
{
    /// <inheritdoc/>
    public override LayoutBlockType LayoutBlockType => LayoutBlockType.Section;

    /// <summary>Gets the child elements stacked inside this section.</summary>
    public IReadOnlyList<LayoutBlock> Children { get; init; } = [];

    /// <inheritdoc/>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        float maxWidth = 0f;
        float totalHeight = 0f;

        foreach (var child in Children)
        {
            child.LayoutSize(availableSize, context);
            maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
            totalHeight += child.DesiredSize.Height;
        }

        DesiredSize = new Size(maxWidth, totalHeight);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
        float y = finalRect.Y;

        foreach (var child in Children)
        {
            child.Arrange(new Rect(finalRect.X, y, finalRect.Width, child.DesiredSize.Height));
            y += child.DesiredSize.Height;
        }
    }
}
