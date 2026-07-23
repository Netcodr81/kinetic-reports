namespace KineticReports.Samples.Blazor.DependencyInjection;

using KineticReports.Core.Engine.Data;
using KineticReports.Samples.Blazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Sample-only extensions for <see cref="KeneticReportsBuilder"/>.
/// </summary>
public static class KeneticReportsSampleBuilderExtensions
{
    /// <summary>
    /// Adds the sample data resolver implementation.
    /// </summary>
    public static KeneticReportsBuilder UseSampleDataResolver(this KeneticReportsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSourceResolver, SampleDataResolver>());
        return builder;
    }

    /// <summary>
    /// Adds sample authoring workflow and report catalog services.
    /// </summary>
    public static KeneticReportsBuilder UseSampleAuthoringServices(this KeneticReportsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddScoped<IAuthoringJsonWorkflowService, AuthoringJsonWorkflowService>();
        builder.Services.AddSingleton<IAuthoredReportCatalogService, AuthoredReportCatalogService>();
        return builder;
    }
}
