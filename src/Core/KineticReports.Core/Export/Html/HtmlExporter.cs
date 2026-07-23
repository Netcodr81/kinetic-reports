namespace KineticReports.Core.Export.Html;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using System.Text.Json;
using System.Text;
using KineticReports.Core.Export.Html.Styles;

/// <summary>
/// Exports an immutable ReportDocument to semantic HTML with inline CSS.
/// </summary>
public sealed class HtmlExporter : IHtmlExporter
{
    /// <summary>
    /// Exports the report document to HTML.
    /// </summary>
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

    private static void BuildHtmlDocument(HtmlBuilder html, ReportDocument reportDocument)
    {
        html
            .Raw("<!DOCTYPE html>\n")
            .OpenTag("html", classAttr: "kinetic-report")
            .OpenTag("head")
            .VoidTag("meta", attributes: new() { ["charset"] = "UTF-8" })
            .OpenTag("title").Text("Report").CloseTag("title")
            .OpenTag("style")
            .Raw(HtmlExportStylesheet.Css)
            .CloseTag("style")
            .CloseTag("head")
            .OpenTag("body");

        var reportModel = BuildReportDocumentModelJson(reportDocument);

        html
            .OpenTag("script", id: "report-document-model", attributes: new Dictionary<string, string> { ["type"] = "application/json" })
            .Raw(reportModel)
            .CloseTag("script")
            .OpenTag("div", id: "report-document", classAttr: "report-document");

        // Render all pages
        foreach (var page in reportDocument.Pages)
        {
            RenderPage(html, page);
        }

        html
            .CloseTag("div")
            .CloseTag("body")
            .CloseTag("html");
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
            RenderElement(html, page.Header, 0f, 0f);
            html.CloseTag("header");
        }

        // Render body elements
        html.OpenTag("main", classAttr: "page-body-region");
        foreach (var child in page.Children)
        {
            RenderElement(html, child, 0f, 0f);
        }
        html.CloseTag("main");

