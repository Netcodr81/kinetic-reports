namespace KineticReports.Core.Rendering.Skia;

using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using SkiaSharp;

/// <summary>
/// <see cref="IRenderer"/> implementation backed by SkiaSharp.
/// </summary>
/// <remarks>
/// Output format is controlled by <see cref="RenderOptions.Format"/>:
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="RenderFormat.Pdf"/> — writes a multi-page PDF document via
///       <see cref="SKDocument"/>. All pages are written to a single output stream.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="RenderFormat.Png"/> — rasterizes page 1 to a PNG image.
///       Suitable for previews, thumbnails, and unit tests.
///     </description>
///   </item>
/// </list>
/// The <see cref="SkiaTextLayout"/> instance supplied to the layout engine
/// must be the same instance referenced here to guarantee that text runs fit
/// exactly in their measured bounds (ADR-015).
/// </remarks>
public sealed class SkiaRenderer : IRenderer
{
    private readonly RenderOptions _options;
    private readonly ReportDocumentRenderer _treeRenderer = new();

    /// <summary>
    /// Initialises a new <see cref="SkiaRenderer"/> with the given render options.
    /// </summary>
    /// <param name="options">
    /// Output configuration. Uses defaults when <see langword="null"/>.
    /// </param>
    public SkiaRenderer(RenderOptions? options = null)
    {
        _options = options ?? new RenderOptions();
    }

    /// <inheritdoc/>
    public Task RenderAsync(ReportDocument reportDocument, Stream output, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            if (_options.Format == RenderFormat.Pdf)
                RenderPdf(reportDocument, output, cancellationToken);
            else
                RenderPng(reportDocument, output, cancellationToken);
        }, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // PDF rendering
    // -------------------------------------------------------------------------

    private void RenderPdf(ReportDocument reportDocument, Stream output, CancellationToken ct)
    {
        using var imageCache = new SkiaImageCache();
        using var document = SKDocument.CreatePdf(output);

        foreach (var page in reportDocument.Pages)
        {
            ct.ThrowIfCancellationRequested();

            using var canvas = document.BeginPage(page.PageWidth, page.PageHeight);
            var gfx = new SkiaGraphicsContext(canvas, 1f, imageCache);
            _treeRenderer.RenderPage(page, gfx, _options);
            DrawPdfWatermark(canvas, page.PageWidth, page.PageHeight);
            document.EndPage();
        }

        document.Close();
    }

    private void DrawPdfWatermark(SKCanvas canvas, float pageWidth, float pageHeight)
    {
        var watermark = _options.PdfWatermark;
        if (watermark == null || string.IsNullOrWhiteSpace(watermark.Text))
            return;

        var clampedOpacity = Math.Clamp(watermark.Opacity, 0f, 1f);
        if (clampedOpacity <= 0f)
            return;

        var color = watermark.Color;
        var alpha = (byte)Math.Clamp((int)Math.Round(color.A * clampedOpacity), 0, 255);
        var skColor = new SKColor(color.R, color.G, color.B, alpha);

        using var paint = new SKPaint
        {
            Color = skColor,
            IsAntialias = true
        };

        using var font = new SKFont
        {
            Size = Math.Max(8f, watermark.FontSize)
        };

        var maxTextWidth = Math.Max(120f, pageWidth * 0.8f);
        var lines = WrapWatermarkText(watermark.Text, maxTextWidth, font, paint);
        if (lines.Count == 0)
            return;

        var lineHeight = Math.Max(font.Size * 1.15f, 10f);

        canvas.Save();
        canvas.Translate(pageWidth / 2f, pageHeight / 2f);
        canvas.RotateDegrees(watermark.RotationDegrees);

        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var bounds = new SKRect();
            font.MeasureText(line, out bounds, paint);
            var yOffset = (index - (lines.Count - 1) / 2f) * lineHeight;
            canvas.DrawText(line, 0f, yOffset - bounds.MidY, SKTextAlign.Center, font, paint);
        }

        canvas.Restore();
    }

    private static List<string> WrapWatermarkText(string text, float maxWidth, SKFont font, SKPaint paint)
    {
        var lines = new List<string>();
        var paragraphs = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');

        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                if (lines.Count > 0)
                    lines.Add(string.Empty);

                continue;
            }

            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                continue;

            var currentLine = string.Empty;
            foreach (var word in words)
            {
                if (MeasureTextWidth(word, font, paint) > maxWidth)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine);
                        currentLine = string.Empty;
                    }

                    lines.AddRange(BreakLongWord(word, maxWidth, font, paint));
                    continue;
                }

                if (string.IsNullOrEmpty(currentLine))
                {
                    currentLine = word;
                    continue;
                }

                var candidate = $"{currentLine} {word}";
                if (MeasureTextWidth(candidate, font, paint) <= maxWidth)
                {
                    currentLine = candidate;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);
        }

        return lines;
    }

    private static IEnumerable<string> BreakLongWord(string word, float maxWidth, SKFont font, SKPaint paint)
    {
        var segments = new List<string>();
        var current = string.Empty;

        foreach (var ch in word)
        {
            var candidate = current + ch;
            if (string.IsNullOrEmpty(current) || MeasureTextWidth(candidate, font, paint) <= maxWidth)
            {
                current = candidate;
                continue;
            }

            segments.Add(current);
            current = ch.ToString();
        }

        if (!string.IsNullOrEmpty(current))
            segments.Add(current);

        return segments;
    }

    private static float MeasureTextWidth(string text, SKFont font, SKPaint paint)
    {
        var bounds = new SKRect();
        font.MeasureText(text, out bounds, paint);
        return bounds.Width;
    }

    // -------------------------------------------------------------------------
    // PNG rendering (page 1 only)
    // -------------------------------------------------------------------------

    private void RenderPng(ReportDocument reportDocument, Stream output, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (reportDocument.PageCount == 0) return;

        var page = reportDocument.Pages[0];
        float scale = _options.Dpi / 96f;
        int pixelWidth = (int)(page.PageWidth * scale);
        int pixelHeight = (int)(page.PageHeight * scale);

        var info = new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);

        using var surface = SKSurface.Create(info);
        surface.Canvas.Clear(_options.Background.ToSkia());

        using var imageCache = new SkiaImageCache();
        var gfx = new SkiaGraphicsContext(surface.Canvas, scale, imageCache);
        _treeRenderer.RenderPage(page, gfx, _options);

        using var snapshot = surface.Snapshot();
        using var encoded = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        encoded.SaveTo(output);
    }
}
