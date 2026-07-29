namespace KineticReports.Core.Layout;

using KineticReports.Core.Styling;

/// <summary>
/// One chart data point.
/// </summary>
public sealed record ChartDataPoint
{
    /// <summary>Gets or inits the data point label.</summary>
    public required string Label { get; init; }

    /// <summary>Gets or inits the numeric value.</summary>
    public required double Value { get; init; }

    /// <summary>Gets or inits an optional color override for this point.</summary>
    public Color? Color { get; init; }
}

/// <summary>
/// Chart data payload used by built-in chart rendering.
/// </summary>
public sealed record ChartSeriesData
{
    /// <summary>Gets or inits the chart points.</summary>
    public IReadOnlyList<ChartDataPoint> Points { get; init; } = [];

    /// <summary>Gets or inits chart rendering options.</summary>
    public ChartOptions Options { get; init; } = new();
}

/// <summary>
/// Chart rendering options that control axes, ticks, labels, and line styling.
/// </summary>
public sealed record ChartOptions
{
    /// <summary>Gets or inits whether axes are rendered for cartesian charts.</summary>
    public bool ShowAxes { get; init; } = true;

    /// <summary>Gets or inits whether grid lines are rendered for cartesian charts.</summary>
    public bool ShowGridLines { get; init; } = true;

    /// <summary>Gets or inits whether axis tick marks are rendered.</summary>
    public bool ShowTicks { get; init; } = true;

    /// <summary>Gets or inits whether axis tick value/category labels are rendered.</summary>
    public bool ShowTickLabels { get; init; } = true;

    /// <summary>Gets or inits optional X-axis label text.</summary>
    public string? XAxisLabel { get; init; }

    /// <summary>Gets or inits optional Y-axis label text.</summary>
    public string? YAxisLabel { get; init; }

    /// <summary>Gets or inits optional minimum Y value for cartesian scaling.</summary>
    public double? MinValue { get; init; }

    /// <summary>Gets or inits optional maximum Y value for cartesian scaling.</summary>
    public double? MaxValue { get; init; }

    /// <summary>Gets or inits desired Y-axis tick count for cartesian charts.</summary>
    public int YAxisTickCount { get; init; } = 5;

    /// <summary>Gets or inits desired X-axis tick count for cartesian charts.</summary>
    public int XAxisTickCount { get; init; } = 0;

    /// <summary>Gets or inits axis line color.</summary>
    public Color AxisColor { get; init; } = new(255, 31, 41, 55);

    /// <summary>Gets or inits grid line color.</summary>
    public Color GridLineColor { get; init; } = new(255, 209, 213, 219);

    /// <summary>Gets or inits axis/tick label text color.</summary>
    public Color LabelColor { get; init; } = new(255, 55, 65, 81);

    /// <summary>Gets or inits axis line width in DIPs.</summary>
    public float AxisLineWidth { get; init; } = 1f;

    /// <summary>Gets or inits grid line width in DIPs.</summary>
    public float GridLineWidth { get; init; } = 1f;

    /// <summary>Gets or inits tick mark length in DIPs.</summary>
    public float TickLength { get; init; } = 4f;

    /// <summary>Gets or inits axis/tick label font size in DIPs.</summary>
    public float LabelFontSize { get; init; } = 9f;

    /// <summary>Gets or inits bar gap ratio (0..0.9) for bar charts.</summary>
    public float BarGapRatio { get; init; } = 0.2f;

    /// <summary>Gets or inits optional line series color override.</summary>
    public Color? LineColor { get; init; }

    /// <summary>Gets or inits line stroke width in DIPs.</summary>
    public float LineWidth { get; init; } = 2f;

    /// <summary>Gets or inits whether line chart markers are rendered.</summary>
    public bool ShowMarkers { get; init; } = true;

    /// <summary>Gets or inits whether pie slice labels are rendered.</summary>
    public bool ShowPieLabels { get; init; } = true;

    /// <summary>Gets or inits an optional pie-slice label color override.</summary>
    public Color? PieLabelColor { get; init; }

    /// <summary>Gets or inits optional pie-slice label font weight override.</summary>
    public FontWeight? PieLabelFontWeight { get; init; }

    /// <summary>Gets or inits whether a legend is rendered for pie charts.</summary>
    public bool ShowLegend { get; init; } = false;

    /// <summary>Gets or inits the legend placement.</summary>
    public PieLegendPosition LegendPosition { get; init; } = PieLegendPosition.Right;

    /// <summary>Gets or inits legend marker size in DIPs.</summary>
    public float LegendMarkerSize { get; init; } = 10f;

    /// <summary>Gets or inits legend font size in DIPs.</summary>
    public float LegendFontSize { get; init; } = 9f;

    /// <summary>Gets or inits optional legend text color override.</summary>
    public Color? LegendTextColor { get; init; }
}

/// <summary>
/// Pie legend placement options.
/// </summary>
public enum PieLegendPosition
{
    /// <summary>Place legend on the right side of the chart.</summary>
    Right,

    /// <summary>Place legend at the bottom of the chart.</summary>
    Bottom,

    /// <summary>Place legend on the left side of the chart.</summary>
    Left,

    /// <summary>Place legend at the top of the chart.</summary>
    Top
}
