namespace KineticReports.Viewer.Blazor;

using KineticReports.Core.Export.Html;
using KineticReports.Core.Rendering.Skia;
using KineticReports.Core.Visual;
using KineticReports.Viewer.Blazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Dependency injection extensions for the Blazor viewer component package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers viewer component supporting services for in-process Blazor hosts.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional pipeline configuration callback.</param>
    /// <returns>The input <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddKineticReportsViewerBlazor(
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
        services.TryAddScoped<IReportService, LocalReportService>();

        return services;
    }
}
