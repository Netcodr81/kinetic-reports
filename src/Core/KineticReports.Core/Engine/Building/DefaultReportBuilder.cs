using KineticReports.Core.Definition;
using KineticReports.Core.Engine.Data;
using KineticReports.Core.Engine.Expressions;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Plugins;
using KineticReports.Core.Styling;

namespace KineticReports.Core.Engine.Building;

/// <summary>
/// Default implementation of <see cref="IReportBuilder"/> and a fluent, code-first API
/// for constructing report blocks.
/// </summary>
/// <remarks>
/// This builder is intended to cover common report construction scenarios:
/// static blocks, text/image/chart/shape regions, data-row repetition, and
/// data-source-driven tables.
/// Consumers can configure the builder through fluent methods and register it as
/// <see cref="IReportBuilder"/>. For bespoke scenarios, implement <see cref="IReportBuilder"/>
/// directly.
/// </remarks>
public sealed class DefaultReportBuilder : IReportBuilder
{
    private static readonly IReadOnlyDictionary<string, object?> EmptyParameters =
        new Dictionary<string, object?>(StringComparer.Ordinal);
    private readonly List<Func<DataContext, IExpressionEvaluator, IReadOnlyList<ReportBlock>>> _steps = [];
    private readonly IPluginManager? _pluginManager;

    private AppliedStyle _defaultBlockStyle = CreateDefaultBlockStyle();
    private AppliedStyle _defaultTextStyle = CreateDefaultTextStyle();

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultReportBuilder"/>.
    /// </summary>
    public DefaultReportBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultReportBuilder"/> with optional plugin support.
    /// </summary>
    /// <param name="pluginManager">
    /// Optional plugin manager. When available, loaded
    /// <see cref="IReportBlocksPostProcessorPlugin"/> instances are applied
    /// after fluent steps complete.
    /// </param>
    public DefaultReportBuilder(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
    }

    /// <summary>
    /// Creates a new empty <see cref="DefaultReportBuilder"/>.
    /// </summary>
    /// <returns>A new fluent report builder instance.</returns>
    public static DefaultReportBuilder Create() => new();

