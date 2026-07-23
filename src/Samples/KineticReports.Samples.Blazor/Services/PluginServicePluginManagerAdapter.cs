namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Plugins;

/// <summary>
/// Adapts the sample <see cref="IPluginService"/> to <see cref="IPluginManager"/>
/// so core services can consume loaded plugins without depending on sample-specific types.
/// </summary>
internal sealed class PluginServicePluginManagerAdapter : IPluginManager
{
    private readonly IPluginService _pluginService;

    public PluginServicePluginManagerAdapter(IPluginService pluginService)
    {
        _pluginService = pluginService ?? throw new ArgumentNullException(nameof(pluginService));
    }

    /// <inheritdoc/>
    public IReadOnlyList<IPlugin> LoadedPlugins => _pluginService.LoadedPlugins;

    /// <inheritdoc/>
    public Task DiscoverAndLoadPluginsAsync(
        string pluginDirectory,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        return _pluginService.InitializePluginAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPlugin> LoadPluginAsync(
        string assemblyPath,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "Sample plugin manager adapter only supports bulk initialization through IPluginService.");
    }

    /// <inheritdoc/>
    public Task UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            throw new ArgumentException("Plugin ID cannot be null or empty.", nameof(pluginId));

        throw new NotSupportedException(
            "Sample plugin manager adapter supports unloading all plugins through IPluginService only.");
    }

    /// <inheritdoc/>
    public IPlugin? GetPluginById(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return null;

        return _pluginService.LoadedPlugins.FirstOrDefault(plugin => plugin.Id == pluginId);
    }
}
