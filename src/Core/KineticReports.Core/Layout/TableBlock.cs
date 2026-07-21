namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Defines a column within a <see cref="TableBlock"/>.
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
/// Represents a tabular data block in the report layout.
/// The full table layout algorithm (column widths, row heights, spans, pagination)
/// is implemented in <c>KineticReports.Layout</c>.
/// </summary>
public sealed class TableBlock : LayoutBlock
{
    private float[] _columnWidths = [];
    private float[] _rowHeights = [];

    /// <inheritdoc/>
    public override LayoutBlockType ElementType => LayoutBlockType.Table;

    /// <summary>Gets the column definitions for this table.</summary>
    public IReadOnlyList<TableColumn> Columns { get; init; } = [];

    /// <summary>Gets all rows in this table, in document order.</summary>
    public IReadOnlyList<RowBlock> Rows { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether header rows should repeat at the top of
    /// each page when the table spans multiple pages.
    /// </summary>
    public bool RepeatHeaders { get; init; } = true;

    /// <inheritdoc/>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        var columnCount = DetermineColumnCount();
        if (columnCount == 0)
        {
            _columnWidths = [];
            _rowHeights = [];
            DesiredSize = new Size(0f, 0f);
            return;
        }

        var availableWidth = float.IsInfinity(availableSize.Width)
            ? columnCount * 120f
            : Math.Max(availableSize.Width, columnCount * 40f);

        _columnWidths = ComputeColumnWidths(columnCount, availableWidth);
        _rowHeights = new float[Rows.Count];

        for (var rowIndex = 0; rowIndex < Rows.Count; rowIndex++)
        {
            var row = Rows[rowIndex];
            var maxHeight = 0f;

            foreach (var cell in row.Cells)
            {
                var cellWidth = GetCellWidth(cell, _columnWidths);
                cell.LayoutSize(new Size(cellWidth, availableSize.Height), context);
                maxHeight = Math.Max(maxHeight, cell.DesiredSize.Height);
            }

            _rowHeights[rowIndex] = maxHeight;
        }

        var tableWidth = _columnWidths.Sum();
        var tableHeight = _rowHeights.Sum();
        DesiredSize = new Size(tableWidth, tableHeight);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;

        if (_columnWidths.Length == 0 || Rows.Count == 0)
            return;

        var totalMeasuredWidth = _columnWidths.Sum();
        var widthScale = totalMeasuredWidth > 0f
            ? finalRect.Width / totalMeasuredWidth
            : 1f;

        var scaledWidths = new float[_columnWidths.Length];
        for (var i = 0; i < _columnWidths.Length; i++)
            scaledWidths[i] = _columnWidths[i] * widthScale;

        var rowTop = finalRect.Y;
        for (var rowIndex = 0; rowIndex < Rows.Count; rowIndex++)
        {
            var row = Rows[rowIndex];
            var rowHeight = rowIndex < _rowHeights.Length ? _rowHeights[rowIndex] : 0f;
            row.Arrange(new Rect(finalRect.X, rowTop, finalRect.Width, rowHeight));

            var x = finalRect.X;
            for (var col = 0; col < scaledWidths.Length; col++)
            {
                var cell = row.Cells.FirstOrDefault(c => c.ColumnIndex == col);
                if (cell == null)
                {
                    x += scaledWidths[col];
                    continue;
                }

                var span = Math.Max(1, cell.ColSpan);
                var width = 0f;
                for (var spanCol = col; spanCol < Math.Min(col + span, scaledWidths.Length); spanCol++)
                    width += scaledWidths[spanCol];

                cell.Arrange(new Rect(x, rowTop, width, rowHeight));
                x += width;
                col += span - 1;
            }

            rowTop += rowHeight;
        }
    }

    private int DetermineColumnCount()
    {
        var fromDefinition = Columns.Count;
        var fromCells = Rows
            .SelectMany(r => r.Cells)
            .Select(c => c.ColumnIndex + Math.Max(c.ColSpan, 1))
            .DefaultIfEmpty(0)
            .Max();

        return Math.Max(fromDefinition, fromCells);
    }

    private float[] ComputeColumnWidths(int columnCount, float availableWidth)
    {
        var widths = new float[columnCount];
        var grow = new float[columnCount];
        var fixedWidth = 0f;

        for (var i = 0; i < columnCount; i++)
        {
            var column = i < Columns.Count ? Columns[i] : null;
            var minWidth = column?.MinWidth ?? 40f;

            if (column?.Width is float explicitWidth)
            {
                widths[i] = Math.Max(minWidth, explicitWidth);
                fixedWidth += widths[i];
                continue;
            }

            widths[i] = minWidth;
            fixedWidth += widths[i];
            grow[i] = column?.Grow ?? 1f;
        }

        var remaining = Math.Max(0f, availableWidth - fixedWidth);
        var totalGrow = grow.Sum();
        if (remaining > 0f && totalGrow > 0f)
        {
            for (var i = 0; i < columnCount; i++)
            {
                if (grow[i] <= 0f)
                    continue;

                widths[i] += remaining * (grow[i] / totalGrow);
            }
        }

        return widths;
    }

    private static float GetCellWidth(CellBlock cell, IReadOnlyList<float> columnWidths)
    {
        if (columnWidths.Count == 0)
            return 0f;

        var start = Math.Clamp(cell.ColumnIndex, 0, columnWidths.Count - 1);
        var span = Math.Max(1, cell.ColSpan);

        var width = 0f;
        for (var i = start; i < Math.Min(start + span, columnWidths.Count); i++)
            width += columnWidths[i];

        return width;
    }
}
