namespace KineticReports.Core.Rendering.Skia;

using KineticReports.Core.Geometry;
using KineticReports.Core.Visual;
using SkiaSharp;

/// <summary>
/// Baseline SkiaSharp renderer for <see cref="VisualDocument"/>.
/// </summary>
/// <remarks>
/// This adapter is intentionally minimal for phase E6-S1 and currently targets
/// PDF output only.
/// </remarks>
public sealed class VisualSkiaRenderer
{
    /// <summary>
    /// Renders a visual document as a multi-page PDF stream.
    /// </summary>
    /// <param name="visualDocument">Visual document to render.</param>
    /// <param name="output">Output stream for PDF bytes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task RenderPdfAsync(
        VisualDocument visualDocument,
        Stream output,
        CancellationToken cancellationToken = default)
    {
        if (visualDocument == null) throw new ArgumentNullException(nameof(visualDocument));
        if (output == null) throw new ArgumentNullException(nameof(output));

        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            using var document = SKDocument.CreatePdf(output);

            foreach (var page in visualDocument.Pages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var canvas = document.BeginPage(page.Width, page.Height);
                canvas.Clear(SKColors.White);

                foreach (var layer in page.Layers)
                {
                    foreach (var element in layer.Elements)
                        RenderElement(canvas, element);
                }

                document.EndPage();
            }

            document.Close();
        }, cancellationToken);
    }

    private static void RenderElement(SKCanvas canvas, VisualElement element)
    {
        canvas.Save();
        ApplyClips(canvas, element.Clips);

        switch (element)
        {
            case VisualText text:
                DrawText(canvas, text);
                break;

            case VisualShape shape:
                DrawShape(canvas, shape);
                break;

            case VisualImage image:
                DrawImage(canvas, image);
                break;

            case VisualTablePlaceholder table:
                DrawTablePlaceholder(canvas, table);
                break;

            case VisualUnsupportedElement unsupported:
                DrawUnsupported(canvas, unsupported);
                break;
        }

        if (element is VisualContainer container)
        {
            foreach (var child in container.Children)
                RenderElement(canvas, child);
        }

        canvas.Restore();
    }

    private static void ApplyClips(SKCanvas canvas, IReadOnlyList<VisualClip> clips)
    {
        foreach (var clip in clips)
            canvas.ClipRect(ToSkRect(clip.Bounds));
    }

    private static void DrawText(SKCanvas canvas, VisualText text)
    {
        using var paint = new SKPaint
        {
            Color = ParseColor(text.TextColorHex) ?? SKColors.Black,
            IsAntialias = true
        };

        using var font = new SKFont
        {
            Size = text.FontSize <= 0f ? 12f : text.FontSize
        };

        var x = text.Bounds.X;
        var y = text.Bounds.Y + font.Size;
        canvas.DrawText(text.Text, x, y, SKTextAlign.Left, font, paint);
    }

    private static void DrawShape(SKCanvas canvas, VisualShape shape)
    {
        var rect = ToSkRect(shape.Bounds);

        if (!string.IsNullOrWhiteSpace(shape.FillColorHex))
        {
            using var fill = new SKPaint
            {
                Color = ParseColor(shape.FillColorHex) ?? SKColors.Transparent,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            DrawShapeGeometry(canvas, shape.Kind, rect, fill);
        }

        if (!string.IsNullOrWhiteSpace(shape.StrokeColorHex) && shape.StrokeWidth > 0f)
        {
            using var stroke = new SKPaint
            {
                Color = ParseColor(shape.StrokeColorHex) ?? SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = shape.StrokeWidth
            };

            DrawShapeGeometry(canvas, shape.Kind, rect, stroke);
        }
    }

    private static void DrawShapeGeometry(SKCanvas canvas, VisualShapeKind kind, SKRect rect, SKPaint paint)
    {
        switch (kind)
        {
            case VisualShapeKind.Rectangle:
                canvas.DrawRect(rect, paint);
                break;

            case VisualShapeKind.Ellipse:
                canvas.DrawOval(rect, paint);
                break;

            case VisualShapeKind.Line:
                canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Bottom, paint);
                break;

            default:
                canvas.DrawRect(rect, paint);
                break;
        }
    }

    private static void DrawImage(SKCanvas canvas, VisualImage image)
    {
        var bounds = ToSkRect(image.Bounds);
        if (TryResolveBitmap(image.SourceKey, out var bitmap) && bitmap != null)
        {
            using (bitmap)
            {
                canvas.DrawBitmap(bitmap, bounds, SKSamplingOptions.Default);
            }

            return;
        }

        using var border = new SKPaint
        {
            Color = new SKColor(180, 189, 207),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f
        };

        canvas.DrawRect(bounds, border);
    }

    private static bool TryResolveBitmap(string source, out SKBitmap? bitmap)
    {
        if (TryDecodeDataUri(source, out bitmap) && bitmap != null)
            return true;

        if (TryDecodeFileSource(source, out bitmap) && bitmap != null)
            return true;

        bitmap = null;
        return false;
    }

    private static void DrawTablePlaceholder(SKCanvas canvas, VisualTablePlaceholder table)
    {
        var bounds = ToSkRect(table.Bounds);

        using var fill = new SKPaint
        {
            Color = new SKColor(243, 246, 252),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawRect(bounds, fill);

        using var border = new SKPaint
        {
            Color = new SKColor(197, 210, 234),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            IsAntialias = true
        };
        canvas.DrawRect(bounds, border);
    }

    private static void DrawUnsupported(SKCanvas canvas, VisualUnsupportedElement unsupported)
    {
        var bounds = ToSkRect(unsupported.Bounds);

        using var fill = new SKPaint
        {
            Color = new SKColor(255, 244, 232),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawRect(bounds, fill);

        using var border = new SKPaint
        {
            Color = new SKColor(240, 198, 142),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            IsAntialias = true
        };
        canvas.DrawRect(bounds, border);
    }

    private static SKRect ToSkRect(Rect rect) => new(rect.X, rect.Y, rect.Right, rect.Bottom);

    private static SKColor? ParseColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return null;

        if (hex[0] == '#')
            hex = hex[1..];

        if (hex.Length == 8 && uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var argb))
        {
            var a = (byte)(argb >> 24);
            var r = (byte)(argb >> 16);
            var g = (byte)(argb >> 8);
            var b = (byte)argb;
            return new SKColor(r, g, b, a);
        }

        if (hex.Length == 6 && uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
        {
            var r = (byte)(rgb >> 16);
            var g = (byte)(rgb >> 8);
            var b = (byte)rgb;
            return new SKColor(r, g, b, 255);
        }

        return null;
    }

    private static bool TryDecodeDataUri(string source, out SKBitmap? bitmap)
    {
        bitmap = null;

        if (string.IsNullOrWhiteSpace(source)
            || !source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var commaIndex = source.IndexOf(',');
        if (commaIndex <= 0 || commaIndex + 1 >= source.Length)
            return false;

        var metadata = source[..commaIndex];
        if (!metadata.Contains(";base64", StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            var payload = source[(commaIndex + 1)..];
            var bytes = Convert.FromBase64String(payload);
            bitmap = SKBitmap.Decode(bytes);
            return bitmap != null;
        }
        catch
        {
            bitmap = null;
            return false;
        }
    }

    private static bool TryDecodeFileSource(string source, out SKBitmap? bitmap)
    {
        bitmap = null;

        if (string.IsNullOrWhiteSpace(source))
            return false;

        string? localPath = null;
        if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            localPath = uri.LocalPath;
        }
        else if (Path.IsPathRooted(source) || File.Exists(source))
        {
            localPath = source;
        }

        if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
            return false;

        try
        {
            bitmap = SKBitmap.Decode(localPath);
            return bitmap != null;
        }
        catch
        {
            bitmap = null;
            return false;
        }
    }
}
