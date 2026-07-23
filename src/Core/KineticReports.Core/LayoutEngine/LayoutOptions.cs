namespace KineticReports.Core.LayoutEngine;

using KineticReports.Core.Geometry;

/// <summary>
/// Configures page dimensions and margins for the layout engine.
/// All measurements are in Device Independent Pixels (DIPs, 1/96 inch).
/// </summary>
public sealed record LayoutOptions
{
    /// <summary>
    /// Gets the page width in DIPs.
    /// Default is 816 DIPs (8.5 inches at 96 DPI — US Letter).
    /// </summary>
    public float PageWidth { get; init; } = 816f;

    /// <summary>
    /// Gets the page height in DIPs.
    /// Default is 1056 DIPs (11 inches at 96 DPI — US Letter).
    /// </summary>
    public float PageHeight { get; init; } = 1056f;

    /// <summary>
    /// Gets the page margins applied on all four sides.
    /// Default is 96 DIPs (1 inch) on every side.
    /// </summary>
    public Thickness PageMargins { get; init; } = new Thickness(96f);
}
