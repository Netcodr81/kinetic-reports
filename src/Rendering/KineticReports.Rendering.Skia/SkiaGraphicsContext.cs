namespace KineticReports.Rendering.Skia;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Rendering.Skia.Extensions;
using SkiaSharp;

/// <summary>
/// <see cref="IGraphicsContext"/> implementation backed by an <see cref="SKCanvas"/>.
/// All DIP coordinates are multiplied by <see cref="_scale"/> (pixels-per-DIP)
/// before being passed to the canvas.
/// </summary>
internal sealed class SkiaGraphicsContext : IGraphicsContext
{
    private readonly SKCanvas _canvas;
    private readonly float _scale;
    private readonly SkiaImageCache _imageCache;

    internal SkiaGraphicsContext(SKCanvas canvas, float scale, SkiaImageCache imageCache)
    {
        _canvas = canvas;
        _scale = scale;
        _imageCache = imageCache;
    }

    // -------------------------------------------------------------------------
    // Filled shapes
    // -------------------------------------------------------------------------

    public void FillRectangle(Rect bounds, Color color)
    {
        using var paint = Fill(color);
        _canvas.DrawRect(bounds.ToSkia(_scale), paint);
    }

    public void FillEllipse(Rect bounds, Color color)
    {
        using var paint = Fill(color);
        _canvas.DrawOval(bounds.ToSkia(_scale), paint);
    }

    // -------------------------------------------------------------------------
    // Stroked shapes
    // -------------------------------------------------------------------------

    public void StrokeRectangle(Rect bounds, Color color, float strokeWidth)
    {
        using var paint = Stroke(color, strokeWidth);
        _canvas.DrawRect(bounds.ToSkia(_scale), paint);
    }

    public void StrokeEllipse(Rect bounds, Color color, float strokeWidth)
    {
        using var paint = Stroke(color, strokeWidth);
        _canvas.DrawOval(bounds.ToSkia(_scale), paint);
    }

    public void DrawLine(Point from, Point to, Color color, float strokeWidth)
    {
        using var paint = Stroke(color, strokeWidth);
        _canvas.DrawLine(from.ToSkia(_scale), to.ToSkia(_scale), paint);
    }

    // -------------------------------------------------------------------------
    // Path
    // -------------------------------------------------------------------------

    public void DrawPath(PathGeometry path)
    {
        using var skPath = BuildPath(path);

        if (path.Fill.HasValue)
        {
            using var fillPaint = Fill(path.Fill.Value);
            _canvas.DrawPath(skPath, fillPaint);
        }

        if (path.Stroke.HasValue)
        {
            using var strokePaint = Stroke(path.Stroke.Value, path.StrokeWidth);
            _canvas.DrawPath(skPath, strokePaint);
        }
    }

    // -------------------------------------------------------------------------
    // Text
    // -------------------------------------------------------------------------

    public void DrawText(TextRun run)
    {
        using var font = SkiaFontMetrics.CreateFont(run.Style);
        font.Size *= _scale;

        using var paint = new SKPaint { Color = run.Style.TextColor.ToSkia(), IsAntialias = true };

        _canvas.DrawText(
            run.Text,
            run.BaselineOrigin.X * _scale,
            run.BaselineOrigin.Y * _scale,
            SKTextAlign.Left,
            font,
            paint);
    }

    // -------------------------------------------------------------------------
    // Images
    // -------------------------------------------------------------------------

    public void DrawImage(Rect destinationBounds, ImageReference image, ImageStretch stretch)
    {
        var bitmap = _imageCache.GetOrCreate(image);
        var dest = destinationBounds.ToSkia(_scale);

        switch (stretch)
        {
            case ImageStretch.None:
                _canvas.DrawBitmap(bitmap,
                    new SKRect(dest.Left, dest.Top,
                        dest.Left + bitmap.Width,
                        dest.Top + bitmap.Height),
                    SKSamplingOptions.Default);
                break;

            case ImageStretch.Fill:
                _canvas.DrawBitmap(bitmap, dest, SKSamplingOptions.Default);
                break;

            case ImageStretch.Uniform:
                _canvas.DrawBitmap(bitmap, ComputeUniformRect(bitmap.Width, bitmap.Height, dest), SKSamplingOptions.Default);
                break;

            case ImageStretch.UniformToFill:
                _canvas.Save();
                _canvas.ClipRect(dest);
                _canvas.DrawBitmap(bitmap, ComputeUniformToFillRect(bitmap.Width, bitmap.Height, dest), SKSamplingOptions.Default);
                _canvas.Restore();
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Clip / opacity stack
    // -------------------------------------------------------------------------

    public void PushClip(Rect clip)
    {
        _canvas.Save();
        _canvas.ClipRect(clip.ToSkia(_scale));
    }

    public void PopClip() => _canvas.Restore();

    public void PushOpacity(float opacity)
    {
        _canvas.SaveLayer(new SKPaint
        {
            Color = new SKColor(255, 255, 255, (byte)(opacity * 255))
        });
    }

    public void PopOpacity() => _canvas.Restore();

    // -------------------------------------------------------------------------
    // Paint factories
    // -------------------------------------------------------------------------

    private static SKPaint Fill(Color color) =>
        new SKPaint { Color = color.ToSkia(), IsAntialias = true };

    private static SKPaint Stroke(Color color, float width) =>
        new SKPaint
        {
            Color = color.ToSkia(),
            IsStroke = true,
            StrokeWidth = width,
            IsAntialias = true
        };

    // -------------------------------------------------------------------------
    // Path builder
    // -------------------------------------------------------------------------

    private SKPath BuildPath(PathGeometry geometry)
    {
        var builder = new SKPathBuilder();
        foreach (var cmd in geometry.Commands)
        {
            switch (cmd.Kind)
            {
                case PathCommandKind.MoveTo when cmd.Points.Count >= 1:
                    builder.MoveTo(cmd.Points[0].ToSkia(_scale));
                    break;
                case PathCommandKind.LineTo when cmd.Points.Count >= 1:
                    builder.LineTo(cmd.Points[0].ToSkia(_scale));
                    break;
                case PathCommandKind.CubicBezierTo when cmd.Points.Count >= 3:
                    builder.CubicTo(
                        cmd.Points[0].ToSkia(_scale),
                        cmd.Points[1].ToSkia(_scale),
                        cmd.Points[2].ToSkia(_scale));
                    break;
                case PathCommandKind.Close:
                    builder.Close();
                    break;
            }
        }
        return builder.Snapshot();
    }

    // -------------------------------------------------------------------------
    // Stretch helpers
    // -------------------------------------------------------------------------

    private static SKRect ComputeUniformRect(float srcW, float srcH, SKRect dest)
    {
        float scale = Math.Min(dest.Width / srcW, dest.Height / srcH);
        float w = srcW * scale;
        float h = srcH * scale;
        return new SKRect(
            dest.MidX - w / 2f,
            dest.MidY - h / 2f,
            dest.MidX + w / 2f,
            dest.MidY + h / 2f);
    }

    private static SKRect ComputeUniformToFillRect(float srcW, float srcH, SKRect dest)
    {
        float scale = Math.Max(dest.Width / srcW, dest.Height / srcH);
        float w = srcW * scale;
        float h = srcH * scale;
        return new SKRect(
            dest.MidX - w / 2f,
            dest.MidY - h / 2f,
            dest.MidX + w / 2f,
            dest.MidY + h / 2f);
    }
}
