namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;

/// <summary>
/// Unified layout block representing any type of content in a report.
/// This consolidates all 12 previous block types (TextBlock, ImageBlock, etc.) into a single
/// sealed class with type-discriminated properties.
///
/// Usage pattern: Create with the required <see cref="ContentType"/>, then initialize
/// content-specific properties as needed (e.g., <see cref="Text"/> for Text blocks,
/// <see cref="SourceKey"/> for Image blocks).
/// </summary>
public sealed class ContentBlock : LayoutBlock
{
    private ILayoutSizingContext? _layoutSizingContext;

    /// <summary>Gets the content type of this block, which determines which properties are meaningful.</summary>
    public required BlockContentType ContentType { get; init; }

    /// <inheritdoc/>
    public override LayoutBlockType LayoutBlockType => LayoutBlockType.Container;

    // ===== Text Properties =====
    /// <summary>Gets the display text content (for Text blocks after expression evaluation).</summary>
    public string? Text { get; init; }

    /// <summary>Gets the shaped, word-wrapped text runs produced by the Arrange pass (Text blocks only).</summary>
    public IReadOnlyList<TextRun> TextRuns { get; private set; } = [];

    // ===== Container Properties =====
    /// <summary>Gets the child blocks (for Container, Table, Row, ReportSection, PageSection blocks).</summary>
    public IReadOnlyList<ContentBlock> Children { get; init; } = [];

    // ===== Table/Cell Properties =====
    /// <summary>Gets the zero-based column index of this cell within its row (Cell blocks only).</summary>
    public int ColumnIndex { get; init; }

    /// <summary>Gets the number of columns this cell spans (Cell blocks only).</summary>
    public int ColSpan { get; init; } = 1;

    /// <summary>Gets the number of rows this cell spans (Cell blocks only).</summary>
    public int RowSpan { get; init; } = 1;

    // ===== Image Properties =====
    /// <summary>Gets the image source key or URI (Image blocks only).</summary>
    public string? SourceKey { get; init; }

    /// <summary>Gets the image stretch behavior (Image blocks only).</summary>
    public ImageStretch Stretch { get; init; } = ImageStretch.Uniform;

    // ===== Shape Properties =====
    /// <summary>Gets the kind of shape to draw (Shape blocks only).</summary>
    public ShapeKind Kind { get; init; } = ShapeKind.Rectangle;

    /// <summary>Gets the fill color. <see langword="null"/> means the shape is not filled (Shape blocks only).</summary>
    public Color? Fill { get; init; }

    /// <summary>Gets the stroke (outline) color. <see langword="null"/> means no outline (Shape blocks only).</summary>
    public Color? Stroke { get; init; }

    /// <summary>Gets the stroke width in DIPs (Shape blocks only).</summary>
    public float StrokeWidth { get; init; } = 1f;

    // ===== Barcode Properties =====
    /// <summary>Gets the strongly typed barcode symbology (Barcode blocks only).</summary>
    public BarcodeSymbology? SymbologyType { get; init; }

    /// <summary>Gets the barcode symbology identifier (e.g., "QR", "Code128", "EAN13"; Barcode blocks only).</summary>
    public string? Symbology { get; init; }

    /// <summary>Gets the value to encode in the barcode (Barcode blocks only).</summary>
    public string? Value { get; init; }

    /// <summary>Gets whether a human-readable text label is rendered beneath the barcode (Barcode blocks only).</summary>
    public bool ShowText { get; init; } = true;

    // ===== Chart Properties =====
    /// <summary>Gets the strongly typed chart type (Chart blocks only).</summary>
    public ChartTypeName? ChartTypeValue { get; init; }

    /// <summary>Gets the chart type identifier (e.g., "Bar", "Line", "Pie"; Chart blocks only).</summary>
    public string? ChartType { get; init; }

    /// <summary>Gets the opaque chart data payload (Chart blocks only).</summary>
    public object? ChartData { get; init; }

    /// <summary>
    /// Performs the LayoutSizing pass, computing <see cref="LayoutBlock.DesiredSize"/> given the
    /// <paramref name="availableSize"/> constraint. Delegates to type-specific logic based on <see cref="ContentType"/>.
    /// </summary>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        _layoutSizingContext = context;

