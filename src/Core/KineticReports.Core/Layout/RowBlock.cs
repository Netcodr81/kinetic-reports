namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>Specifies the functional role of a <see cref="RowBlock"/>.</summary>
public enum RowType
{
    /// <summary>A header row; repeated at the top of each page when the table paginates.</summary>
    Header,

    /// <summary>A data row in the body of the table.</summary>
    Data,

    /// <summary>A summary or aggregate footer row at the bottom of the table.</summary>
    Footer,
}

/// <summary>
/// Represents a single row within a <see cref="TableBlock"/>.
/// </summary>
public sealed class RowBlock : LayoutBlock
{
    /// <inheritdoc/>
    public override LayoutBlockType LayoutBlockType => LayoutBlockType.Row;

    /// <summary>Gets the functional role of this row.</summary>
    public RowType RowType { get; init; } = RowType.Data;

    /// <summary>
    /// Gets a value indicating whether this row must not be split across pages.
    /// </summary>
    public bool KeepTogether { get; init; }

    /// <summary>Gets the cells in this row, in column order.</summary>
    public IReadOnlyList<CellBlock> Cells { get; init; } = [];

    /// <inheritdoc/>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        float maxHeight = 0f;

        foreach (var cell in Cells)
        {
            cell.LayoutSize(availableSize, context);
            maxHeight = Math.Max(maxHeight, cell.DesiredSize.Height);
        }

        DesiredSize = new Size(availableSize.Width, maxHeight);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }
}
