namespace KineticReports.Samples.Plugins;

using KineticReports.Plugins;

/// <summary>
/// Sample plugin demonstrating custom watermark rendering.
/// Shows how to create a simple plugin that extends report rendering capabilities.
/// </summary>
public sealed class WatermarkPlugin : PluginBase
{
    public override string Id => "kinetic.sample.watermark";
    public override string Name => "Watermark Plugin";
    public override string Version => "1.0.0";
    public override string? Author => "KineticReports Team";
    public override string? Description => "Adds custom watermark rendering to reports";

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        // Example: Register custom services in the DI container
        // ServiceProvider?.GetRequiredService<IServiceCollection>().AddScoped<IWatermarkRenderer, CustomWatermarkRenderer>();

        System.Diagnostics.Debug.WriteLine($"Initialized: {Name} v{Version}");
        return Task.CompletedTask;
    }

    protected override Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine($"Unloading: {Name}");
        return Task.CompletedTask;
    }
}
