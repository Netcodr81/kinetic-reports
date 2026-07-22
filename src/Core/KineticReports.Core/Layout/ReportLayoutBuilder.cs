namespace KineticReports.Core.Layout;

using KineticReports.Core.Styling;

/// <summary>
/// Fluent builder for creating ordered report blocks in code-first scenarios.
/// </summary>
public sealed class ReportLayoutBuilder
{
    private readonly List<ReportBlock> _blocks = [];

    /// <summary>
    /// Begins building a table-backed report region and returns a fluent table builder.
    /// </summary>
    /// <param name="regionId">Stable containing region id.</param>
    /// <param name="regionStyle">Resolved style for the containing report region.</param>
    /// <param name="tableId">Stable table element id.</param>
    /// <param name="tableStyle">Resolved style for the table element.</param>
    /// <param name="columns">Table columns.</param>
    /// <param name="blockType">Region role used to wrap the table.</param>
    /// <param name="repeatHeaders">Whether header rows repeat across pages.</param>
    /// <returns>A table-region builder linked to this report builder.</returns>
    public TableRegionBuilder BeginTable(
        string regionId,
        AppliedStyle regionStyle,
        string tableId,
        AppliedStyle tableStyle,
        IReadOnlyList<TableColumn> columns,
        BlockType blockType = BlockType.Detail,
        bool repeatHeaders = true)
    {
        return new TableRegionBuilder(
            owner: this,
            regionId,
            regionStyle,
            tableId,
            tableStyle,
            columns,
            blockType,
            repeatHeaders);
    }

    /// <summary>
    /// Creates a standalone table-region builder that can be materialized with
    /// <see cref="TableRegionBuilder.BuildBlock"/>.
    /// </summary>
    /// <param name="regionId">Stable containing region id.</param>
    /// <param name="regionStyle">Resolved style for the containing report region.</param>
    /// <param name="tableId">Stable table element id.</param>
    /// <param name="tableStyle">Resolved style for the table element.</param>
    /// <param name="columns">Table columns.</param>
    /// <param name="blockType">Region role used to wrap the table.</param>
    /// <param name="repeatHeaders">Whether header rows repeat across pages.</param>
    /// <returns>A standalone table-region builder.</returns>
    public static TableRegionBuilder CreateTableRegion(
        string regionId,
        AppliedStyle regionStyle,
        string tableId,
        AppliedStyle tableStyle,
        IReadOnlyList<TableColumn> columns,
        BlockType blockType = BlockType.Detail,
        bool repeatHeaders = true)
    {
        return new TableRegionBuilder(
            owner: null,
            regionId,
            regionStyle,
            tableId,
            tableStyle,
            columns,
            blockType,
            repeatHeaders);
    }

    /// <summary>
    /// Adds a block to the report.
    /// </summary>
    /// <param name="block">The block to append.</param>
    /// <returns>The current builder.</returns>
    public ReportLayoutBuilder Add(ReportBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        _blocks.Add(block);
        return this;
    }

    /// <summary>
    /// Adds multiple blocks to the report.
    /// </summary>
    /// <param name="blocks">The blocks to append.</param>
    /// <returns>The current builder.</returns>
    public ReportLayoutBuilder AddRange(IEnumerable<ReportBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        foreach (var block in blocks)
            Add(block);

        return this;
    }

    /// <summary>
    /// Adds a single-text report region block.
    /// </summary>
    /// <param name="id">Stable region id.</param>
    /// <param name="blockType">Report region role.</param>
    /// <param name="blockStyle">Resolved style for the containing region.</param>
    /// <param name="textStyle">Resolved style for the text child.</param>
    /// <param name="text">Text content.</param>
    /// <returns>The current builder.</returns>
    public ReportLayoutBuilder AddTextRegion(
        string id,
        BlockType blockType,
        AppliedStyle blockStyle,
        AppliedStyle textStyle,
        string text)
    {
        return Add(ReportDsl.TextRegion(id, blockType, blockStyle, textStyle, text));
    }

    /// <summary>
    /// Adds a page-break marker block.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved style for the marker block.</param>
    /// <returns>The current builder.</returns>
    public ReportLayoutBuilder AddPageBreak(string id, AppliedStyle style)
    {
        return Add(ReportBlockFactory.CreatePageBreak(id, style));
    }

    /// <summary>
    /// Produces an immutable ordered block list.
    /// </summary>
    /// <returns>All currently added blocks.</returns>
    public IReadOnlyList<ReportBlock> Build() => _blocks.ToList();

    /// <summary>
    /// Fluent builder for creating a table-backed report region.
    /// </summary>
    public sealed class TableRegionBuilder
    {
        private readonly ReportLayoutBuilder? _owner;
        private readonly string _regionId;
        private readonly AppliedStyle _regionStyle;
        private readonly string _tableId;
        private readonly AppliedStyle _tableStyle;
        private readonly IReadOnlyList<TableColumn> _columns;
        private readonly BlockType _blockType;
        private readonly bool _repeatHeaders;
        private readonly List<RowBlock> _rows = [];

