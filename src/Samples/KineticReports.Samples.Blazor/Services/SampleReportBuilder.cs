namespace KineticReports.Samples.Blazor.Services;

using System.Globalization;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Engine.Building;
using KineticReports.Engine.Data;
using KineticReports.Engine.Expressions;

/// <summary>
/// Builds simple, data-backed report blocks for the Blazor sample reports.
/// </summary>
internal sealed class SampleReportBuilder : IReportBuilder
{
    private const float HeaderBlockBottomSpacing = 12f;
    private const float SectionTitleBlockTopSpacing = 4f;
    private const float SectionTitleBlockBottomSpacing = 8f;
    private const float DetailBlockBottomSpacing = 6f;
    private const float TableBlockBottomSpacing = 12f;

    private static readonly AppliedStyle HeaderTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 20f,
        FontWeight = FontWeight.Bold
    };

    private static readonly AppliedStyle SectionTitleStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 14f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(36, 70, 140)
    };

    private static readonly AppliedStyle DetailTextStyle = new()
    {
        FontFamily = "Consolas",
        FontSize = 11f,
        LineHeight = 1.45f
    };

    private static readonly AppliedStyle GroupHeaderTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 12f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(31, 58, 111)
    };

    private static readonly AppliedStyle EmphasisTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 12f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(44, 94, 69)
    };

    private static readonly AppliedStyle SubtleTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 11f,
        TextColor = Color.FromRgb(80, 88, 102)
    };

    private static readonly AppliedStyle TableHeaderCellStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 11f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(255, 255, 255),
        Background = Color.FromRgb(31, 58, 111),
        Padding = new Core.Geometry.Thickness(6f, 8f, 6f, 8f),
        Border = Border.Uniform(1f, Color.FromRgb(199, 210, 230))
    };

    private static readonly AppliedStyle TableCellStyle = new()
    {
        FontFamily = "Consolas",
        FontSize = 11f,
        TextColor = Color.FromRgb(28, 32, 42),
        Padding = new Core.Geometry.Thickness(5f, 8f, 5f, 8f),
        Border = Border.Uniform(1f, Color.FromRgb(220, 226, 239))
    };

    /// <inheritdoc/>
    public IReadOnlyList<ReportBlock> Build(DataContext dataContext, IExpressionEvaluator evaluator)
    {
        var builder = new ReportLayoutBuilder()
            .Add(CreateSingleTextBlock(
                "sample-header",
                BlockType.PageHeader,
                HeaderTextStyle,
                "KineticReports Sample Data Preview"));

        foreach (var dataSource in dataContext.DataSources)
        {
            var sourceId = dataSource.Key;
            var rows = dataSource.Value;

            builder.Add(CreateSingleTextBlock(
                $"source-{sourceId}-title",
                BlockType.ReportHeader,
                SectionTitleStyle,
                $"Data Source: {sourceId} ({rows.Count} row(s))"));

            if (rows.Count == 0)
            {
                builder.Add(CreateSingleTextBlock(
                    $"source-{sourceId}-empty",
                    BlockType.Detail,
                    DetailTextStyle,
                    "No rows returned."));

                continue;
            }

            if (string.Equals(sourceId, "sales-orders", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(CreateSalesOrdersTableBlock(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "quote-daily", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddRange(CreateQuoteDailyBlocks(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "customer-activity", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddRange(CreateCustomerActivityGroupedBlocks(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "kpi-summary", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddRange(CreateKpiSummaryBlocks(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "regional-performance", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(CreateRegionalPerformanceTableBlock(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "style-showcase", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddRange(CreateStyleShowcaseBlocks(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "expression-demo", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(CreateExpressionDemoTableBlock(sourceId, rows, evaluator));

                continue;
            }

            builder.Add(CreateGenericTableBlock(sourceId, rows));
        }

        if (dataContext.DataSources.Count == 0)
        {
            builder.Add(CreateSingleTextBlock(
                "sample-empty",
                BlockType.Detail,
                DetailTextStyle,
                "No data sources were resolved for this report."));
        }

        return builder.Build();
    }

    private static ReportBlock CreateSingleTextBlock(
        string id,
        BlockType blockType,
        AppliedStyle textStyle,
        string text)
    {
        return ReportDsl.TextRegion(id, blockType, CreateBlockStyle(blockType), textStyle, text);
    }

    private static IReadOnlyList<ReportBlock> CreateQuoteDailyBlocks(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var row = rows[0];

        return
        [
            CreateSingleTextBlock(
                $"source-{sourceId}-heading",
                BlockType.Detail,
                HeaderTextStyle,
                GetRowValue(row, "Heading")),
            CreateSingleTextBlock(
                $"source-{sourceId}-quote",
                BlockType.Detail,
                EmphasisTextStyle,
                GetRowValue(row, "Quote")),
            CreateSingleTextBlock(
                $"source-{sourceId}-meta",
                BlockType.Detail,
                SubtleTextStyle,
                $"Generated: {GetRowValue(row, "GeneratedOn")} | Author: {GetRowValue(row, "Author")}")
        ];
    }

    private static IReadOnlyList<ReportBlock> CreateCustomerActivityGroupedBlocks(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var contentRegions = new List<ReportBlock>();

        var groupedRows = rows
            .GroupBy(row => GetRowValue(row, "Tier"), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in groupedRows)
        {
            contentRegions.Add(CreateSingleTextBlock(
                $"source-{sourceId}-group-{SanitizeForId(group.Key)}",
                BlockType.GroupHeader,
                GroupHeaderTextStyle,
                $"Tier: {group.Key} ({group.Count()} customer(s))"));

            var customerRows = group
                .OrderBy(row => GetRowValue(row, "CustomerName"), StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var index = 0; index < customerRows.Count; index++)
            {
                var row = customerRows[index];
                var detail =
                    $"{index + 1:00}. {GetRowValue(row, "CustomerName")} | " +
                    $"Rep: {GetRowValue(row, "AssignedRep")} | " +
                    $"Balance: {GetRowValue(row, "Balance")} | " +
                    $"Last Invoice: {GetRowValue(row, "LastInvoice")}";

                contentRegions.Add(CreateSingleTextBlock(
                    $"source-{sourceId}-{SanitizeForId(group.Key)}-row-{index + 1}",
                    BlockType.Detail,
                    DetailTextStyle,
                    detail));
            }
        }

        return contentRegions;
    }

    private static IReadOnlyList<ReportBlock> CreateKpiSummaryBlocks(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        return rows
            .Select((row, index) =>
                CreateSingleTextBlock(
                    $"source-{sourceId}-kpi-{index + 1}",
                    BlockType.Detail,
                    EmphasisTextStyle,
                    $"{GetRowValue(row, "Metric")}: {GetRowValue(row, "Value")} ({GetRowValue(row, "Trend")})"))
            .ToList();
    }

    private static ReportBlock CreateRegionalPerformanceTableBlock(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var columns = new[]
        {
            new TableColumn { Width = 140f },
            new TableColumn { Width = 140f },
            new TableColumn { Width = 140f },
            new TableColumn { Width = 120f }
        };

        var tableRegion = ReportLayoutBuilder.CreateTableRegion(
            regionId: $"source-{sourceId}-table",
            regionStyle: new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBlockBottomSpacing)
            },
            tableId: $"source-{sourceId}-table-element",
            tableStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            columns: columns,
            blockType: BlockType.Detail,
            repeatHeaders: true);

        tableRegion.AddHeaderRow(
            rowId: $"source-{sourceId}-header-row",
            rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            cellStyle: TableHeaderCellStyle,
            values: ["Region", "Quota", "Actual", "Attainment"]);

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];

            tableRegion.AddDataRow(
                rowId: $"source-{sourceId}-row-{rowIndex + 1}",
                rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                cellStyle: TableCellStyle,
                values:
                [
                    GetRowValue(row, "Region"),
                    GetRowValue(row, "Quota"),
                    GetRowValue(row, "Actual"),
                    GetRowValue(row, "Attainment")
                ]);
        }

        return tableRegion.BuildBlock();
    }

    private static IReadOnlyList<ReportBlock> CreateStyleShowcaseBlocks(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var contentRegions = new List<ReportBlock>();

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var label = GetRowValue(row, "Label");
            var preview = GetRowValue(row, "Preview");

            contentRegions.Add(CreateSingleTextBlock(
                $"source-{sourceId}-label-{index + 1}",
                BlockType.Detail,
                ResolveShowcaseLabelStyle(label),
                label));

            contentRegions.Add(CreateSingleTextBlock(
                $"source-{sourceId}-preview-{index + 1}",
                BlockType.Detail,
                ResolveShowcasePreviewStyle(label),
                preview));
        }

        return contentRegions;
    }

    private static ReportBlock CreateSalesOrdersTableBlock(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var columns = new[]
        {
            new TableColumn { Width = 120f },
            new TableColumn { Width = 120f },
            new TableColumn { Width = 140f },
            new TableColumn { Width = 120f },
            new TableColumn { Width = 120f }
        };

        var tableRegion = ReportLayoutBuilder.CreateTableRegion(
            regionId: $"source-{sourceId}-table",
            regionStyle: new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBlockBottomSpacing)
            },
            tableId: $"source-{sourceId}-table-element",
            tableStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            columns: columns,
            blockType: BlockType.Detail,
            repeatHeaders: true);

        tableRegion.AddHeaderRow(
            rowId: $"source-{sourceId}-header-row",
            rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            cellStyle: TableHeaderCellStyle,
            values: ["OrderId", "Region", "SalesPerson", "Amount", "Status"]);

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];

            tableRegion.AddDataRow(
                rowId: $"source-{sourceId}-row-{rowIndex + 1}",
                rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                cellStyle: TableCellStyle,
                values:
                [
                    GetRowValue(row, "OrderId"),
                    GetRowValue(row, "Region"),
                    GetRowValue(row, "SalesPerson"),
                    GetRowValue(row, "Amount"),
                    GetRowValue(row, "Status")
                ]);
        }

        return tableRegion.BuildBlock();
    }

    private static ReportBlock CreateExpressionDemoTableBlock(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        IExpressionEvaluator evaluator)
    {
        var expressionRows = new List<IReadOnlyDictionary<string, object?>>(rows.Count);

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var context = new ExpressionContext
            {
                CurrentRow = row,
                Parameters = new Dictionary<string, object?>
                {
                    ["ReportLabel"] = "Expression Demo"
                },
                RowIndex = rowIndex,
                PageNumber = 1
            };

            expressionRows.Add(new Dictionary<string, object?>
            {
                ["Row"] = rowIndex + 1,
                ["{CustomerName}"] = EvaluateToText(evaluator, "{CustomerName}", context),
                ["{Tier}"] = EvaluateToText(evaluator, "{Tier}", context),
                ["{AssignedRep}"] = EvaluateToText(evaluator, "{AssignedRep}", context),
                ["{ReportLabel}"] = EvaluateToText(evaluator, "{ReportLabel}", context),
                ["{DoesNotExist}"] = EvaluateToText(evaluator, "{DoesNotExist}", context),
                ["Literal"] = EvaluateToText(evaluator, "Literal text", context)
            });
        }

        return CreateGenericTableBlock(sourceId, expressionRows);
    }

    private static ReportBlock CreateGenericTableBlock(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var columnNames = rows[0].Keys.ToList();

        var columns = columnNames
            .Select(_ => new TableColumn { MinWidth = 110f, Grow = 1f })
            .ToArray();

        var tableRegion = ReportLayoutBuilder.CreateTableRegion(
            regionId: $"source-{sourceId}-table",
            regionStyle: new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBlockBottomSpacing)
            },
            tableId: $"source-{sourceId}-table-element",
            tableStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            columns: columns,
            blockType: BlockType.Detail,
            repeatHeaders: true);

        tableRegion.AddHeaderRow(
            rowId: $"source-{sourceId}-header-row",
            rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            cellStyle: TableHeaderCellStyle,
            values: columnNames);

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var values = columnNames
                .Select(columnName => GetRowValue(row, columnName))
                .ToList();

            tableRegion.AddDataRow(
                rowId: $"source-{sourceId}-row-{rowIndex + 1}",
                rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                cellStyle: TableCellStyle,
                values: values);
        }

        return tableRegion.BuildBlock();
    }

    private static string GetRowValue(IReadOnlyDictionary<string, object?> row, string fieldName)
    {
        return row.TryGetValue(fieldName, out var value)
            ? FormatValue(value)
            : string.Empty;
    }

    private static string SanitizeForId(string value)
    {
        var chars = value
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();

        return string.Join(string.Empty, chars).Trim('-');
    }

    private static AppliedStyle ResolveShowcaseLabelStyle(string label)
    {
        if (string.Equals(label, "Primary Heading", StringComparison.OrdinalIgnoreCase))
        {
            return new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 18f,
                FontWeight = FontWeight.Bold,
                TextColor = Color.FromRgb(27, 43, 74)
            };
        }

        if (string.Equals(label, "Accent Note", StringComparison.OrdinalIgnoreCase))
        {
            return new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 13f,
                FontWeight = FontWeight.SemiBold,
                TextColor = Color.FromRgb(130, 67, 0)
            };
        }

        return GroupHeaderTextStyle;
    }

    private static AppliedStyle ResolveShowcasePreviewStyle(string label)
    {
        if (string.Equals(label, "Primary Heading", StringComparison.OrdinalIgnoreCase))
        {
            return new AppliedStyle
            {
                FontFamily = "Georgia",
                FontSize = 12f,
                FontStyle = FontStyleValue.Italic,
                TextColor = Color.FromRgb(55, 64, 82)
            };
        }

        if (string.Equals(label, "Accent Note", StringComparison.OrdinalIgnoreCase))
        {
            return new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                TextColor = Color.FromRgb(122, 74, 25)
            };
        }

        return SubtleTextStyle;
    }

    private static AppliedStyle CreateBlockStyle(BlockType kind)
    {
        var baseStyle = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        return kind switch
        {
            BlockType.PageHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, HeaderBlockBottomSpacing)
            },
            BlockType.ReportHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, SectionTitleBlockTopSpacing, 0f, SectionTitleBlockBottomSpacing)
            },
            BlockType.GroupHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 8f, 0f, 4f)
            },
            BlockType.Detail => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, DetailBlockBottomSpacing)
            },
            _ => baseStyle
        };
    }

    private static string EvaluateToText(
        IExpressionEvaluator evaluator,
        string expression,
        ExpressionContext context)
    {
        var result = evaluator.Evaluate(expression, context);
        return FormatValue(result);
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "(null)",
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture),
            decimal amount => amount.ToString("N2", CultureInfo.InvariantCulture),
            double number => number.ToString("N2", CultureInfo.InvariantCulture),
            float number => number.ToString("N2", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }
}
