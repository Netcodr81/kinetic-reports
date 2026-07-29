namespace KineticReports.Core.Rendering;

using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using SkiaSharp;
using SkiaSharp.QrCode.Image;
using ZXing;
using ZXing.Common;
using ZXing.PDF417;

internal static class BarcodeImageFactory
{
    public static bool IsSquareSymbology(BarcodeSymbology? symbologyType, string? symbology)
    {
        return TryResolveSymbology(symbologyType, symbology, out var resolved)
            && resolved is BarcodeSymbology.QrCode or BarcodeSymbology.MicroQr;
    }

    public static bool TryCreateImageReference(
        string id,
        BarcodeSymbology? symbologyType,
        string? symbology,
        string? value,
        Rect bounds,
        out ImageReference imageReference)
    {
        imageReference = null!;

        if (!TryCreatePngBytes(symbologyType, symbology, value, bounds, out var pngBytes))
            return false;

        imageReference = new ImageReference
        {
            SourceKey = $"barcode:{id}:{symbology}:{value}",
            IntrinsicSize = new Size(Math.Max(1f, bounds.Width), Math.Max(1f, bounds.Height)),
            PixelWidth = 1,
            PixelHeight = 1,
            PixelFormat = "PNG",
            PixelData = pngBytes
        };

        return true;
    }

    public static bool TryCreateDataUri(BarcodeSymbology? symbologyType, string? symbology, string? value, Rect bounds, out string dataUri)
    {
        dataUri = string.Empty;

        if (!TryCreatePngBytes(symbologyType, symbology, value, bounds, out var pngBytes))
            return false;

        dataUri = $"data:image/png;base64,{Convert.ToBase64String(pngBytes)}";
        return true;
    }

    private static bool TryCreatePngBytes(BarcodeSymbology? symbologyType, string? symbology, string? value, Rect bounds, out byte[] pngBytes)
    {
        pngBytes = [];

        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!TryResolveSymbology(symbologyType, symbology, out var resolvedSymbology))
            return false;

        var width = GetPixelDimension(bounds.Width, 128);
        var height = GetPixelDimension(bounds.Height, 64);

        return resolvedSymbology is BarcodeSymbology.QrCode or BarcodeSymbology.MicroQr
            ? TryCreateQrPng(value.Trim(), width, height, out pngBytes)
            : TryCreateZxingPng(resolvedSymbology, value.Trim(), width, height, out pngBytes);
    }

    private static bool TryCreateQrPng(string value, int width, int height, out byte[] pngBytes)
    {
        pngBytes = [];

        try
        {
            var size = Math.Max(width, height);
            pngBytes = QRCodeImageBuilder.GetPngBytes(value, size: size);
            return pngBytes.Length > 0;
        }
        catch
        {
            pngBytes = [];
            return false;
        }
    }

    private static bool TryCreateZxingPng(BarcodeSymbology symbology, string value, int width, int height, out byte[] pngBytes)
    {
        pngBytes = [];

        if (!TryMapBarcodeFormat(symbology, out var format))
            return false;

        try
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = format,
                Options = CreateEncodingOptions(format, width, height)
            };

            var pixelData = writer.Write(value);
            var info = new SKImageInfo(pixelData.Width, pixelData.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

            using var bitmap = new SKBitmap(info);
            using var pixmap = bitmap.PeekPixels();
            pixelData.Pixels.AsSpan().CopyTo(pixmap.GetPixelSpan());

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            if (data is null)
                return false;

            pngBytes = data.ToArray();
            return pngBytes.Length > 0;
        }
        catch
        {
            pngBytes = [];
            return false;
        }
    }

    private static EncodingOptions CreateEncodingOptions(BarcodeFormat format, int width, int height)
    {
        if (format == BarcodeFormat.PDF_417)
        {
            return new PDF417EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 2,
                PureBarcode = true
            };
        }

        return new EncodingOptions
        {
            Width = width,
            Height = height,
            Margin = 2,
            PureBarcode = true
        };
    }

    private static bool TryMapBarcodeFormat(BarcodeSymbology symbology, out BarcodeFormat format)
    {
        format = symbology switch
        {
            BarcodeSymbology.Code128 => BarcodeFormat.CODE_128,
            BarcodeSymbology.Code39 => BarcodeFormat.CODE_39,
            BarcodeSymbology.Ean13 => BarcodeFormat.EAN_13,
            BarcodeSymbology.Ean8 => BarcodeFormat.EAN_8,
            BarcodeSymbology.UpcA => BarcodeFormat.UPC_A,
            BarcodeSymbology.UpcE => BarcodeFormat.UPC_E,
            BarcodeSymbology.Itf => BarcodeFormat.ITF,
            BarcodeSymbology.Pdf417 => BarcodeFormat.PDF_417,
            BarcodeSymbology.DataMatrix => BarcodeFormat.DATA_MATRIX,
            BarcodeSymbology.Codabar => BarcodeFormat.CODABAR,
            _ => BarcodeFormat.QR_CODE
        };

        return symbology is not BarcodeSymbology.MicroQr;
    }

    private static bool TryResolveSymbology(BarcodeSymbology? symbologyType, string? symbology, out BarcodeSymbology resolved)
    {
        resolved = default;

        if (symbologyType.HasValue)
        {
            resolved = symbologyType.Value;
            return true;
        }

        if (string.IsNullOrWhiteSpace(symbology))
        {
            resolved = BarcodeSymbology.QrCode;
            return true;
        }

        return BarcodeSymbologyName.TryParse(symbology, out resolved);
    }

    private static int GetPixelDimension(float dimension, int fallback)
    {
        if (float.IsNaN(dimension) || float.IsInfinity(dimension) || dimension <= 0f)
            return fallback;

        return Math.Max(32, (int)MathF.Ceiling(dimension));
    }
}