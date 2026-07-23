namespace KineticReports.Samples.Blazor.Services;

using KineticReports.Core.Plugins;
using System.Reflection;

/// <summary>
/// Service for managing plugin lifecycle and discovery.
/// </summary>
public interface IPluginService
{
    /// <summary>
    /// Gets all loaded plugins.
    /// </summary>
    IReadOnlyList<IPlugin> LoadedPlugins { get; }

    /// <summary>
    /// Initializes the built-in sample plugin.
    /// </summary>
    Task InitializePluginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Unloads all plugins.
    /// </summary>
    Task UnloadAllAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IPluginService"/>.
/// </summary>
public sealed class PluginService : IPluginService, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IPlugin> _loadedPlugins = new();

    /// <summary>
    /// Initializes a new <see cref="PluginService"/>.
    /// </summary>
    public PluginService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IReadOnlyList<IPlugin> LoadedPlugins => _loadedPlugins.AsReadOnly();

    /// <inheritdoc/>
    public async Task InitializePluginAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Dynamically load the sample plugin assembly
            var assemblyPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "KineticReports.Samples.Plugins.dll");

            if (!File.Exists(assemblyPath))
            {
                // Fallback: Try to load from the same directory as this assembly
                var thisAssembly = Assembly.GetExecutingAssembly();
                var directory = Path.GetDirectoryName(thisAssembly.Location) ?? ".";
                assemblyPath = Path.Combine(directory, "KineticReports.Samples.Plugins.dll");
            }

            if (!File.Exists(assemblyPath))
            {
                System.Diagnostics.Debug.WriteLine($"Sample plugin assembly not found: {assemblyPath}");
                return;
            }

            var assembly = Assembly.LoadFrom(assemblyPath);

            // Find all classes implementing IPlugin
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var pluginType in pluginTypes)
            {
                var plugin = (IPlugin?)Activator.CreateInstance(pluginType);
                if (plugin != null)
                {
                    await plugin.InitializeAsync(_serviceProvider, cancellationToken).ConfigureAwait(false);
                    _loadedPlugins.Add(plugin);
                    System.Diagnostics.Debug.WriteLine($"Loaded plugin: {plugin.Name} v{plugin.Version}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading plugins: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task UnloadAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var plugin in _loadedPlugins)
        {
            try
            {
                await plugin.UnloadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unloading plugin {plugin.Name}: {ex.Message}");
            }
        }

        _loadedPlugins.Clear();
    }

    /// <inheritdoc/>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await UnloadAllAsync().ConfigureAwait(false);
    }
}
