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

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var text = BuildRowText(rowIndex + 1, row);

                bands.Add(CreateSingleTextBand(
                    $"source-{sourceId}-row-{rowIndex + 1}",
                    BandKind.Detail,
                    DetailTextStyle,
                    text));
            }
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
            Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f },
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

    private static string BuildRowText(int rowNumber, IReadOnlyDictionary<string, object?> row)
    {
        var formattedFields = row.Select(field => $"{field.Key}: {FormatValue(field.Value)}");
        return $"{rowNumber:00}. {string.Join(" | ", formattedFields)}";
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
