namespace KineticReports.Core.Export.Html;

using KineticReports.Core.Visual;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

/// <summary>
/// Exports a <see cref="VisualDocument"/> to semantic HTML.
/// </summary>
public sealed class VisualHtmlExporter : IVisualHtmlExporter
{
    private readonly HtmlExportOptions _options;

    /// <inheritdoc/>
    public VisualHtmlExporter(IOptions<HtmlExportOptions>? options = null)
    {
        _options = options?.Value ?? new HtmlExportOptions();
    }

    public async Task ExportAsync(VisualDocument visualDocument, Stream output, CancellationToken ct = default)
    {
        if (visualDocument == null) throw new ArgumentNullException(nameof(visualDocument));
        if (output == null) throw new ArgumentNullException(nameof(output));

        var builder = new HtmlBuilder();
        BuildHtmlDocument(builder, visualDocument);

        var html = builder.Build();
        var bytes = Encoding.UTF8.GetBytes(html);
        await output.WriteAsync(bytes, ct);
    }

    private void BuildHtmlDocument(HtmlBuilder html, VisualDocument visualDocument)
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

        html
            .OpenTag("script", id: "report-document-model", attributes: new Dictionary<string, string> { ["type"] = "application/json" })
            .Raw(BuildVisualDocumentModelJson(visualDocument))
            .CloseTag("script")
            .OpenTag("main", id: "report-document", classAttr: "report-document");

        foreach (var page in visualDocument.Pages)
            RenderPage(html, page);

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

    private static void RenderPage(HtmlBuilder html, VisualPage page)
    {
        var pageStyle = $"width: {page.Width:F1}px; height: {page.Height:F1}px; position: relative; margin: 20px auto;";

        html
            .OpenTag("section", style: pageStyle, id: $"page-{page.PageNumber}", classAttr: "kinetic-page page-block")
            .OpenTag("div", classAttr: "page-background");

        foreach (var layer in page.Layers)
        {
            var (tag, className) = ResolveLayerWrapper(layer.Name);
            html.OpenTag(tag, classAttr: className);

            foreach (var element in layer.Elements)
                RenderElement(html, element, 0f, 0f);

            html.CloseTag(tag);
        }

        html
            .CloseTag("div")
            .CloseTag("section");
    }

    private static void RenderElement(HtmlBuilder html, VisualElement element, float parentX, float parentY)
    {
        var style = BuildElementStyle(element, parentX, parentY);
        var tag = GetElementTagName(element);
        html.OpenTag(tag, style: style, id: element.Id, classAttr: $"element {GetElementClassName(element)}");

        switch (element)
        {
            case VisualText text:
                html
                    .OpenTag("span", style: BuildTextStyle(text))
                    .Text(text.Text)
                    .CloseTag("span");
                break;

            case VisualImage image:
                RenderImage(html, image);
                break;

            case VisualShape shape:
                RenderShape(html, shape);
                break;

            case VisualTablePlaceholder table:
                html
                    .OpenTag("div", classAttr: "visual-table-placeholder")
                    .Text($"[Table Placeholder] Rows: {table.RowCount?.ToString() ?? "?"}, Columns: {table.ColumnCount?.ToString() ?? "?"}")
                    .CloseTag("div");
                break;

            case VisualUnsupportedElement unsupported:
                html
                    .OpenTag("div", classAttr: "visual-unsupported")
                    .Text($"[Unsupported: {unsupported.SourceType}] {unsupported.Reason}")
                    .CloseTag("div");
                break;

            case VisualContainer container:
                foreach (var child in container.Children)
                    RenderElement(html, child, element.Bounds.X, element.Bounds.Y);
                break;
        }

        html.CloseTag(tag);
    }

    private static void RenderImage(HtmlBuilder html, VisualImage image)
    {
        var source = string.IsNullOrWhiteSpace(image.SourceKey)
            ? string.Empty
            : image.SourceKey;

        if (Uri.TryCreate(source, UriKind.Absolute, out _) || source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            html.VoidTag("img", attributes: new()
            {
                ["src"] = source,
                ["alt"] = image.Id,
                ["style"] = $"width: {image.Bounds.Width:F1}px; height: {image.Bounds.Height:F1}px; object-fit: {GetObjectFit(image.Stretch)};"
            });
            return;
        }

        html
            .OpenTag("div", classAttr: "visual-image-placeholder")
            .Text($"[Image: {source}]")
            .CloseTag("div");
    }

    private static void RenderShape(HtmlBuilder html, VisualShape shape)
    {
        var width = shape.Bounds.Width;
        var height = shape.Bounds.Height;

        html
            .OpenTag("svg", style: $"width: {width:F1}px; height: {height:F1}px; display: block;", id: shape.Id)
            .Raw($"viewBox=\"0 0 {width:F1} {height:F1}\" ")
            .Raw(RenderSvgShape(shape))
            .CloseTag("svg");
    }

