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
                // Note: ContentBlock for images doesn't have ImageReference set by layout engine yet
                // This will be implemented in Phase 3.4+ when image resolution is handled
                break;

            case BlockContentType.Shape:
                RenderShape(content, context);
                break;

            case BlockContentType.Chart:
                // Chart rendering is renderer-specific; this is a placeholder
                break;

            case BlockContentType.Barcode:
                // Barcode rendering is renderer-specific; this is a placeholder
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
}
