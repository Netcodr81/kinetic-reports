namespace KineticReports.Viewer.Mvc;

/// <summary>
/// Configures optional presentation and export behavior for the MVC viewer component.
/// </summary>
public sealed class KineticReportViewerOptions
{
    /// <summary>
    /// Gets or sets the built-in density profile.
    /// </summary>
    public ReportViewerDensity Density { get; set; } = ReportViewerDensity.Comfortable;

    /// <summary>
    /// Gets or sets the built-in viewer theme.
    /// </summary>
    public ReportViewerTheme Theme { get; set; } = ReportViewerTheme.Light;

    /// <summary>
    /// Gets or sets optional accent color CSS token.
    /// </summary>
    public string? AccentColor { get; set; }

    /// <summary>
    /// Gets or sets optional surface background CSS token.
    /// </summary>
    public string? SurfaceColor { get; set; }

    /// <summary>
    /// Gets or sets optional border color CSS token.
    /// </summary>
    public string? BorderColor { get; set; }

    /// <summary>
    /// Gets or sets optional page background CSS token.
    /// </summary>
    public string? PageBackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets optional HTML export endpoint URL.
    /// </summary>
    public string? ExportHtmlUrl { get; set; }

    /// <summary>
    /// Gets or sets optional PDF export endpoint URL.
    /// </summary>
    public string? ExportPdfUrl { get; set; }

    /// <summary>
    /// Gets or sets whether the component toolbar is shown.
    /// </summary>
    public bool ShowToolbar { get; set; } = true;
}

/// <summary>
/// Density profiles for the MVC viewer.
/// </summary>
public enum ReportViewerDensity
{
    /// <summary>
    /// Compact spacing profile for dense report inspection.
    /// </summary>
    Compact,

    /// <summary>
    /// Balanced spacing profile for typical report usage.
    /// </summary>
    Comfortable,

    /// <summary>
    /// Spacious spacing profile for readability-first layouts.
    /// </summary>
    Spacious
}

/// <summary>
/// Built-in theme choices for the MVC viewer.
/// </summary>
public enum ReportViewerTheme
{
    /// <summary>
    /// Light theme with bright surfaces.
    /// </summary>
    Light,

    /// <summary>
    /// Dark theme with dark surfaces.
    /// </summary>
    Dark
}
