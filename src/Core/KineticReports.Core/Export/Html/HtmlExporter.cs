namespace KineticReports.Core.Export.Html;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using System.Text.Json;

/// <summary>
/// Exports an immutable ReportDocument to semantic HTML.
/// </summary>
public sealed class HtmlExporter : IHtmlExporter
{
    private readonly HtmlExportOptions _options;

    /// <summary>
    /// Exports the report document to HTML.
    /// </summary>
    public HtmlExporter(IOptions<HtmlExportOptions>? options = null)
    {
        _options = options?.Value ?? new HtmlExportOptions();
    }

    public async Task ExportAsync(ReportDocument reportDocument, Stream output, CancellationToken ct = default)
    {
        if (reportDocument == null) throw new ArgumentNullException(nameof(reportDocument));
        if (output == null) throw new ArgumentNullException(nameof(output));

        var builder = new HtmlBuilder();
        BuildHtmlDocument(builder, reportDocument);

        var html = builder.Build();
        var bytes = Encoding.UTF8.GetBytes(html);
        await output.WriteAsync(bytes, ct);
    }

    private void BuildHtmlDocument(HtmlBuilder html, ReportDocument reportDocument)
    {
        html
            .Raw("<!DOCTYPE html>\n")
            .OpenTag("html", classAttr: "kinetic-report")
            .OpenTag("head")
            .VoidTag("meta", attributes: new() { ["charset"] = "UTF-8" })
            .OpenTag("title").Text("Report").CloseTag("title");

        RenderStylesheetLink(html);

        html
            .CloseTag("head")
            .OpenTag("body");

        var reportModel = BuildReportDocumentModelJson(reportDocument);

        html
            .OpenTag("script", id: "report-document-model", attributes: new Dictionary<string, string> { ["type"] = "application/json" })
            .Raw(reportModel)
            .CloseTag("script")
            .OpenTag("main", id: "report-document", classAttr: "report-document");

        // Render all pages
        foreach (var page in reportDocument.Pages)
        {
            RenderPage(html, page);
        }

        html
            .CloseTag("main")
            .CloseTag("body")
            .CloseTag("html");
    }

    private void RenderStylesheetLink(HtmlBuilder html)
    {
        if (string.IsNullOrWhiteSpace(_options.StylesheetHref))
            return;

        html.VoidTag("link", attributes: new()
        {
            ["rel"] = "stylesheet",
            ["href"] = _options.StylesheetHref!
        });
    }

    private static void RenderPage(HtmlBuilder html, PageBlock page)
    {
        var pageStyle = $"width: {page.PageWidth:F1}px; height: {page.PageHeight:F1}px; position: relative; margin: 20px auto;";

        html
            .OpenTag("section", style: pageStyle, id: $"page-{page.PageNumber}", classAttr: "kinetic-page page-block")
            .OpenTag("div", style: CssBuilder.BuildStyle(page.Style), classAttr: "page-background");

        // Render header if present
        if (page.Header != null)
        {
            html.OpenTag("header", classAttr: "page-header-region");
            RenderElement(html, page.Header, 0f, 0f, page.PageNumber);
            html.CloseTag("header");
        }

        // Render body elements
        html.OpenTag("main", classAttr: "page-body-region");
        foreach (var child in page.Children)
        {
            RenderElement(html, child, 0f, 0f, page.PageNumber);
        }
        html.CloseTag("main");

        // Render footer if present
        if (page.Footer != null)
        {
            html.OpenTag("footer", classAttr: "page-footer-region");
            RenderElement(html, page.Footer, 0f, 0f, page.PageNumber);
            html.CloseTag("footer");
        }

        html
            .CloseTag("div") // page-background
            .CloseTag("section"); // kinetic-page
    }

    private static string BuildReportDocumentModelJson(ReportDocument reportDocument)
    {
        var model = new
        {
            pageCount = reportDocument.PageCount,
            pages = reportDocument.Pages.Select(page => new
            {
                pageNumber = page.PageNumber,
                pageWidth = page.PageWidth,
                pageHeight = page.PageHeight,
                header = page.Header is null ? null : ToBlockModel(page.Header),
                children = page.Children.Select(ToBlockModel).ToList(),
                footer = page.Footer is null ? null : ToBlockModel(page.Footer)
            }).ToList()
        };

        return JsonSerializer.Serialize(model);
    }

    private static object ToBlockModel(LayoutBlock block)
    {
        return new
        {
            id = block.Id,
            type = block.LayoutBlockType.ToString(),
            bounds = new
            {
                x = block.Bounds.X,
                y = block.Bounds.Y,
                width = block.Bounds.Width,
                height = block.Bounds.Height
            },
            children = GetChildBlocks(block).Select(ToBlockModel).ToList()
        };
    }

    private static IReadOnlyList<LayoutBlock> GetChildBlocks(LayoutBlock block)
    {
        return block switch
        {
            ContentBlock content => content.Children.Cast<LayoutBlock>().ToList(),
            ContainerBlock container => container.Children,
            PageSectionBlock section => section.Children,
            ReportBlock reportBlock => reportBlock.Children,
            RowBlock row => row.Cells.Cast<LayoutBlock>().ToList(),
            CellBlock cell => cell.Children,
            _ => []
        };
    }

    private static void RenderElement(HtmlBuilder html, LayoutBlock element, float parentX, float parentY, int pageNumber)
    {
        var style = BuildElementStyle(element, parentX, parentY);
        var tag = GetElementTagName(element);

        html.OpenTag(tag, style: style, id: element.Id, classAttr: $"element {GetElementClassName(element)}");

        switch (element)
        {
            // ContentBlock unified dispatch
            case ContentBlock content:
                RenderContentBlock(html, content, pageNumber);
                break;

            case TextBlock textElem:
                RenderTextBlock(html, textElem, pageNumber);
                break;

            case ImageBlock imgElem:
                RenderImageElement(html, imgElem);
                break;

            case ShapeBlock shapeElem:
                RenderShapeElement(html, shapeElem);
                break;

            case TableBlock tableElem:
                RenderTableElement(html, tableElem, pageNumber);
                break;

            case ContainerBlock container:
                foreach (var child in container.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y, pageNumber);
                break;

            case PageSectionBlock section:
                foreach (var child in section.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y, pageNumber);
                break;

            case ReportBlock block:
                foreach (var child in block.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y, pageNumber);
                break;

            case RowBlock row:
                foreach (var cell in row.Cells)
                    RenderElement(html, cell, element.Bounds.X, element.Bounds.Y, pageNumber);
                break;

            case CellBlock cell:
                foreach (var child in cell.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y, pageNumber);
                break;

            case ChartBlock chart:
                RenderChartElement(html, chart);
                break;

            case BarcodeBlock barcode:
                RenderBarcodeElement(html, barcode, pageNumber);
                break;
        }

        html.CloseTag(tag);
    }

