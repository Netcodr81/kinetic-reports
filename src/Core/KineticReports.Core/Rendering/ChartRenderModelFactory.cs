namespace KineticReports.Core.Rendering;

using System.Globalization;
using System.Text.Json;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

internal enum ResolvedChartType
{
    BarVertical,
    BarHorizontal,
    Line,
    Pie,
}

internal sealed record ChartPointModel(string Label, double Value, Color Color);

internal sealed record ChartRenderOptions(
    bool ShowAxes,
    bool ShowGridLines,
    bool ShowTicks,
    bool ShowTickLabels,
    string? XAxisLabel,
    string? YAxisLabel,
    double? MinValue,
    double? MaxValue,
    int YAxisTickCount,
    int XAxisTickCount,
    Color AxisColor,
    Color GridLineColor,
    Color LabelColor,
    float AxisLineWidth,
    float GridLineWidth,
    float TickLength,
    float LabelFontSize,
    float BarGapRatio,
    Color? LineColor,
    float LineWidth,
    bool ShowMarkers,
    bool ShowPieLabels,
    Color? PieLabelColor,
    FontWeight? PieLabelFontWeight,
    bool ShowLegend,
    PieLegendPosition LegendPosition,
    float LegendMarkerSize,
    float LegendFontSize,
    Color? LegendTextColor);

internal static class ChartRenderModelFactory
{
    private static readonly Color[] Palette =
    [
        Color.FromRgb(37, 99, 235),
        Color.FromRgb(16, 185, 129),
        Color.FromRgb(245, 158, 11),
        Color.FromRgb(239, 68, 68),
        Color.FromRgb(139, 92, 246),
        Color.FromRgb(20, 184, 166),
    ];

    public static bool TryCreate(
        ChartTypeName? chartTypeValue,
        string? chartType,
        object? chartData,
        out ResolvedChartType resolvedType,
        out IReadOnlyList<ChartPointModel> points,
        out ChartRenderOptions options)
    {
        points = [];
        resolvedType = default;
        options = CreateDefaultOptions();

        if (!TryResolveChartType(chartTypeValue, chartType, out resolvedType))
            return false;

        points = ResolvePoints(chartData, out options);
        return points.Count > 0;
    }

    private static bool TryResolveChartType(ChartTypeName? chartTypeValue, string? chartType, out ResolvedChartType resolved)
    {
        if (chartTypeValue.HasValue)
        {
            resolved = chartTypeValue.Value switch
            {
                ChartTypeName.BarVertical => ResolvedChartType.BarVertical,
                ChartTypeName.BarHorizontal => ResolvedChartType.BarHorizontal,
                ChartTypeName.Line => ResolvedChartType.Line,
                ChartTypeName.Pie => ResolvedChartType.Pie,
                _ => ResolvedChartType.BarVertical
            };

            return true;
        }

        if (!ChartTypeNames.TryParse(chartType, out var parsed))
        {
            resolved = default;
            return false;
        }

        return TryResolveChartType(parsed, null, out resolved);
    }

    private static IReadOnlyList<ChartPointModel> ResolvePoints(object? chartData, out ChartRenderOptions options)
    {
        options = CreateDefaultOptions();

        if (chartData is null)
            return [];

        if (chartData is ChartSeriesData typed)
        {
            options = FromChartOptions(typed.Options);
            return FromPoints(typed.Points);
        }

        if (chartData is IReadOnlyList<ChartDataPoint> typedList)
            return FromPoints(typedList);

        if (chartData is IEnumerable<ChartDataPoint> typedEnumerable)
            return FromPoints(typedEnumerable.ToList());

        if (chartData is JsonElement json)
            return FromJson(json, out options);

        if (TryGetPropertyValue(chartData, "Points", out var pointsObj)
            || TryGetPropertyValue(chartData, "Data", out pointsObj))
        {
            if (TryGetPropertyValue(chartData, "Options", out var reflectedOptions))
                options = ResolveReflectedOptions(reflectedOptions);

            var nested = ResolvePoints(pointsObj, out var nestedOptions);
            if (nested.Count > 0)
            {
                if (TryGetPropertyValue(chartData, "Options", out _))
                {
                    // Keep explicitly supplied parent options.
                }
                else
                {
                    options = nestedOptions;
                }

                return nested;
            }
        }

        if (TryGetPropertyValue(chartData, "Series", out var seriesObj)
            && TryGetPropertyValue(chartData, "Values", out var valuesObj)
            && TryZipSeriesAndValues(seriesObj, valuesObj, out var zipped))
        {
            if (TryGetPropertyValue(chartData, "Options", out var reflectedOptions))
                options = ResolveReflectedOptions(reflectedOptions);
            return zipped;
        }

        return [];
    }

