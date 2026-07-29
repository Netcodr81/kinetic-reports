namespace KineticReports.Core.Layout;

/// <summary>
/// Supported barcode symbologies for report barcode elements.
/// </summary>
public enum BarcodeSymbology
{
    /// <summary>Standard QR code (ISO/IEC 18004).</summary>
    QrCode,

    /// <summary>Micro QR code (ISO/IEC 18004).</summary>
    MicroQr,

    /// <summary>Code 128 linear barcode.</summary>
    Code128,

    /// <summary>Code 39 linear barcode.</summary>
    Code39,

    /// <summary>EAN-13 linear barcode.</summary>
    Ean13,

    /// <summary>EAN-8 linear barcode.</summary>
    Ean8,

    /// <summary>UPC-A linear barcode.</summary>
    UpcA,

    /// <summary>UPC-E linear barcode.</summary>
    UpcE,

    /// <summary>Interleaved 2 of 5 linear barcode.</summary>
    Itf,

    /// <summary>PDF417 stacked linear barcode.</summary>
    Pdf417,

    /// <summary>Data Matrix 2D barcode.</summary>
    DataMatrix,

    /// <summary>Codabar linear barcode.</summary>
    Codabar
}

/// <summary>
/// Converts between string symbology identifiers and <see cref="BarcodeSymbology"/> values.
/// </summary>
public static class BarcodeSymbologyName
{
    /// <summary>
    /// Converts a strongly typed symbology to its canonical identifier string.
    /// </summary>
    /// <param name="symbology">The strongly typed symbology.</param>
    /// <returns>The canonical string identifier.</returns>
    public static string ToIdentifier(BarcodeSymbology symbology) => symbology switch
    {
        BarcodeSymbology.QrCode => "QR",
        BarcodeSymbology.MicroQr => "MICROQR",
        BarcodeSymbology.Code128 => "CODE128",
        BarcodeSymbology.Code39 => "CODE39",
        BarcodeSymbology.Ean13 => "EAN13",
        BarcodeSymbology.Ean8 => "EAN8",
        BarcodeSymbology.UpcA => "UPCA",
        BarcodeSymbology.UpcE => "UPCE",
        BarcodeSymbology.Itf => "ITF",
        BarcodeSymbology.Pdf417 => "PDF417",
        BarcodeSymbology.DataMatrix => "DATAMATRIX",
        BarcodeSymbology.Codabar => "CODABAR",
        _ => "QR"
    };

    /// <summary>
    /// Attempts to parse a string symbology identifier into a strongly typed value.
    /// </summary>
    /// <param name="symbology">The string symbology identifier.</param>
    /// <param name="parsed">The parsed typed value if successful.</param>
    /// <returns><see langword="true"/> when parsing succeeded; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string? symbology, out BarcodeSymbology parsed)
    {
        parsed = default;

        if (string.IsNullOrWhiteSpace(symbology))
            return false;

        var normalized = new string(symbology
            .Where(static c => char.IsLetterOrDigit(c))
            .ToArray())
            .ToUpperInvariant();

        return normalized switch
        {
            "QR" or "QRCODE" => Set(BarcodeSymbology.QrCode, out parsed),
            "MICROQR" => Set(BarcodeSymbology.MicroQr, out parsed),
            "CODE128" or "GS1128" => Set(BarcodeSymbology.Code128, out parsed),
            "CODE39" or "CODE3OF9" => Set(BarcodeSymbology.Code39, out parsed),
            "EAN13" or "JAN13" => Set(BarcodeSymbology.Ean13, out parsed),
            "EAN8" => Set(BarcodeSymbology.Ean8, out parsed),
            "UPCA" => Set(BarcodeSymbology.UpcA, out parsed),
            "UPCE" => Set(BarcodeSymbology.UpcE, out parsed),
            "ITF" or "INTERLEAVED2OF5" => Set(BarcodeSymbology.Itf, out parsed),
            "PDF417" => Set(BarcodeSymbology.Pdf417, out parsed),
            "DATAMATRIX" => Set(BarcodeSymbology.DataMatrix, out parsed),
            "CODABAR" => Set(BarcodeSymbology.Codabar, out parsed),
            _ => false
        };
    }

    private static bool Set(BarcodeSymbology value, out BarcodeSymbology parsed)
    {
        parsed = value;
        return true;
    }
}