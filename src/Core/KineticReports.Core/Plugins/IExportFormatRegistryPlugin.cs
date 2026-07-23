namespace KineticReports.Core.Plugins;

/// <summary>
/// Plugin seam that contributes export format descriptors to the host registry.
/// </summary>
public interface IExportFormatRegistryPlugin : IPlugin
{
    /// <summary>
    /// Gets execution order for deterministic contribution.
    /// Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Returns export formats contributed by this plugin.
    /// </summary>
    IReadOnlyList<ExportFormatDescriptor> GetFormats();
}
