namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Defines a column within a <see cref="TableElement"/>.
/// </summary>
public sealed record TableColumn
{
    /// <summary>
    /// Gets or inits the explicit width in DIPs.
    /// <see langword="null"/> means the column is sized automatically.
    /// </summary>
    public float? Width { get; init; }

    /// <summary>Gets or inits the minimum column width in DIPs.</summary>
    public float MinWidth { get; init; }

    /// <summary>Gets or inits the maximum column width in DIPs.</summary>
    public float MaxWidth { get; init; } = float.MaxValue;

    /// <summary>
    /// Gets or inits the proportional grow factor used when distributing remaining
    /// table width (similar to CSS <c>flex-grow</c>). A value of 0 means fixed width.
    /// </summary>
    public float Grow { get; init; }
}

/// <summary>
/// Represents a tabular data element in the report layout.
/// The full table layout algorithm (column widths, row heights, spans, pagination)
/// is implemented in <c>KineticReports.Layout</c>.
/// </summary>
public sealed class TableElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Table;

    /// <summary>Gets the column definitions for this table.</summary>
    public IReadOnlyList<TableColumn> Columns { get; init; } = [];

    /// <summary>Gets all rows in this table, in document order.</summary>
    public IReadOnlyList<RowElement> Rows { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether header rows should repeat at the top of
    /// each page when the table spans multiple pages.
    /// </summary>
    public bool RepeatHeaders { get; init; } = true;

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        // Full table layout is performed by KineticReports.Layout.
        DesiredSize = availableSize;
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }
}
