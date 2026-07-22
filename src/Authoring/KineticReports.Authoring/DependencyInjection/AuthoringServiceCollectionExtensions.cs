namespace KineticReports.Authoring.DependencyInjection;

using KineticReports.Authoring.Compilation;
using KineticReports.Authoring.Components;
using KineticReports.Authoring.Serialization;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers default report authoring services for component catalogs, compilation, and JSON serialization.
/// </summary>
public static class AuthoringServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default KineticReports authoring services.
    /// </summary>
    public static IServiceCollection AddKineticReportsAuthoring(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IReportComponentCatalog, DefaultReportComponentCatalog>();
        services.AddSingleton<IDesignerDocumentCompiler, DefaultDesignerDocumentCompiler>();
        services.AddSingleton<IReportDefinitionSerializer, SystemTextJsonReportDefinitionSerializer>();

        return services;
    }
}