    /// <summary>
    /// Sets the default style applied to region/container blocks when a style is not explicitly supplied.
    /// </summary>
    /// <param name="style">The default region style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder WithDefaultBlockStyle(AppliedStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        _defaultBlockStyle = style;
        return this;
    }

    /// <summary>
    /// Sets the default style applied to text children when a text style is not explicitly supplied.
    /// </summary>
    /// <param name="style">The default text style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder WithDefaultTextStyle(AppliedStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        _defaultTextStyle = style;
        return this;
    }

    /// <summary>
    /// Adds a pre-built block to the report in order.
    /// </summary>
    /// <param name="block">The block to append.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddBlock(ReportBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        _steps.Add((_, _) => [block]);
        return this;
    }

    /// <summary>
    /// Adds multiple pre-built blocks to the report in order.
    /// </summary>
    /// <param name="blocks">Blocks to append.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddBlocks(IEnumerable<ReportBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        var captured = blocks.ToList();
        _steps.Add((_, _) => captured.ToList());
        return this;
    }

    /// <summary>
    /// Adds a single-text report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="text">Text content.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="textStyle">Optional text style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddTextRegion(
        string id,
        string text,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? textStyle = null)
    {
        ValidateRequired(id, nameof(id));
        ArgumentNullException.ThrowIfNull(text);

        var resolvedBlockStyle = ResolveBlockStyle(blockStyle);
        var resolvedTextStyle = ResolveTextStyle(textStyle);

        _steps.Add((_, _) => [ReportDsl.TextRegion(id, blockType, resolvedBlockStyle, resolvedTextStyle, text)]);
        return this;
    }

    /// <summary>
    /// Adds a single-image report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="sourceKey">Image source key or URI.</param>
    /// <param name="stretch">Image stretch mode.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="imageStyle">Optional image style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddImageRegion(
        string id,
        string sourceKey,
        ImageStretch stretch = ImageStretch.Uniform,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? imageStyle = null)
    {
        ValidateRequired(id, nameof(id));
        ValidateRequired(sourceKey, nameof(sourceKey));

        var child = ContentBlockFactory.CreateImage(
            $"{id}-image",
            ResolveBlockStyle(imageStyle),
            sourceKey,
            stretch);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a single-chart report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="chartType">Chart type key (for example Bar, Line, Pie).</param>
    /// <param name="chartData">Optional chart payload for the renderer.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="chartStyle">Optional chart element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddChartRegion(
        string id,
        string chartType,
        object? chartData = null,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? chartStyle = null)
    {
        ValidateRequired(id, nameof(id));
        ValidateRequired(chartType, nameof(chartType));

        var child = ContentBlockFactory.CreateChart(
            $"{id}-chart",
            ResolveBlockStyle(chartStyle),
            chartType,
            chartData);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a single-chart report region using strongly typed chart type.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="chartType">Strongly typed chart type.</param>
    /// <param name="chartData">Optional chart payload for the renderer.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="chartStyle">Optional chart element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddChartRegion(
        string id,
        ChartTypeName chartType,
        object? chartData = null,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? chartStyle = null)
    {
        ValidateRequired(id, nameof(id));

        var child = ContentBlockFactory.CreateChart(
            $"{id}-chart",
            ResolveBlockStyle(chartStyle),
            chartType,
            chartData);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a vertical bar chart report region.
    /// </summary>
    public DefaultReportBuilder AddVerticalBarChartRegion(
        string id,
        IReadOnlyList<ChartDataPoint> points,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? chartStyle = null)
    {
        ArgumentNullException.ThrowIfNull(points);
        return AddChartRegion(id, ChartTypeName.BarVertical, new ChartSeriesData { Points = points }, blockType, blockStyle, chartStyle);
    }

    /// <summary>
    /// Adds a horizontal bar chart report region.
    /// </summary>
    public DefaultReportBuilder AddHorizontalBarChartRegion(
        string id,
        IReadOnlyList<ChartDataPoint> points,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? chartStyle = null)
    {
        ArgumentNullException.ThrowIfNull(points);
        return AddChartRegion(id, ChartTypeName.BarHorizontal, new ChartSeriesData { Points = points }, blockType, blockStyle, chartStyle);
    }

    /// <summary>
    /// Adds a line chart report region.
    /// </summary>
    public DefaultReportBuilder AddLineChartRegion(
        string id,
        IReadOnlyList<ChartDataPoint> points,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? chartStyle = null)
    {
        ArgumentNullException.ThrowIfNull(points);
        return AddChartRegion(id, ChartTypeName.Line, new ChartSeriesData { Points = points }, blockType, blockStyle, chartStyle);
    }

    /// <summary>
    /// Adds a pie chart report region.
    /// </summary>
    public DefaultReportBuilder AddPieChartRegion(
        string id,
        IReadOnlyList<ChartDataPoint> points,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? chartStyle = null)
    {
        ArgumentNullException.ThrowIfNull(points);
        return AddChartRegion(id, ChartTypeName.Pie, new ChartSeriesData { Points = points }, blockType, blockStyle, chartStyle);
    }

    /// <summary>
    /// Adds a single-barcode report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="symbology">Barcode symbology (for example QR, Code128, EAN13).</param>
    /// <param name="value">Encoded value.</param>
    /// <param name="showText">Whether to display human-readable text.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="barcodeStyle">Optional barcode element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddBarcodeRegion(
        string id,
        string symbology,
        string value,
        bool showText = true,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? barcodeStyle = null)
    {
        ValidateRequired(id, nameof(id));
        ValidateRequired(symbology, nameof(symbology));
        ValidateRequired(value, nameof(value));

        var child = ContentBlockFactory.CreateBarcode(
            $"{id}-barcode",
            ResolveBlockStyle(barcodeStyle),
            symbology,
            value,
            showText);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a single-barcode report region using strongly typed symbology.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="symbology">Strongly typed barcode symbology.</param>
    /// <param name="value">Encoded value.</param>
    /// <param name="showText">Whether to display human-readable text.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="barcodeStyle">Optional barcode element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddBarcodeRegion(
        string id,
        BarcodeSymbology symbology,
        string value,
        bool showText = true,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? barcodeStyle = null)
    {
        ValidateRequired(id, nameof(id));
        ValidateRequired(value, nameof(value));

        var child = ContentBlockFactory.CreateBarcode(
            $"{id}-barcode",
            ResolveBlockStyle(barcodeStyle),
            symbology,
            value,
            showText);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a single QR-code report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="value">Encoded value.</param>
    /// <param name="showText">Whether to display human-readable text.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="barcodeStyle">Optional QR-code element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddQrCodeRegion(
        string id,
        string value,
        bool showText = false,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? barcodeStyle = null)
    {
        ValidateRequired(id, nameof(id));
        ValidateRequired(value, nameof(value));

        var child = ContentBlockFactory.CreateQrCode(
            $"{id}-qr",
            ResolveBlockStyle(barcodeStyle),
            value,
            showText);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a single Micro QR-code report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="value">Encoded value.</param>
    /// <param name="showText">Whether to display human-readable text.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="barcodeStyle">Optional QR-code element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddMicroQrCodeRegion(
        string id,
        string value,
        bool showText = false,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? barcodeStyle = null)
    {
        ValidateRequired(id, nameof(id));
        ValidateRequired(value, nameof(value));

        var child = ContentBlockFactory.CreateMicroQrCode(
            $"{id}-micro-qr",
            ResolveBlockStyle(barcodeStyle),
            value,
            showText);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a single rectangle report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="fill">Optional fill color.</param>
    /// <param name="stroke">Optional stroke color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="shapeStyle">Optional shape element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddRectangleRegion(
        string id,
        Color? fill = null,
        Color? stroke = null,
        float strokeWidth = 1f,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? shapeStyle = null)
    {
        ValidateRequired(id, nameof(id));

        var child = ContentBlockFactory.CreateRectangle(
            $"{id}-shape",
            ResolveBlockStyle(shapeStyle),
            fill,
            stroke,
            strokeWidth);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a single ellipse report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="fill">Optional fill color.</param>
    /// <param name="stroke">Optional stroke color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="shapeStyle">Optional shape element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddEllipseRegion(
        string id,
        Color? fill = null,
        Color? stroke = null,
        float strokeWidth = 1f,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? shapeStyle = null)
    {
        ValidateRequired(id, nameof(id));

        var child = ContentBlockFactory.CreateEllipse(
            $"{id}-shape",
            ResolveBlockStyle(shapeStyle),
            fill,
            stroke,
            strokeWidth);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a single line report region.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="stroke">Line color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="shapeStyle">Optional line element style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddLineRegion(
        string id,
        Color stroke,
        float strokeWidth = 1f,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? shapeStyle = null)
    {
        ValidateRequired(id, nameof(id));

        var child = ContentBlockFactory.CreateLine(
            $"{id}-line",
            ResolveBlockStyle(shapeStyle),
            stroke,
            strokeWidth);

        _steps.Add((_, _) => [ReportBlockFactory.Create(blockType, id, ResolveBlockStyle(blockStyle), [child])]);
        return this;
    }

    /// <summary>
    /// Adds a page-break marker block.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Optional style for the page-break marker block.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddPageBreak(string id, AppliedStyle? style = null)
    {
        ValidateRequired(id, nameof(id));
        _steps.Add((_, _) => [ReportBlockFactory.CreatePageBreak(id, ResolveBlockStyle(style))]);
        return this;
    }

    /// <summary>
    /// Adds a custom build step that can emit any number of blocks.
    /// </summary>
    /// <param name="step">Step callback invoked during <see cref="Build"/>.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddStep(Func<DataContext, IExpressionEvaluator, IReadOnlyList<ReportBlock>> step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Adds a data-source row repeater. One block is generated per row.
    /// </summary>
    /// <param name="dataSourceId">The source id from <see cref="DataContext.DataSources"/>.</param>
    /// <param name="blockFactory">Factory invoked once per row.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder ForEachRow(
        string dataSourceId,
        Func<RowBuildContext, ReportBlock> blockFactory)
    {
        ValidateRequired(dataSourceId, nameof(dataSourceId));
        ArgumentNullException.ThrowIfNull(blockFactory);

        _steps.Add((dataContext, evaluator) =>
        {
            var rows = dataContext.GetRows(dataSourceId);
            var blocks = new List<ReportBlock>(rows.Count);

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var expressionContext = CreateExpressionContext(row, rowIndex);
                var context = new RowBuildContext(dataSourceId, rowIndex, row, expressionContext, evaluator);
                blocks.Add(blockFactory(context));
            }

            return blocks;
        });

        return this;
    }

    /// <summary>
    /// Adds a common data-row text pattern: one text region per row.
    /// </summary>
    /// <param name="dataSourceId">The source id from <see cref="DataContext.DataSources"/>.</param>
    /// <param name="blockIdPrefix">Prefix used for generated block ids.</param>
    /// <param name="textExpression">Expression evaluated for each row.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="blockStyle">Optional region style.</param>
    /// <param name="textStyle">Optional text style.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder ForEachRowText(
        string dataSourceId,
        string blockIdPrefix,
        string textExpression,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? blockStyle = null,
        AppliedStyle? textStyle = null)
    {
        ValidateRequired(dataSourceId, nameof(dataSourceId));
        ValidateRequired(blockIdPrefix, nameof(blockIdPrefix));
        ArgumentNullException.ThrowIfNull(textExpression);

        return ForEachRow(
            dataSourceId,
            rowContext =>
            {
                var blockId = $"{blockIdPrefix}-{rowContext.RowIndex + 1}";
                var value = rowContext.EvaluateAsString(textExpression);
                return ReportDsl.TextRegion(
                    blockId,
                    blockType,
                    ResolveBlockStyle(blockStyle),
                    ResolveTextStyle(textStyle),
                    value);
            });
    }

    /// <summary>
    /// Adds a data-bound table block where rows are generated from a data source.
    /// </summary>
    /// <param name="dataSourceId">The source id from <see cref="DataContext.DataSources"/>.</param>
    /// <param name="regionId">Stable report region id that wraps the table.</param>
    /// <param name="tableId">Stable table id.</param>
    /// <param name="columns">Column bindings and header metadata.</param>
    /// <param name="blockType">Region role.</param>
    /// <param name="regionStyle">Optional region style.</param>
    /// <param name="tableStyle">Optional table style.</param>
    /// <param name="headerRowStyle">Optional style for the table header row.</param>
    /// <param name="headerCellStyle">Optional style for table header cells.</param>
    /// <param name="dataRowStyle">Optional style for data rows.</param>
    /// <param name="dataCellStyle">Optional style for data cells.</param>
    /// <param name="repeatHeaders">Whether header rows should repeat across pages.</param>
    /// <param name="includeHeader">Whether a header row should be generated.</param>
    /// <param name="groupByExpression">
    /// Optional expression evaluated per row. When the evaluated value changes,
    /// a group header row is emitted before the first row of the new group.
    /// </param>
    /// <param name="groupHeaderRowStyle">Optional style for generated group header rows.</param>
    /// <param name="groupHeaderCellStyle">Optional style for generated group header cells.</param>
    /// <param name="footerAggregates">Optional aggregate definitions for a generated footer row.</param>
    /// <param name="footerRowStyle">Optional style for the generated footer row.</param>
    /// <param name="footerCellStyle">Optional style for generated footer cells.</param>
    /// <param name="footerLabel">Optional label text for the footer label cell.</param>
    /// <param name="footerLabelColumnIndex">Zero-based column index where <paramref name="footerLabel"/> is placed.</param>
    /// <returns>The current builder.</returns>
    public DefaultReportBuilder AddDataSourceTable(
        string dataSourceId,
        string regionId,
        string tableId,
        IReadOnlyList<DataSourceTableColumn> columns,
        BlockType blockType = BlockType.Detail,
        AppliedStyle? regionStyle = null,
        AppliedStyle? tableStyle = null,
        AppliedStyle? headerRowStyle = null,
        AppliedStyle? headerCellStyle = null,
        AppliedStyle? dataRowStyle = null,
        AppliedStyle? dataCellStyle = null,
        bool repeatHeaders = true,
        bool includeHeader = true,
        string? groupByExpression = null,
        AppliedStyle? groupHeaderRowStyle = null,
        AppliedStyle? groupHeaderCellStyle = null,
        IReadOnlyList<TableAggregateDefinition>? footerAggregates = null,
        AppliedStyle? footerRowStyle = null,
        AppliedStyle? footerCellStyle = null,
        string? footerLabel = "Total",
        int footerLabelColumnIndex = 0)
    {
        ValidateRequired(dataSourceId, nameof(dataSourceId));
        ValidateRequired(regionId, nameof(regionId));
        ValidateRequired(tableId, nameof(tableId));
        ArgumentNullException.ThrowIfNull(columns);

        if (columns.Count == 0)
            throw new ArgumentException("At least one table column must be provided.", nameof(columns));

        var resolvedRegionStyle = ResolveBlockStyle(regionStyle);
        var resolvedTableStyle = ResolveBlockStyle(tableStyle);
        var resolvedHeaderRowStyle = ResolveBlockStyle(headerRowStyle);
        var resolvedHeaderCellStyle = EnsureDefaultTableCellChrome(ResolveBlockStyle(headerCellStyle), isHeaderCell: true);
        var resolvedDataRowStyle = ResolveBlockStyle(dataRowStyle);
        var resolvedDataCellStyle = EnsureDefaultTableCellChrome(ResolveBlockStyle(dataCellStyle));
        var resolvedGroupHeaderRowStyle = ResolveBlockStyle(groupHeaderRowStyle);
        var resolvedGroupHeaderCellStyle = EnsureDefaultTableCellChrome(ResolveBlockStyle(groupHeaderCellStyle));
        var resolvedFooterRowStyle = ResolveBlockStyle(footerRowStyle);
        var resolvedFooterCellStyle = EnsureDefaultTableCellChrome(ResolveBlockStyle(footerCellStyle));

        var configuredAggregates = footerAggregates?.ToList() ?? [];
        ValidateAggregateDefinitions(configuredAggregates, columns.Count, nameof(footerAggregates));
        ValidateFooterLabelColumnIndex(footerLabelColumnIndex, columns.Count, nameof(footerLabelColumnIndex));
        var columnCount = columns.Count;

        _steps.Add((dataContext, evaluator) =>
        {
            var rows = dataContext.GetRows(dataSourceId);
            var estimatedExtraRows = (includeHeader ? 1 : 0)
                + (configuredAggregates.Count > 0 ? 1 : 0)
                + (string.IsNullOrWhiteSpace(groupByExpression) ? 0 : rows.Count);
            var tableRows = new List<RowBlock>(rows.Count + estimatedExtraRows);

            var aggregateStates = configuredAggregates
                .Select(definition => new AggregateState(definition))
                .ToList();

            if (includeHeader)
            {
                var headerCells = columns
                    .Select((column, index) => ReportDsl.TextCell(
                        id: $"{tableId}-h-c{index + 1}",
                        columnIndex: index,
                        cellStyle: resolvedHeaderCellStyle,
                        textStyle: CreateCellTextStyle(resolvedHeaderCellStyle),
                        text: column.Header))
                    .ToList();

                tableRows.Add(new RowBlock
                {
                    Id = $"{tableId}-h-r1",
                    RowType = RowType.Header,
                    Style = resolvedHeaderRowStyle,
                    Cells = headerCells
                });
            }

            string? currentGroupKey = null;

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var expressionContext = CreateExpressionContext(row, rowIndex);

                if (!string.IsNullOrWhiteSpace(groupByExpression))
                {
                    var nextGroupKey = EvaluateAsString(evaluator, groupByExpression, expressionContext);
                    if (!string.Equals(currentGroupKey, nextGroupKey, StringComparison.Ordinal))
                    {
                        currentGroupKey = nextGroupKey;
                        tableRows.Add(CreateGroupHeaderRow(
                            tableId,
                            tableRows.Count + 1,
                            columnCount,
                            currentGroupKey,
                            resolvedGroupHeaderRowStyle,
                            resolvedGroupHeaderCellStyle));
                    }
                }

                var cells = columns
                    .Select((column, index) => ReportDsl.TextCell(
                        id: $"{tableId}-d-r{rowIndex + 1}-c{index + 1}",
                        columnIndex: index,
                        cellStyle: resolvedDataCellStyle,
                        textStyle: CreateCellTextStyle(resolvedDataCellStyle),
                        text: EvaluateAsString(evaluator, column.ValueExpression, expressionContext)))
                    .ToList();

                tableRows.Add(new RowBlock
                {
                    Id = $"{tableId}-d-r{rowIndex + 1}",
                    RowType = RowType.Data,
                    Style = resolvedDataRowStyle,
                    Cells = cells
                });

                foreach (var aggregateState in aggregateStates)
                {
                    var aggregateValue = evaluator.Evaluate(aggregateState.Definition.ValueExpression, expressionContext);
                    aggregateState.Accumulate(aggregateValue);
                }
            }

            if (aggregateStates.Count > 0)
            {
                tableRows.Add(CreateFooterRow(
                    tableId,
                    tableRows.Count + 1,
                    columnCount,
                    footerLabel,
                    footerLabelColumnIndex,
                    aggregateStates,
                    resolvedFooterRowStyle,
                    resolvedFooterCellStyle));
            }

            var table = new TableBlock
            {
                Id = tableId,
                Style = resolvedTableStyle,
                RepeatHeaders = repeatHeaders,
                Columns = columns.Select(c => c.Column).ToList(),
                Rows = tableRows
            };

            var block = ReportBlockFactory.Create(blockType, regionId, resolvedRegionStyle, [table]);
            return [block];
        });

        return this;
    }

    private static void ValidateAggregateDefinitions(
        IReadOnlyList<TableAggregateDefinition> definitions,
        int columnCount,
        string parameterName)
    {
        foreach (var definition in definitions)
        {
            ValidateRequired(definition.ValueExpression, $"{parameterName}.{nameof(TableAggregateDefinition.ValueExpression)}");

            if (definition.ColumnIndex < 0 || definition.ColumnIndex >= columnCount)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    definition.ColumnIndex,
                    "Aggregate column index must refer to an existing table column.");
            }
        }
    }

    private static void ValidateFooterLabelColumnIndex(int columnIndex, int columnCount, string parameterName)
    {
        if (columnIndex < 0 || columnIndex >= columnCount)
            throw new ArgumentOutOfRangeException(parameterName, columnIndex, "Footer label column index must refer to an existing table column.");
    }

    private static RowBlock CreateGroupHeaderRow(
        string tableId,
        int rowOrdinal,
        int columnCount,
        string groupKey,
        AppliedStyle rowStyle,
        AppliedStyle cellStyle)
    {
        var text = string.IsNullOrWhiteSpace(groupKey)
            ? "Group: (blank)"
            : $"Group: {groupKey}";

        return new RowBlock
        {
            Id = $"{tableId}-g-r{rowOrdinal}",
            RowType = RowType.Data,
            Style = rowStyle,
            Cells =
            [
                new CellBlock
                {
                    Id = $"{tableId}-g-r{rowOrdinal}-c1",
                    ColumnIndex = 0,
                    ColSpan = columnCount,
                    Style = cellStyle,
                    Children =
                    [
                        ContentBlockFactory.CreateText(
                            $"{tableId}-g-r{rowOrdinal}-c1-text",
                            CreateCellTextStyle(cellStyle),
                            text)
                    ]
                }
            ]
        };
    }

    private static RowBlock CreateFooterRow(
        string tableId,
        int rowOrdinal,
        int columnCount,
        string? footerLabel,
        int footerLabelColumnIndex,
        IReadOnlyList<AggregateState> aggregateStates,
        AppliedStyle rowStyle,
        AppliedStyle cellStyle)
    {
        var cells = new List<CellBlock>(columnCount);

        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var matchingAggregate = aggregateStates
                .FirstOrDefault(state => state.Definition.ColumnIndex == columnIndex);

            var text = matchingAggregate is null
                ? string.Empty
                : matchingAggregate.ToDisplayString();

            if (matchingAggregate is null
                && !string.IsNullOrWhiteSpace(footerLabel)
                && columnIndex == footerLabelColumnIndex)
            {
                text = footerLabel;
            }

            cells.Add(ReportDsl.TextCell(
                id: $"{tableId}-f-r{rowOrdinal}-c{columnIndex + 1}",
                columnIndex: columnIndex,
                cellStyle: cellStyle,
                textStyle: CreateCellTextStyle(cellStyle),
                text: text));
        }

        return new RowBlock
        {
            Id = $"{tableId}-f-r{rowOrdinal}",
            RowType = RowType.Footer,
            Style = rowStyle,
            Cells = cells
        };
    }

    /// <summary>
    /// Builds report blocks from the configured fluent steps.
    /// </summary>
    /// <param name="dataContext">Resolved report data context.</param>
    /// <param name="evaluator">Expression evaluator used for dynamic bindings.</param>
    /// <returns>Ordered blocks built by all configured steps.</returns>
    public IReadOnlyList<ReportBlock> Build(
        DataContext dataContext,
        IExpressionEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(dataContext);
        ArgumentNullException.ThrowIfNull(evaluator);

        if (_steps.Count == 0
            && TryCreateDefinitionBackedBuilder(dataContext.Definition, out var definitionBackedBuilder))
        {
            return definitionBackedBuilder.Build(dataContext, evaluator);
        }

        if (_steps.Count == 0)
            return [];

        var blocks = new List<ReportBlock>();

        foreach (var step in _steps)
        {
            var built = step(dataContext, evaluator);
            if (built.Count == 0)
                continue;

            blocks.AddRange(built);
        }

        return ApplyPostProcessors(blocks, dataContext.Definition);
    }

    private bool TryCreateDefinitionBackedBuilder(
        ReportDefinition? definition,
        out DefaultReportBuilder builder)
    {
        builder = null!;

        if (definition?.Layout is null)
            return false;

        builder = _pluginManager is null ? new DefaultReportBuilder() : new DefaultReportBuilder(_pluginManager);
        ConfigureFromLayoutDefinition(builder, definition.Layout, definition.Styles);
        return true;
    }

    private static void ConfigureFromLayoutDefinition(
        DefaultReportBuilder builder,
        ReportLayoutDefinition layout,
        IReadOnlyList<StyleDefinition> styles)
    {
        var namedStyles = BuildNamedStyleLookup(styles);

        foreach (var item in layout.PageHeader)
            AppendLayoutItem(builder, item, BlockType.PageHeader, namedStyles);

        foreach (var item in layout.Body)
            AppendLayoutItem(builder, item, BlockType.Detail, namedStyles);

        foreach (var item in layout.PageFooter)
            AppendLayoutItem(builder, item, BlockType.PageFooter, namedStyles);
    }

    private static void AppendLayoutItem(
        DefaultReportBuilder builder,
        ReportLayoutItemDefinition item,
        BlockType blockType,
        IReadOnlyDictionary<string, AppliedStyle> namedStyles)
    {
        AppliedStyle? ResolveStyleId(string? styleId, string slotName)
        {
            if (string.IsNullOrWhiteSpace(styleId))
                return null;

            if (namedStyles.TryGetValue(styleId, out var style))
                return style;

            throw new InvalidOperationException(
                $"Layout item '{item.Id}' references unknown style id '{styleId}' for '{slotName}'.");
        }

        switch (item.Kind)
        {
            case ReportLayoutItemKind.Text:
                builder.AddTextRegion(
                    id: item.Id,
                    text: item.Text ?? string.Empty,
                    blockType: blockType,
                    blockStyle: ResolveStyleId(item.BlockStyleId, nameof(item.BlockStyleId)),
                    textStyle: ResolveStyleId(item.TextStyleId, nameof(item.TextStyleId)));
                break;

            case ReportLayoutItemKind.Image:
                if (string.IsNullOrWhiteSpace(item.SourceKey))
                    break;

                builder.AddImageRegion(
                    id: item.Id,
                    sourceKey: item.SourceKey,
                    stretch: item.Stretch,
                    blockType: blockType,
                    blockStyle: ResolveStyleId(item.BlockStyleId, nameof(item.BlockStyleId)),
                    imageStyle: ResolveStyleId(item.ImageStyleId, nameof(item.ImageStyleId)));
                break;

            case ReportLayoutItemKind.Barcode:
                var value = item.Value ?? item.Text;
                if (string.IsNullOrWhiteSpace(value))
                    break;

                if (item.SymbologyType.HasValue)
                {
                    builder.AddBarcodeRegion(
                        id: item.Id,
                        symbology: item.SymbologyType.Value,
                        value: value,
                        showText: item.ShowText,
                        blockType: blockType,
                        blockStyle: ResolveStyleId(item.BlockStyleId, nameof(item.BlockStyleId)),
                        barcodeStyle: ResolveStyleId(item.BarcodeStyleId, nameof(item.BarcodeStyleId)));
                    break;
                }

                builder.AddBarcodeRegion(
                    id: item.Id,
                    symbology: item.Symbology ?? "QR",
                    value: value,
                    showText: item.ShowText,
                    blockType: blockType,
                    blockStyle: ResolveStyleId(item.BlockStyleId, nameof(item.BlockStyleId)),
                    barcodeStyle: ResolveStyleId(item.BarcodeStyleId, nameof(item.BarcodeStyleId)));
                break;

            case ReportLayoutItemKind.Chart:
                object? chartData = null;
                if (!string.IsNullOrWhiteSpace(item.ChartDataJson))
                {
                    chartData = global::System.Text.Json.JsonSerializer.Deserialize<object>(item.ChartDataJson);
                }

                if (item.ChartTypeValue.HasValue)
                {
                    builder.AddChartRegion(
                        id: item.Id,
                        chartType: item.ChartTypeValue.Value,
                        chartData: chartData,
                        blockType: blockType,
                        blockStyle: ResolveStyleId(item.BlockStyleId, nameof(item.BlockStyleId)),
                        chartStyle: ResolveStyleId(item.ChartStyleId, nameof(item.ChartStyleId)));
                    break;
                }

                builder.AddChartRegion(
                    id: item.Id,
                    chartType: item.ChartType ?? "BAR_VERTICAL",
                    chartData: chartData,
                    blockType: blockType,
                    blockStyle: ResolveStyleId(item.BlockStyleId, nameof(item.BlockStyleId)),
                    chartStyle: ResolveStyleId(item.ChartStyleId, nameof(item.ChartStyleId)));
                break;

            case ReportLayoutItemKind.PageBreak:
                builder.AddPageBreak(
                    id: item.Id,
                    style: ResolveStyleId(item.BlockStyleId, nameof(item.BlockStyleId)));
                break;

            case ReportLayoutItemKind.Table:
                if (string.IsNullOrWhiteSpace(item.DataSourceId) || item.Columns.Count == 0)
                    break;

                builder.AddDataSourceTable(
                    dataSourceId: item.DataSourceId,
                    regionId: $"{item.Id}-region",
                    tableId: item.Id,
                    columns: item.Columns.Select(column => new DataSourceTableColumn
                    {
                        Header = column.Header,
                        ValueExpression = column.ValueExpression,
                        Column = new TableColumn
                        {
                            MinWidth = column.MinWidth,
                            Grow = column.Grow
                        }
                    }).ToList(),
                    blockType: blockType,
                    regionStyle: ResolveStyleId(item.RegionStyleId ?? item.BlockStyleId, nameof(item.RegionStyleId)),
                    tableStyle: ResolveStyleId(item.TableStyleId, nameof(item.TableStyleId)),
                    headerRowStyle: ResolveStyleId(item.HeaderRowStyleId, nameof(item.HeaderRowStyleId)),
                    headerCellStyle: ResolveStyleId(item.HeaderCellStyleId, nameof(item.HeaderCellStyleId)),
                    dataRowStyle: ResolveStyleId(item.DataRowStyleId, nameof(item.DataRowStyleId)),
                    dataCellStyle: ResolveStyleId(item.DataCellStyleId, nameof(item.DataCellStyleId)),
                    repeatHeaders: item.RepeatHeaders,
                    includeHeader: item.IncludeHeader,
                    groupByExpression: string.IsNullOrWhiteSpace(item.GroupByExpression) ? null : item.GroupByExpression,
                    groupHeaderRowStyle: ResolveStyleId(item.GroupHeaderRowStyleId, nameof(item.GroupHeaderRowStyleId)),
                    groupHeaderCellStyle: ResolveStyleId(item.GroupHeaderCellStyleId, nameof(item.GroupHeaderCellStyleId)),
                    footerAggregates: item.FooterAggregates.Select(aggregate => new TableAggregateDefinition
                    {
                        ColumnIndex = aggregate.ColumnIndex,
                        ValueExpression = aggregate.ValueExpression,
                        Kind = Enum.TryParse<TableAggregateKind>(aggregate.Kind, true, out var parsedKind)
                            ? parsedKind
                            : TableAggregateKind.Sum,
                        FormatString = aggregate.FormatString
                    }).ToList(),
                    footerRowStyle: ResolveStyleId(item.FooterRowStyleId, nameof(item.FooterRowStyleId)),
                    footerCellStyle: ResolveStyleId(item.FooterCellStyleId, nameof(item.FooterCellStyleId)),
                    footerLabel: item.FooterLabel,
                    footerLabelColumnIndex: item.FooterLabelColumnIndex);
                break;
        }
    }

    private static IReadOnlyDictionary<string, AppliedStyle> BuildNamedStyleLookup(IReadOnlyList<StyleDefinition> styles)
    {
        if (styles.Count == 0)
            return new Dictionary<string, AppliedStyle>(StringComparer.Ordinal);

        var definitionsById = new Dictionary<string, StyleDefinition>(StringComparer.Ordinal);
        foreach (var style in styles)
        {
            if (string.IsNullOrWhiteSpace(style.Id))
                throw new InvalidOperationException("Style definitions must include a non-empty id.");

            if (!definitionsById.TryAdd(style.Id, style))
                throw new InvalidOperationException($"Duplicate style id '{style.Id}' found in report definition.");
        }

        var resolved = new Dictionary<string, AppliedStyle>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);

        foreach (var styleId in definitionsById.Keys)
            ResolveStyle(styleId);

        return resolved;

        AppliedStyle ResolveStyle(string styleId)
        {
            if (resolved.TryGetValue(styleId, out var style))
                return style;

            if (!definitionsById.TryGetValue(styleId, out var definition))
                throw new InvalidOperationException($"Style id '{styleId}' is not defined.");

            if (!visiting.Add(styleId))
                throw new InvalidOperationException($"Cycle detected in style inheritance for style id '{styleId}'.");

            var inherited = string.IsNullOrWhiteSpace(definition.BasedOn)
                ? CreateDefaultBlockStyle()
                : ResolveStyle(definition.BasedOn!);

            var resolvedStyle = ApplyNamedStyle(inherited, definition);
            resolved[styleId] = resolvedStyle;
            visiting.Remove(styleId);
            return resolvedStyle;
        }
    }

    private static AppliedStyle ApplyNamedStyle(AppliedStyle parent, StyleDefinition style)
    {
        var typography = style.Typography;

        return parent with
        {
            FontFamily = typography?.Family ?? parent.FontFamily,
            FontSize = typography?.Size ?? parent.FontSize,
            FontWeight = typography?.Weight ?? parent.FontWeight,
            FontStyle = typography?.Style ?? parent.FontStyle,
            LineHeight = typography?.LineHeight ?? parent.LineHeight,
            LetterSpacing = typography?.LetterSpacing ?? parent.LetterSpacing,
            TextColor = typography?.Color ?? parent.TextColor,
            TextAlignment = typography?.Alignment ?? parent.TextAlignment,
            VerticalAlignment = typography?.VerticalAlignment ?? parent.VerticalAlignment,
            TextDecoration = typography?.Decoration ?? parent.TextDecoration,
            Border = style.Border ?? parent.Border,
            Background = style.Background ?? parent.Background,
            Margin = style.Margin ?? parent.Margin,
            Padding = style.Padding ?? parent.Padding,
            Opacity = style.Opacity ?? parent.Opacity,
            Overflow = style.Overflow ?? parent.Overflow
        };
    }

    private IReadOnlyList<ReportBlock> ApplyPostProcessors(
        IReadOnlyList<ReportBlock> blocks,
        ReportDefinition? definition)
    {
        if (_pluginManager is null)
            return blocks;

        var postProcessors = _pluginManager.LoadedPlugins
            .OfType<IReportBlocksPostProcessorPlugin>()
            .Where(plugin => PluginExecutionPolicy.IsEnabled(definition, plugin.Id))
            .OrderBy(plugin => plugin.Order)
            .ThenBy(plugin => plugin.Id, StringComparer.Ordinal)
            .ToList();

        if (postProcessors.Count == 0)
            return blocks;

        var current = blocks;
        foreach (var postProcessor in postProcessors)
        {
            current = postProcessor.ProcessBlocks(current);
        }

        return current;
    }

    /// <summary>
    /// Row-scoped context provided to <see cref="ForEachRow(string, Func{RowBuildContext, ReportBlock})"/> callbacks.
    /// </summary>
    public sealed class RowBuildContext
    {
        private readonly IExpressionEvaluator _evaluator;

        internal RowBuildContext(
            string dataSourceId,
            int rowIndex,
            IReadOnlyDictionary<string, object?> row,
            ExpressionContext expressionContext,
            IExpressionEvaluator evaluator)
        {
            DataSourceId = dataSourceId;
            RowIndex = rowIndex;
            Row = row;
            ExpressionContext = expressionContext;
            _evaluator = evaluator;
        }

        /// <summary>
        /// Gets the data source id associated with this row.
        /// </summary>
        public string DataSourceId { get; }

        /// <summary>
        /// Gets the zero-based row index within the source.
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        /// Gets the current row values.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Row { get; }

        /// <summary>
        /// Gets the expression context used for evaluation.
        /// </summary>
        public ExpressionContext ExpressionContext { get; }

        /// <summary>
        /// Evaluates an expression and returns the raw value.
        /// </summary>
        /// <param name="expression">Expression text.</param>
        /// <returns>The evaluated value or <see langword="null"/>.</returns>
        public object? Evaluate(string expression)
        {
            ArgumentNullException.ThrowIfNull(expression);
            return _evaluator.Evaluate(expression, ExpressionContext);
        }

        /// <summary>
        /// Evaluates an expression and returns a text-safe value.
        /// </summary>
        /// <param name="expression">Expression text.</param>
        /// <returns>A non-null string suitable for text rendering.</returns>
        public string EvaluateAsString(string expression)
        {
            ArgumentNullException.ThrowIfNull(expression);
            var value = _evaluator.Evaluate(expression, ExpressionContext);
            return ToText(value);
        }
    }

    /// <summary>
    /// Defines a data-source-backed table column.
    /// </summary>
    public sealed record DataSourceTableColumn
    {
        /// <summary>
        /// Gets or inits the header text shown for this column.
        /// </summary>
        public required string Header { get; init; }

        /// <summary>
        /// Gets or inits the value expression evaluated for each data row.
        /// </summary>
        public required string ValueExpression { get; init; }

        /// <summary>
        /// Gets or inits the table column sizing metadata.
        /// </summary>
        public TableColumn Column { get; init; } = new()
        {
            MinWidth = 40f,
            Grow = 1f
        };
    }

    /// <summary>
    /// Defines an aggregate emitted into a generated table footer row.
    /// </summary>
    public sealed record TableAggregateDefinition
    {
        /// <summary>
        /// Gets or inits the zero-based output column index for the aggregate value.
        /// </summary>
        public required int ColumnIndex { get; init; }

        /// <summary>
        /// Gets or inits the value expression evaluated for each row.
        /// </summary>
        public required string ValueExpression { get; init; }

        /// <summary>
        /// Gets or inits the aggregate function to apply.
        /// </summary>
        public TableAggregateKind Kind { get; init; } = TableAggregateKind.Sum;

        /// <summary>
        /// Gets or inits an optional .NET format string applied to the aggregate value.
        /// </summary>
        public string? FormatString { get; init; }
    }

    /// <summary>
    /// Supported aggregate functions for <see cref="TableAggregateDefinition"/>.
    /// </summary>
    public enum TableAggregateKind
    {
        /// <summary>Sum all numeric values.</summary>
        Sum,

        /// <summary>Average all numeric values.</summary>
        Average,

        /// <summary>Return the smallest numeric value.</summary>
        Min,

        /// <summary>Return the largest numeric value.</summary>
        Max,

        /// <summary>Count all rows processed by the table.</summary>
        Count,

        /// <summary>Count only rows where the expression result is not null.</summary>
        CountNonNull,
    }

    private sealed class AggregateState
    {
        private decimal _sum;
        private decimal _min = decimal.MaxValue;
        private decimal _max = decimal.MinValue;
        private int _numericCount;
        private int _rowCount;
        private int _nonNullCount;

        internal AggregateState(TableAggregateDefinition definition)
        {
            Definition = definition;
        }

        internal TableAggregateDefinition Definition { get; }

        internal void Accumulate(object? value)
        {
            _rowCount++;

            if (value is not null)
                _nonNullCount++;

            if (!TryToDecimal(value, out var numericValue))
                return;

            _sum += numericValue;
            _numericCount++;
            _min = Math.Min(_min, numericValue);
            _max = Math.Max(_max, numericValue);
        }

        internal string ToDisplayString()
        {
            object? rawValue = Definition.Kind switch
            {
                TableAggregateKind.Sum => _numericCount == 0 ? null : _sum,
                TableAggregateKind.Average => _numericCount == 0 ? null : _sum / _numericCount,
                TableAggregateKind.Min => _numericCount == 0 ? null : _min,
                TableAggregateKind.Max => _numericCount == 0 ? null : _max,
                TableAggregateKind.Count => _rowCount,
                TableAggregateKind.CountNonNull => _nonNullCount,
                _ => null
            };

            if (rawValue is null)
                return string.Empty;

            if (rawValue is IFormattable formattable && !string.IsNullOrWhiteSpace(Definition.FormatString))
            {
                return formattable.ToString(Definition.FormatString, System.Globalization.CultureInfo.InvariantCulture);
            }

            return ToText(rawValue);
        }

        private static bool TryToDecimal(object? value, out decimal result)
        {
            if (value is null)
            {
                result = 0m;
                return false;
            }

            switch (value)
            {
                case decimal d:
                    result = d;
                    return true;
                case byte b:
                    result = b;
                    return true;
                case sbyte sb:
                    result = sb;
                    return true;
                case short s:
                    result = s;
                    return true;
                case ushort us:
                    result = us;
                    return true;
                case int i:
                    result = i;
                    return true;
                case uint ui:
                    result = ui;
                    return true;
                case long l:
                    result = l;
                    return true;
                case ulong ul:
                    result = ul;
                    return true;
                case float f:
                    result = (decimal)f;
                    return true;
                case double dbl:
                    result = (decimal)dbl;
                    return true;
            }

            return decimal.TryParse(
                Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out result);
        }
    }


    private static ExpressionContext CreateExpressionContext(IReadOnlyDictionary<string, object?> row, int rowIndex)
    {
        return new ExpressionContext
        {
            CurrentRow = row,
            Parameters = EmptyParameters,
            RowIndex = rowIndex,
            PageNumber = 1
        };
    }

    private static string EvaluateAsString(IExpressionEvaluator evaluator, string expression, ExpressionContext context)
    {
        var value = evaluator.Evaluate(expression, context);
        return ToText(value);
    }

    private static string ToText(object? value)
    {
        if (value is null)
            return string.Empty;

        if (value is string text)
            return text;

        return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static AppliedStyle CreateCellTextStyle(AppliedStyle source)
    {
        return new AppliedStyle
        {
            FontFamily = source.FontFamily,
            FontSize = source.FontSize,
            FontWeight = source.FontWeight,
            FontStyle = source.FontStyle,
            LineHeight = source.LineHeight,
            LetterSpacing = source.LetterSpacing,
            TextColor = source.TextColor,
            TextAlignment = source.TextAlignment,
            VerticalAlignment = source.VerticalAlignment,
            TextDecoration = source.TextDecoration,
            Opacity = source.Opacity,
            Overflow = source.Overflow
        };
    }

    private static AppliedStyle EnsureDefaultTableCellChrome(AppliedStyle source, bool isHeaderCell = false)
    {
        var style = source;

        if (style.Padding == Thickness.Zero)
            style = style with { Padding = new Thickness(horizontal: 8f, vertical: 6f) };

        if (style.Border is null)
            style = style with { Border = Border.Uniform(1f, Color.Black) };

        if (isHeaderCell && style.FontWeight == FontWeight.Normal)
            style = style with { FontWeight = FontWeight.SemiBold };

        return style;
    }

    private AppliedStyle ResolveBlockStyle(AppliedStyle? style) => style ?? _defaultBlockStyle;

    private AppliedStyle ResolveTextStyle(AppliedStyle? style) => style ?? _defaultTextStyle;

    private static AppliedStyle CreateDefaultBlockStyle()
    {
        return new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 11f
        };
    }

    private static AppliedStyle CreateDefaultTextStyle()
    {
        return new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 11f
        };
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