        // Render footer if present
        if (page.Footer != null)
        {
            html.OpenTag("footer", classAttr: "page-footer-region");
            RenderElement(html, page.Footer, 0f, 0f);
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
            ContainerBlock container => container.Children,
            SectionBlock section => section.Children,
            ReportBlock reportBlock => reportBlock.Children,
            RowBlock row => row.Cells.Cast<LayoutBlock>().ToList(),
            CellBlock cell => cell.Children,
            _ => []
        };
    }

    private static void RenderElement(HtmlBuilder html, LayoutBlock element, float parentX, float parentY)
    {
        var style = BuildElementStyle(element, parentX, parentY);

        html.OpenTag("div", style: style, id: element.Id, classAttr: $"element {GetElementClassName(element)}");

        switch (element)
        {
            case TextBlock textElem:
                RenderTextBlock(html, textElem);
                break;

            case ImageBlock imgElem:
                RenderImageElement(html, imgElem);
                break;

            case ShapeBlock shapeElem:
                RenderShapeElement(html, shapeElem);
                break;

            case TableBlock tableElem:
                RenderTableElement(html, tableElem);
                break;

            case ContainerBlock container:
                foreach (var child in container.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;

            case SectionBlock section:
                foreach (var child in section.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;

            case ReportBlock block:
                foreach (var child in block.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;

            case RowBlock row:
                foreach (var cell in row.Cells)
                    RenderElement(html, cell, element.Bounds.X, element.Bounds.Y);
                break;

            case CellBlock cell:
                foreach (var child in cell.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;

            case ChartBlock:
                html.Text("[Chart Element]");
                break;

            case BarcodeBlock:
                html.Text("[Barcode Element]");
                break;
        }

        html.CloseTag("div");
    }

    private static void RenderTextBlock(HtmlBuilder html, TextBlock textBlock)
    {
        if (textBlock.TextRuns.Count == 0)
        {
            html.Text(textBlock.Text);
        }
        else
        {
            foreach (var run in textBlock.TextRuns)
            {
                var runStyle = CssBuilder.BuildStyle(run.Style);
                html
                    .OpenTag("p", style: runStyle)
                    .Text(run.Text)
                    .CloseTag("p");
            }
        }
    }

    private static void RenderImageElement(HtmlBuilder html, ImageBlock imgElem)
    {
        if (imgElem.ImageReference != null)
        {
            var base64 = Convert.ToBase64String(imgElem.ImageReference.PixelData);
            var mimeType = imgElem.ImageReference.PixelFormat switch
            {
                "RGBA8888" => "image/png",
                "PNG" => "image/png",
                "JPEG" => "image/jpeg",
                "WebP" => "image/webp",
                _ => "image/png"
            };
            var dataUrl = $"data:{mimeType};base64,{base64}";

            html.VoidTag("img", attributes: new()
            {
                ["src"] = dataUrl,
                ["alt"] = imgElem.Id,
                ["style"] = $"width: {imgElem.Bounds.Width:F1}px; height: {imgElem.Bounds.Height:F1}px; object-fit: {GetObjectFit(imgElem.Stretch)};"
            });
        }
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

    private static void RenderTableElement(HtmlBuilder html, TableBlock tableElem)
    {
        html.OpenTag("table", classAttr: "kinetic-table");

        var headerRows = tableElem.Rows.Where(r => r.RowType == RowType.Header).ToList();
        var bodyRows = tableElem.Rows.Where(r => r.RowType != RowType.Header).ToList();

        if (headerRows.Count > 0)
        {
            html.OpenTag("thead");
            foreach (var row in headerRows)
            {
                RenderTableRow(html, row, "th");
            }
            html.CloseTag("thead");
        }

        html.OpenTag("tbody");
        foreach (var row in bodyRows)
        {
            RenderTableRow(html, row, "td");
        }
        html.CloseTag("tbody");

        html.CloseTag("table");
    }

    private static void RenderTableRow(HtmlBuilder html, RowBlock row, string cellTag)
    {
        html.OpenTag("tr", classAttr: row.RowType == RowType.Header ? "kinetic-table-header-row" : "kinetic-table-row");

        foreach (var cell in row.Cells)
        {
            var cellStyle = $"width: {cell.Bounds.Width:F1}px; height: {cell.Bounds.Height:F1}px; {CssBuilder.BuildStyle(cell.Style)}";
            var attributes = new Dictionary<string, string>();

            if (cell.ColSpan > 1)
                attributes["colspan"] = cell.ColSpan.ToString();

            if (cell.RowSpan > 1)
                attributes["rowspan"] = cell.RowSpan.ToString();

            html.OpenTag(cellTag, style: cellStyle, attributes: attributes.Count == 0 ? null : attributes);

            foreach (var child in cell.Children)
                RenderTableCellChild(html, child);

            html.CloseTag(cellTag);
        }

        html.CloseTag("tr");
    }

    private static void RenderTableCellChild(HtmlBuilder html, LayoutBlock child)
    {
        if (child is TextBlock textElem)
        {
            RenderTextBlockInline(html, textElem);
            return;
        }

        RenderElement(html, child, child.Bounds.X, child.Bounds.Y);
    }

    private static void RenderTextBlockInline(HtmlBuilder html, TextBlock textBlock)
    {
        if (textBlock.TextRuns.Count == 0)
        {
            html
                .OpenTag("span", style: CssBuilder.BuildStyle(textBlock.Style))
                .Text(textBlock.Text)
                .CloseTag("span");
            return;
        }

        foreach (var run in textBlock.TextRuns)
        {
            html
                .OpenTag("span", style: CssBuilder.BuildStyle(run.Style))
                .Text(run.Text)
                .CloseTag("span");
        }
    }

    private static string BuildElementStyle(LayoutBlock element, float parentX, float parentY)
    {
        var bounds = element.Bounds;
        var relativeX = bounds.X - parentX;
        var relativeY = bounds.Y - parentY;

        var css = $"position: absolute; left: {relativeX:F1}px; top: {relativeY:F1}px; width: {bounds.Width:F1}px; height: {bounds.Height:F1}px; " +
                  CssBuilder.BuildStyle(element.Style) +
                  (element.Style.Overflow == Overflow.Hidden ? "overflow: hidden;" : "overflow: visible;");
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