    private static string RenderSvgShape(VisualShape shape)
    {
        var fill = string.IsNullOrWhiteSpace(shape.FillColorHex) ? "fill=\"none\"" : $"fill=\"{shape.FillColorHex}\"";
        var stroke = string.IsNullOrWhiteSpace(shape.StrokeColorHex)
            ? "stroke=\"none\""
            : $"stroke=\"{shape.StrokeColorHex}\" stroke-width=\"{shape.StrokeWidth:F1}\"";
        var attrs = $"{fill} {stroke}";

        return shape.Kind switch
        {
            VisualShapeKind.Rectangle => $"<rect x=\"0\" y=\"0\" width=\"{shape.Bounds.Width:F1}\" height=\"{shape.Bounds.Height:F1}\" {attrs} />",
            VisualShapeKind.Ellipse => $"<ellipse cx=\"{shape.Bounds.Width / 2:F1}\" cy=\"{shape.Bounds.Height / 2:F1}\" rx=\"{shape.Bounds.Width / 2:F1}\" ry=\"{shape.Bounds.Height / 2:F1}\" {attrs} />",
            VisualShapeKind.Line => $"<line x1=\"0\" y1=\"0\" x2=\"{shape.Bounds.Width:F1}\" y2=\"{shape.Bounds.Height:F1}\" {stroke} />",
            _ => string.Empty
        };
    }

    private static string BuildTextStyle(VisualText text)
    {
        var style = new StringBuilder();
        style.Append($"font-family: {text.FontFamily};");
        style.Append($"font-size: {text.FontSize:F1}px;");
        if (!string.IsNullOrWhiteSpace(text.FontWeight))
            style.Append($"font-weight: {text.FontWeight};");
        if (!string.IsNullOrWhiteSpace(text.TextColorHex))
            style.Append($"color: {text.TextColorHex};");
        return style.ToString();
    }

    private static string BuildElementStyle(VisualElement element, float parentX, float parentY)
    {
        var bounds = element.Bounds;
        var relativeX = bounds.X - parentX;
        var relativeY = bounds.Y - parentY;

        var style = new StringBuilder();
        style.Append($"position: absolute; left: {relativeX:F1}px; top: {relativeY:F1}px; width: {bounds.Width:F1}px; height: {bounds.Height:F1}px;");

        if (element.Clips.Count > 0)
            style.Append("overflow: hidden;");

        return style.ToString();
    }

    private static (string Tag, string ClassName) ResolveLayerWrapper(string layerName)
    {
        if (string.Equals(layerName, "header", StringComparison.OrdinalIgnoreCase))
            return ("header", "page-header-region");

        if (string.Equals(layerName, "footer", StringComparison.OrdinalIgnoreCase))
            return ("footer", "page-footer-region");

        if (string.Equals(layerName, "body", StringComparison.OrdinalIgnoreCase))
            return ("main", "page-body-region");

        return ("div", "page-body-region");
    }

    private static string BuildVisualDocumentModelJson(VisualDocument visualDocument)
    {
        var model = new
        {
            schemaVersion = visualDocument.SchemaVersion,
            pageCount = visualDocument.Pages.Count,
            pages = visualDocument.Pages.Select(page => new
            {
                pageNumber = page.PageNumber,
                pageWidth = page.Width,
                pageHeight = page.Height,
                layers = page.Layers.Select(layer => new
                {
                    name = layer.Name,
                    elements = layer.Elements.Select(ToElementModel).ToList()
                }).ToList()
            }).ToList()
        };

        return JsonSerializer.Serialize(model);
    }

    private static object ToElementModel(VisualElement element)
    {
        return new
        {
            id = element.Id,
            type = element.GetType().Name,
            bounds = new
            {
                x = element.Bounds.X,
                y = element.Bounds.Y,
                width = element.Bounds.Width,
                height = element.Bounds.Height
            },
            children = element is VisualContainer container
                ? container.Children.Select(ToElementModel).ToList()
                : new List<object>()
        };
    }

    private static string GetElementClassName(VisualElement element) => element switch
    {
        VisualText => "text-element",
        VisualImage => "image-element",
        VisualShape => "shape-element",
        VisualTablePlaceholder => "table-element",
        VisualContainer => "container-element",
        _ => "unknown-element"
    };

    private static string GetElementTagName(VisualElement element) => element switch
    {
        VisualText => "p",
        VisualImage => "figure",
        VisualShape => "figure",
        VisualTablePlaceholder => "section",
        VisualContainer => "section",
        VisualUnsupportedElement => "aside",
        _ => "div"
    };

    private static string GetObjectFit(VisualImageStretch stretch) => stretch switch
    {
        VisualImageStretch.None => "none",
        VisualImageStretch.Fill => "fill",
        VisualImageStretch.Uniform => "contain",
        VisualImageStretch.UniformToFill => "cover",
        _ => "contain"
    };
}
