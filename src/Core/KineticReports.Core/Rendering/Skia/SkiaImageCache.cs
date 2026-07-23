namespace KineticReports.Core.Rendering.Skia;

using KineticReports.Core.Rendering;
using SkiaSharp;

/// <summary>
/// Decodes and caches <see cref="SKBitmap"/> instances keyed by
/// <see cref="ImageReference.SourceKey"/>.
/// Bitmaps are decoded once from the raw pixel data in the <see cref="ImageReference"/>
/// and reused across pages. Call <see cref="Dispose"/> after the render pass completes.
/// </summary>
internal sealed class SkiaImageCache : IDisposable
{
    private readonly Dictionary<string, SKBitmap> _cache = [];
    private bool _disposed;

    /// <summary>
    /// Returns the <see cref="SKBitmap"/> for <paramref name="image"/>, decoding it
    /// from <see cref="ImageReference.PixelData"/> on first access.
    /// </summary>
    internal SKBitmap GetOrCreate(ImageReference image)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cache.TryGetValue(image.SourceKey, out var cached))
            return cached;

        var bitmap = Decode(image);
        _cache[image.SourceKey] = bitmap;
        return bitmap;
    }

    private static SKBitmap Decode(ImageReference image)
    {
        // Try standard image decoding first (PNG, JPEG, WebP, etc.)
        using var data = SKData.CreateCopy(image.PixelData);
        var decoded = SKBitmap.Decode(data);
        if (decoded is not null)
            return decoded;

        // Fallback: treat as raw RGBA8888 pixel data
        var info = new SKImageInfo(image.PixelWidth, image.PixelHeight, SKColorType.Rgba8888);
        var bitmap = new SKBitmap(info);
        using var pixmap = bitmap.PeekPixels();
        new ReadOnlySpan<byte>(image.PixelData).CopyTo(pixmap.GetPixelSpan());
        return bitmap;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var bmp in _cache.Values)
            bmp.Dispose();
        _cache.Clear();
    }
}
