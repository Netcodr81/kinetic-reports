namespace KineticReports.Samples.Blazor.Services;

using System.Globalization;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Engine.Building;
using KineticReports.Engine.Data;
using KineticReports.Engine.Expressions;

/// <summary>
/// Builds simple, data-backed report bands for the Blazor sample reports.
/// </summary>
internal sealed class SampleReportBuilder : IReportBuilder
{
    private const float HeaderBandBottomSpacing = 12f;
    private const float SectionTitleBandTopSpacing = 4f;
    private const float SectionTitleBandBottomSpacing = 8f;
    private const float DetailBandBottomSpacing = 6f;
    private const float TableBandBottomSpacing = 12f;

    private static readonly ResolvedStyle HeaderTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 20f,
        FontWeight = FontWeight.Bold
    };

    private static readonly ResolvedStyle SectionTitleStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 14f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(36, 70, 140)
    };

    private static readonly ResolvedStyle DetailTextStyle = new()
    {
        FontFamily = "Consolas",
        FontSize = 11f,
        LineHeight = 1.45f
    };

    private static readonly ResolvedStyle GroupHeaderTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 12f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(31, 58, 111)
    };

    private static readonly ResolvedStyle EmphasisTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 12f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(44, 94, 69)
    };

    private static readonly ResolvedStyle SubtleTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 11f,
        TextColor = Color.FromRgb(80, 88, 102)
    };

    private static readonly ResolvedStyle TableHeaderCellStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 11f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(255, 255, 255),
        Background = Color.FromRgb(31, 58, 111),
        Padding = new Core.Geometry.Thickness(6f, 8f, 6f, 8f),
        Border = Border.Uniform(1f, Color.FromRgb(199, 210, 230))
    };

    private static readonly ResolvedStyle TableCellStyle = new()
    {
        FontFamily = "Consolas",
        FontSize = 11f,
        TextColor = Color.FromRgb(28, 32, 42),
        Padding = new Core.Geometry.Thickness(5f, 8f, 5f, 8f),
        Border = Border.Uniform(1f, Color.FromRgb(220, 226, 239))
    };

    /// <inheritdoc/>
    public IReadOnlyList<BandElement> Build(DataContext dataContext, IExpressionEvaluator evaluator)
    {
        var bands = new List<BandElement>
        {
            CreateSingleTextBand(
                "sample-header",
                BandKind.PageHeader,
                HeaderTextStyle,
                "KineticReports Sample Data Preview")
        };

        foreach (var dataSource in dataContext.DataSources)
        {
            var sourceId = dataSource.Key;
            var rows = dataSource.Value;

            bands.Add(CreateSingleTextBand(
                $"source-{sourceId}-title",
                BandKind.ReportHeader,
                SectionTitleStyle,
                $"Data Source: {sourceId} ({rows.Count} row(s))"));

            if (rows.Count == 0)
            {
                bands.Add(CreateSingleTextBand(
                    $"source-{sourceId}-empty",
                    BandKind.Detail,
                    DetailTextStyle,
                    "No rows returned."));

                continue;
            }

            if (string.Equals(sourceId, "sales-orders", StringComparison.OrdinalIgnoreCase))
            {
                bands.Add(CreateSalesOrdersTableBand(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "quote-daily", StringComparison.OrdinalIgnoreCase))
            {
                bands.AddRange(CreateQuoteDailyBands(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "customer-activity", StringComparison.OrdinalIgnoreCase))
            {
                bands.AddRange(CreateCustomerActivityGroupedBands(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "kpi-summary", StringComparison.OrdinalIgnoreCase))
            {
                bands.AddRange(CreateKpiSummaryBands(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "regional-performance", StringComparison.OrdinalIgnoreCase))
            {
                bands.Add(CreateRegionalPerformanceTableBand(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "style-showcase", StringComparison.OrdinalIgnoreCase))
            {
                bands.AddRange(CreateStyleShowcaseBands(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "expression-demo", StringComparison.OrdinalIgnoreCase))
            {
                bands.Add(CreateExpressionDemoTableBand(sourceId, rows, evaluator));

                continue;
            }

            bands.Add(CreateGenericTableBand(sourceId, rows));
        }

        if (dataContext.DataSources.Count == 0)
        {
            bands.Add(CreateSingleTextBand(
                "sample-empty",
                BandKind.Detail,
                DetailTextStyle,
                "No data sources were resolved for this report."));
        }

        return bands;
    }

    private static BandElement CreateSingleTextBand(
        string id,
        BandKind kind,
        ResolvedStyle textStyle,
        string text)
    {
        return new BandElement
        {
            Id = id,
            Kind = kind,
            Style = CreateBandStyle(kind),
            Children =
            [
                new TextElement
                {
                    Id = $"{id}-text",
                    Style = textStyle,
                    Text = text
                }
            ]
        };
    }

    private static IReadOnlyList<BandElement> CreateQuoteDailyBands(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var row = rows[0];

        return
        [
            CreateSingleTextBand(
                $"source-{sourceId}-heading",
                BandKind.Detail,
                HeaderTextStyle,
                GetRowValue(row, "Heading")),
            CreateSingleTextBand(
                $"source-{sourceId}-quote",
                BandKind.Detail,
                EmphasisTextStyle,
                GetRowValue(row, "Quote")),
            CreateSingleTextBand(
                $"source-{sourceId}-meta",
                BandKind.Detail,
                SubtleTextStyle,
                $"Generated: {GetRowValue(row, "GeneratedOn")} | Author: {GetRowValue(row, "Author")}")
        ];
    }

    private static IReadOnlyList<BandElement> CreateCustomerActivityGroupedBands(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var bands = new List<BandElement>();

        var groupedRows = rows
            .GroupBy(row => GetRowValue(row, "Tier"), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in groupedRows)
        {
            bands.Add(CreateSingleTextBand(
                $"source-{sourceId}-group-{SanitizeForId(group.Key)}",
                BandKind.GroupHeader,
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

                bands.Add(CreateSingleTextBand(
                    $"source-{sourceId}-{SanitizeForId(group.Key)}-row-{index + 1}",
                    BandKind.Detail,
                    DetailTextStyle,
                    detail));
            }
        }

        return bands;
    }

    private static IReadOnlyList<BandElement> CreateKpiSummaryBands(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        return rows
            .Select((row, index) =>
                CreateSingleTextBand(
                    $"source-{sourceId}-kpi-{index + 1}",
                    BandKind.Detail,
                    EmphasisTextStyle,
                    $"{GetRowValue(row, "Metric")}: {GetRowValue(row, "Value")} ({GetRowValue(row, "Trend")})"))
            .ToList();
    }

    private static BandElement CreateRegionalPerformanceTableBand(
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

        var headerRow = new RowElement
        {
            Id = $"source-{sourceId}-header-row",
            Kind = RowKind.Header,
            Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
            Cells =
            [
                CreateTableCell($"source-{sourceId}-header-region", 0, "Region", TableHeaderCellStyle),
                CreateTableCell($"source-{sourceId}-header-quota", 1, "Quota", TableHeaderCellStyle),
                CreateTableCell($"source-{sourceId}-header-actual", 2, "Actual", TableHeaderCellStyle),
                CreateTableCell($"source-{sourceId}-header-attainment", 3, "Attainment", TableHeaderCellStyle)
            ]
        };

        var dataRows = rows
            .Select((row, rowIndex) =>
                new RowElement
                {
                    Id = $"source-{sourceId}-row-{rowIndex + 1}",
                    Kind = RowKind.Data,
                    Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
                    Cells =
                    [
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-region", 0, GetRowValue(row, "Region"), TableCellStyle),
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-quota", 1, GetRowValue(row, "Quota"), TableCellStyle),
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-actual", 2, GetRowValue(row, "Actual"), TableCellStyle),
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-attainment", 3, GetRowValue(row, "Attainment"), TableCellStyle)
                    ]
                })
            .ToList();

        var tableRows = new List<RowElement> { headerRow };
        tableRows.AddRange(dataRows);

        return new BandElement
        {
            Id = $"source-{sourceId}-table",
            Kind = BandKind.Detail,
            Style = new ResolvedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBandBottomSpacing)
            },
            Children =
            [
                new TableElement
                {
                    Id = $"source-{sourceId}-table-element",
                    Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
                    Columns = columns,
                    Rows = tableRows,
                    RepeatHeaders = true
                }
            ]
        };
    }

    private static IReadOnlyList<BandElement> CreateStyleShowcaseBands(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var bands = new List<BandElement>();

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var label = GetRowValue(row, "Label");
            var preview = GetRowValue(row, "Preview");

            bands.Add(CreateSingleTextBand(
                $"source-{sourceId}-label-{index + 1}",
                BandKind.Detail,
                ResolveShowcaseLabelStyle(label),
                label));

            bands.Add(CreateSingleTextBand(
                $"source-{sourceId}-preview-{index + 1}",
                BandKind.Detail,
                ResolveShowcasePreviewStyle(label),
                preview));
        }

        return bands;
    }

    private static BandElement CreateSalesOrdersTableBand(
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

        var headerRow = new RowElement
        {
            Id = $"source-{sourceId}-header-row",
            Kind = RowKind.Header,
            Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
            Cells =
            [
                CreateTableCell($"source-{sourceId}-header-order-id", 0, "OrderId", TableHeaderCellStyle),
                CreateTableCell($"source-{sourceId}-header-region", 1, "Region", TableHeaderCellStyle),
                CreateTableCell($"source-{sourceId}-header-sales-person", 2, "SalesPerson", TableHeaderCellStyle),
                CreateTableCell($"source-{sourceId}-header-amount", 3, "Amount", TableHeaderCellStyle),
                CreateTableCell($"source-{sourceId}-header-status", 4, "Status", TableHeaderCellStyle)
            ]
        };

        var dataRows = rows
            .Select((row, rowIndex) =>
                new RowElement
                {
                    Id = $"source-{sourceId}-row-{rowIndex + 1}",
                    Kind = RowKind.Data,
                    Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
                    Cells =
                    [
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-order-id", 0, GetRowValue(row, "OrderId"), TableCellStyle),
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-region", 1, GetRowValue(row, "Region"), TableCellStyle),
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-sales-person", 2, GetRowValue(row, "SalesPerson"), TableCellStyle),
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-amount", 3, GetRowValue(row, "Amount"), TableCellStyle),
                        CreateTableCell($"source-{sourceId}-row-{rowIndex + 1}-status", 4, GetRowValue(row, "Status"), TableCellStyle)
                    ]
                })
            .ToList();

        var tableRows = new List<RowElement> { headerRow };
        tableRows.AddRange(dataRows);

        return new BandElement
        {
            Id = $"source-{sourceId}-table",
            Kind = BandKind.Detail,
            Style = new ResolvedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBandBottomSpacing)
            },
            Children =
            [
                new TableElement
                {
                    Id = $"source-{sourceId}-table-element",
                    Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
                    Columns = columns,
                    Rows = tableRows,
                    RepeatHeaders = true
                }
            ]
        };
    }

    private static BandElement CreateExpressionDemoTableBand(
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

        return CreateGenericTableBand(sourceId, expressionRows);
    }

    private static BandElement CreateGenericTableBand(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var columnNames = rows[0].Keys.ToList();

        var columns = columnNames
            .Select(_ => new TableColumn { MinWidth = 110f, Grow = 1f })
            .ToArray();

        var headerRow = new RowElement
        {
            Id = $"source-{sourceId}-header-row",
            Kind = RowKind.Header,
            Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
            Cells = columnNames
                .Select((columnName, columnIndex) =>
                    CreateTableCell(
                        $"source-{sourceId}-header-{SanitizeForId(columnName)}",
                        columnIndex,
                        columnName,
                        TableHeaderCellStyle))
                .ToList()
        };

        var dataRows = rows
            .Select((row, rowIndex) =>
                new RowElement
                {
                    Id = $"source-{sourceId}-row-{rowIndex + 1}",
                    Kind = RowKind.Data,
                    Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
                    Cells = columnNames
                        .Select((columnName, columnIndex) =>
                            CreateTableCell(
                                $"source-{sourceId}-row-{rowIndex + 1}-{SanitizeForId(columnName)}",
                                columnIndex,
                                GetRowValue(row, columnName),
                                TableCellStyle))
                        .ToList()
                })
            .ToList();

        var tableRows = new List<RowElement> { headerRow };
        tableRows.AddRange(dataRows);

        return new BandElement
        {
            Id = $"source-{sourceId}-table",
            Kind = BandKind.Detail,
            Style = new ResolvedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBandBottomSpacing)
            },
            Children =
            [
                new TableElement
                {
                    Id = $"source-{sourceId}-table-element",
                    Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 11f },
                    Columns = columns,
                    Rows = tableRows,
                    RepeatHeaders = true
                }
            ]
        };
    }

    private static CellElement CreateTableCell(string id, int columnIndex, string text, ResolvedStyle style)
    {
        var textStyle = style with
        {
            Background = Color.Transparent,
            Border = null,
            Padding = Core.Geometry.Thickness.Zero,
            Margin = Core.Geometry.Thickness.Zero,
            Overflow = Overflow.Visible
        };

        return new CellElement
        {
            Id = id,
            ColumnIndex = columnIndex,
            Style = style,
            Children =
            [
                new TextElement
                {
                    Id = $"{id}-text",
                    Style = textStyle,
                    Text = text
                }
            ]
        };
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

    private static ResolvedStyle ResolveShowcaseLabelStyle(string label)
    {
        if (string.Equals(label, "Primary Heading", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedStyle
            {
                FontFamily = "Arial",
                FontSize = 18f,
                FontWeight = FontWeight.Bold,
                TextColor = Color.FromRgb(27, 43, 74)
            };
        }

        if (string.Equals(label, "Accent Note", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedStyle
            {
                FontFamily = "Arial",
                FontSize = 13f,
                FontWeight = FontWeight.SemiBold,
                TextColor = Color.FromRgb(130, 67, 0)
            };
        }

        return GroupHeaderTextStyle;
    }

    private static ResolvedStyle ResolveShowcasePreviewStyle(string label)
    {
        if (string.Equals(label, "Primary Heading", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedStyle
            {
                FontFamily = "Georgia",
                FontSize = 12f,
                FontStyle = FontStyleValue.Italic,
                TextColor = Color.FromRgb(55, 64, 82)
            };
        }

        if (string.Equals(label, "Accent Note", StringComparison.OrdinalIgnoreCase))
        {
            return new ResolvedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                TextColor = Color.FromRgb(122, 74, 25)
            };
        }

        return SubtleTextStyle;
    }

    private static ResolvedStyle CreateBandStyle(BandKind kind)
    {
        var baseStyle = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f };

        return kind switch
        {
            BandKind.PageHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, HeaderBandBottomSpacing)
            },
            BandKind.ReportHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, SectionTitleBandTopSpacing, 0f, SectionTitleBandBottomSpacing)
            },
            BandKind.GroupHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 8f, 0f, 4f)
            },
            BandKind.Detail => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, DetailBandBottomSpacing)
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
