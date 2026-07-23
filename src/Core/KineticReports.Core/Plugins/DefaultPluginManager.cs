namespace KineticReports.Plugins;

using System.Reflection;

/// <summary>
/// Default implementation of <see cref="IPluginManager"/>.
/// Discovers, loads, and manages plugin lifecycles using reflection and dynamic assembly loading.
/// </summary>
/// <remarks>
/// This implementation:
/// - Scans directories for plugin assemblies
/// - Uses reflection to discover <see cref="IPlugin"/> implementations
/// - Initializes plugins with dependency injection support
/// - Manages plugin lifecycle (load/unload)
/// - Provides thread-safe access to loaded plugins
/// </remarks>
public sealed class DefaultPluginManager : IPluginManager, IAsyncDisposable
{
    private readonly List<IPlugin> _loadedPlugins = new();
    private readonly object _lockObject = new object();

    /// <inheritdoc/>
    public IReadOnlyList<IPlugin> LoadedPlugins
    {
        get
        {
            lock (_lockObject)
            {
                return _loadedPlugins.AsReadOnly();
            }
        }
    }

    /// <inheritdoc/>
    public async Task DiscoverAndLoadPluginsAsync(
        string pluginDirectory,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginDirectory))
        {
            throw new ArgumentException("Plugin directory path cannot be null or empty.", nameof(pluginDirectory));
        }

        if (!Directory.Exists(pluginDirectory))
        {
            throw new DirectoryNotFoundException($"Plugin directory not found: {pluginDirectory}");
        }

        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        var assemblyFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly);

        if (assemblyFiles.Length == 0)
        {
            return;
        }

        foreach (var assemblyPath in assemblyFiles)
        {
            try
            {
                await LoadPluginAsync(assemblyPath, serviceProvider, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log error but continue loading other plugins
                System.Diagnostics.Debug.WriteLine(
                    $"Error loading plugin from {assemblyPath}: {ex.GetType().Name} - {ex.Message}");
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IPlugin> LoadPluginAsync(
        string assemblyPath,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            throw new ArgumentException("Assembly path cannot be null or empty.", nameof(assemblyPath));
        }

        if (!File.Exists(assemblyPath))
        {
            throw new FileNotFoundException($"Plugin assembly not found: {assemblyPath}");
        }

        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        // Load the assembly
        var assembly = Assembly.LoadFrom(assemblyPath);

        // Find IPlugin implementations
        var pluginType = FindPluginType(assembly);

        if (pluginType == null)
        {
            throw new InvalidOperationException(
                $"No IPlugin implementation found in assembly: {assemblyPath}");
        }

        // Create instance
        var plugin = (IPlugin?)Activator.CreateInstance(pluginType)
            ?? throw new InvalidOperationException(
                $"Failed to create instance of plugin type: {pluginType.FullName}");

        // Check for duplicate
        lock (_lockObject)
        {
            if (_loadedPlugins.Any(p => p.Id == plugin.Id))
            {
                throw new InvalidOperationException(
                    $"A plugin with ID '{plugin.Id}' is already loaded.");
            }
        }

        // Initialize plugin
        await plugin.InitializeAsync(serviceProvider, cancellationToken).ConfigureAwait(false);

        // Add to loaded plugins
        lock (_lockObject)
        {
            _loadedPlugins.Add(plugin);
        }

        System.Diagnostics.Debug.WriteLine(
            $"Loaded plugin: {plugin.Name} (ID: {plugin.Id}, Version: {plugin.Version})");

        return plugin;
    }

    /// <inheritdoc/>
    public async Task UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            throw new ArgumentException("Plugin ID cannot be null or empty.", nameof(pluginId));
        }

        IPlugin? plugin;

        lock (_lockObject)
        {
            plugin = _loadedPlugins.FirstOrDefault(p => p.Id == pluginId);
            if (plugin == null)
            {
                throw new InvalidOperationException($"Plugin with ID '{pluginId}' is not loaded.");
            }

            _loadedPlugins.Remove(plugin);
        }

        try
        {
            await plugin.UnloadAsync(cancellationToken).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"Unloaded plugin: {plugin.Name} (ID: {plugin.Id})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Error unloading plugin {plugin.Id}: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public IPlugin? GetPluginById(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            return null;
        }

        lock (_lockObject)
        {
            return _loadedPlugins.FirstOrDefault(p => p.Id == pluginId);
        }
    }

    /// <summary>
    /// Unloads all plugins and cleans up resources.
    /// </summary>
    public async Task UnloadAllAsync(CancellationToken cancellationToken = default)
    {
        IPlugin[] pluginsToUnload;

        lock (_lockObject)
        {
            pluginsToUnload = _loadedPlugins.ToArray();
        }

        foreach (var plugin in pluginsToUnload)
        {
            try
            {
                await UnloadPluginAsync(plugin.Id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error during cleanup of plugin {plugin.Id}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Disposes all loaded plugins asynchronously.
    /// </summary>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await UnloadAllAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Finds the first type in an assembly that implements <see cref="IPlugin"/>.
    /// </summary>
    private static Type? FindPluginType(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes()
                .FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Error scanning assembly {assembly.FullName} for plugin types: {ex.Message}");
            return null;
        }
    }
}
