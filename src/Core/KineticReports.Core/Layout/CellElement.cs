namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a single cell within a <see cref="RowElement"/>.
/// A cell may span multiple columns (<see cref="ColSpan"/>) or rows (<see cref="RowSpan"/>).
/// </summary>
public sealed class CellElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Cell;

    /// <summary>Gets the zero-based column index of this cell.</summary>
    public int ColumnIndex { get; init; }

    /// <summary>
    /// Gets the number of columns this cell spans. Must be at least 1.
    /// </summary>
    public int ColSpan { get; init; } = 1;

    /// <summary>
    /// Gets the number of rows this cell spans. Must be at least 1.
    /// </summary>
    public int RowSpan { get; init; } = 1;

    /// <summary>Gets the child elements inside this cell.</summary>
    public IReadOnlyList<LayoutElement> Children { get; init; } = [];

    /// <inheritdoc/>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        float totalHeight = 0f;

        foreach (var child in Children)
        {
            child.LayoutSize(availableSize, context);
            totalHeight += child.DesiredSize.Height;
        }

        DesiredSize = new Size(availableSize.Width, totalHeight);
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
