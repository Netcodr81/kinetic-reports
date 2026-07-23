namespace KineticReports.Core.Plugins;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for registering the plugin system with ASP.NET Core dependency injection.
/// </summary>
public static class PluginManagerExtensions
{
    /// <summary>
    /// Registers the default plugin manager with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Registers <see cref="IPluginManager"/> as a singleton with the default implementation.
    /// 
    /// Example usage:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Services.AddPluginManager();
    /// </code>
    /// </remarks>
    public static IServiceCollection AddPluginManager(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<IPluginManager, DefaultPluginManager>();
        return services;
    }

    /// <summary>
    /// Registers the plugin manager with a custom implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The plugin manager implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Registers <see cref="IPluginManager"/> as a singleton with a custom implementation.
    /// 
    /// Example usage:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Services.AddPluginManager&lt;MyCustomPluginManager&gt;();
    /// </code>
    /// </remarks>
    public static IServiceCollection AddPluginManager<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IPluginManager
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<IPluginManager, TImplementation>();
        return services;
    }

    /// <summary>
    /// Registers the plugin manager and automatically discovers and loads plugins from the specified directory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="pluginDirectory">The directory containing plugin assemblies.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method:
    /// 1. Registers <see cref="IPluginManager"/> as a singleton
    /// 2. Adds a hosted service that loads plugins on application startup
    /// 
    /// Example usage:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// var pluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
    /// builder.Services.AddPluginManager(pluginDir);
    /// </code>
    /// </remarks>
    public static IServiceCollection AddPluginManager(
        this IServiceCollection services,
        string pluginDirectory)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(pluginDirectory))
        {
            throw new ArgumentException(
                "Plugin directory cannot be null or empty.",
                nameof(pluginDirectory));
        }

        services.TryAddSingleton<IPluginManager, DefaultPluginManager>();
        services.AddHostedService(sp => new PluginLoaderHostedService(
            sp.GetRequiredService<IPluginManager>(),
            pluginDirectory,
            sp));

        return services;
    }

    /// <summary>
    /// Registers the plugin manager with a custom implementation and automatically loads plugins.
    /// </summary>
    /// <typeparam name="TImplementation">The plugin manager implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="pluginDirectory">The directory containing plugin assemblies.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPluginManager<TImplementation>(
        this IServiceCollection services,
        string pluginDirectory)
        where TImplementation : class, IPluginManager
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(pluginDirectory))
        {
            throw new ArgumentException(
                "Plugin directory cannot be null or empty.",
                nameof(pluginDirectory));
        }

        services.TryAddSingleton<IPluginManager, TImplementation>();
        services.AddHostedService(sp => new PluginLoaderHostedService(
            sp.GetRequiredService<IPluginManager>(),
            pluginDirectory,
            sp));

        return services;
    }
}
