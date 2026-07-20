namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>Specifies the functional role of a <see cref="RowElement"/>.</summary>
public enum RowKind
{
    /// <summary>A header row; repeated at the top of each page when the table paginates.</summary>
    Header,

    /// <summary>A data row in the body of the table.</summary>
    Data,

    /// <summary>A summary or aggregate footer row at the bottom of the table.</summary>
    Footer,
}

/// <summary>
/// Represents a single row within a <see cref="TableElement"/>.
/// </summary>
public sealed class RowElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Row;

    /// <summary>Gets the functional role of this row.</summary>
    public RowKind Kind { get; init; } = RowKind.Data;

    /// <summary>
    /// Gets a value indicating whether this row must not be split across pages.
    /// </summary>
    public bool KeepTogether { get; init; }

    /// <summary>Gets the cells in this row, in column order.</summary>
    public IReadOnlyList<CellElement> Cells { get; init; } = [];

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        float maxHeight = 0f;

        foreach (var cell in Cells)
        {
            cell.Measure(availableSize, context);
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
