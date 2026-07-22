namespace KineticReports.Plugins;

/// <summary>
/// Manages plugin discovery, loading, and lifecycle.
/// </summary>
public interface IPluginManager
{
    /// <summary>Gets all loaded plugins.</summary>
    IReadOnlyList<IPlugin> LoadedPlugins { get; }

    /// <summary>
    /// Discovers and loads all plugins in the specified directory.
    /// </summary>
    /// <param name="pluginDirectory">Path to directory containing plugin assemblies.</param>
    /// <param name="serviceProvider">DI service provider for plugin initialization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DiscoverAndLoadPluginsAsync(
        string pluginDirectory,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a plugin from the specified assembly path.
    /// </summary>
    /// <param name="assemblyPath">Path to the plugin assembly.</param>
    /// <param name="serviceProvider">DI service provider for plugin initialization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IPlugin> LoadPluginAsync(
        string assemblyPath,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unloads a plugin by its ID.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a plugin by ID.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The plugin, or null if not found.</returns>
    IPlugin? GetPluginById(string pluginId);
}
