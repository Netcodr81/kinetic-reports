namespace KineticReports.Plugins;

/// <summary>
/// Plugin seam that can map a user request to a final export format.
/// </summary>
public interface IExportNegotiationPlugin : IPlugin
{
    /// <summary>
    /// Gets execution order for deterministic negotiation.
    /// Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Attempts to negotiate the final export format.
    /// </summary>
    /// <param name="requestedFormat">Requested format ID.</param>
    /// <param name="availableFormats">Currently available formats.</param>
    /// <returns>
    /// A chosen format ID when the plugin can decide; otherwise <c>null</c>.
    /// </returns>
    string? Negotiate(
        string requestedFormat,
        IReadOnlyList<ExportFormatDescriptor> availableFormats);
}
