namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a single cell within a <see cref="RowBlock"/>.
/// A cell may span multiple columns (<see cref="ColSpan"/>) or rows (<see cref="RowSpan"/>).
/// </summary>
public sealed class CellBlock : LayoutBlock
{
    /// <inheritdoc/>
    public override LayoutBlockType ElementType => LayoutBlockType.Cell;

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
    public IReadOnlyList<LayoutBlock> Children { get; init; } = [];

    /// <inheritdoc/>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        var horizontalInset = Style.Padding.Horizontal + GetBorderLeftWidth() + GetBorderRightWidth();
        var verticalInset = Style.Padding.Vertical + GetBorderTopWidth() + GetBorderBottomWidth();

        var contentWidth = Math.Max(0f, availableSize.Width - horizontalInset);

        float totalHeight = 0f;

        foreach (var child in Children)
        {
            child.LayoutSize(new Size(contentWidth, availableSize.Height), context);
            totalHeight += child.DesiredSize.Height;
        }

        DesiredSize = new Size(availableSize.Width, totalHeight + verticalInset);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;

        var x = finalRect.X + Style.Padding.Left + GetBorderLeftWidth();
        var y = finalRect.Y + Style.Padding.Top + GetBorderTopWidth();
        var contentWidth = Math.Max(0f, finalRect.Width - Style.Padding.Horizontal - GetBorderLeftWidth() - GetBorderRightWidth());

        foreach (var child in Children)
        {
            child.Arrange(new Rect(x, y, contentWidth, child.DesiredSize.Height));
            y += child.DesiredSize.Height;
        }
    }

    private float GetBorderTopWidth() => Style.Border?.Top?.Width ?? 0f;

    private float GetBorderRightWidth() => Style.Border?.Right?.Width ?? 0f;

    private float GetBorderBottomWidth() => Style.Border?.Bottom?.Width ?? 0f;

    private float GetBorderLeftWidth() => Style.Border?.Left?.Width ?? 0f;
}