        switch (ContentType)
        {
            case BlockContentType.Text:
                LayoutSizeText(availableSize, context);
                break;

            case BlockContentType.Container:
            case BlockContentType.ReportSection:
            case BlockContentType.PageSection:
            case BlockContentType.Table:
            case BlockContentType.Row:
            case BlockContentType.Page:
                LayoutSizeContainer(availableSize, context);
                break;

            case BlockContentType.Cell:
                LayoutSizeCell(availableSize, context);
                break;

            case BlockContentType.Image:
                LayoutSizeImage(availableSize, context);
                break;

            case BlockContentType.Shape:
                LayoutSizeShape(availableSize, context);
                break;

            case BlockContentType.Chart:
                LayoutSizeChart(availableSize, context);
                break;

            case BlockContentType.Barcode:
                LayoutSizeBarcode(availableSize, context);
                break;
        }
    }

    /// <summary>
    /// Performs the Arrange pass, assigning the final <see cref="LayoutBlock.Bounds"/> within
    /// the given <paramref name="finalRect"/>. Delegates to type-specific logic based on <see cref="ContentType"/>.
    /// </summary>
    public override void Arrange(Rect finalRect)
    {
        switch (ContentType)
        {
            case BlockContentType.Text:
                ArrangeText(finalRect);
                break;

            case BlockContentType.Container:
            case BlockContentType.ReportSection:
            case BlockContentType.PageSection:
            case BlockContentType.Table:
            case BlockContentType.Row:
            case BlockContentType.Page:
                ArrangeContainer(finalRect);
                break;

            case BlockContentType.Cell:
                ArrangeCell(finalRect);
                break;

            case BlockContentType.Image:
            case BlockContentType.Shape:
            case BlockContentType.Chart:
            case BlockContentType.Barcode:
                ArrangeSimple(finalRect);
                break;
        }
    }

    /// <summary>
    /// Sets the shaped text runs. Called internally by the layout pipeline (Text blocks only).
    /// </summary>
    internal void SetTextRuns(IReadOnlyList<TextRun> runs) => TextRuns = runs;

    // ===== Type-Specific LayoutSize Implementations =====

    private void LayoutSizeText(Size availableSize, ILayoutSizingContext context)
    {
        if (string.IsNullOrEmpty(Text))
        {
            DesiredSize = Size.Zero;
        }
        else
        {
            DesiredSize = context.TextLayout.MeasureText(Text, Style, availableSize.Width);
        }
    }

    private void LayoutSizeContainer(Size availableSize, ILayoutSizingContext context)
    {
        var horizontalInset = Style.Padding.Horizontal + GetBorderLeftWidth() + GetBorderRightWidth();
        var verticalInset = Style.Padding.Vertical + GetBorderTopWidth() + GetBorderBottomWidth();

        var contentWidth = Math.Max(0f, availableSize.Width - horizontalInset);
        var contentHeight = Math.Max(0f, availableSize.Height - verticalInset);

        float totalHeight = 0f;

        foreach (var child in Children)
        {
            child.LayoutSize(new Size(contentWidth, availableSize.Height), context);
            totalHeight += child.DesiredSize.Height;
        }

        DesiredSize = new Size(availableSize.Width, totalHeight + verticalInset);
    }

    private void LayoutSizeCell(Size availableSize, ILayoutSizingContext context)
    {
        // Cells size based on content
        var horizontalInset = Style.Padding.Horizontal + GetBorderLeftWidth() + GetBorderRightWidth();
        var verticalInset = Style.Padding.Vertical + GetBorderTopWidth() + GetBorderBottomWidth();

        var contentWidth = Math.Max(0f, availableSize.Width - horizontalInset);

        float totalHeight = 0f;

        foreach (var child in Children)
        {
            child.LayoutSize(new Size(contentWidth, availableSize.Height), context);
            totalHeight += child.DesiredSize.Height;
        }

        DesiredSize = new Size(availableSize.Width, totalHeight + verticalInset);
    }

    private void LayoutSizeImage(Size availableSize, ILayoutSizingContext context)
    {
        var horizontalInset = Style.Padding.Horizontal + GetBorderLeftWidth() + GetBorderRightWidth();
        var verticalInset = Style.Padding.Vertical + GetBorderTopWidth() + GetBorderBottomWidth();

        var maxWidth = NormalizeLayoutAxis(availableSize.Width - horizontalInset, 320f);
        var maxHeight = NormalizeLayoutAxis(availableSize.Height - verticalInset, 180f);

        var imageSize = context.ResolveImageSize(SourceKey ?? string.Empty);
        if (imageSize is null)
        {
            // Fallback to available space when no intrinsic size resolver is configured.
            DesiredSize = new Size(maxWidth + horizontalInset, maxHeight + verticalInset);
            return;
        }

        var scaledSize = ScaleImageSize(imageSize.Value, maxWidth, maxHeight);
        DesiredSize = new Size(scaledSize.Width + horizontalInset, scaledSize.Height + verticalInset);
    }

    private void LayoutSizeShape(Size availableSize, ILayoutSizingContext context)
    {
        var horizontalInset = Style.Padding.Horizontal + GetBorderLeftWidth() + GetBorderRightWidth();
        var verticalInset = Style.Padding.Vertical + GetBorderTopWidth() + GetBorderBottomWidth();

        var maxWidth = NormalizeLayoutAxis(availableSize.Width - horizontalInset, 240f);
        var maxHeight = NormalizeLayoutAxis(availableSize.Height - verticalInset, 120f);

        DesiredSize = new Size(maxWidth + horizontalInset, maxHeight + verticalInset);
    }

    private void LayoutSizeChart(Size availableSize, ILayoutSizingContext context)
    {
        var horizontalInset = Style.Padding.Horizontal + GetBorderLeftWidth() + GetBorderRightWidth();
        var verticalInset = Style.Padding.Vertical + GetBorderTopWidth() + GetBorderBottomWidth();

        var maxWidth = NormalizeLayoutAxis(availableSize.Width - horizontalInset, 360f);
        var maxHeight = NormalizeLayoutAxis(availableSize.Height - verticalInset, 220f);

        DesiredSize = new Size(maxWidth + horizontalInset, maxHeight + verticalInset);
    }

    private void LayoutSizeBarcode(Size availableSize, ILayoutSizingContext context)
    {
        var horizontalInset = Style.Padding.Horizontal + GetBorderLeftWidth() + GetBorderRightWidth();
        var verticalInset = Style.Padding.Vertical + GetBorderTopWidth() + GetBorderBottomWidth();

        // Barcodes must always resolve to finite dimensions; unconstrained inputs can be Infinity.
        // Keep barcode visuals compact by default, even when a wide/tall region is available.
        var maxWidth = MathF.Min(NormalizeLayoutAxis(availableSize.Width - horizontalInset, 200f), 200f);
        var maxHeight = MathF.Min(NormalizeLayoutAxis(availableSize.Height - verticalInset, ShowText ? 96f : 80f), ShowText ? 96f : 80f);

        DesiredSize = new Size(maxWidth + horizontalInset, maxHeight + verticalInset);
    }

    // ===== Type-Specific Arrange Implementations =====

    private void ArrangeText(Rect finalRect)
    {
        Bounds = finalRect;

        if (_layoutSizingContext != null && !string.IsNullOrEmpty(Text))
        {
            SetTextRuns(_layoutSizingContext.TextLayout.ShapeText(Text, Style, finalRect));
            _layoutSizingContext = null;
        }
    }

    private void ArrangeContainer(Rect finalRect)
    {
        Bounds = finalRect;

        var contentRect = GetContentBounds(finalRect);
        float currentY = contentRect.Y;

        foreach (var child in Children)
        {
            var childHeight = child.DesiredSize.Height;
            child.Arrange(new Rect(contentRect.X, currentY, contentRect.Width, childHeight));
            currentY += childHeight;
        }
    }

    private void ArrangeCell(Rect finalRect)
    {
        Bounds = finalRect;

        var contentRect = GetContentBounds(finalRect);
        float currentY = contentRect.Y;

        foreach (var child in Children)
        {
            var childHeight = child.DesiredSize.Height;
            child.Arrange(new Rect(contentRect.X, currentY, contentRect.Width, childHeight));
            currentY += childHeight;
        }
    }

    private void ArrangeSimple(Rect finalRect)
    {
        Bounds = finalRect;
    }

    // ===== Helper Methods =====

    private Rect GetContentBounds(Rect containerBounds)
    {
        var left = containerBounds.X + Style.Padding.Left + GetBorderLeftWidth();
        var top = containerBounds.Y + Style.Padding.Top + GetBorderTopWidth();
        var width = Math.Max(0f, containerBounds.Width - Style.Padding.Horizontal - GetBorderLeftWidth() - GetBorderRightWidth());
        var height = Math.Max(0f, containerBounds.Height - Style.Padding.Vertical - GetBorderTopWidth() - GetBorderBottomWidth());

        return new Rect(left, top, width, height);
    }

    private Size ScaleImageSize(Size originalSize, float maxWidth, float maxHeight)
    {
        maxWidth = NormalizeLayoutAxis(maxWidth, 320f);
        maxHeight = NormalizeLayoutAxis(maxHeight, 180f);

        var originalWidth = NormalizeLayoutAxis(originalSize.Width, maxWidth);
        var originalHeight = NormalizeLayoutAxis(originalSize.Height, maxHeight);

        if (Stretch == ImageStretch.Fill)
        {
            return new Size(maxWidth, maxHeight);
        }

        var aspectRatio = originalWidth / originalHeight;
        if (!float.IsFinite(aspectRatio) || aspectRatio <= 0f)
            aspectRatio = maxWidth / maxHeight;

        if (Stretch == ImageStretch.UniformToFill)
        {
            if (maxWidth / maxHeight > aspectRatio)
            {
                return new Size(maxHeight * aspectRatio, maxHeight);
            }
            else
            {
                return new Size(maxWidth, maxWidth / aspectRatio);
            }
        }

        // Default: Uniform
        if (maxWidth / maxHeight < aspectRatio)
        {
            return new Size(maxWidth, maxWidth / aspectRatio);
        }
        else
        {
            return new Size(maxHeight * aspectRatio, maxHeight);
        }
    }

    private static float NormalizeLayoutAxis(float value, float fallback)
    {
        if (!float.IsFinite(value) || value <= 0f)
            return fallback;

        return value;
    }

    private float GetBorderLeftWidth() => Style.Border?.Left.Width ?? 0f;

    private float GetBorderRightWidth() => Style.Border?.Right.Width ?? 0f;

    private float GetBorderTopWidth() => Style.Border?.Top.Width ?? 0f;

    private float GetBorderBottomWidth() => Style.Border?.Bottom.Width ?? 0f;
}