    private static IReadOnlyList<ChartPointModel> FromPoints(IReadOnlyList<ChartDataPoint> points)
    {
        if (points.Count == 0)
            return [];

        var output = new List<ChartPointModel>(points.Count);
        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            output.Add(new ChartPointModel(
                string.IsNullOrWhiteSpace(point.Label) ? $"Series {i + 1}" : point.Label,
                point.Value,
                point.Color ?? Palette[i % Palette.Length]));
        }

        return output;
    }

    private static IReadOnlyList<ChartPointModel> FromJson(JsonElement json, out ChartRenderOptions options)
    {
        options = CreateDefaultOptions();

        if (json.ValueKind == JsonValueKind.Object)
        {
            if (json.TryGetProperty("options", out var optionsJson) && optionsJson.ValueKind == JsonValueKind.Object)
                options = ResolveJsonOptions(optionsJson);

            if (json.TryGetProperty("points", out var points))
                return FromJson(points, out _);

            if (json.TryGetProperty("series", out var series)
                && json.TryGetProperty("values", out var values)
                && TryZipSeriesAndValues(series, values, out var zipped))
            {
                return zipped;
            }

            return [];
        }

        if (json.ValueKind != JsonValueKind.Array)
            return [];

        var output = new List<ChartPointModel>();
        var index = 0;

        foreach (var element in json.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
                continue;

            var label = element.TryGetProperty("label", out var labelValue)
                ? labelValue.GetString()
                : null;

            var value = element.TryGetProperty("value", out var valueElement)
                ? GetDouble(valueElement)
                : 0d;

            Color? color = null;
            if (element.TryGetProperty("color", out var colorElement)
                && colorElement.ValueKind == JsonValueKind.String)
            {
                color = TryParseHexColor(colorElement.GetString());
            }

            output.Add(new ChartPointModel(
                string.IsNullOrWhiteSpace(label) ? $"Series {index + 1}" : label!,
                value,
                color ?? Palette[index % Palette.Length]));

            index++;
        }

        return output;
    }

    private static bool TryZipSeriesAndValues(object seriesObj, object valuesObj, out IReadOnlyList<ChartPointModel> points)
    {
        points = [];

        var labels = ToStringList(seriesObj);
        var values = ToDoubleList(valuesObj);

        if (labels.Count == 0 || values.Count == 0)
            return false;

        var count = Math.Min(labels.Count, values.Count);
        if (count <= 0)
            return false;

        var output = new List<ChartPointModel>(count);
        for (var i = 0; i < count; i++)
        {
            output.Add(new ChartPointModel(labels[i], values[i], Palette[i % Palette.Length]));
        }

        points = output;
        return true;
    }

    private static bool TryZipSeriesAndValues(JsonElement series, JsonElement values, out IReadOnlyList<ChartPointModel> points)
    {
        points = [];

        var labels = ToStringList(series);
        var numericValues = ToDoubleList(values);

        if (labels.Count == 0 || numericValues.Count == 0)
            return false;

        var count = Math.Min(labels.Count, numericValues.Count);
        if (count <= 0)
            return false;

        var output = new List<ChartPointModel>(count);
        for (var i = 0; i < count; i++)
        {
            output.Add(new ChartPointModel(labels[i], numericValues[i], Palette[i % Palette.Length]));
        }

        points = output;
        return true;
    }

    private static bool TryGetPropertyValue(object source, string name, out object value)
    {
        value = null!;

        var property = source.GetType().GetProperty(name);
        if (property is null)
            return false;

        var propertyValue = property.GetValue(source);
        if (propertyValue is null)
            return false;

        value = propertyValue;
        return true;
    }

    private static IReadOnlyList<string> ToStringList(object value)
    {
        if (value is JsonElement json)
            return ToStringList(json);

        if (value is IEnumerable<string> strings)
            return strings.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();

        if (value is IEnumerable<object?> objectValues)
        {
            return objectValues
                .Select(item => Convert.ToString(item, CultureInfo.InvariantCulture) ?? string.Empty)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .ToList();
        }

        return [];
    }

    private static IReadOnlyList<string> ToStringList(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
            return [];

        var output = new List<string>();
        foreach (var item in value.EnumerateArray())
        {
            var text = item.ValueKind == JsonValueKind.String
                ? item.GetString()
                : Convert.ToString(item.ToString(), CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(text))
                output.Add(text.Trim());
        }

        return output;
    }

    private static IReadOnlyList<double> ToDoubleList(object value)
    {
        if (value is JsonElement json)
            return ToDoubleList(json);

        if (value is IEnumerable<double> doubles)
            return doubles.ToList();

        if (value is IEnumerable<float> floats)
            return floats.Select(item => (double)item).ToList();

        if (value is IEnumerable<int> ints)
            return ints.Select(item => (double)item).ToList();

        if (value is IEnumerable<long> longs)
            return longs.Select(item => (double)item).ToList();

        if (value is IEnumerable<object?> objectValues)
        {
            var output = new List<double>();
            foreach (var item in objectValues)
            {
                if (item is null)
                    continue;

                if (double.TryParse(Convert.ToString(item, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    output.Add(parsed);
            }

            return output;
        }

        return [];
    }

    private static IReadOnlyList<double> ToDoubleList(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
            return [];

        var output = new List<double>();
        foreach (var item in value.EnumerateArray())
        {
            output.Add(GetDouble(item));
        }

        return output;
    }

    private static double GetDouble(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.String when double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 0d
        };
    }

    private static Color? TryParseHexColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (!trimmed.StartsWith('#'))
            return null;

        var hex = trimmed[1..];

        if (hex.Length == 6
            && byte.TryParse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r)
            && byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g)
            && byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
        {
            return Color.FromRgb(r, g, b);
        }

        if (hex.Length == 8
            && byte.TryParse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a)
            && byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r)
            && byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g)
            && byte.TryParse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
        {
            return new Color(a, r, g, b);
        }

        return null;
    }

    private static ChartRenderOptions ResolveJsonOptions(JsonElement options)
    {
        var defaults = CreateDefaultOptions();

        return new ChartRenderOptions(
            ShowAxes: GetBool(options, "showAxes") ?? defaults.ShowAxes,
            ShowGridLines: GetBool(options, "showGridLines") ?? defaults.ShowGridLines,
            ShowTicks: GetBool(options, "showTicks") ?? defaults.ShowTicks,
            ShowTickLabels: GetBool(options, "showTickLabels") ?? defaults.ShowTickLabels,
            XAxisLabel: GetString(options, "xAxisLabel") ?? defaults.XAxisLabel,
            YAxisLabel: GetString(options, "yAxisLabel") ?? defaults.YAxisLabel,
            MinValue: GetDoubleOrNull(options, "minValue") ?? defaults.MinValue,
            MaxValue: GetDoubleOrNull(options, "maxValue") ?? defaults.MaxValue,
            YAxisTickCount: GetInt(options, "yAxisTickCount") ?? defaults.YAxisTickCount,
            XAxisTickCount: GetInt(options, "xAxisTickCount") ?? defaults.XAxisTickCount,
            AxisColor: GetColor(options, "axisColor") ?? defaults.AxisColor,
            GridLineColor: GetColor(options, "gridLineColor") ?? defaults.GridLineColor,
            LabelColor: GetColor(options, "labelColor") ?? defaults.LabelColor,
            AxisLineWidth: GetFloat(options, "axisLineWidth") ?? defaults.AxisLineWidth,
            GridLineWidth: GetFloat(options, "gridLineWidth") ?? defaults.GridLineWidth,
            TickLength: GetFloat(options, "tickLength") ?? defaults.TickLength,
            LabelFontSize: GetFloat(options, "labelFontSize") ?? defaults.LabelFontSize,
            BarGapRatio: GetFloat(options, "barGapRatio") ?? defaults.BarGapRatio,
            LineColor: GetColor(options, "lineColor") ?? defaults.LineColor,
            LineWidth: GetFloat(options, "lineWidth") ?? defaults.LineWidth,
            ShowMarkers: GetBool(options, "showMarkers") ?? defaults.ShowMarkers,
            ShowPieLabels: GetBool(options, "showPieLabels") ?? defaults.ShowPieLabels,
            PieLabelColor: GetColor(options, "pieLabelColor") ?? defaults.PieLabelColor,
            PieLabelFontWeight: GetFontWeight(options, "pieLabelFontWeight") ?? defaults.PieLabelFontWeight,
            ShowLegend: GetBool(options, "showLegend") ?? defaults.ShowLegend,
            LegendPosition: GetLegendPosition(options, "legendPosition") ?? defaults.LegendPosition,
            LegendMarkerSize: GetFloat(options, "legendMarkerSize") ?? defaults.LegendMarkerSize,
            LegendFontSize: GetFloat(options, "legendFontSize") ?? defaults.LegendFontSize,
            LegendTextColor: GetColor(options, "legendTextColor") ?? defaults.LegendTextColor);
    }

    private static ChartRenderOptions ResolveReflectedOptions(object value)
    {
        if (value is ChartOptions typed)
            return FromChartOptions(typed);

        if (value is JsonElement json && json.ValueKind == JsonValueKind.Object)
            return ResolveJsonOptions(json);

        return CreateDefaultOptions();
    }

    private static ChartRenderOptions FromChartOptions(ChartOptions options)
    {
        return new ChartRenderOptions(
            ShowAxes: options.ShowAxes,
            ShowGridLines: options.ShowGridLines,
            ShowTicks: options.ShowTicks,
            ShowTickLabels: options.ShowTickLabels,
            XAxisLabel: options.XAxisLabel,
            YAxisLabel: options.YAxisLabel,
            MinValue: options.MinValue,
            MaxValue: options.MaxValue,
            YAxisTickCount: options.YAxisTickCount,
            XAxisTickCount: options.XAxisTickCount,
            AxisColor: options.AxisColor,
            GridLineColor: options.GridLineColor,
            LabelColor: options.LabelColor,
            AxisLineWidth: options.AxisLineWidth,
            GridLineWidth: options.GridLineWidth,
            TickLength: options.TickLength,
            LabelFontSize: options.LabelFontSize,
            BarGapRatio: options.BarGapRatio,
            LineColor: options.LineColor,
            LineWidth: options.LineWidth,
            ShowMarkers: options.ShowMarkers,
            ShowPieLabels: options.ShowPieLabels,
            PieLabelColor: options.PieLabelColor,
            PieLabelFontWeight: options.PieLabelFontWeight,
            ShowLegend: options.ShowLegend,
            LegendPosition: options.LegendPosition,
            LegendMarkerSize: options.LegendMarkerSize,
            LegendFontSize: options.LegendFontSize,
            LegendTextColor: options.LegendTextColor);
    }

    private static ChartRenderOptions CreateDefaultOptions() => new(
        ShowAxes: true,
        ShowGridLines: true,
        ShowTicks: true,
        ShowTickLabels: true,
        XAxisLabel: null,
        YAxisLabel: null,
        MinValue: null,
        MaxValue: null,
        YAxisTickCount: 5,
        XAxisTickCount: 0,
        AxisColor: new Color(255, 31, 41, 55),
        GridLineColor: new Color(255, 209, 213, 219),
        LabelColor: new Color(255, 55, 65, 81),
        AxisLineWidth: 1f,
        GridLineWidth: 1f,
        TickLength: 4f,
        LabelFontSize: 9f,
        BarGapRatio: 0.2f,
        LineColor: null,
        LineWidth: 2f,
        ShowMarkers: true,
        ShowPieLabels: true,
        PieLabelColor: null,
        PieLabelFontWeight: null,
        ShowLegend: false,
        LegendPosition: PieLegendPosition.Right,
        LegendMarkerSize: 10f,
        LegendFontSize: 9f,
        LegendTextColor: null);

    private static string? GetString(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool? GetBool(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            ? value.GetBoolean()
            : null;

    private static int? GetInt(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsed)
            ? parsed
            : null;

    private static float? GetFloat(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetSingle()
            : null;

    private static double? GetDoubleOrNull(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;

    private static Color? GetColor(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
            return null;

        if (value.ValueKind == JsonValueKind.String)
            return TryParseHexColor(value.GetString());

        if (value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("a", out var a)
            && value.TryGetProperty("r", out var r)
            && value.TryGetProperty("g", out var g)
            && value.TryGetProperty("b", out var b)
            && a.TryGetByte(out var av)
            && r.TryGetByte(out var rv)
            && g.TryGetByte(out var gv)
            && b.TryGetByte(out var bv))
        {
            return new Color(av, rv, gv, bv);
        }

        return null;
    }

    private static FontWeight? GetFontWeight(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
            return null;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var numeric)
            && Enum.IsDefined(typeof(FontWeight), numeric))
        {
            return (FontWeight)numeric;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedNumeric)
                && Enum.IsDefined(typeof(FontWeight), parsedNumeric))
            {
                return (FontWeight)parsedNumeric;
            }

            if (Enum.TryParse<FontWeight>(text, ignoreCase: true, out var parsedEnum))
                return parsedEnum;
        }

        return null;
    }

    private static PieLegendPosition? GetLegendPosition(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.String)
            return null;

        var text = value.GetString();
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return Enum.TryParse<PieLegendPosition>(text, ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }
}