    private static void RenderTextBlock(HtmlBuilder html, TextBlock textBlock, int pageNumber)
    {
        if (textBlock.TextRuns.Count == 0)
        {
            html.Text(ResolveSystemTextTokens(textBlock.Text, pageNumber));
        }
        else
        {
            foreach (var run in textBlock.TextRuns)
            {
                var runStyle = CssBuilder.BuildStyle(run.Style);
                html
                    .OpenTag("p", style: runStyle)
                    .Text(ResolveSystemTextTokens(run.Text, pageNumber))
                    .CloseTag("p");
            }
        }
    }

    private static void RenderContentBlock(HtmlBuilder html, ContentBlock content, int pageNumber)
    {
        switch (content.ContentType)
        {
            case BlockContentType.Text:
                if (content.TextRuns.Count == 0)
                {
                    html.Text(ResolveSystemTextTokens(content.Text, pageNumber));
                }
                else
                {
                    foreach (var run in content.TextRuns)
                    {
                        var runStyle = CssBuilder.BuildStyle(run.Style);
                        html
                            .OpenTag("p", style: runStyle)
                            .Text(ResolveSystemTextTokens(run.Text, pageNumber))
                            .CloseTag("p");
                    }
                }
                break;

            case BlockContentType.Image:
                if (TryGetImageSource(content.SourceKey, out var contentSource))
                {
                    html.VoidTag("img", attributes: new()
                    {
                        ["src"] = contentSource,
                        ["alt"] = content.Id,
                        ["style"] = $"width: {content.Bounds.Width:F1}px; height: {content.Bounds.Height:F1}px; object-fit: {GetObjectFit(content.Stretch)};"
                    });
                }
                else
                {
                    html
                        .OpenTag("div", classAttr: "visual-image-placeholder")
                        .Text("[Image placeholder]")
                        .CloseTag("div");
                }
                break;

            case BlockContentType.Shape:
                RenderContentBlockShape(html, content);
                break;

            case BlockContentType.Chart:
                RenderContentBlockChart(html, content);
                break;

            case BlockContentType.Barcode:
                RenderContentBlockBarcode(html, content, pageNumber);
                break;

            case BlockContentType.Table:
                RenderContentBlockTable(html, content, pageNumber);
                break;

            case BlockContentType.Row:
                RenderContentBlockRow(html, content, "td", pageNumber);
                break;

            case BlockContentType.Cell:
                RenderContentBlockCellChildren(html, content, pageNumber);
                break;

            case BlockContentType.Container:
            case BlockContentType.ReportSection:
            case BlockContentType.PageSection:
            case BlockContentType.Page:
                // Container-like blocks: render children
                foreach (var child in content.Children)
                    RenderElement(html, child, content.Bounds.X, content.Bounds.Y, pageNumber);
                break;
        }
    }

    private static void RenderContentBlockTable(HtmlBuilder html, ContentBlock table, int pageNumber)
    {
        html.OpenTag("table", classAttr: "kinetic-table");

        // Separate rows by type (header vs body)
        // Note: ContentBlock rows don't have RowType property, so treat all as body rows
        // unless we have custom metadata; for now, treat first row as header if present
        var rows = table.Children.ToList();

        if (rows.Count > 0)
        {
            // Render first row as header if present
            html.OpenTag("thead");
            RenderContentBlockRow(html, rows[0], "th", pageNumber);
            html.CloseTag("thead");

            // Render remaining rows as body
            if (rows.Count > 1)
            {
                html.OpenTag("tbody");
                for (int i = 1; i < rows.Count; i++)
                {
                    RenderContentBlockRow(html, rows[i], "td", pageNumber);
                }
                html.CloseTag("tbody");
            }
        }

        html.CloseTag("table");
    }

    private static void RenderContentBlockRow(HtmlBuilder html, ContentBlock row, string cellTag, int pageNumber)
    {
        html.OpenTag("tr", classAttr: "kinetic-table-row");

        foreach (var cell in row.Children)
        {
            if (cell.ContentType != BlockContentType.Cell)
                continue;

            var cellStyle = $"position: relative; width: {cell.Bounds.Width:F1}px; height: {cell.Bounds.Height:F1}px; {CssBuilder.BuildStyle(cell.Style)}";
            var attributes = new Dictionary<string, string>();

            if (cell.ColSpan > 1)
                attributes["colspan"] = cell.ColSpan.ToString();

            if (cell.RowSpan > 1)
                attributes["rowspan"] = cell.RowSpan.ToString();

            html.OpenTag(cellTag, style: cellStyle, attributes: attributes.Count == 0 ? null : attributes);

            RenderContentBlockCellChildren(html, cell, pageNumber);

            html.CloseTag(cellTag);
        }

        html.CloseTag("tr");
    }

    private static void RenderContentBlockCellChildren(HtmlBuilder html, ContentBlock cell, int pageNumber)
    {
        foreach (var child in cell.Children)
        {
            RenderContentBlockCellChildInline(html, child, pageNumber);
        }
    }

