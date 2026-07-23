namespace KineticReports.Plugins;

using Microsoft.Extensions.Hosting;

/// <summary>
/// Hosted service that automatically discovers and loads plugins on application startup.
/// </summary>
/// <remarks>
/// This service is registered automatically when using the <see cref="PluginManagerExtensions.AddPluginManager(Microsoft.Extensions.DependencyInjection.IServiceCollection, string)"/>
/// extension method with a plugin directory.
/// 
/// The service lifecycle:
/// - On application start: Discovers and loads all plugins from the configured directory
/// - On application stop: Unloads all plugins
/// </remarks>
internal sealed class PluginLoaderHostedService : BackgroundService
{
    private readonly IPluginManager _pluginManager;
    private readonly string _pluginDirectory;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoaderHostedService"/> class.
    /// </summary>
    /// <param name="pluginManager">The plugin manager instance.</param>
    /// <param name="pluginDirectory">The directory containing plugin assemblies.</param>
    /// <param name="serviceProvider">The service provider for DI.</param>
    public PluginLoaderHostedService(
        IPluginManager pluginManager,
        string pluginDirectory,
        IServiceProvider serviceProvider)
    {
        _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
        _pluginDirectory = pluginDirectory ?? throw new ArgumentNullException(nameof(pluginDirectory));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Discovers and loads plugins when the application starts.
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!Directory.Exists(_pluginDirectory))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Plugin directory not found: {_pluginDirectory}");
                await base.StartAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"Starting plugin discovery from: {_pluginDirectory}");

            await _pluginManager.DiscoverAndLoadPluginsAsync(
                _pluginDirectory,
                _serviceProvider,
                cancellationToken).ConfigureAwait(false);

            var pluginCount = _pluginManager.LoadedPlugins.Count;
            System.Diagnostics.Debug.WriteLine(
                $"Plugin discovery complete. Loaded {pluginCount} plugin(s).");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Error during plugin discovery: {ex.GetType().Name} - {ex.Message}");
            // Don't rethrow - allow application to start even if plugin loading fails
        }

        await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Unloads all plugins when the application stops.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Unloading all plugins...");

            if (_pluginManager is DefaultPluginManager defaultManager)
            {
                await defaultManager.UnloadAllAsync(cancellationToken).ConfigureAwait(false);
            }

            System.Diagnostics.Debug.WriteLine("All plugins unloaded.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Error during plugin unload: {ex.GetType().Name} - {ex.Message}");
        }

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Background execution (not used - plugins load on startup).
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
