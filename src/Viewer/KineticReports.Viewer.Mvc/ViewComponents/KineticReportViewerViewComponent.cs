namespace KineticReports.Viewer.Mvc.ViewComponents;

using KineticReports.Core.Definition;
using KineticReports.Viewer.Mvc.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// MVC view component that renders a report definition as HTML.
/// </summary>
public sealed class KineticReportViewerViewComponent : ViewComponent
{
    private readonly IReportMvcService _reportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="KineticReportViewerViewComponent"/> class.
    /// </summary>
    public KineticReportViewerViewComponent(IReportMvcService reportService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    /// <summary>
    /// Renders the supplied report definition.
    /// </summary>
    /// <param name="definition">Report definition to execute.</param>
    /// <param name="parameters">Optional runtime parameter values.</param>
    public async Task<IViewComponentResult> InvokeAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?>? parameters = null)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));

        var effectiveParameters = parameters ?? new Dictionary<string, object?>();
        var html = await _reportService.RenderHtmlAsync(definition, effectiveParameters, ViewContext.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var model = new KineticReportViewModel
        {
            HtmlContent = html,
            Trace = _reportService.GetLatestTrace()
        };

        return View(model);
    }
}
