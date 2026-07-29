namespace KineticReports.Viewer.Mvc;

using KineticReports.Core.Export.Document;
using KineticReports.Core.Export.Html;
using KineticReports.Core.Rendering.Skia;
using KineticReports.Core.Visual;
using KineticReports.Core.Viewer.Services;
using KineticReports.Viewer.Mvc.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Dependency injection extensions for MVC viewer hosting.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers MVC viewer supporting services for in-process rendering.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional rendering pipeline options callback.</param>
    /// <returns>The input <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddKineticReportsViewerMvc(
        this IServiceCollection services,
        Action<RenderingPipelineOptions>? configure = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddOptions<RenderingPipelineOptions>();
        services.AddOptions<HtmlExportOptions>();
        if (configure != null)
            services.Configure(configure);

        services.TryAddScoped<IHtmlExporter, HtmlExporter>();
        services.TryAddScoped<VisualSkiaRenderer>();
        services.TryAddScoped<VisualHitTestIndexBuilder>();
        services.TryAddScoped<VisualTextSearchIndexBuilder>();
        services.TryAddScoped<IVisualDocumentBuilder, DefaultVisualDocumentBuilder>();
        services.TryAddScoped<IReportService, DefaultReportService>();
        services.TryAddScoped<IReportMvcService, DefaultReportMvcService>();
        return services;
    }
}
