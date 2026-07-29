namespace KineticReports.Core.Tests.Layout;

using KineticReports.Core.Layout;

public class BarcodeSymbologyNameTests
{
    [Theory]
    [InlineData(BarcodeSymbology.QrCode, "QR")]
    [InlineData(BarcodeSymbology.MicroQr, "MICROQR")]
    [InlineData(BarcodeSymbology.Code128, "CODE128")]
    [InlineData(BarcodeSymbology.Code39, "CODE39")]
    [InlineData(BarcodeSymbology.Ean13, "EAN13")]
    [InlineData(BarcodeSymbology.Ean8, "EAN8")]
    [InlineData(BarcodeSymbology.UpcA, "UPCA")]
    [InlineData(BarcodeSymbology.UpcE, "UPCE")]
    [InlineData(BarcodeSymbology.Itf, "ITF")]
    [InlineData(BarcodeSymbology.Pdf417, "PDF417")]
    [InlineData(BarcodeSymbology.DataMatrix, "DATAMATRIX")]
    [InlineData(BarcodeSymbology.Codabar, "CODABAR")]
    public void ToIdentifier_WithKnownValues_ReturnsCanonicalIdentifier(BarcodeSymbology symbology, string expected)
    {
        BarcodeSymbologyName.ToIdentifier(symbology).ShouldBe(expected);
    }

    [Theory]
    [InlineData("QR", BarcodeSymbology.QrCode)]
    [InlineData("QRCode", BarcodeSymbology.QrCode)]
    [InlineData("Micro QR", BarcodeSymbology.MicroQr)]
    [InlineData("Code-128", BarcodeSymbology.Code128)]
    [InlineData("EAN13", BarcodeSymbology.Ean13)]
    [InlineData("UPC-A", BarcodeSymbology.UpcA)]
    [InlineData("PDF_417", BarcodeSymbology.Pdf417)]
    public void TryParse_WithKnownAliases_ReturnsTypedValue(string value, BarcodeSymbology expected)
    {
        var ok = BarcodeSymbologyName.TryParse(value, out var parsed);

        ok.ShouldBeTrue();
        parsed.ShouldBe(expected);
    }

    [Fact]
    public void TryParse_WithUnknownIdentifier_ReturnsFalse()
    {
        var ok = BarcodeSymbologyName.TryParse("NoSuchSymbology", out _);

        ok.ShouldBeFalse();
    }
}