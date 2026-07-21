namespace KineticReports.Rendering.Skia;

using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Rendering.Skia.Extensions;
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
    private readonly ReportLayoutRenderer _treeRenderer = new();

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
    public Task RenderAsync(ReportLayout reportLayout, Stream output, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            if (_options.Format == RenderFormat.Pdf)
                RenderPdf(reportLayout, output, cancellationToken);
            else
                RenderPng(reportLayout, output, cancellationToken);
        }, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // PDF rendering
    // -------------------------------------------------------------------------

    private void RenderPdf(ReportLayout reportLayout, Stream output, CancellationToken ct)
    {
        using var imageCache = new SkiaImageCache();
        using var document = SKDocument.CreatePdf(output);

        foreach (var page in reportLayout.Pages)
        {
            ct.ThrowIfCancellationRequested();

            using var canvas = document.BeginPage(page.PageWidth, page.PageHeight);
            var gfx = new SkiaGraphicsContext(canvas, 1f, imageCache);
            _treeRenderer.RenderPage(page, gfx, _options);
            document.EndPage();
        }

        document.Close();
    }

    // -------------------------------------------------------------------------
    // PNG rendering (page 1 only)
    // -------------------------------------------------------------------------

    private void RenderPng(ReportLayout reportLayout, Stream output, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (reportLayout.PageCount == 0) return;

        var page = reportLayout.Pages[0];
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
