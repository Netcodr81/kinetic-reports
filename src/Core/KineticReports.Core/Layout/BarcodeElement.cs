namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a barcode or QR code element in the layout tree.
/// </summary>
public sealed class BarcodeElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Barcode;

    /// <summary>
    /// Gets the barcode symbology identifier (e.g. "QR", "Code128", "EAN13", "PDF417").
    /// </summary>
    public required string Symbology { get; init; }

    /// <summary>Gets the value to encode in the barcode.</summary>
    public required string Value { get; init; }

    /// <summary>
    /// Gets a value indicating whether a human-readable text label is rendered
    /// beneath the barcode symbol.
    /// </summary>
    public bool ShowText { get; init; } = true;

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        DesiredSize = availableSize;
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }
}
