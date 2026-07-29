namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a barcode or QR code block in the report layout.
/// </summary>
public sealed class BarcodeBlock : LayoutBlock
{
    /// <inheritdoc/>
    public override LayoutBlockType LayoutBlockType => LayoutBlockType.Barcode;

    /// <summary>
    /// Gets the strongly typed barcode symbology.
    /// </summary>
    public BarcodeSymbology? SymbologyType { get; init; }

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
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        DesiredSize = availableSize;
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }
}
