namespace KineticReports.Core.Plugins;

/// <summary>
/// Base interface for all KineticReports plugins.
/// Plugins extend the reporting engine with custom renderers, exporters, or data providers.
/// </summary>
public interface IPlugin
{
    /// <summary>Gets the unique identifier for this plugin.</summary>
    string Id { get; }

    /// <summary>Gets the human-readable name of this plugin.</summary>
    string Name { get; }

    /// <summary>Gets the version string (e.g., "1.0.0").</summary>
    string Version { get; }

    /// <summary>Gets the plugin author.</summary>
    string? Author { get; }

    /// <summary>Gets an optional description of plugin functionality.</summary>
    string? Description { get; }

    /// <summary>
    /// Initializes the plugin with the given service provider for dependency resolution.
    /// Called once when the plugin is loaded into the engine.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider for registering plugin services.</param>
    /// <param name="cancellationToken">Cancellation token for async initialization.</param>
    Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unloads the plugin and releases resources.
    /// Called when the plugin is being removed from the engine.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async cleanup.</param>
    Task UnloadAsync(CancellationToken cancellationToken = default);
}
