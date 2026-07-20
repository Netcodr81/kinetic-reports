namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a generic container that groups child elements using
/// absolute positioning relative to its own bounds.
/// </summary>
public sealed class ContainerElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Container;

    /// <summary>Gets the child elements within this container.</summary>
    public IReadOnlyList<LayoutElement> Children { get; init; } = [];

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        foreach (var child in Children)
            child.Measure(availableSize, context);

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
