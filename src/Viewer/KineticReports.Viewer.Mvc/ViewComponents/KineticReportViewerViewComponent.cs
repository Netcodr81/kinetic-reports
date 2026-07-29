namespace KineticReports.Viewer.Mvc.ViewComponents;

using System.Text;
using System.Text.RegularExpressions;
using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
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
    /// Renders either a prebuilt report document or a report definition.
    /// </summary>
    /// <param name="reportDocument">Optional prebuilt report document to preview.</param>
    /// <param name="reportDefinition">Optional report definition to execute when <paramref name="reportDocument"/> is not provided.</param>
    /// <param name="parameters">Optional runtime parameters used when executing <paramref name="reportDefinition"/>.</param>
    /// <param name="options">Optional viewer presentation and export options.</param>
    public async Task<IViewComponentResult> InvokeAsync(
        ReportDocument? reportDocument = null,
        ReportDefinition? reportDefinition = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        KineticReportViewerOptions? options = null)
    {
        var effectiveOptions = options ?? new KineticReportViewerOptions();

        KineticReportViewModel model;
        if (reportDocument is not null)
        {
            var htmlFromDocument = await _reportService
                .RenderHtmlAsync(reportDocument, reportDefinition, ViewContext.HttpContext.RequestAborted)
                .ConfigureAwait(false);

            model = BuildModel(htmlFromDocument, reportDocument.PageCount, effectiveOptions);
        }
        else
        {
            if (reportDefinition is null)
            {
                throw new ArgumentException(
                    "Either reportDocument or reportDefinition must be provided.",
                    nameof(reportDefinition));
            }

            var effectiveParameters = parameters ?? new Dictionary<string, object?>();
            var htmlFromDefinition = await _reportService
                .RenderHtmlAsync(reportDefinition, effectiveParameters, ViewContext.HttpContext.RequestAborted)
                .ConfigureAwait(false);

            model = BuildModel(htmlFromDefinition, CountPages(htmlFromDefinition), effectiveOptions);
        }

        model.Trace = _reportService.GetLatestTrace();

        return View(model);
    }

    private static KineticReportViewModel BuildModel(string html, int pageCount, KineticReportViewerOptions options)
    {
        var printMarkup = BuildPrintMarkup(html);

        return new KineticReportViewModel
        {
            HtmlContent = html,
            PreviewMarkup = BuildInlinePreviewMarkup(html),
            PrintMarkupBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(printMarkup)),
            PageCount = pageCount,
            ViewerCssClass = $"kinetic-report-viewer kinetic-report-viewer--{GetDensityClass(options.Density)} kinetic-report-viewer--{GetThemeClass(options.Theme)}",
            ViewerStyle = BuildViewerStyle(options),
            ExportHtmlUrl = options.ExportHtmlUrl,
            ExportPdfUrl = options.ExportPdfUrl,
            ShowToolbar = options.ShowToolbar
        };
    }

    private static string BuildInlinePreviewMarkup(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var styleBuilder = new StringBuilder();
        var styleRegex = new Regex("<style\\b[^>]*>[\\s\\S]*?</style>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        foreach (Match match in styleRegex.Matches(html))
            styleBuilder.Append(match.Value);

        var bodyStart = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
        if (bodyStart < 0)
            return styleBuilder + html;

        var bodyOpenEnd = html.IndexOf('>', bodyStart);
        if (bodyOpenEnd < 0)
            return styleBuilder + html;

        var bodyCloseStart = html.IndexOf("</body>", bodyOpenEnd, StringComparison.OrdinalIgnoreCase);
        if (bodyCloseStart < 0)
            bodyCloseStart = html.Length;

        var bodyMarkup = html.Substring(bodyOpenEnd + 1, bodyCloseStart - bodyOpenEnd - 1);
        return styleBuilder + bodyMarkup;
    }

    private static string BuildPrintMarkup(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var styleBuilder = new StringBuilder();
        var styleRegex = new Regex("<style\\b[^>]*>[\\s\\S]*?</style>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        foreach (Match match in styleRegex.Matches(html))
            styleBuilder.Append(match.Value);

        styleBuilder.Append("<style>#report-document > .kinetic-page { display: block !important; }</style>");

        var bodyStart = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
        if (bodyStart < 0)
            return $"<!doctype html><html class=\"kinetic-report\"><head><meta charset=\"utf-8\">{styleBuilder}</head><body>{html}</body></html>";

        var bodyOpenEnd = html.IndexOf('>', bodyStart);
        if (bodyOpenEnd < 0)
            return $"<!doctype html><html class=\"kinetic-report\"><head><meta charset=\"utf-8\">{styleBuilder}</head><body>{html}</body></html>";

        var bodyCloseStart = html.IndexOf("</body>", bodyOpenEnd, StringComparison.OrdinalIgnoreCase);
        if (bodyCloseStart < 0)
            bodyCloseStart = html.Length;

        var bodyMarkup = html.Substring(bodyOpenEnd + 1, bodyCloseStart - bodyOpenEnd - 1);
        return $"<!doctype html><html class=\"kinetic-report\"><head><meta charset=\"utf-8\">{styleBuilder}</head><body>{bodyMarkup}</body></html>";
    }

    private static int CountPages(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return 0;

        return Regex.Matches(html, "class=\"kinetic-page", RegexOptions.IgnoreCase).Count;
    }

    private static string GetDensityClass(ReportViewerDensity density)
    {
        return density switch
        {
            ReportViewerDensity.Compact => "compact",
            ReportViewerDensity.Spacious => "spacious",
            _ => "comfortable"
        };
    }

    private static string GetThemeClass(ReportViewerTheme theme)
    {
        return theme switch
        {
            ReportViewerTheme.Dark => "dark",
            _ => "light"
        };
    }

    private static string BuildViewerStyle(KineticReportViewerOptions options)
    {
        var parts = new List<string>();
        AppendCssVariable(parts, "--kr-accent", options.AccentColor);
        AppendCssVariable(parts, "--kr-surface", options.SurfaceColor);
        AppendCssVariable(parts, "--kr-border", options.BorderColor);
        AppendCssVariable(parts, "--kr-page-bg", options.PageBackgroundColor);
        return string.Join(' ', parts);
    }

    private static void AppendCssVariable(List<string> parts, string variableName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        parts.Add($"{variableName}: {value.Trim()};");
    }
}
