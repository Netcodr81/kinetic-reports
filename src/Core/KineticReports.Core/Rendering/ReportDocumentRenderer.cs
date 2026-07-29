namespace KineticReports.Core.Rendering;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

/// <summary>
/// Walks an immutable <see cref="ReportDocument"/> in deterministic rendering order and
/// translates every element into <see cref="IGraphicsContext"/> draw calls.
/// </summary>
/// <remarks>
/// Rendering order per spec §3:
/// page background → header → body children (depth-first) → footer.
/// Within each element: background fill → border → content → children.
/// Renderers never perform layout (ADR-013).
/// </remarks>
public sealed class ReportDocumentRenderer
{
    /// <summary>
    /// Renders a single <see cref="PageBlock"/> onto <paramref name="context"/>.
    /// </summary>
    /// <param name="page">The fully-arranged page to render.</param>
    /// <param name="context">The backend drawing surface.</param>
    /// <param name="options">Render configuration (background color, DPI, etc.).</param>
    public void RenderPage(PageBlock page, IGraphicsContext context, RenderOptions options)
    {
        // Page background fills the full page bounds.
        context.FillRectangle(page.Bounds, options.Background);

        // Header
        if (page.Header is not null)
            RenderElement(page.Header, context, page.PageNumber);

        // Body children
        foreach (var child in page.Children)
            RenderElement(child, context, page.PageNumber);

        // Footer
        if (page.Footer is not null)
            RenderElement(page.Footer, context, page.PageNumber);
    }

    // -------------------------------------------------------------------------
    // Core traversal
    // -------------------------------------------------------------------------

    private void RenderElement(LayoutBlock element, IGraphicsContext context, int pageNumber)
    {
        bool pushOpacity = element.Style.Opacity < 1f;
        bool pushClip = element.Style.Overflow == Overflow.Hidden;

        // Background
        if (element.Style.Background != Color.Transparent)
            context.FillRectangle(element.Bounds, element.Style.Background);

        // Border sides
        RenderBorder(element.Bounds, element.Style.Border, context);

        // Clip before rendering children/content
        if (pushClip) context.PushClip(element.Bounds);
        if (pushOpacity) context.PushOpacity(element.Style.Opacity);

        // Dispatch by element type
        switch (element)
        {
            // Unified ContentBlock dispatch (handles Text, Image, Shape, Barcode, Chart, Container, etc.)
            case ContentBlock content:
                RenderContentBlock(content, context, pageNumber);
                break;

            case TextBlock text:
                foreach (var run in text.TextRuns)
                    context.DrawText(ResolveSystemTextTokens(run, pageNumber));
                break;

            case ImageBlock image when image.ImageReference is not null:
                context.DrawImage(image.Bounds, image.ImageReference, image.Stretch);
                break;

            case ShapeBlock shape:
                RenderShape(shape, context);
                break;

            case ChartBlock chart:
                RenderChart(chart.ChartTypeValue, chart.ChartType, chart.ChartData, chart.Bounds, chart.Style, context);
                break;

            case BarcodeBlock barcode:
                RenderBarcodeBlock(barcode, context, pageNumber);
                break;

            case ContainerBlock container:
                foreach (var child in container.Children)
                    RenderElement(child, context, pageNumber);
                break;

            case PageSectionBlock section:
                foreach (var child in section.Children)
                    RenderElement(child, context, pageNumber);
                break;

            case ReportBlock block:
                foreach (var child in block.Children)
                    RenderElement(child, context, pageNumber);
                break;

            case TableBlock table:
                foreach (var row in table.Rows)
                    RenderElement(row, context, pageNumber);
                break;

            case RowBlock row:
                foreach (var cell in row.Cells)
                    RenderElement(cell, context, pageNumber);
                break;

            case CellBlock cell:
                foreach (var child in cell.Children)
                    RenderElement(child, context, pageNumber);
                break;
        }

        if (pushOpacity) context.PopOpacity();
        if (pushClip) context.PopClip();
    }

    // -------------------------------------------------------------------------
    // ContentBlock rendering (unified dispatch for Text, Image, Shape, Barcode, Chart, Container)
    // -------------------------------------------------------------------------

    private void RenderContentBlock(ContentBlock content, IGraphicsContext context, int pageNumber)
    {
        switch (content.ContentType)
        {
            case BlockContentType.Text:
                foreach (var run in content.TextRuns)
                    context.DrawText(ResolveSystemTextTokens(run, pageNumber));
                break;

            case BlockContentType.Image:
                if (TryCreateImageReference(content, out var imageReference))
                    context.DrawImage(content.Bounds, imageReference, content.Stretch);
                break;

            case BlockContentType.Shape:
                RenderShape(content, context);
                break;

            case BlockContentType.Chart:
                RenderChart(content.ChartTypeValue, content.ChartType, content.ChartData, content.Bounds, content.Style, context);
                break;

            case BlockContentType.Barcode:
                RenderContentBarcode(content, context, pageNumber);
                break;

            case BlockContentType.Container:
            case BlockContentType.ReportSection:
            case BlockContentType.PageSection:
            case BlockContentType.Page:
            case BlockContentType.Table:
            case BlockContentType.Row:
                // Container-like blocks: render all children
                foreach (var child in content.Children)
                    RenderElement(child, context, pageNumber);
                break;

            case BlockContentType.Cell:
                // Cell blocks: render all children
                foreach (var child in content.Children)
                    RenderElement(child, context, pageNumber);
                break;
        }
    }

