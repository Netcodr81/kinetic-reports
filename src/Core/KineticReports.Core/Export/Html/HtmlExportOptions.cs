namespace KineticReports.Core.Export.Html;

/// <summary>
/// Options that control how HTML exporters emit shared document assets.
/// </summary>
public sealed class HtmlExportOptions
{
    /// <summary>
    /// Gets or sets the stylesheet URL to include in exported HTML.
    /// When null or empty, no stylesheet link is emitted.
    /// </summary>
    public string? StylesheetHref { get; set; }
}