namespace KineticReports.Viewer.Mvc;

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
    /// <returns>The input <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddKineticReportsViewerMvc(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.TryAddScoped<IReportMvcService, LocalReportMvcService>();
        return services;
    }
}
