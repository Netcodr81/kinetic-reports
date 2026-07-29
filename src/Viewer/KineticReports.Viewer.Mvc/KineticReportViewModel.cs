namespace KineticReports.Viewer.Mvc;

/// <summary>
/// View model rendered by the MVC report viewer component.
/// </summary>
public sealed class KineticReportViewModel
{
    /// <summary>
    /// Gets or sets stable viewer DOM id.
    /// </summary>
    public string ViewerId { get; set; } = $"kr-mvc-viewer-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets or sets rendered inline preview markup.
    /// </summary>
    public string PreviewMarkup { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets rendered HTML content.
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets print-friendly full document markup encoded in base64.
    /// </summary>
    public string PrintMarkupBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the known page count for pager controls.
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// Gets or sets the composed viewer CSS class.
    /// </summary>
    public string ViewerCssClass { get; set; } = "kinetic-report-viewer kinetic-report-viewer--comfortable kinetic-report-viewer--light";

    /// <summary>
    /// Gets or sets optional inline CSS variables applied to the viewer root.
    /// </summary>
    public string ViewerStyle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional HTML export URL.
    /// </summary>
    public string? ExportHtmlUrl { get; set; }

    /// <summary>
    /// Gets or sets optional PDF export URL.
    /// </summary>
    public string? ExportPdfUrl { get; set; }

    /// <summary>
    /// Gets or sets whether toolbar actions should be shown.
    /// </summary>
    public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Gets or sets trace entries captured while rendering.
    /// </summary>
    public IReadOnlyList<string> Trace { get; set; } = [];
}
