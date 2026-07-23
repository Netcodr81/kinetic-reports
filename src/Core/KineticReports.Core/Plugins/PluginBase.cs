namespace KineticReports.Core.Plugins;

/// <summary>
/// Base class for plugins, providing common initialization and lifecycle management.
/// </summary>
public abstract class PluginBase : IPlugin
{
    /// <inheritdoc/>
    public abstract string Id { get; }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract string Version { get; }

    /// <inheritdoc/>
    public virtual string? Author => null;

    /// <inheritdoc/>
    public virtual string? Description => null;

    /// <summary>
    /// Gets the service provider available during and after initialization.
    /// </summary>
    protected IServiceProvider? ServiceProvider { get; private set; }

    /// <inheritdoc/>
    public virtual async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        ServiceProvider = serviceProvider;
        await OnInitializeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Override this method to perform plugin-specific initialization.
    /// The base implementation does nothing; derived classes should register custom services here.
    /// </summary>
    protected virtual Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async Task UnloadAsync(CancellationToken cancellationToken = default)
    {
        await OnUnloadAsync(cancellationToken).ConfigureAwait(false);
        ServiceProvider = null;
    }

    /// <summary>
    /// Override this method to perform plugin-specific cleanup.
    /// The base implementation does nothing.
    /// </summary>
    protected virtual Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