    private static void RenderContentBlockCellChildInline(HtmlBuilder html, ContentBlock child, int pageNumber)
    {
        if (child.ContentType == BlockContentType.Text)
        {
            if (child.TextRuns.Count == 0)
            {
                html
                    .OpenTag("span", style: CssBuilder.BuildStyle(child.Style))
                    .Text(ResolveSystemTextTokens(child.Text, pageNumber))
                    .CloseTag("span");
                return;
            }

            foreach (var run in child.TextRuns)
            {
                html
                    .OpenTag("span", style: CssBuilder.BuildStyle(run.Style))
                    .Text(ResolveSystemTextTokens(run.Text, pageNumber))
                    .CloseTag("span");
            }

            return;
        }

        var isContainerLike = child.ContentType is BlockContentType.Container
            or BlockContentType.Cell
            or BlockContentType.Row
            or BlockContentType.ReportSection
            or BlockContentType.PageSection
            or BlockContentType.Page;

        if (isContainerLike)
        {
            foreach (var grandchild in child.Children)
            {
                RenderContentBlockCellChildInline(html, grandchild, pageNumber);
            }

            return;
        }

        RenderElement(html, child, child.Bounds.X, child.Bounds.Y, pageNumber);
    }

    private static void RenderContentBlockShape(HtmlBuilder html, ContentBlock shape)
    {
        var width = shape.Bounds.Width;
        var height = shape.Bounds.Height;
        var svgStyle = $"width: {width:F1}px; height: {height:F1}px; display: block;";

        html
            .OpenTag("svg", style: svgStyle, id: shape.Id)
            .Raw($"viewBox=\"0 0 {width:F1} {height:F1}\" ")
            .Raw(RenderSvgContentShape(shape))
            .CloseTag("svg");
    }

    private static string RenderSvgContentShape(ContentBlock shape)
    {
        var fill = shape.Fill.HasValue ? $"fill=\"{CssColorToSvg(shape.Fill.Value)}\"" : "fill=\"none\"";
        var stroke = shape.Stroke.HasValue ? $"stroke=\"{CssColorToSvg(shape.Stroke.Value)}\" stroke-width=\"{shape.StrokeWidth:F1}\"" : "stroke=\"none\"";
        var attrs = $"{fill} {stroke}";

        return shape.Kind switch
        {
            ShapeKind.Rectangle => $"<rect x=\"0\" y=\"0\" width=\"{shape.Bounds.Width:F1}\" height=\"{shape.Bounds.Height:F1}\" {attrs} />",
            ShapeKind.Ellipse => $"<ellipse cx=\"{shape.Bounds.Width / 2:F1}\" cy=\"{shape.Bounds.Height / 2:F1}\" rx=\"{shape.Bounds.Width / 2:F1}\" ry=\"{shape.Bounds.Height / 2:F1}\" {attrs} />",
            ShapeKind.Line => $"<line x1=\"0\" y1=\"0\" x2=\"{shape.Bounds.Width:F1}\" y2=\"{shape.Bounds.Height:F1}\" {stroke} />",
            _ => string.Empty
        };
    }

    private static void RenderChartElement(HtmlBuilder html, ChartBlock chart)
    {
        RenderChartSvg(html, chart.Bounds, chart.ChartTypeValue, chart.ChartType, chart.ChartData, chart.Id);
    }

    private static void RenderContentBlockChart(HtmlBuilder html, ContentBlock chart)
    {
        RenderChartSvg(html, chart.Bounds, chart.ChartTypeValue, chart.ChartType, chart.ChartData, chart.Id);
    }

    private static void RenderChartSvg(
        HtmlBuilder html,
        Rect bounds,
        ChartTypeName? chartTypeValue,
        string? chartType,
        object? chartData,
        string id)
    {
        var width = Math.Max(1f, bounds.Width);
        var height = Math.Max(1f, bounds.Height);

        var svgStyle = $"width: {width:F1}px; height: {height:F1}px; display: block;";
        html.OpenTag("svg", style: svgStyle, id: id)
            .Raw($"viewBox=\"0 0 {width:F1} {height:F1}\" ");

        if (!ChartRenderModelFactory.TryCreate(chartTypeValue, chartType, chartData, out var resolvedType, out var points, out var options))
        {
            html.Raw($"<rect x=\"0\" y=\"0\" width=\"{width:F1}\" height=\"{height:F1}\" fill=\"none\" stroke=\"#9CA3AF\" stroke-width=\"1\" />");
            html.CloseTag("svg");
            return;
        }

        var plot = GetChartPlotBounds(width, height, options, resolvedType, points);
        html.Raw(RenderChartSvgContent(resolvedType, points, plot, width, height, options));
        html.CloseTag("svg");
    }

