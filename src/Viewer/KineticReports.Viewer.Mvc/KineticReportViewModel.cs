using KineticReports.Core.Layout;

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
    /// Gets or sets the report document rendered by the viewer.
    /// </summary>
    public ReportDocument? ReportDocument { get; set; }

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


}
