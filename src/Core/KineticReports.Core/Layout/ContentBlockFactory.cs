namespace KineticReports.Core.Layout;

using KineticReports.Core.Styling;

/// <summary>
/// Creates strongly typed content blocks for code-first report authoring.
/// </summary>
public static class ContentBlockFactory
{
    /// <summary>
    /// Creates a <see cref="TextBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="text">Text content.</param>
    /// <returns>A configured <see cref="TextBlock"/>.</returns>
    public static TextBlock CreateText(string id, AppliedStyle style, string text)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(text);

        return new TextBlock
        {
            Id = id,
            Style = style,
            Text = text
        };
    }

    /// <summary>
    /// Creates an <see cref="ImageBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="sourceKey">Image source key or URI.</param>
    /// <param name="stretch">Image stretch behavior.</param>
    /// <returns>A configured <see cref="ImageBlock"/>.</returns>
    public static ImageBlock CreateImage(
        string id,
        AppliedStyle style,
        string sourceKey,
        ImageStretch stretch = ImageStretch.Uniform)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(sourceKey);

        return new ImageBlock
        {
            Id = id,
            Style = style,
            SourceKey = sourceKey,
            Stretch = stretch
        };
    }

    /// <summary>
    /// Creates a rectangle <see cref="ShapeBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="fill">Optional fill color.</param>
    /// <param name="stroke">Optional stroke color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <returns>A configured rectangle <see cref="ShapeBlock"/>.</returns>
    public static ShapeBlock CreateRectangle(
        string id,
        AppliedStyle style,
        Color? fill = null,
        Color? stroke = null,
        float strokeWidth = 1f)
    {
        return CreateShape(id, style, ShapeKind.Rectangle, fill, stroke, strokeWidth);
    }

    /// <summary>
    /// Creates an ellipse <see cref="ShapeBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="fill">Optional fill color.</param>
    /// <param name="stroke">Optional stroke color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <returns>A configured ellipse <see cref="ShapeBlock"/>.</returns>
    public static ShapeBlock CreateEllipse(
        string id,
        AppliedStyle style,
        Color? fill = null,
        Color? stroke = null,
        float strokeWidth = 1f)
    {
        return CreateShape(id, style, ShapeKind.Ellipse, fill, stroke, strokeWidth);
    }

    /// <summary>
    /// Creates a line <see cref="ShapeBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="stroke">Stroke color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <returns>A configured line <see cref="ShapeBlock"/>.</returns>
    public static ShapeBlock CreateLine(
        string id,
        AppliedStyle style,
        Color stroke,
        float strokeWidth = 1f)
    {
        return CreateShape(id, style, ShapeKind.Line, null, stroke, strokeWidth);
    }

    /// <summary>
    /// Creates a <see cref="BarcodeBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="symbology">Barcode symbology (for example QR, Code128, EAN13).</param>
    /// <param name="value">Encoded value.</param>
    /// <param name="showText">Whether to render human-readable text.</param>
    /// <returns>A configured <see cref="BarcodeBlock"/>.</returns>
    public static BarcodeBlock CreateBarcode(
        string id,
        AppliedStyle style,
        string symbology,
        string value,
        bool showText = true)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(symbology);
        ArgumentNullException.ThrowIfNull(value);

        return new BarcodeBlock
        {
            Id = id,
            Style = style,
            Symbology = symbology,
            Value = value,
            ShowText = showText
        };
    }

    /// <summary>
    /// Creates a QR code block using barcode infrastructure.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="value">Encoded value.</param>
    /// <param name="showText">Whether to render human-readable text.</param>
    /// <returns>A configured QR <see cref="BarcodeBlock"/>.</returns>
    public static BarcodeBlock CreateQrCode(
        string id,
        AppliedStyle style,
        string value,
        bool showText = false)
    {
        return CreateBarcode(id, style, "QR", value, showText);
    }

    /// <summary>
    /// Creates a <see cref="ChartBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="chartType">Chart type key (for example Bar, Line, Pie).</param>
    /// <param name="chartData">Optional renderer-specific chart payload.</param>
    /// <returns>A configured <see cref="ChartBlock"/>.</returns>
    public static ChartBlock CreateChart(
        string id,
        AppliedStyle style,
        string chartType,
        object? chartData = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(chartType);

        return new ChartBlock
        {
            Id = id,
            Style = style,
            ChartType = chartType,
            ChartData = chartData
        };
    }

    private static ShapeBlock CreateShape(
        string id,
        AppliedStyle style,
        ShapeKind kind,
        Color? fill,
        Color? stroke,
        float strokeWidth)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);

        if (strokeWidth <= 0f)
            throw new ArgumentOutOfRangeException(nameof(strokeWidth), strokeWidth, "Stroke width must be greater than zero.");

        return new ShapeBlock
        {
            Id = id,
            Style = style,
            Kind = kind,
            Fill = fill,
            Stroke = stroke,
            StrokeWidth = strokeWidth
        };
    }
}