    private static Rect GetChartPlotBounds(float width, float height, ChartRenderOptions options, ResolvedChartType chartType, IReadOnlyList<ChartPointModel> points)
    {
        if (chartType == ResolvedChartType.Pie)
        {
            const float pieInset = 8f;
            var bounds = new Rect(0f, 0f, width, height);
            var legendReserve = options.ShowLegend ? GetPieLegendReserve(bounds, options, points) : 0f;

            return options.LegendPosition switch
            {
                PieLegendPosition.Right => new Rect(
                    pieInset,
                    pieInset,
                    Math.Max(1f, width - (pieInset * 2f) - legendReserve),
                    Math.Max(1f, height - (pieInset * 2f))),
                PieLegendPosition.Left => new Rect(
                    pieInset + legendReserve,
                    pieInset,
                    Math.Max(1f, width - (pieInset * 2f) - legendReserve),
                    Math.Max(1f, height - (pieInset * 2f))),
                PieLegendPosition.Bottom => new Rect(
                    pieInset,
                    pieInset,
                    Math.Max(1f, width - (pieInset * 2f)),
                    Math.Max(1f, height - (pieInset * 2f) - legendReserve)),
                PieLegendPosition.Top => new Rect(
                    pieInset,
                    pieInset + legendReserve,
                    Math.Max(1f, width - (pieInset * 2f)),
                    Math.Max(1f, height - (pieInset * 2f) - legendReserve)),
                _ => new Rect(pieInset, pieInset, Math.Max(1f, width - pieInset * 2f), Math.Max(1f, height - pieInset * 2f))
            };
        }

        var left = 12f + (options.ShowTickLabels ? 40f : 0f) + (!string.IsNullOrWhiteSpace(options.YAxisLabel) ? 22f : 0f);
        var right = 10f;
        var top = 10f;
        var bottom = 12f + (options.ShowTickLabels ? 16f : 0f) + (!string.IsNullOrWhiteSpace(options.XAxisLabel) ? 16f : 0f);

        var plotWidth = Math.Max(1f, width - left - right);
        var plotHeight = Math.Max(1f, height - top - bottom);
        var plot = new Rect(left, top, plotWidth, plotHeight);

        if (!options.ShowLegend)
            return plot;

        var cartesianLegendReserve = GetPieLegendReserve(new Rect(0f, 0f, width, height), options, points);
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

    private static string RenderChartSvgContent(
        ResolvedChartType chartType,
        IReadOnlyList<ChartPointModel> points,
        Rect plot,
        float totalWidth,
        float totalHeight,
        ChartRenderOptions options)
    {
        var svg = new StringBuilder();

        if (chartType is ResolvedChartType.BarVertical or ResolvedChartType.BarHorizontal or ResolvedChartType.Line)
            svg.Append(RenderCartesianGuidesSvg(chartType, points, plot, options));

        var content = chartType switch
        {
            ResolvedChartType.BarVertical => svg.Append(RenderVerticalBarsSvg(points, plot, options)).Append(RenderAxisTitleSvg(totalWidth, totalHeight, options)).ToString(),
            ResolvedChartType.BarHorizontal => svg.Append(RenderHorizontalBarsSvg(points, plot, options)).Append(RenderAxisTitleSvg(totalWidth, totalHeight, options)).ToString(),
            ResolvedChartType.Line => svg.Append(RenderLineSvg(points, plot, options)).Append(RenderAxisTitleSvg(totalWidth, totalHeight, options)).ToString(),
            ResolvedChartType.Pie => RenderPieSvg(points, plot, options, totalWidth, totalHeight),
            _ => svg.ToString()
        };

        if (!options.ShowLegend || chartType == ResolvedChartType.Pie)
            return content;

        return content + RenderSeriesLegendSvg(points, totalWidth, totalHeight, options);
    }

    private static string RenderVerticalBarsSvg(IReadOnlyList<ChartPointModel> points, Rect plot, ChartRenderOptions options)
    {
        var scale = BuildScale(points, options);
        if (scale.Range <= 0d)
            return string.Empty;

        var gapRatio = Math.Clamp(options.BarGapRatio, 0f, 0.9f);
        var gap = Math.Max(2f, plot.Width * gapRatio / Math.Max(1, points.Count));
        var barWidth = Math.Max(1f, (plot.Width - gap * (points.Count + 1)) / points.Count);
        var svg = new StringBuilder();

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var normalized = (Math.Max(scale.Min, point.Value) - scale.Min) / scale.Range;
            var barHeight = (float)(plot.Height * normalized);
            var x = plot.X + gap + i * (barWidth + gap);
            var y = plot.Bottom - barHeight;

            svg.Append($"<rect x=\"{x:F1}\" y=\"{y:F1}\" width=\"{barWidth:F1}\" height=\"{barHeight:F1}\" fill=\"{CssColorToSvg(point.Color)}\" />");
        }

        return svg.ToString();
    }

    private static string RenderHorizontalBarsSvg(IReadOnlyList<ChartPointModel> points, Rect plot, ChartRenderOptions options)
    {
        var scale = BuildScale(points, options);
        if (scale.Range <= 0d)
            return string.Empty;

        var gapRatio = Math.Clamp(options.BarGapRatio, 0f, 0.9f);
        var gap = Math.Max(2f, plot.Height * gapRatio / Math.Max(1, points.Count));
        var barHeight = Math.Max(1f, (plot.Height - gap * (points.Count + 1)) / points.Count);
        var svg = new StringBuilder();

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var normalized = (Math.Max(scale.Min, point.Value) - scale.Min) / scale.Range;
            var barWidth = (float)(plot.Width * normalized);
            var x = plot.X;
            var y = plot.Y + gap + i * (barHeight + gap);

            svg.Append($"<rect x=\"{x:F1}\" y=\"{y:F1}\" width=\"{barWidth:F1}\" height=\"{barHeight:F1}\" fill=\"{CssColorToSvg(point.Color)}\" />");
        }