    private static void RenderShape(ContentBlock shape, IGraphicsContext context)
    {
        switch (shape.Kind)
        {
            case ShapeKind.Rectangle:
                if (shape.Fill.HasValue)
                    context.FillRectangle(shape.Bounds, shape.Fill.Value);
                if (shape.Stroke.HasValue)
                    context.StrokeRectangle(shape.Bounds, shape.Stroke.Value, shape.StrokeWidth);
                break;

            case ShapeKind.Ellipse:
                if (shape.Fill.HasValue)
                    context.FillEllipse(shape.Bounds, shape.Fill.Value);
                if (shape.Stroke.HasValue)
                    context.StrokeEllipse(shape.Bounds, shape.Stroke.Value, shape.StrokeWidth);
                break;

            case ShapeKind.Line:
                if (shape.Stroke.HasValue)
                {
                    var from = new Point(shape.Bounds.X, shape.Bounds.Y);
                    var to = new Point(shape.Bounds.Right, shape.Bounds.Bottom);
                    context.DrawLine(from, to, shape.Stroke.Value, shape.StrokeWidth);
                }
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Shape rendering (legacy - for backward compatibility with ShapeBlock)
    // -------------------------------------------------------------------------

    private static void RenderShape(ShapeBlock shape, IGraphicsContext context)
    {
        switch (shape.Kind)
        {
            case ShapeKind.Rectangle:
                if (shape.Fill.HasValue)
                    context.FillRectangle(shape.Bounds, shape.Fill.Value);
                if (shape.Stroke.HasValue)
                    context.StrokeRectangle(shape.Bounds, shape.Stroke.Value, shape.StrokeWidth);
                break;

            case ShapeKind.Ellipse:
                if (shape.Fill.HasValue)
                    context.FillEllipse(shape.Bounds, shape.Fill.Value);
                if (shape.Stroke.HasValue)
                    context.StrokeEllipse(shape.Bounds, shape.Stroke.Value, shape.StrokeWidth);
                break;

            case ShapeKind.Line:
                if (shape.Stroke.HasValue)
                {
                    var from = new Point(shape.Bounds.X, shape.Bounds.Y);
                    var to = new Point(shape.Bounds.Right, shape.Bounds.Bottom);
                    context.DrawLine(from, to, shape.Stroke.Value, shape.StrokeWidth);
                }
                break;
        }
    }

    private static void RenderChart(
        ChartTypeName? chartTypeValue,
        string? chartType,
        object? chartData,
        Rect bounds,
        AppliedStyle style,
        IGraphicsContext context)
    {
        if (!ChartRenderModelFactory.TryCreate(chartTypeValue, chartType, chartData, out var resolvedType, out var points, out var options))
        {
            context.StrokeRectangle(bounds, Color.FromRgb(156, 163, 175), 1f);
            return;
        }

        var chartBounds = GetChartPlotBounds(bounds, options, resolvedType, points);
        if (chartBounds.Width <= 1f || chartBounds.Height <= 1f)
            return;

        if (resolvedType is ResolvedChartType.BarVertical or ResolvedChartType.BarHorizontal or ResolvedChartType.Line)
            RenderCartesianGuides(points, chartBounds, bounds, style, options, resolvedType, context);

        switch (resolvedType)
        {
            case ResolvedChartType.BarVertical:
                RenderVerticalBarChart(points, chartBounds, options, context);
                break;
            case ResolvedChartType.BarHorizontal:
                RenderHorizontalBarChart(points, chartBounds, options, context);
                break;
            case ResolvedChartType.Line:
                RenderLineChart(points, chartBounds, style, options, context);
                break;
            case ResolvedChartType.Pie:
                RenderPieChart(points, chartBounds, style, options, context);
                break;
        }

        if (options.ShowLegend)
            RenderSeriesLegend(points, bounds, style, options, context);

        RenderAxisTitles(bounds, style, options, context);
    }

    private static Rect GetChartPlotBounds(Rect bounds, ChartRenderOptions options, ResolvedChartType chartType, IReadOnlyList<ChartPointModel> points)
    {
        if (chartType == ResolvedChartType.Pie)
        {
            const float pieInset = 8f;
            var legendReserve = options.ShowLegend ? GetPieLegendReserve(bounds, options, points) : 0f;

            return options.LegendPosition switch
            {
                PieLegendPosition.Right => new Rect(
                    bounds.X + pieInset,
                    bounds.Y + pieInset,
                    Math.Max(1f, bounds.Width - (pieInset * 2f) - legendReserve),
                    Math.Max(1f, bounds.Height - (pieInset * 2f))),
                PieLegendPosition.Left => new Rect(
                    bounds.X + pieInset + legendReserve,
                    bounds.Y + pieInset,
                    Math.Max(1f, bounds.Width - (pieInset * 2f) - legendReserve),
                    Math.Max(1f, bounds.Height - (pieInset * 2f))),
                PieLegendPosition.Bottom => new Rect(
                    bounds.X + pieInset,
                    bounds.Y + pieInset,
                    Math.Max(1f, bounds.Width - (pieInset * 2f)),
                    Math.Max(1f, bounds.Height - (pieInset * 2f) - legendReserve)),
                PieLegendPosition.Top => new Rect(
                    bounds.X + pieInset,
                    bounds.Y + pieInset + legendReserve,
                    Math.Max(1f, bounds.Width - (pieInset * 2f)),
                    Math.Max(1f, bounds.Height - (pieInset * 2f) - legendReserve)),
                _ => new Rect(bounds.X + pieInset, bounds.Y + pieInset, Math.Max(1f, bounds.Width - (pieInset * 2f)), Math.Max(1f, bounds.Height - (pieInset * 2f)))
            };
        }

        var left = 12f + (options.ShowTickLabels ? 40f : 0f) + (!string.IsNullOrWhiteSpace(options.YAxisLabel) ? 22f : 0f);
        var right = 10f;
        var top = 10f;
        var bottom = 12f + (options.ShowTickLabels ? 16f : 0f) + (!string.IsNullOrWhiteSpace(options.XAxisLabel) ? 16f : 0f);

        var width = Math.Max(1f, bounds.Width - left - right);
        var height = Math.Max(1f, bounds.Height - top - bottom);
        var plot = new Rect(bounds.X + left, bounds.Y + top, width, height);

        if (!options.ShowLegend)
            return plot;

        var cartesianLegendReserve = GetPieLegendReserve(bounds, options, points);
        return options.LegendPosition switch
        {
            PieLegendPosition.Right => new Rect(plot.X, plot.Y, Math.Max(1f, plot.Width - cartesianLegendReserve), plot.Height),
            PieLegendPosition.Left => new Rect(plot.X + cartesianLegendReserve, plot.Y, Math.Max(1f, plot.Width - cartesianLegendReserve), plot.Height),
            PieLegendPosition.Bottom => new Rect(plot.X, plot.Y, plot.Width, Math.Max(1f, plot.Height - cartesianLegendReserve)),
            PieLegendPosition.Top => new Rect(plot.X, plot.Y + cartesianLegendReserve, plot.Width, Math.Max(1f, plot.Height - cartesianLegendReserve)),
            _ => plot
        };
    }

    private static float GetPieLegendReserve(Rect bounds, ChartRenderOptions options, IReadOnlyList<ChartPointModel> points)
    {
        var marker = Math.Max(6f, options.LegendMarkerSize);
        var fontSize = Math.Max(8f, options.LegendFontSize);
        var lineHeight = Math.Max(fontSize + 2f, marker + 2f);
        const float inset = 8f;

        if (points.Count == 0)
            return options.LegendPosition is PieLegendPosition.Top or PieLegendPosition.Bottom
                ? Math.Max(28f, lineHeight + 8f)
                : Math.Max(90f, Math.Min(bounds.Width * 0.35f, marker + 10f + 96f));

        return options.LegendPosition switch
        {
            PieLegendPosition.Right or PieLegendPosition.Left => Math.Max(90f, Math.Min(bounds.Width * 0.35f, marker + 10f + 96f)),
            PieLegendPosition.Top or PieLegendPosition.Bottom => GetTopBottomLegendReserve(bounds, points, marker, fontSize, lineHeight, inset),
            _ => 0f
        };
    }

    private static float GetTopBottomLegendReserve(
        Rect bounds,
        IReadOnlyList<ChartPointModel> points,
        float marker,
        float fontSize,
        float lineHeight,
        float inset)
    {
        if (points.Count == 0)
            return Math.Max(28f, lineHeight + 8f);

        var availableWidth = Math.Max(40f, bounds.Width - (inset * 2f));
        var averageLabelLength = points.Average(p => Math.Max(4, p.Label.Length));
        var estimatedItemWidth = Math.Max(marker + 20f, marker + 10f + (float)averageLabelLength * fontSize * 0.55f);
        var itemsPerRow = Math.Max(1, (int)MathF.Floor(availableWidth / estimatedItemWidth));
        var rows = (int)Math.Ceiling(points.Count / (double)itemsPerRow);
        var compactRows = Math.Min(4, Math.Max(1, rows));

        return Math.Max(28f, compactRows * lineHeight + 8f);
    }

    private static void RenderVerticalBarChart(IReadOnlyList<ChartPointModel> points, Rect bounds, ChartRenderOptions options, IGraphicsContext context)
    {
        var scale = BuildScale(points, options);
        if (scale.Range <= 0d)
            return;

        var gapRatio = Math.Clamp(options.BarGapRatio, 0f, 0.9f);
        var gap = Math.Max(2f, bounds.Width * gapRatio / Math.Max(1, points.Count));
        var barWidth = Math.Max(1f, (bounds.Width - gap * (points.Count + 1)) / points.Count);

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var normalized = (Math.Max(scale.Min, point.Value) - scale.Min) / scale.Range;
            var barHeight = (float)(bounds.Height * normalized);
            var x = bounds.X + gap + i * (barWidth + gap);
            var y = bounds.Bottom - barHeight;

            context.FillRectangle(new Rect(x, y, barWidth, barHeight), point.Color);
        }
    }

    private static void RenderHorizontalBarChart(IReadOnlyList<ChartPointModel> points, Rect bounds, ChartRenderOptions options, IGraphicsContext context)
    {
        var scale = BuildScale(points, options);
        if (scale.Range <= 0d)
            return;

        var gapRatio = Math.Clamp(options.BarGapRatio, 0f, 0.9f);
        var gap = Math.Max(2f, bounds.Height * gapRatio / Math.Max(1, points.Count));
        var barHeight = Math.Max(1f, (bounds.Height - gap * (points.Count + 1)) / points.Count);

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var normalized = (Math.Max(scale.Min, point.Value) - scale.Min) / scale.Range;
            var barWidth = (float)(bounds.Width * normalized);
            var x = bounds.X;
            var y = bounds.Y + gap + i * (barHeight + gap);

            context.FillRectangle(new Rect(x, y, barWidth, barHeight), point.Color);
        }
    }

