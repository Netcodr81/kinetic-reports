namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a generic container that groups child blocks using
/// absolute positioning relative to its own bounds.
/// </summary>
public sealed class ContainerBlock : LayoutBlock
{
    /// <inheritdoc/>
    public override LayoutBlockType ElementType => LayoutBlockType.Container;

    /// <summary>Gets the child elements within this container.</summary>
    public IReadOnlyList<LayoutBlock> Children { get; init; } = [];

    /// <inheritdoc/>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        foreach (var child in Children)
            child.LayoutSize(availableSize, context);

        DesiredSize = availableSize;
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;

        foreach (var child in Children)
            child.Arrange(finalRect);
    }
}
