namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a chart element in the report layout.
/// Chart-specific rendering is delegated to the renderer implementation
/// via the opaque <see cref="ChartData"/> payload.
/// </summary>
public sealed class ChartElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Chart;

    /// <summary>
    /// Gets the chart type identifier (e.g. "Bar", "Line", "Pie", "Area").
    /// The renderer uses this to select the appropriate drawing strategy.
    /// </summary>
    public required string ChartType { get; init; }

    /// <summary>
    /// Gets the opaque chart data payload produced by the chart provider.
    /// The format is agreed upon between the chart data provider and the renderer.
    /// </summary>
    public object? ChartData { get; init; }

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        DesiredSize = availableSize;
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }
}
