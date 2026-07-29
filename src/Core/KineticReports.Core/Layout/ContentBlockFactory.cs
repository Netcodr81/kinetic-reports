namespace KineticReports.Core.Layout;

using KineticReports.Core.Styling;
using KineticReports.Core.Geometry;

/// <summary>
/// Creates content blocks for code-first report authoring.
/// All factory methods return unified <see cref="ContentBlock"/> instances
/// with the appropriate <see cref="BlockContentType"/> discriminator.
/// </summary>
public static class ContentBlockFactory
{
    /// <summary>
    /// Creates a text <see cref="ContentBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="text">Text content.</param>
    /// <returns>A configured text <see cref="ContentBlock"/>.</returns>
    public static ContentBlock CreateText(string id, AppliedStyle style, string text)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(text);

        return new ContentBlock
        {
            Id = id,
            Style = style,
            ContentType = BlockContentType.Text,
            Text = text
        };
    }

    /// <summary>
    /// Creates an image <see cref="ContentBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="sourceKey">Image source key or URI.</param>
    /// <param name="stretch">Image stretch behavior.</param>
    /// <returns>A configured image <see cref="ContentBlock"/>.</returns>
    public static ContentBlock CreateImage(
        string id,
        AppliedStyle style,
        string sourceKey,
        ImageStretch stretch = ImageStretch.Uniform)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(sourceKey);

        return new ContentBlock
        {
            Id = id,
            Style = style,
            ContentType = BlockContentType.Image,
            SourceKey = sourceKey,
            Stretch = stretch
        };
    }

    /// <summary>
    /// Creates a rectangle <see cref="ContentBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="fill">Optional fill color.</param>
    /// <param name="stroke">Optional stroke color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <returns>A configured rectangle <see cref="ContentBlock"/>.</returns>
    public static ContentBlock CreateRectangle(
        string id,
        AppliedStyle style,
        Color? fill = null,
        Color? stroke = null,
        float strokeWidth = 1f)
    {
        return CreateShape(id, style, ShapeKind.Rectangle, fill, stroke, strokeWidth);
    }

    /// <summary>
    /// Creates an ellipse <see cref="ContentBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="fill">Optional fill color.</param>
    /// <param name="stroke">Optional stroke color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <returns>A configured ellipse <see cref="ContentBlock"/>.</returns>
    public static ContentBlock CreateEllipse(
        string id,
        AppliedStyle style,
        Color? fill = null,
        Color? stroke = null,
        float strokeWidth = 1f)
    {
        return CreateShape(id, style, ShapeKind.Ellipse, fill, stroke, strokeWidth);
    }

    /// <summary>
    /// Creates a line <see cref="ContentBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="stroke">Stroke color.</param>
    /// <param name="strokeWidth">Stroke width in DIPs.</param>
    /// <returns>A configured line <see cref="ContentBlock"/>.</returns>
    public static ContentBlock CreateLine(
        string id,
        AppliedStyle style,
        Color stroke,
        float strokeWidth = 1f)
    {
        return CreateShape(id, style, ShapeKind.Line, null, stroke, strokeWidth);
    }

    /// <summary>
    /// Creates a barcode <see cref="ContentBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="symbology">Barcode symbology (for example QR, Code128, EAN13).</param>
    /// <param name="value">Encoded value.</param>
    /// <param name="showText">Whether to render human-readable text.</param>
    /// <returns>A configured barcode <see cref="ContentBlock"/>.</returns>
    public static ContentBlock CreateBarcode(
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

        return new ContentBlock
        {
            Id = id,
            Style = style,
            ContentType = BlockContentType.Barcode,
            Symbology = symbology,
            Value = value,
            ShowText = showText
        };
    }

    /// <summary>
    /// Creates a QR code <see cref="ContentBlock"/> using barcode infrastructure.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="value">Encoded value.</param>
    /// <param name="showText">Whether to render human-readable text.</param>
    /// <returns>A configured QR <see cref="ContentBlock"/>.</returns>
    public static ContentBlock CreateQrCode(
        string id,
        AppliedStyle style,
        string value,
        bool showText = false)
    {
        return CreateBarcode(id, style, "QR", value, showText);
    }

    /// <summary>
    /// Creates a chart <see cref="ContentBlock"/>.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved block style.</param>
    /// <param name="chartType">Chart type key (for example Bar, Line, Pie).</param>
    /// <param name="chartData">Optional renderer-specific chart payload.</param>
    /// <returns>A configured chart <see cref="ContentBlock"/>.</returns>
    public static ContentBlock CreateChart(
        string id,
        AppliedStyle style,
        string chartType,
        object? chartData = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(chartType);

        return new ContentBlock
        {
            Id = id,
            Style = style,
            ContentType = BlockContentType.Chart,
            ChartType = chartType,
            ChartData = chartData
        };
    }

    private static ContentBlock CreateShape(
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

        return new ContentBlock
        {
            Id = id,
            Style = style,
            ContentType = BlockContentType.Shape,
            Kind = kind,
            Fill = fill,
            Stroke = stroke,
            StrokeWidth = strokeWidth
        };
    }
}