    private static void RenderLineChart(IReadOnlyList<ChartPointModel> points, Rect bounds, AppliedStyle style, ChartRenderOptions options, IGraphicsContext context)
    {
        var scale = BuildScale(points, options);
        if (scale.Range <= 0d)
            return;

        var stepX = points.Count > 1
            ? bounds.Width / (points.Count - 1)
            : 0f;

        var lineColor = options.LineColor ?? style.TextColor;
        var markerSize = 4f;

        var plotPoints = new List<Point>(points.Count);
        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var normalized = (Math.Max(scale.Min, point.Value) - scale.Min) / scale.Range;
            var x = bounds.X + i * stepX;
            var y = bounds.Bottom - (float)(bounds.Height * normalized);
            plotPoints.Add(new Point(x, y));
        }

        for (var i = 1; i < plotPoints.Count; i++)
        {
            context.DrawLine(plotPoints[i - 1], plotPoints[i], lineColor, Math.Max(1f, options.LineWidth));
        }

        if (!options.ShowMarkers)
            return;

        for (var i = 0; i < plotPoints.Count; i++)
        {
            var marker = new Rect(
                plotPoints[i].X - markerSize / 2f,
                plotPoints[i].Y - markerSize / 2f,
                markerSize,
                markerSize);

            context.FillEllipse(marker, points[i].Color);
        }
    }

    private static void RenderCartesianGuides(
        IReadOnlyList<ChartPointModel> points,
        Rect plot,
        Rect bounds,
        AppliedStyle style,
        ChartRenderOptions options,
        ResolvedChartType chartType,
        IGraphicsContext context)
    {
        var scale = BuildScale(points, options);
        var tickCount = Math.Max(2, options.YAxisTickCount);

        for (var i = 0; i < tickCount; i++)
        {
            var frac = tickCount == 1 ? 0f : (float)i / (tickCount - 1);
            var y = plot.Bottom - frac * plot.Height;

            if (options.ShowGridLines)
                context.DrawLine(new Point(plot.X, y), new Point(plot.Right, y), options.GridLineColor, Math.Max(0.5f, options.GridLineWidth));

            if (options.ShowTicks)
                context.DrawLine(new Point(plot.X - options.TickLength, y), new Point(plot.X, y), options.AxisColor, Math.Max(0.5f, options.AxisLineWidth));

            if (options.ShowTickLabels)
            {
                var value = scale.Min + scale.Range * frac;
                DrawChartText(context, style, options.LabelColor, options.LabelFontSize, value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture), plot.X - 38f, y - options.LabelFontSize * 0.2f, 34f);
            }
        }

        if (chartType == ResolvedChartType.BarVertical || chartType == ResolvedChartType.Line)
        {
            var count = points.Count;
            var stepX = count > 1 ? plot.Width / (count - 1) : plot.Width;

            for (var i = 0; i < count; i++)
            {
                var x = count == 1 ? plot.X + plot.Width / 2f : plot.X + i * stepX;
                if (options.ShowTicks)
                    context.DrawLine(new Point(x, plot.Bottom), new Point(x, plot.Bottom + options.TickLength), options.AxisColor, Math.Max(0.5f, options.AxisLineWidth));

                if (options.ShowTickLabels)
                    DrawChartText(context, style, options.LabelColor, options.LabelFontSize, points[i].Label, x - 18f, plot.Bottom + options.LabelFontSize + 3f, 36f);
            }
        }
        else if (chartType == ResolvedChartType.BarHorizontal)
        {
            var count = points.Count;
            var gap = Math.Max(2f, plot.Height * Math.Clamp(options.BarGapRatio, 0f, 0.9f) / Math.Max(1, count));
            var barHeight = Math.Max(1f, (plot.Height - gap * (count + 1)) / count);

            for (var i = 0; i < count; i++)
            {
                var y = plot.Y + gap + i * (barHeight + gap) + barHeight / 2f;
                if (options.ShowTickLabels)
                    DrawChartText(context, style, options.LabelColor, options.LabelFontSize, points[i].Label, plot.X - 44f, y + options.LabelFontSize * 0.25f, 40f);
            }

            var xTickCount = Math.Max(2, options.XAxisTickCount > 0 ? options.XAxisTickCount : options.YAxisTickCount);
            for (var i = 0; i < xTickCount; i++)
            {
                var frac = xTickCount == 1 ? 0f : (float)i / (xTickCount - 1);
                var x = plot.X + frac * plot.Width;
                if (options.ShowGridLines)
                    context.DrawLine(new Point(x, plot.Y), new Point(x, plot.Bottom), options.GridLineColor, Math.Max(0.5f, options.GridLineWidth));

                if (options.ShowTicks)
                    context.DrawLine(new Point(x, plot.Bottom), new Point(x, plot.Bottom + options.TickLength), options.AxisColor, Math.Max(0.5f, options.AxisLineWidth));

                if (options.ShowTickLabels)
                {
                    var value = scale.Min + scale.Range * frac;
                    DrawChartText(context, style, options.LabelColor, options.LabelFontSize, value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture), x - 16f, plot.Bottom + options.LabelFontSize + 3f, 32f);
                }
            }
        }

        if (options.ShowAxes)
        {
            context.DrawLine(new Point(plot.X, plot.Y), new Point(plot.X, plot.Bottom), options.AxisColor, Math.Max(0.5f, options.AxisLineWidth));
            context.DrawLine(new Point(plot.X, plot.Bottom), new Point(plot.Right, plot.Bottom), options.AxisColor, Math.Max(0.5f, options.AxisLineWidth));
        }
    }

    private static void RenderAxisTitles(Rect bounds, AppliedStyle style, ChartRenderOptions options, IGraphicsContext context)
    {
        if (!string.IsNullOrWhiteSpace(options.XAxisLabel))
            DrawChartText(context, style, options.LabelColor, Math.Max(9f, options.LabelFontSize + 0.5f), options.XAxisLabel!, bounds.X + bounds.Width / 2f - 60f, bounds.Bottom - 4f, 120f);

        if (!string.IsNullOrWhiteSpace(options.YAxisLabel))
            DrawChartText(context, style, options.LabelColor, Math.Max(9f, options.LabelFontSize + 0.5f), options.YAxisLabel!, bounds.X + 4f, bounds.Y + 12f, 120f);
    }

    private static void DrawChartText(IGraphicsContext context, AppliedStyle baseStyle, Color color, float fontSize, string text, float baselineX, float baselineY, float width)
    {
        var runStyle = baseStyle with
        {
            FontSize = fontSize,
            TextColor = color
        };

        context.DrawText(new TextRun
        {
            Text = text,
            BaselineOrigin = new Point(baselineX, baselineY),
            Bounds = new Rect(baselineX, baselineY - fontSize, width, fontSize + 2f),
            Style = runStyle,
            IsRightToLeft = false
        });
    }

    private static void DrawChartText(IGraphicsContext context, AppliedStyle baseStyle, Color color, float fontSize, FontWeight? fontWeight, string text, float baselineX, float baselineY, float width)
    {
        var runStyle = baseStyle with
        {
            FontSize = fontSize,
            TextColor = color,
            FontWeight = fontWeight ?? baseStyle.FontWeight
        };

        context.DrawText(new TextRun
        {
            Text = text,
            BaselineOrigin = new Point(baselineX, baselineY),
            Bounds = new Rect(baselineX, baselineY - fontSize, width, fontSize + 2f),
            Style = runStyle,
            IsRightToLeft = false
        });
    }

    private static (double Min, double Max, double Range) BuildScale(IReadOnlyList<ChartPointModel> points, ChartRenderOptions options)
    {
        var dataMin = points.Min(point => point.Value);
        var dataMax = points.Max(point => point.Value);

        var min = options.MinValue ?? Math.Min(0d, dataMin);
        var max = options.MaxValue ?? Math.Max(dataMax, min + 1d);
        if (max <= min)
            max = min + 1d;

        return (min, max, max - min);
    }

    private static void RenderPieChart(IReadOnlyList<ChartPointModel> points, Rect bounds, AppliedStyle style, ChartRenderOptions options, IGraphicsContext context)
    {
        var total = points.Sum(point => Math.Max(0d, point.Value));
        if (total <= 0d)
            return;

        var radius = Math.Max(1f, MathF.Min(bounds.Width, bounds.Height) / 2f);
        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;

        var startAngle = -MathF.PI / 2f;

        foreach (var point in points)
        {
            var proportion = (float)(Math.Max(0d, point.Value) / total);
            if (proportion <= 0f)
                continue;

            var sweepAngle = proportion * MathF.PI * 2f;
            var endAngle = startAngle + sweepAngle;

            var slice = CreatePieSlicePath(centerX, centerY, radius, startAngle, endAngle, point.Color);
            context.DrawPath(slice);

            if (options.ShowPieLabels && !string.IsNullOrWhiteSpace(point.Label))
            {
                var midAngle = startAngle + (sweepAngle / 2f);
                var labelRadius = SectorCentroidRadius(radius, sweepAngle);
                var labelCenter = PointOnCircle(centerX, centerY, labelRadius, midAngle);
                var labelText = $"{point.Label} ({proportion * 100f:0.#}%)";
                var fontSize = Math.Max(8f, options.LabelFontSize);
                var approxWidth = Math.Max(70f, labelText.Length * fontSize * 0.55f);
                DrawChartText(
                    context,
                    style,
                    options.PieLabelColor ?? options.LabelColor,
                    fontSize,
                    options.PieLabelFontWeight,
                    labelText,
                    labelCenter.X - (approxWidth / 2f),
                    labelCenter.Y,
                    approxWidth);
            }

            startAngle = endAngle;
        }
    }

    private static void RenderSeriesLegend(IReadOnlyList<ChartPointModel> points, Rect bounds, AppliedStyle style, ChartRenderOptions options, IGraphicsContext context)
    {
        if (points.Count == 0)
            return;

        var marker = Math.Max(6f, options.LegendMarkerSize);
        var fontSize = Math.Max(8f, options.LegendFontSize);
        var textColor = options.LegendTextColor ?? options.PieLabelColor ?? options.LabelColor;
        var lineHeight = Math.Max(marker + 2f, fontSize + 2f);
        var reserve = GetPieLegendReserve(bounds, options, points);
        const float inset = 8f;

        float startX;
        float startY;
        float availableWidth;

        switch (options.LegendPosition)
        {
            case PieLegendPosition.Left:
                startX = bounds.X + inset;
                startY = bounds.Y + inset;
                availableWidth = Math.Max(40f, reserve - (inset * 2f));
                break;
            case PieLegendPosition.Bottom:
                startX = bounds.X + inset;
                startY = bounds.Bottom - reserve + 4f;
                availableWidth = Math.Max(40f, bounds.Width - (inset * 2f));
                break;
            case PieLegendPosition.Top:
                startX = bounds.X + inset;
                startY = bounds.Y + 4f;
                availableWidth = Math.Max(40f, bounds.Width - (inset * 2f));
                break;
            default:
                startX = bounds.Right - reserve + inset;
                startY = bounds.Y + inset;
                availableWidth = Math.Max(40f, reserve - (inset * 2f));
                break;
        }

        var availableHeight = Math.Max(20f, bounds.Height - (inset * 2f));
        if (options.LegendPosition is PieLegendPosition.Left or PieLegendPosition.Right)
        {
            var rowCapacity = Math.Max(1, (int)MathF.Floor(availableHeight / lineHeight));
            var columnCount = Math.Max(1, (int)Math.Ceiling(points.Count / (double)rowCapacity));
            const float columnGap = 8f;
            var columnWidth = Math.Max(24f, (availableWidth - (columnCount - 1) * columnGap) / columnCount);

            for (var i = 0; i < points.Count; i++)
            {
                var col = i / rowCapacity;
                var row = i % rowCapacity;
                if (col >= columnCount)
                    break;

                var x = startX + col * (columnWidth + columnGap);
                var y = startY + row * lineHeight;

                context.FillRectangle(new Rect(x, y, marker, marker), points[i].Color);
                DrawChartText(
                    context,
                    style,
                    textColor,
                    fontSize,
                    options.PieLabelFontWeight,
                    points[i].Label,
                    x + marker + 6f,
                    y + marker,
                    Math.Max(12f, columnWidth - marker - 6f));
            }

            return;
        }

        var rowLimit = Math.Max(1, (int)MathF.Floor(Math.Max(20f, reserve - 8f) / lineHeight));
        var cursorX = startX;
        var cursorY = startY;
        var currentRow = 0;
        const float itemGap = 10f;

        for (var i = 0; i < points.Count; i++)
        {
            var textWidth = Math.Max(24f, points[i].Label.Length * fontSize * 0.55f);
            var itemWidth = marker + 6f + textWidth;

            if (cursorX > startX && cursorX + itemWidth > startX + availableWidth)
            {
                currentRow++;
                if (currentRow >= rowLimit)
                    break;

                cursorX = startX;
                cursorY += lineHeight;
            }

            context.FillRectangle(new Rect(cursorX, cursorY, marker, marker), points[i].Color);
            DrawChartText(
                context,
                style,
                textColor,
                fontSize,
                options.PieLabelFontWeight,
                points[i].Label,
                cursorX + marker + 6f,
                cursorY + marker,
                textWidth);

            cursorX += itemWidth + itemGap;
        }
    }

    private static float SectorCentroidRadius(float radius, float sweepAngle)
    {
        var theta = MathF.Abs(sweepAngle);
        if (theta <= 0.0001f)
            return radius * 0.62f;

        var centroid = (4f * radius * MathF.Sin(theta / 2f)) / (3f * theta);
        return Math.Clamp(centroid, radius * 0.35f, radius * 0.72f);
    }

    private static PathGeometry CreatePieSlicePath(float centerX, float centerY, float radius, float startAngle, float endAngle, Color fill)
    {
        var commands = new List<PathCommand>
        {
            new(PathCommandKind.MoveTo, [new Point(centerX, centerY)]),
            new(PathCommandKind.LineTo, [PointOnCircle(centerX, centerY, radius, startAngle)])
        };

        var sweep = MathF.Abs(endAngle - startAngle);
        var segmentCount = Math.Max(3, (int)MathF.Ceiling(sweep / (MathF.PI / 18f)));

        for (var i = 1; i <= segmentCount; i++)
        {
            var t = (float)i / segmentCount;
            var angle = startAngle + (endAngle - startAngle) * t;
            commands.Add(new PathCommand(PathCommandKind.LineTo, [PointOnCircle(centerX, centerY, radius, angle)]));
        }

        commands.Add(new PathCommand(PathCommandKind.Close, []));

        return new PathGeometry
        {
            Commands = commands,
            Fill = fill,
            Stroke = Color.White,
            StrokeWidth = 1f
        };
    }

    private static Point PointOnCircle(float centerX, float centerY, float radius, float angle)
    {
        return new Point(
            centerX + MathF.Cos(angle) * radius,
            centerY + MathF.Sin(angle) * radius);
    }

    // -------------------------------------------------------------------------
    // Border rendering
    // -------------------------------------------------------------------------

    private static void RenderBorder(Rect bounds, Border? border, IGraphicsContext context)
    {
        if (border is null) return;

        DrawBorderSide(border.Top, new Point(bounds.X, bounds.Y), new Point(bounds.Right, bounds.Y), context);
        DrawBorderSide(border.Right, new Point(bounds.Right, bounds.Y), new Point(bounds.Right, bounds.Bottom), context);
        DrawBorderSide(border.Bottom, new Point(bounds.X, bounds.Bottom), new Point(bounds.Right, bounds.Bottom), context);
        DrawBorderSide(border.Left, new Point(bounds.X, bounds.Y), new Point(bounds.X, bounds.Bottom), context);
    }

    private static void DrawBorderSide(BorderSide? side, Point from, Point to, IGraphicsContext context)
    {
        if (side is null || side.Style == BorderLineStyle.None || side.Width <= 0f) return;
        context.DrawLine(from, to, side.Color, side.Width);
    }

    private static void RenderContentBarcode(ContentBlock content, IGraphicsContext context, int pageNumber)
    {
        var resolvedValue = ResolveSystemTextTokens(content.Value ?? string.Empty, pageNumber);
        if (!TryRenderBarcodeImage(content.Id, content.SymbologyType, content.Symbology, resolvedValue, content.Bounds, context))
            return;

        if (content.ShowText)
            RenderBarcodeLabel(resolvedValue, content.Style, content.Bounds, context);
    }

    private static void RenderBarcodeBlock(BarcodeBlock barcode, IGraphicsContext context, int pageNumber)
    {
        var resolvedValue = ResolveSystemTextTokens(barcode.Value, pageNumber);
        if (!TryRenderBarcodeImage(barcode.Id, barcode.SymbologyType, barcode.Symbology, resolvedValue, barcode.Bounds, context))
            return;

        if (barcode.ShowText)
            RenderBarcodeLabel(resolvedValue, barcode.Style, barcode.Bounds, context);
    }

    private static bool TryRenderBarcodeImage(
        string id,
        BarcodeSymbology? symbologyType,
        string? symbology,
        string value,
        Rect bounds,
        IGraphicsContext context)
    {
        var symbolBounds = GetBarcodeSymbolBounds(bounds, symbologyType, symbology);
        if (!BarcodeImageFactory.TryCreateImageReference(id, symbologyType, symbology, value, symbolBounds, out var imageReference))
            return false;

        context.DrawImage(symbolBounds, imageReference, ImageStretch.Fill);
        return true;
    }

    private static Rect GetBarcodeSymbolBounds(Rect bounds, BarcodeSymbology? symbologyType, string? symbology)
    {
        var labelHeight = GetBarcodeLabelHeight(bounds);
        var symbolHeight = Math.Max(1f, bounds.Height - labelHeight);
        var symbolWidth = Math.Max(1f, bounds.Width);

        if (!BarcodeImageFactory.IsSquareSymbology(symbologyType, symbology))
            return new Rect(bounds.X, bounds.Y, symbolWidth, symbolHeight);

        var side = Math.Max(1f, MathF.Min(symbolWidth, symbolHeight));
        var offsetX = (symbolWidth - side) / 2f;
        return new Rect(bounds.X + offsetX, bounds.Y, side, side);
    }

    private static float GetBarcodeLabelHeight(Rect bounds)
    {
        if (bounds.Height < 28f)
            return 0f;

        return MathF.Min(18f, MathF.Max(12f, bounds.Height * 0.2f));
    }

    private static void RenderBarcodeLabel(string value, AppliedStyle style, Rect bounds, IGraphicsContext context)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var labelHeight = GetBarcodeLabelHeight(bounds);
        if (labelHeight <= 0f)
            return;

        var labelRect = new Rect(bounds.X, bounds.Bottom - labelHeight, bounds.Width, labelHeight);
        var fontSize = MathF.Min(style.FontSize, MathF.Max(9f, labelHeight - 3f));
        var estimatedWidth = value.Length * fontSize * 0.52f;
        var baselineX = labelRect.X + MathF.Max(0f, (labelRect.Width - estimatedWidth) / 2f);
        var baselineY = labelRect.Bottom - 2f;

        var runStyle = style with { FontSize = fontSize };
        context.DrawText(new TextRun
        {
            Text = value,
            BaselineOrigin = new Point(baselineX, baselineY),
            Bounds = labelRect,
            Style = runStyle,
            IsRightToLeft = false
        });
    }

    private static TextRun ResolveSystemTextTokens(TextRun run, int pageNumber)
    {
        var resolvedText = ResolveSystemTextTokens(run.Text, pageNumber);
        if (string.Equals(resolvedText, run.Text, StringComparison.Ordinal))
            return run;

        return run with { Text = resolvedText };
    }

    private static string ResolveSystemTextTokens(string text, int pageNumber)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("{PageNumber}", pageNumber.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{CurrentDate}", DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{CurrentTime}", DateTime.UtcNow.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryCreateImageReference(ContentBlock content, out ImageReference imageReference)
    {
        imageReference = null!;

        if (!TryResolveImageBytes(content.SourceKey, out var bytes))
            return false;

        imageReference = new ImageReference
        {
            SourceKey = content.SourceKey ?? content.Id,
            IntrinsicSize = new Size(
                Math.Max(1f, content.Bounds.Width),
                Math.Max(1f, content.Bounds.Height)),
            PixelWidth = 1,
            PixelHeight = 1,
            PixelFormat = "PNG",
            PixelData = bytes
        };

        return true;
    }

    private static bool TryResolveImageBytes(string? sourceKey, out byte[] bytes)
    {
        bytes = [];

        if (string.IsNullOrWhiteSpace(sourceKey))
            return false;

        return TryDecodeDataUri(sourceKey, out bytes)
            || TryReadFileSource(sourceKey, out bytes)
            || TryReadWebRootRelativeSource(sourceKey, out bytes);
    }

    private static bool TryDecodeDataUri(string source, out byte[] bytes)
    {
        bytes = [];

        if (!source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return false;

        var commaIndex = source.IndexOf(',');
        if (commaIndex <= 0 || commaIndex + 1 >= source.Length)
            return false;

        var metadata = source[..commaIndex];
        if (!metadata.Contains(";base64", StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            bytes = Convert.FromBase64String(source[(commaIndex + 1)..]);
            return bytes.Length > 0;
        }
        catch
        {
            bytes = [];
            return false;
        }
    }

    private static bool TryReadFileSource(string source, out byte[] bytes)
    {
        bytes = [];

        string? localPath = null;
        if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            localPath = uri.LocalPath;
        }
        else if (Path.IsPathRooted(source))
        {
            localPath = source;
        }

        if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
            return false;

        try
        {
            bytes = File.ReadAllBytes(localPath);
            return bytes.Length > 0;
        }
        catch
        {
            bytes = [];
            return false;
        }
    }

    private static bool TryReadWebRootRelativeSource(string source, out byte[] bytes)
    {
        bytes = [];

        if (!source.StartsWith("/", StringComparison.Ordinal) && !source.StartsWith("\\", StringComparison.Ordinal))
            return false;

        var relative = source.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);

        foreach (var root in GetSearchRoots())
        {
            var directCandidate = Path.Combine(root, "wwwroot", relative);
            if (TryReadBytes(directCandidate, out bytes))
                return true;

            if (Path.GetFileName(root).Equals("wwwroot", StringComparison.OrdinalIgnoreCase))
            {
                var nestedCandidate = Path.Combine(root, relative);
                if (TryReadBytes(nestedCandidate, out bytes))
                    return true;
            }

            foreach (var webRootDir in EnumerateWebRootDirectories(root))
            {
                var discoveredCandidate = Path.Combine(webRootDir, relative);
                if (TryReadBytes(discoveredCandidate, out bytes))
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetSearchRoots()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static IEnumerable<string> EnumerateAncestors(string start)
        {
            var current = Path.GetFullPath(start);
            while (!string.IsNullOrWhiteSpace(current))
            {
                yield return current;
                var parent = Directory.GetParent(current);
                if (parent is null)
                    yield break;

                current = parent.FullName;
            }
        }

        foreach (var path in EnumerateAncestors(AppContext.BaseDirectory))
        {
            if (seen.Add(path))
                yield return path;
        }

        foreach (var path in EnumerateAncestors(Directory.GetCurrentDirectory()))
        {
            if (seen.Add(path))
                yield return path;
        }
    }

    private static IEnumerable<string> EnumerateWebRootDirectories(string root)
    {
        try
        {
            return Directory.EnumerateDirectories(root, "wwwroot", SearchOption.AllDirectories);
        }
        catch
        {
            return [];
        }
    }

    private static bool TryReadBytes(string path, out byte[] bytes)
    {
        bytes = [];

        if (!File.Exists(path))
            return false;

        try
        {
            bytes = File.ReadAllBytes(path);
            return bytes.Length > 0;
        }
        catch
        {
            bytes = [];
            return false;
        }
    }
}
