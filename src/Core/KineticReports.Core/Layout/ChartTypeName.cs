namespace KineticReports.Core.Layout;

/// <summary>
/// Supported built-in chart types for report chart elements.
/// </summary>
public enum ChartTypeName
{
    /// <summary>Vertical bar chart.</summary>
    BarVertical,

    /// <summary>Horizontal bar chart.</summary>
    BarHorizontal,

    /// <summary>Line chart.</summary>
    Line,

    /// <summary>Pie chart.</summary>
    Pie,
}

/// <summary>
/// Helpers for converting chart type names to and from stable string identifiers.
/// </summary>
public static class ChartTypeNames
{
    /// <summary>Converts a typed chart name to its canonical identifier.</summary>
    public static string ToIdentifier(ChartTypeName chartType)
    {
        return chartType switch
        {
            ChartTypeName.BarVertical => "BAR_VERTICAL",
            ChartTypeName.BarHorizontal => "BAR_HORIZONTAL",
            ChartTypeName.Line => "LINE",
            ChartTypeName.Pie => "PIE",
            _ => "BAR_VERTICAL"
        };
    }

    /// <summary>
    /// Attempts to parse a chart type identifier into a typed chart name.
    /// </summary>
    public static bool TryParse(string? identifier, out ChartTypeName parsed)
    {
        parsed = default;

        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        var normalized = identifier.Trim().Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();

        return normalized switch
        {
            "BAR" or "BARCHART" or "VERTICALBAR" or "BARVERTICAL" or "VBAR" => Set(ChartTypeName.BarVertical, out parsed),
            "HORIZONTALBAR" or "BARHORIZONTAL" or "HBAR" => Set(ChartTypeName.BarHorizontal, out parsed),
            "LINE" or "LINECHART" => Set(ChartTypeName.Line, out parsed),
            "PIE" or "PIECHART" => Set(ChartTypeName.Pie, out parsed),
            _ => false
        };
    }

    private static bool Set(ChartTypeName value, out ChartTypeName parsed)
    {
        parsed = value;
        return true;
    }
}
