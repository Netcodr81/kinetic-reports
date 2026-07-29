namespace KineticReports.Core.Export.Html;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using Microsoft.Extensions.Options;
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

            case ChartBlock:
                html.Text("[Chart Element]");
                break;

            case BarcodeBlock:
                html.Text("[Barcode Element]");
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
                html.Text("[Chart Element]");
                break;

            case BlockContentType.Barcode:
                html.Text("[Barcode Element]");
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
