namespace KineticReports.Viewer.Mvc;

/// <summary>
/// View model rendered by the MVC report viewer component.
/// </summary>
public sealed class KineticReportViewModel
{
    /// <summary>
    /// Gets or sets rendered HTML content.
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets trace entries captured while rendering.
    /// </summary>
    public IReadOnlyList<string> Trace { get; set; } = [];
}
