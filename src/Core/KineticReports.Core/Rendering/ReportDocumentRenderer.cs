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
                // Chart rendering is renderer-specific; this is a placeholder
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
