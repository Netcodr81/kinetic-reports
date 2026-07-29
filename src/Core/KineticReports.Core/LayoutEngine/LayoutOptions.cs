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

    /// <summary>
    /// Predefined layout option presets for common page formats and scenarios.
    /// </summary>
    /// <remarks>
    /// Use these presets as convenient defaults when running reports:
    /// <example>
    /// <code>
    /// var doc = await engine.RunAsync(
    ///     definition,
    ///     parameters,
    ///     layoutContext,
    ///     LayoutOptions.Presets.StandardLetter()
    /// );
    /// </code>
    /// </example>
    /// All dimensions are calculated in Device Independent Pixels (DIPs, 1/96 inch at 96 DPI).
    /// </remarks>
    public static class Presets
    {
        /// <summary>
        /// Standard US Letter size (8.5" × 11") with 0.5" margins.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Dimensions:</strong>
        /// <list type="bullet">
        /// <item>Page width: 816 DIPs (8.5" × 96 DPI)</item>
        /// <item>Page height: 1056 DIPs (11" × 96 DPI)</item>
        /// <item>Margins: 48 DIPs (0.5" × 96 DPI) on all sides</item>
        /// <item>Usable area: 720 × 960 DIPs</item>
        /// </list>
        /// </para>
        /// <para>
        /// This is the most common format for business reports, letters, and forms in North America.
        /// </para>
        /// </remarks>
        /// <returns>Configured <see cref="LayoutOptions"/> for standard US Letter size.</returns>
        public static LayoutOptions StandardLetter() => new()
        {
            PageWidth = 816f,      // 8.5" × 96 DPI
            PageHeight = 1056f,    // 11" × 96 DPI
            PageMargins = new Thickness(48f),  // 0.5" × 96 DPI
        };

        /// <summary>
        /// ISO A4 size (210 × 297 mm) with 20 mm margins.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Dimensions:</strong>
        /// <list type="bullet">
        /// <item>Page width: 794 DIPs (210 mm ÷ 25.4 × 96 DPI)</item>
        /// <item>Page height: 1123 DIPs (297 mm ÷ 25.4 × 96 DPI)</item>
        /// <item>Margins: 75 DIPs (20 mm ÷ 25.4 × 96 DPI) on all sides</item>
        /// <item>Usable area: 644 × 973 DIPs</item>
        /// </list>
        /// </para>
        /// <para>
        /// A4 is the standard international page size used in most countries outside North America.
        /// Common format for business reports, documents, and formal publications.
        /// </para>
        /// </remarks>
        /// <returns>Configured <see cref="LayoutOptions"/> for ISO A4 size.</returns>
        public static LayoutOptions StandardA4() => new()
        {
            PageWidth = 794f,      // 210 mm × 96 DPI ÷ 25.4
            PageHeight = 1123f,    // 297 mm × 96 DPI ÷ 25.4
            PageMargins = new Thickness(75f),  // 20 mm × 96 DPI ÷ 25.4
        };

        /// <summary>
        /// Web/screen display (1024 × 768) with no margins.
        /// Optimized for on-screen viewing and interactive HTML rendering.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Dimensions:</strong>
        /// <list type="bullet">
        /// <item>Page width: 1024 DIPs</item>
        /// <item>Page height: 768 DIPs</item>
        /// <item>Margins: 0 DIPs (no margins)</item>
        /// <item>Usable area: 1024 × 768 DIPs (entire page)</item>
        /// </list>
        /// </para>
        /// <para>
        /// Use this preset for:
        /// <list type="bullet">
        /// <item>HTML reports displayed in browsers</item>
        /// <item>On-screen dashboard displays</item>
        /// <item>Interactive viewers with custom margins via CSS</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <returns>Configured <see cref="LayoutOptions"/> for web screen display.</returns>
        public static LayoutOptions WebScreen() => new()
        {
            PageWidth = 1024f,
            PageHeight = 768f,
            PageMargins = new Thickness(0f),
        };

        /// <summary>
        /// Custom page format factory for non-standard dimensions.
        /// </summary>
        /// <param name="widthDips">Page width in Device Independent Pixels (DIPs).</param>
        /// <param name="heightDips">Page height in Device Independent Pixels (DIPs).</param>
        /// <param name="marginDips">Margin on all four sides in DIPs. Default is 0.</param>
        /// <remarks>
        /// <para>
        /// <strong>Calculation Tips:</strong>
        /// <list type="bullet">
        /// <item>1 DIP = 1/96 inch (at 96 DPI)</item>
        /// <item>Inches to DIPs: multiply by 96 (e.g., 8.5" = 8.5 × 96 = 816 DIPs)</item>
        /// <item>Millimeters to DIPs: multiply by 96 ÷ 25.4 (e.g., 210 mm = 210 × 96 ÷ 25.4 = 794 DIPs)</item>
        /// <item>Centimeters to DIPs: multiply by 96 ÷ 2.54 (e.g., 21 cm = 21 × 96 ÷ 2.54 = 794 DIPs)</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <returns>Configured <see cref="LayoutOptions"/> with custom dimensions.</returns>
        public static LayoutOptions Custom(float widthDips, float heightDips, float marginDips = 0f) =>
            new()
            {
                PageWidth = widthDips,
                PageHeight = heightDips,
                PageMargins = new Thickness(marginDips),
            };
    }
}