        return svg.ToString();
    }

    private static string RenderLineSvg(IReadOnlyList<ChartPointModel> points, Rect plot, ChartRenderOptions options)
    {
        var scale = BuildScale(points, options);
        if (scale.Range <= 0d)
            return string.Empty;

        var stepX = points.Count > 1
            ? plot.Width / (points.Count - 1)
            : 0f;

        var polylinePoints = new List<Point>(points.Count);
        for (var i = 0; i < points.Count; i++)
        {
            var normalized = (Math.Max(scale.Min, points[i].Value) - scale.Min) / scale.Range;
            var x = plot.X + i * stepX;
            var y = plot.Bottom - (float)(plot.Height * normalized);
            polylinePoints.Add(new Point(x, y));
        }

        var svg = new StringBuilder();
        var lineColor = options.LineColor ?? new Color(255, 31, 41, 55);
        svg.Append($"<polyline fill=\"none\" stroke=\"{CssColorToSvg(lineColor)}\" stroke-width=\"{Math.Max(1f, options.LineWidth):F1}\" points=\"");
        foreach (var point in polylinePoints)
        {
            svg.Append($"{point.X:F1},{point.Y:F1} ");
        }
        svg.Append("\" />");

        if (!options.ShowMarkers)
            return svg.ToString();

        for (var i = 0; i < polylinePoints.Count; i++)
        {
            svg.Append($"<circle cx=\"{polylinePoints[i].X:F1}\" cy=\"{polylinePoints[i].Y:F1}\" r=\"2.5\" fill=\"{CssColorToSvg(points[i].Color)}\" />");
        }

        return svg.ToString();
    }

    private static string RenderCartesianGuidesSvg(ResolvedChartType chartType, IReadOnlyList<ChartPointModel> points, Rect plot, ChartRenderOptions options)
    {
        var svg = new StringBuilder();
        var scale = BuildScale(points, options);
        var yTickCount = Math.Max(2, options.YAxisTickCount);

        for (var i = 0; i < yTickCount; i++)
        {
            var frac = yTickCount == 1 ? 0f : (float)i / (yTickCount - 1);
            var y = plot.Bottom - frac * plot.Height;

            if (options.ShowGridLines)
                svg.Append($"<line x1=\"{plot.X:F1}\" y1=\"{y:F1}\" x2=\"{plot.Right:F1}\" y2=\"{y:F1}\" stroke=\"{CssColorToSvg(options.GridLineColor)}\" stroke-width=\"{Math.Max(0.5f, options.GridLineWidth):F1}\" />");

            if (options.ShowTicks)
                svg.Append($"<line x1=\"{plot.X - options.TickLength:F1}\" y1=\"{y:F1}\" x2=\"{plot.X:F1}\" y2=\"{y:F1}\" stroke=\"{CssColorToSvg(options.AxisColor)}\" stroke-width=\"{Math.Max(0.5f, options.AxisLineWidth):F1}\" />");

            if (options.ShowTickLabels)
            {
                var value = scale.Min + scale.Range * frac;
                svg.Append($"<text x=\"{plot.X - 6f:F1}\" y=\"{y + 3f:F1}\" text-anchor=\"end\" font-size=\"{Math.Max(8f, options.LabelFontSize):F1}\" fill=\"{CssColorToSvg(options.LabelColor)}\">{value:0.##}</text>");
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
                    svg.Append($"<line x1=\"{x:F1}\" y1=\"{plot.Bottom:F1}\" x2=\"{x:F1}\" y2=\"{plot.Bottom + options.TickLength:F1}\" stroke=\"{CssColorToSvg(options.AxisColor)}\" stroke-width=\"{Math.Max(0.5f, options.AxisLineWidth):F1}\" />");

                if (options.ShowTickLabels)
                    svg.Append($"<text x=\"{x:F1}\" y=\"{plot.Bottom + Math.Max(8f, options.LabelFontSize) + 4f:F1}\" text-anchor=\"middle\" font-size=\"{Math.Max(8f, options.LabelFontSize):F1}\" fill=\"{CssColorToSvg(options.LabelColor)}\">{EscapeText(points[i].Label)}</text>");
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
                    svg.Append($"<text x=\"{plot.X - 6f:F1}\" y=\"{y + 3f:F1}\" text-anchor=\"end\" font-size=\"{Math.Max(8f, options.LabelFontSize):F1}\" fill=\"{CssColorToSvg(options.LabelColor)}\">{EscapeText(points[i].Label)}</text>");
            }

            var xTicks = Math.Max(2, options.XAxisTickCount > 0 ? options.XAxisTickCount : options.YAxisTickCount);
            for (var i = 0; i < xTicks; i++)
            {
                var frac = xTicks == 1 ? 0f : (float)i / (xTicks - 1);
                var x = plot.X + frac * plot.Width;

                if (options.ShowGridLines)
                    svg.Append($"<line x1=\"{x:F1}\" y1=\"{plot.Y:F1}\" x2=\"{x:F1}\" y2=\"{plot.Bottom:F1}\" stroke=\"{CssColorToSvg(options.GridLineColor)}\" stroke-width=\"{Math.Max(0.5f, options.GridLineWidth):F1}\" />");

                if (options.ShowTicks)
                    svg.Append($"<line x1=\"{x:F1}\" y1=\"{plot.Bottom:F1}\" x2=\"{x:F1}\" y2=\"{plot.Bottom + options.TickLength:F1}\" stroke=\"{CssColorToSvg(options.AxisColor)}\" stroke-width=\"{Math.Max(0.5f, options.AxisLineWidth):F1}\" />");

                if (options.ShowTickLabels)
                {
                    var value = scale.Min + scale.Range * frac;
                    svg.Append($"<text x=\"{x:F1}\" y=\"{plot.Bottom + Math.Max(8f, options.LabelFontSize) + 4f:F1}\" text-anchor=\"middle\" font-size=\"{Math.Max(8f, options.LabelFontSize):F1}\" fill=\"{CssColorToSvg(options.LabelColor)}\">{value:0.##}</text>");
                }
            }
        }

        if (options.ShowAxes)
        {
            svg.Append($"<line x1=\"{plot.X:F1}\" y1=\"{plot.Y:F1}\" x2=\"{plot.X:F1}\" y2=\"{plot.Bottom:F1}\" stroke=\"{CssColorToSvg(options.AxisColor)}\" stroke-width=\"{Math.Max(0.5f, options.AxisLineWidth):F1}\" />");
            svg.Append($"<line x1=\"{plot.X:F1}\" y1=\"{plot.Bottom:F1}\" x2=\"{plot.Right:F1}\" y2=\"{plot.Bottom:F1}\" stroke=\"{CssColorToSvg(options.AxisColor)}\" stroke-width=\"{Math.Max(0.5f, options.AxisLineWidth):F1}\" />");
        }

        return svg.ToString();
    }

    private static string RenderAxisTitleSvg(float width, float height, ChartRenderOptions options)
    {
        var svg = new StringBuilder();
        var size = Math.Max(8.5f, options.LabelFontSize + 0.5f);

        if (!string.IsNullOrWhiteSpace(options.XAxisLabel))
            svg.Append($"<text x=\"{width / 2f:F1}\" y=\"{height - 2f:F1}\" text-anchor=\"middle\" font-size=\"{size:F1}\" fill=\"{CssColorToSvg(options.LabelColor)}\">{EscapeText(options.XAxisLabel)}</text>");

        if (!string.IsNullOrWhiteSpace(options.YAxisLabel))
            svg.Append($"<text x=\"4\" y=\"14\" text-anchor=\"start\" font-size=\"{size:F1}\" fill=\"{CssColorToSvg(options.LabelColor)}\">{EscapeText(options.YAxisLabel)}</text>");

        return svg.ToString();
    }

    private static string RenderPieSvg(IReadOnlyList<ChartPointModel> points, Rect plot, ChartRenderOptions options, float totalWidth, float totalHeight)
    {
        var total = points.Sum(point => Math.Max(0d, point.Value));
        if (total <= 0d)
            return string.Empty;

        var radius = Math.Max(1f, MathF.Min(plot.Width, plot.Height) / 2f);
        var centerX = plot.X + plot.Width / 2f;
        var centerY = plot.Y + plot.Height / 2f;
        var startAngle = -MathF.PI / 2f;
        var svg = new StringBuilder();

        foreach (var point in points)
        {
            var proportion = (float)(Math.Max(0d, point.Value) / total);
            if (proportion <= 0f)
                continue;

            var sweep = proportion * MathF.PI * 2f;
            var endAngle = startAngle + sweep;

            svg.Append(RenderPieSliceSvg(centerX, centerY, radius, startAngle, endAngle, point.Color));

            if (options.ShowPieLabels && !string.IsNullOrWhiteSpace(point.Label))
            {
                var midAngle = startAngle + (sweep / 2f);
                var labelRadius = SectorCentroidRadius(radius, sweep);
                var label = PointOnCircle(centerX, centerY, labelRadius, midAngle);
                var labelText = EscapeText($"{point.Label} ({proportion * 100f:0.#}%)");
                var size = Math.Max(8f, options.LabelFontSize);
                var weight = ((int)(options.PieLabelFontWeight ?? FontWeight.Normal)).ToString(CultureInfo.InvariantCulture);
                svg.Append($"<text x=\"{label.X:F1}\" y=\"{label.Y:F1}\" text-anchor=\"middle\" dominant-baseline=\"middle\" font-size=\"{size:F1}\" font-weight=\"{weight}\" fill=\"{CssColorToSvg(options.PieLabelColor ?? options.LabelColor)}\">{labelText}</text>");
            }

            startAngle = endAngle;
        }

        if (options.ShowLegend)
            svg.Append(RenderSeriesLegendSvg(points, totalWidth, totalHeight, options));

        return svg.ToString();
    }

    private static string RenderSeriesLegendSvg(IReadOnlyList<ChartPointModel> points, float totalWidth, float totalHeight, ChartRenderOptions options)
    {
        if (points.Count == 0)
            return string.Empty;

        var marker = Math.Max(6f, options.LegendMarkerSize);
        var fontSize = Math.Max(8f, options.LegendFontSize);
        var lineHeight = Math.Max(marker + 2f, fontSize + 2f);
        var reserve = GetPieLegendReserve(new Rect(0f, 0f, totalWidth, totalHeight), options, points);
        var textColor = CssColorToSvg(options.LegendTextColor ?? options.PieLabelColor ?? options.LabelColor);
        var weight = ((int)(options.PieLabelFontWeight ?? FontWeight.Normal)).ToString(CultureInfo.InvariantCulture);
        const float inset = 8f;

        float startX;
        float startY;
        float availableWidth;

        switch (options.LegendPosition)
        {
            case PieLegendPosition.Left:
                startX = inset;
                startY = inset;
                availableWidth = Math.Max(40f, reserve - (inset * 2f));
                break;
            case PieLegendPosition.Bottom:
                startX = inset;
                startY = totalHeight - reserve + 4f;
                availableWidth = Math.Max(40f, totalWidth - (inset * 2f));
                break;
            case PieLegendPosition.Top:
                startX = inset;
                startY = 4f;
                availableWidth = Math.Max(40f, totalWidth - (inset * 2f));
                break;
            default:
                startX = totalWidth - reserve + inset;
                startY = inset;
                availableWidth = Math.Max(40f, reserve - (inset * 2f));
                break;
        }

        var svg = new StringBuilder();
        var availableHeight = Math.Max(20f, totalHeight - (inset * 2f));

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

                svg.Append($"<rect x=\"{x:F1}\" y=\"{y:F1}\" width=\"{marker:F1}\" height=\"{marker:F1}\" fill=\"{CssColorToSvg(points[i].Color)}\" />");
                svg.Append($"<text x=\"{x + marker + 6f:F1}\" y=\"{y + marker - 1f:F1}\" text-anchor=\"start\" font-size=\"{fontSize:F1}\" font-weight=\"{weight}\" fill=\"{textColor}\">{EscapeText(points[i].Label)}</text>");
            }

            return svg.ToString();
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

            svg.Append($"<rect x=\"{cursorX:F1}\" y=\"{cursorY:F1}\" width=\"{marker:F1}\" height=\"{marker:F1}\" fill=\"{CssColorToSvg(points[i].Color)}\" />");
            svg.Append($"<text x=\"{cursorX + marker + 6f:F1}\" y=\"{cursorY + marker - 1f:F1}\" text-anchor=\"start\" font-size=\"{fontSize:F1}\" font-weight=\"{weight}\" fill=\"{textColor}\">{EscapeText(points[i].Label)}</text>");

            cursorX += itemWidth + itemGap;
        }

        return svg.ToString();
    }

    private static float SectorCentroidRadius(float radius, float sweepAngle)
    {
        var theta = MathF.Abs(sweepAngle);
        if (theta <= 0.0001f)
            return radius * 0.62f;

        var centroid = (4f * radius * MathF.Sin(theta / 2f)) / (3f * theta);
        return Math.Clamp(centroid, radius * 0.35f, radius * 0.72f);
    }

    private static string RenderPieSliceSvg(float centerX, float centerY, float radius, float startAngle, float endAngle, Color color)
    {
        var sweep = MathF.Abs(endAngle - startAngle);
        var segmentCount = Math.Max(3, (int)MathF.Ceiling(sweep / (MathF.PI / 18f)));

        var path = new StringBuilder();
        path.Append($"M {centerX:F1} {centerY:F1} ");

        var start = PointOnCircle(centerX, centerY, radius, startAngle);
        path.Append($"L {start.X:F1} {start.Y:F1} ");

        for (var i = 1; i <= segmentCount; i++)
        {
            var t = (float)i / segmentCount;
            var angle = startAngle + (endAngle - startAngle) * t;
            var point = PointOnCircle(centerX, centerY, radius, angle);
            path.Append($"L {point.X:F1} {point.Y:F1} ");
        }

        path.Append("Z");
        return $"<path d=\"{path}\" fill=\"{CssColorToSvg(color)}\" stroke=\"#FFFFFF\" stroke-width=\"1\" />";
    }

    private static Point PointOnCircle(float centerX, float centerY, float radius, float angle)
    {
        return new Point(
            centerX + MathF.Cos(angle) * radius,
            centerY + MathF.Sin(angle) * radius);
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

    private static string EscapeText(string value)
    {
        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&#39;", StringComparison.Ordinal);
    }

    private static void RenderImageElement(HtmlBuilder html, ImageBlock imgElem)
    {
        if (TryGetImageSource(imgElem, out var source))
        {
            html.VoidTag("img", attributes: new()
            {
                ["src"] = source,
                ["alt"] = imgElem.Id,
                ["style"] = $"width: {imgElem.Bounds.Width:F1}px; height: {imgElem.Bounds.Height:F1}px; object-fit: {GetObjectFit(imgElem.Stretch)};"
            });
            return;
        }

        html
            .OpenTag("div", classAttr: "visual-image-placeholder")
            .Text($"[Image: {imgElem.SourceKey}]")
            .CloseTag("div");
    }

    private static bool TryGetImageSource(ImageBlock image, out string source)
    {
        source = string.Empty;

        if (image.ImageReference != null)
        {
            var base64 = Convert.ToBase64String(image.ImageReference.PixelData);
            var mimeType = image.ImageReference.PixelFormat switch
            {
                "RGBA8888" => "image/png",
                "PNG" => "image/png",
                "JPEG" => "image/jpeg",
                "WebP" => "image/webp",
                _ => "image/png"
            };

            source = $"data:{mimeType};base64,{base64}";
            return true;
        }

        return TryGetImageSource(image.SourceKey, out source);
    }

    private static bool TryGetImageSource(string? sourceKey, out string source)
    {
        source = string.Empty;

        if (string.IsNullOrWhiteSpace(sourceKey))
            return false;

        var key = sourceKey.Trim();
        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (key.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("/", StringComparison.Ordinal)
            || key.StartsWith("./", StringComparison.Ordinal)
            || key.StartsWith("../", StringComparison.Ordinal)
            || Uri.TryCreate(key, UriKind.Absolute, out _))
        {
            source = key;
            return true;
        }

        if (Path.IsPathRooted(key))
        {
            source = new Uri(key, UriKind.Absolute).AbsoluteUri;
            return true;
        }

        return false;
    }

    private static void RenderShapeElement(HtmlBuilder html, ShapeBlock shapeElem)
    {
        var width = shapeElem.Bounds.Width;
        var height = shapeElem.Bounds.Height;
        var svgStyle = $"width: {width:F1}px; height: {height:F1}px; display: block;";

        html
            .OpenTag("svg", style: svgStyle, id: shapeElem.Id)
            .Raw($"viewBox=\"0 0 {width:F1} {height:F1}\" ")
            .Raw(RenderSvgShape(shapeElem))
            .CloseTag("svg");
    }

    private static string RenderSvgShape(ShapeBlock shape)
    {
        var fill = shape.Fill.HasValue ? $"fill=\"{CssColorToSvg(shape.Fill.Value)}\"" : "fill=\"none\"";
        var stroke = shape.Stroke.HasValue ? $"stroke=\"{CssColorToSvg(shape.Stroke.Value)}\" stroke-width=\"{shape.StrokeWidth:F1}\"" : "stroke=\"none\"";
        var attrs = $"{fill} {stroke}";

        return shape.Kind switch
        {
            ShapeKind.Rectangle => $"<rect x=\"0\" y=\"0\" width=\"{shape.Bounds.Width:F1}\" height=\"{shape.Bounds.Height:F1}\" {attrs} />",
            ShapeKind.Ellipse => $"<ellipse cx=\"{shape.Bounds.Width / 2:F1}\" cy=\"{shape.Bounds.Height / 2:F1}\" rx=\"{shape.Bounds.Width / 2:F1}\" ry=\"{shape.Bounds.Height / 2:F1}\" {attrs} />",
            ShapeKind.Line => $"<line x1=\"0\" y1=\"0\" x2=\"{shape.Bounds.Width:F1}\" y2=\"{shape.Bounds.Height:F1}\" {stroke} />",
            _ => string.Empty
        };
    }

    private static void RenderBarcodeElement(HtmlBuilder html, BarcodeBlock barcode, int pageNumber)
    {
        var resolvedValue = ResolveSystemTextTokens(barcode.Value, pageNumber);
        var symbolBounds = GetBarcodeSymbolBounds(barcode.Bounds, barcode.SymbologyType, barcode.Symbology);

        if (BarcodeImageFactory.TryCreateDataUri(barcode.SymbologyType, barcode.Symbology, resolvedValue, symbolBounds, out var source))
        {
            html.VoidTag("img", attributes: new()
            {
                ["src"] = source,
                ["alt"] = barcode.Id,
                ["style"] = $"width: {symbolBounds.Width:F1}px; height: {symbolBounds.Height:F1}px; object-fit: fill;"
            });
        }
        else
        {
            html
                .OpenTag("div", classAttr: "visual-barcode-placeholder")
                .Text($"[Barcode: {barcode.Symbology}]")
                .CloseTag("div");
        }

        if (barcode.ShowText && !string.IsNullOrWhiteSpace(resolvedValue))
        {
            html
                .OpenTag("figcaption", classAttr: "barcode-caption")
                .Text(resolvedValue)
                .CloseTag("figcaption");
        }
    }

    private static void RenderContentBlockBarcode(HtmlBuilder html, ContentBlock barcode, int pageNumber)
    {
        var resolvedValue = ResolveSystemTextTokens(barcode.Value ?? string.Empty, pageNumber);
        var symbolBounds = GetBarcodeSymbolBounds(barcode.Bounds, barcode.SymbologyType, barcode.Symbology);

        if (BarcodeImageFactory.TryCreateDataUri(barcode.SymbologyType, barcode.Symbology, resolvedValue, symbolBounds, out var source))
        {
            html.VoidTag("img", attributes: new()
            {
                ["src"] = source,
                ["alt"] = barcode.Id,
                ["style"] = $"width: {symbolBounds.Width:F1}px; height: {symbolBounds.Height:F1}px; object-fit: fill;"
            });
        }
        else
        {
            html
                .OpenTag("div", classAttr: "visual-barcode-placeholder")
                .Text($"[Barcode: {barcode.Symbology}]")
                .CloseTag("div");
        }

        if (barcode.ShowText && !string.IsNullOrWhiteSpace(resolvedValue))
        {
            html
                .OpenTag("figcaption", classAttr: "barcode-caption")
                .Text(resolvedValue)
                .CloseTag("figcaption");
        }
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

    private static void RenderTableElement(HtmlBuilder html, TableBlock tableElem, int pageNumber)
    {
        html.OpenTag("table", classAttr: "kinetic-table");

        var headerRows = tableElem.Rows.Where(r => r.RowType == RowType.Header).ToList();
        var bodyRows = tableElem.Rows.Where(r => r.RowType != RowType.Header).ToList();

        if (headerRows.Count > 0)
        {
            html.OpenTag("thead");
            foreach (var row in headerRows)
            {
                RenderTableRow(html, row, "th", pageNumber);
            }
            html.CloseTag("thead");
        }

        html.OpenTag("tbody");
        foreach (var row in bodyRows)
        {
            RenderTableRow(html, row, "td", pageNumber);
        }
        html.CloseTag("tbody");

        html.CloseTag("table");
    }

    private static void RenderTableRow(HtmlBuilder html, RowBlock row, string cellTag, int pageNumber)
    {
        html.OpenTag("tr", classAttr: row.RowType == RowType.Header ? "kinetic-table-header-row" : "kinetic-table-row");

        foreach (var cell in row.Cells)
        {
            var cellStyle = $"position: relative; width: {cell.Bounds.Width:F1}px; height: {cell.Bounds.Height:F1}px; {CssBuilder.BuildStyle(cell.Style)}";
            var attributes = new Dictionary<string, string>();

            if (cell.ColSpan > 1)
                attributes["colspan"] = cell.ColSpan.ToString();

            if (cell.RowSpan > 1)
                attributes["rowspan"] = cell.RowSpan.ToString();

            html.OpenTag(cellTag, style: cellStyle, attributes: attributes.Count == 0 ? null : attributes);

            foreach (var child in cell.Children)
                RenderTableCellChild(html, child, pageNumber);

            html.CloseTag(cellTag);
        }

        html.CloseTag("tr");
    }

    private static void RenderTableCellChild(HtmlBuilder html, LayoutBlock child, int pageNumber)
    {
        if (child is TextBlock textElem)
        {
            RenderTextBlockInline(html, textElem, pageNumber);
            return;
        }

        if (child is ContentBlock content)
        {
            RenderContentBlockCellChildInline(html, content, pageNumber);
            return;
        }

        if (child is ContainerBlock container)
        {
            foreach (var grandchild in container.Children)
            {
                RenderTableCellChild(html, grandchild, pageNumber);
            }

            return;
        }

        if (child is CellBlock cell)
        {
            foreach (var grandchild in cell.Children)
            {
                RenderTableCellChild(html, grandchild, pageNumber);
            }

            return;
        }

        RenderElement(html, child, child.Bounds.X, child.Bounds.Y, pageNumber);
    }

    private static void RenderTextBlockInline(HtmlBuilder html, TextBlock textBlock, int pageNumber)
    {
        if (textBlock.TextRuns.Count == 0)
        {
            html
                .OpenTag("span", style: CssBuilder.BuildStyle(textBlock.Style))
                .Text(ResolveSystemTextTokens(textBlock.Text, pageNumber))
                .CloseTag("span");
            return;
        }

        foreach (var run in textBlock.TextRuns)
        {
            html
                .OpenTag("span", style: CssBuilder.BuildStyle(run.Style))
                .Text(ResolveSystemTextTokens(run.Text, pageNumber))
                .CloseTag("span");
        }
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

    private static string BuildElementStyle(LayoutBlock element, float parentX, float parentY)
    {
        var bounds = element.Bounds;
        var relativeX = bounds.X - parentX;
        var relativeY = bounds.Y - parentY;
        var tableRelatedContainer = element.LayoutBlockType is LayoutBlockType.ContentRegion
            or LayoutBlockType.Section
            or LayoutBlockType.Container
            or LayoutBlockType.Table
            or LayoutBlockType.Row
            or LayoutBlockType.Cell;

        var overflow = tableRelatedContainer
            ? "overflow: visible;"
            : (element.Style.Overflow == Overflow.Hidden ? "overflow: hidden;" : "overflow: visible;");

        var css = $"position: absolute; left: {relativeX:F1}px; top: {relativeY:F1}px; width: {bounds.Width:F1}px; height: {bounds.Height:F1}px; " +
                  CssBuilder.BuildStyle(element.Style) +
                  overflow;
        return css;
    }

    private static string GetElementClassName(LayoutBlock element) =>
        element.LayoutBlockType switch
        {
            LayoutBlockType.Text => "text-element",
            LayoutBlockType.Image => "image-element",
            LayoutBlockType.Shape => "shape-element",
            LayoutBlockType.Table => "table-element",
            LayoutBlockType.Container => "container-element",
            LayoutBlockType.Section => "section-element",
            LayoutBlockType.ContentRegion => "content-region-element",
            LayoutBlockType.Page => "page-element",
            LayoutBlockType.Row => "row-element",
            LayoutBlockType.Cell => "cell-element",
            LayoutBlockType.Chart => "chart-element",
            LayoutBlockType.Barcode => "barcode-element",
            _ => "unknown-element"
        };

    private static string GetElementTagName(LayoutBlock element) =>
        element.LayoutBlockType switch
        {
            LayoutBlockType.Text => "p",
            LayoutBlockType.Image => "figure",
            LayoutBlockType.Shape => "figure",
            LayoutBlockType.Table => "section",
            LayoutBlockType.Container => "section",
            LayoutBlockType.Section => "section",
            LayoutBlockType.ContentRegion => "article",
            LayoutBlockType.Row => "div",
            LayoutBlockType.Cell => "div",
            LayoutBlockType.Chart => "figure",
            LayoutBlockType.Barcode => "figure",
            _ => "div"
        };

    private static string GetObjectFit(ImageStretch stretch) => stretch switch
    {
        ImageStretch.None => "none",
        ImageStretch.Fill => "fill",
        ImageStretch.Uniform => "contain",
        ImageStretch.UniformToFill => "cover",
        _ => "contain"
    };

    private static string CssColorToSvg(in Color color) =>
        $"rgba({color.R},{color.G},{color.B},{color.A / 255f:F2})";

}
