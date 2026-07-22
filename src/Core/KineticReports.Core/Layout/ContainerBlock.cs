namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a generic container that groups child blocks using
/// absolute positioning relative to its own bounds.
/// </summary>
public sealed class ContainerBlock : LayoutBlock
{
    /// <inheritdoc/>
    public override LayoutBlockType LayoutBlockType => LayoutBlockType.Container;

    /// <summary>Gets the child elements within this container.</summary>
    public IReadOnlyList<LayoutBlock> Children { get; init; } = [];

    /// <summary>
    /// Gets explicit child placements relative to this container's top-left origin.
    /// When present, these bounds are used during Arrange instead of stretching each child.
    /// </summary>
    public IReadOnlyDictionary<string, Rect> ChildPlacements { get; init; } =
        new Dictionary<string, Rect>(StringComparer.Ordinal);

    /// <inheritdoc/>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        float maxRight = 0f;
        float maxBottom = 0f;

        foreach (var child in Children)
        {
            if (ChildPlacements.TryGetValue(child.Id, out var placement))
            {
                child.LayoutSize(new Size(placement.Width, placement.Height), context);
                maxRight = Math.Max(maxRight, placement.X + placement.Width);
                maxBottom = Math.Max(maxBottom, placement.Y + placement.Height);
                continue;
            }

            child.LayoutSize(availableSize, context);
            maxRight = Math.Max(maxRight, child.DesiredSize.Width);
            maxBottom = Math.Max(maxBottom, child.DesiredSize.Height);
        }

        var width = float.IsInfinity(availableSize.Width)
            ? maxRight
            : Math.Max(availableSize.Width, maxRight);

        var height = float.IsInfinity(availableSize.Height)
            ? maxBottom
            : Math.Max(availableSize.Height, maxBottom);

        DesiredSize = new Size(width, height);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;

        foreach (var child in Children)
        {
            if (ChildPlacements.TryGetValue(child.Id, out var placement))
            {
                child.Arrange(new Rect(
                    finalRect.X + placement.X,
                    finalRect.Y + placement.Y,
                    placement.Width,
                    placement.Height));
                continue;
            }

            child.Arrange(finalRect);
        }
    }
}