        internal TableRegionBuilder(
            ReportLayoutBuilder? owner,
            string regionId,
            AppliedStyle regionStyle,
            string tableId,
            AppliedStyle tableStyle,
            IReadOnlyList<TableColumn> columns,
            BlockType blockType,
            bool repeatHeaders)
        {
            _owner = owner;
            _regionId = regionId;
            _regionStyle = regionStyle;
            _tableId = tableId;
            _tableStyle = tableStyle;
            _columns = columns;
            _blockType = blockType;
            _repeatHeaders = repeatHeaders;
        }

        /// <summary>
        /// Adds a header row with pre-built cells.
        /// </summary>
        /// <param name="rowId">Stable row id.</param>
        /// <param name="rowStyle">Resolved row style.</param>
        /// <param name="cells">Cells to append in column order.</param>
        /// <returns>The current table-region builder.</returns>
        public TableRegionBuilder AddHeaderRow(string rowId, AppliedStyle rowStyle, IReadOnlyList<CellBlock> cells)
        {
            return AddRow(RowType.Header, rowId, rowStyle, cells);
        }

        /// <summary>
        /// Adds a data row with pre-built cells.
        /// </summary>
        /// <param name="rowId">Stable row id.</param>
        /// <param name="rowStyle">Resolved row style.</param>
        /// <param name="cells">Cells to append in column order.</param>
        /// <returns>The current table-region builder.</returns>
        public TableRegionBuilder AddDataRow(string rowId, AppliedStyle rowStyle, IReadOnlyList<CellBlock> cells)
        {
            return AddRow(RowType.Data, rowId, rowStyle, cells);
        }

        /// <summary>
        /// Adds a header row from text values using a shared cell style.
        /// </summary>
        /// <param name="rowId">Stable row id.</param>
        /// <param name="rowStyle">Resolved row style.</param>
        /// <param name="cellStyle">Resolved style applied to each cell.</param>
        /// <param name="values">Header text values in column order.</param>
        /// <returns>The current table-region builder.</returns>
        public TableRegionBuilder AddHeaderRow(
            string rowId,
            AppliedStyle rowStyle,
            AppliedStyle cellStyle,
            IReadOnlyList<string> values)
        {
            return AddTextRow(RowType.Header, rowId, rowStyle, cellStyle, values);
        }

        /// <summary>
        /// Adds a data row from text values using a shared cell style.
        /// </summary>
        /// <param name="rowId">Stable row id.</param>
        /// <param name="rowStyle">Resolved row style.</param>
        /// <param name="cellStyle">Resolved style applied to each cell.</param>
        /// <param name="values">Data text values in column order.</param>
        /// <returns>The current table-region builder.</returns>
        public TableRegionBuilder AddDataRow(
            string rowId,
            AppliedStyle rowStyle,
            AppliedStyle cellStyle,
            IReadOnlyList<string> values)
        {
            return AddTextRow(RowType.Data, rowId, rowStyle, cellStyle, values);
        }

        /// <summary>
        /// Builds the report region block containing the configured table.
        /// </summary>
        /// <returns>The table-backed report block.</returns>
        public ReportBlock BuildBlock()
        {
            var table = new TableBlock
            {
                Id = _tableId,
                Style = _tableStyle,
                Columns = _columns,
                Rows = _rows,
                RepeatHeaders = _repeatHeaders
            };

            return ReportBlockFactory.Create(_blockType, _regionId, _regionStyle, [table]);
        }

        /// <summary>
        /// Finalizes the table and appends the resulting region to the owning report builder.
        /// </summary>
        /// <returns>The owning report builder.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this builder was created as a standalone table region.
        /// </exception>
        public ReportLayoutBuilder EndTable()
        {
            if (_owner is null)
                throw new InvalidOperationException("Cannot call EndTable() for a standalone table builder. Call BuildBlock() instead.");

            _owner.Add(BuildBlock());
            return _owner;
        }

        private TableRegionBuilder AddRow(RowType rowType, string rowId, AppliedStyle rowStyle, IReadOnlyList<CellBlock> cells)
        {
            ArgumentNullException.ThrowIfNull(rowId);
            ArgumentNullException.ThrowIfNull(rowStyle);
            ArgumentNullException.ThrowIfNull(cells);

            _rows.Add(new RowBlock
            {
                Id = rowId,
                RowType = rowType,
                Style = rowStyle,
                Cells = cells
            });

            return this;
        }

        private TableRegionBuilder AddTextRow(
            RowType rowType,
            string rowId,
            AppliedStyle rowStyle,
            AppliedStyle cellStyle,
            IReadOnlyList<string> values)
        {
            ArgumentNullException.ThrowIfNull(rowId);
            ArgumentNullException.ThrowIfNull(rowStyle);
            ArgumentNullException.ThrowIfNull(cellStyle);
            ArgumentNullException.ThrowIfNull(values);

            var textStyle = CreateCellTextStyle(cellStyle);
            var cells = values
                .Select((value, index) => ReportDsl.TextCell($"{rowId}-c{index + 1}", index, cellStyle, textStyle, value))
                .ToList();

            return AddRow(rowType, rowId, rowStyle, cells);
        }

        private static AppliedStyle CreateCellTextStyle(AppliedStyle cellStyle)
        {
            return cellStyle with
            {
                Background = Color.Transparent,
                Border = null,
                Padding = Core.Geometry.Thickness.Zero,
                Margin = Core.Geometry.Thickness.Zero,
                Overflow = Overflow.Visible
            };
        }
    }
}