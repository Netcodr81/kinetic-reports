namespace KineticReports.Core.Engine.DependencyInjection;

using KineticReports.Core.Data.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Registers default KineticReports engine services.
/// </summary>
public static class EngineServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default engine pipeline services, including
    /// <see cref="KineticReports.Core.Engine.Building.DefaultReportBuilder"/>,
    /// and allows optional fluent customization of that default builder.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="lifetime">Service lifetime used for engine services. Defaults to scoped.</param>
    /// <param name="configureBuilder">
    /// Optional callback used to configure the default
    /// <see cref="KineticReports.Core.Engine.Building.DefaultReportBuilder"/> instance.
    /// </param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddKineticReportsEngine(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Action<global::KineticReports.Core.Engine.Building.DefaultReportBuilder>? configureBuilder = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(new ServiceDescriptor(
            typeof(global::KineticReports.Core.Engine.Expressions.IExpressionEvaluator),
            typeof(global::KineticReports.Core.Engine.Expressions.DefaultExpressionEvaluator),
            lifetime));

        services.TryAdd(new ServiceDescriptor(
            typeof(global::KineticReports.Core.Engine.Expressions.IExpressionValidator),
            typeof(global::KineticReports.Core.Engine.Expressions.DefaultExpressionValidator),
            lifetime));

        services.AddPocoDataProvider();

        services.TryAddEnumerable(new ServiceDescriptor(
            typeof(global::KineticReports.Core.Engine.Data.IDataSourceResolver),
            typeof(global::KineticReports.Core.Engine.Data.ProviderDataSourceResolver),
            lifetime));

        services.TryAdd(new ServiceDescriptor(
            typeof(global::KineticReports.Core.Engine.Data.IDataResolver),
            typeof(global::KineticReports.Core.Engine.Data.CompositeDataResolver),
            lifetime));

        services.TryAdd(new ServiceDescriptor(
            typeof(global::KineticReports.Core.LayoutEngine.ILayoutEngine),
            typeof(global::KineticReports.Core.LayoutEngine.LayoutEngine),
            lifetime));

        if (configureBuilder is null)
        {
            services.TryAdd(new ServiceDescriptor(
                typeof(global::KineticReports.Core.Engine.Building.IReportBuilder),
                typeof(global::KineticReports.Core.Engine.Building.DefaultReportBuilder),
                lifetime));
        }
        else
        {
            services.TryAdd(new ServiceDescriptor(
                typeof(global::KineticReports.Core.Engine.Building.DefaultReportBuilder),
                _ =>
                {
                    var builder = global::KineticReports.Core.Engine.Building.DefaultReportBuilder.Create();
                    configureBuilder(builder);
                    return builder;
                },
                lifetime));

            services.TryAdd(new ServiceDescriptor(
                typeof(global::KineticReports.Core.Engine.Building.IReportBuilder),
                provider => provider.GetRequiredService<global::KineticReports.Core.Engine.Building.DefaultReportBuilder>(),
                lifetime));
        }

        services.TryAdd(new ServiceDescriptor(
            typeof(global::KineticReports.Core.Engine.IReportEngine),
            typeof(global::KineticReports.Core.Engine.ReportEngine),
            lifetime));

        return services;
    }
}
