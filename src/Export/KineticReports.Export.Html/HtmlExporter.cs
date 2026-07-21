namespace KineticReports.Export.Html;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Export.Html.Styles;
using System.Text;

/// <summary>
/// Exports an immutable ReportLayout to semantic HTML with inline CSS.
/// </summary>
public sealed class HtmlExporter : IHtmlExporter
{
    /// <summary>
    /// Exports the report layout to HTML.
    /// </summary>
    public async Task ExportAsync(ReportLayout reportLayout, Stream output, CancellationToken ct = default)
    {
        if (reportLayout == null) throw new ArgumentNullException(nameof(reportLayout));
        if (output == null) throw new ArgumentNullException(nameof(output));

        var builder = new HtmlBuilder();
        BuildHtmlDocument(builder, reportLayout);

        var html = builder.Build();
        var bytes = Encoding.UTF8.GetBytes(html);
        await output.WriteAsync(bytes, ct);
    }

    private static void BuildHtmlDocument(HtmlBuilder html, ReportLayout reportLayout)
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

        // Render all pages
        foreach (var page in reportLayout.Pages)
        {
            RenderPage(html, page);
        }

        html
            .CloseTag("body")
            .CloseTag("html");
    }

    private static void RenderPage(HtmlBuilder html, PageElement page)
    {
        var pageStyle = $"width: {page.PageWidth:F1}px; height: {page.PageHeight:F1}px; position: relative; margin: 20px auto;";

        html
            .OpenTag("div", style: pageStyle, id: $"page-{page.PageNumber}", classAttr: "kinetic-page")
            .OpenTag("div", style: CssBuilder.BuildStyle(page.Style), classAttr: "page-background");

        // Render header if present
        if (page.Header != null)
        {
            RenderElement(html, page.Header, 0f, 0f);
        }

        // Render body elements
        foreach (var child in page.Children)
        {
            RenderElement(html, child, 0f, 0f);
        }

        // Render footer if present
        if (page.Footer != null)
        {
            RenderElement(html, page.Footer, 0f, 0f);
        }

        html
            .CloseTag("div") // page-background
            .CloseTag("div"); // kinetic-page
    }

    private static void RenderElement(HtmlBuilder html, LayoutElement element, float parentX, float parentY)
    {
        var style = BuildElementStyle(element, parentX, parentY);

        html.OpenTag("div", style: style, id: element.Id, classAttr: $"element {GetElementClassName(element)}");

        switch (element)
        {
            case TextElement textElem:
                RenderTextElement(html, textElem);
                break;

            case ImageElement imgElem:
                RenderImageElement(html, imgElem);
                break;

            case ShapeElement shapeElem:
                RenderShapeElement(html, shapeElem);
                break;

            case TableElement tableElem:
                RenderTableElement(html, tableElem);
                break;

            case ContainerElement container:
                foreach (var child in container.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;

            case SectionElement section:
                foreach (var child in section.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;

            case BandElement band:
                foreach (var child in band.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;

            case RowElement row:
                foreach (var cell in row.Cells)
                    RenderElement(html, cell, element.Bounds.X, element.Bounds.Y);
                break;

            case CellElement cell:
                foreach (var child in cell.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;

            case ChartElement:
                html.Text("[Chart Element]");
                break;

            case BarcodeElement:
                html.Text("[Barcode Element]");
                break;
        }

        html.CloseTag("div");
    }

    private static void RenderTextElement(HtmlBuilder html, TextElement textElem)
    {
        if (textElem.TextRuns.Count == 0)
        {
            html.Text(textElem.Text);
        }
        else
        {
            foreach (var run in textElem.TextRuns)
            {
                var runStyle = CssBuilder.BuildStyle(run.Style);
                html
                    .OpenTag("p", style: runStyle)
                    .Text(run.Text)
                    .CloseTag("p");
            }
        }
    }

    private static void RenderImageElement(HtmlBuilder html, ImageElement imgElem)
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

    private static void RenderShapeElement(HtmlBuilder html, ShapeElement shapeElem)
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

    private static string RenderSvgShape(ShapeElement shape)
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

    private static void RenderTableElement(HtmlBuilder html, TableElement tableElem)
    {
        html.OpenTag("table", classAttr: "kinetic-table");

        var headerRows = tableElem.Rows.Where(r => r.Kind == RowKind.Header).ToList();
        var bodyRows = tableElem.Rows.Where(r => r.Kind != RowKind.Header).ToList();

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

    private static void RenderTableRow(HtmlBuilder html, RowElement row, string cellTag)
    {
        html.OpenTag("tr", classAttr: row.Kind == RowKind.Header ? "kinetic-table-header-row" : "kinetic-table-row");

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

    private static void RenderTableCellChild(HtmlBuilder html, LayoutElement child)
    {
        if (child is TextElement textElem)
        {
            RenderTextElementInline(html, textElem);
            return;
        }

        RenderElement(html, child, child.Bounds.X, child.Bounds.Y);
    }

    private static void RenderTextElementInline(HtmlBuilder html, TextElement textElem)
    {
        if (textElem.TextRuns.Count == 0)
        {
            html
                .OpenTag("span", style: CssBuilder.BuildStyle(textElem.Style))
                .Text(textElem.Text)
                .CloseTag("span");
            return;
        }

        foreach (var run in textElem.TextRuns)
        {
            html
                .OpenTag("span", style: CssBuilder.BuildStyle(run.Style))
                .Text(run.Text)
                .CloseTag("span");
        }
    }

    private static string BuildElementStyle(LayoutElement element, float parentX, float parentY)
    {
        var bounds = element.Bounds;
        var relativeX = bounds.X - parentX;
        var relativeY = bounds.Y - parentY;

        var css = $"position: absolute; left: {relativeX:F1}px; top: {relativeY:F1}px; width: {bounds.Width:F1}px; height: {bounds.Height:F1}px; " +
                  CssBuilder.BuildStyle(element.Style) +
                  (element.Style.Overflow == Overflow.Hidden ? "overflow: hidden;" : "overflow: visible;");
        return css;
    }

    private static string GetElementClassName(LayoutElement element) =>
        element.ElementType switch
        {
            LayoutElementType.Text => "text-element",
            LayoutElementType.Image => "image-element",
            LayoutElementType.Shape => "shape-element",
            LayoutElementType.Table => "table-element",
            LayoutElementType.Container => "container-element",
            LayoutElementType.Section => "section-element",
            LayoutElementType.Band => "band-element",
            LayoutElementType.Page => "page-element",
            LayoutElementType.Row => "row-element",
            LayoutElementType.Cell => "cell-element",
            LayoutElementType.Chart => "chart-element",
            LayoutElementType.Barcode => "barcode-element",
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
