namespace KineticReports.Plugins;

/// <summary>
/// Deterministic orchestration helpers for export-related plugin seams.
/// </summary>
public static class ExportSeamPipeline
{
    /// <summary>
    /// Builds the full export format registry from core formats and plugin contributions.
    /// </summary>
    public static IReadOnlyList<ExportFormatDescriptor> BuildRegistry(
        IReadOnlyList<ExportFormatDescriptor> coreFormats,
        IReadOnlyList<IPlugin> loadedPlugins)
    {
        if (coreFormats == null) throw new ArgumentNullException(nameof(coreFormats));
        if (loadedPlugins == null) throw new ArgumentNullException(nameof(loadedPlugins));

        var orderedContributors = loadedPlugins
            .OfType<IExportFormatRegistryPlugin>()
            .OrderBy(p => p.Order)
            .ThenBy(p => p.Id, StringComparer.Ordinal);

        var allFormats = new List<ExportFormatDescriptor>(coreFormats);
        foreach (var contributor in orderedContributors)
        {
            var contributed = contributor.GetFormats();
            if (contributed.Count > 0)
            {
                allFormats.AddRange(contributed);
            }
        }

        return allFormats
            .GroupBy(f => f.FormatId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToArray();
    }

    /// <summary>
    /// Negotiates the final export format deterministically using plugin negotiators.
    /// </summary>
    public static string Negotiate(
        string requestedFormat,
        IReadOnlyList<ExportFormatDescriptor> availableFormats,
        IReadOnlyList<IPlugin> loadedPlugins)
    {
        if (string.IsNullOrWhiteSpace(requestedFormat))
        {
            throw new ArgumentException("Requested format cannot be null or empty.", nameof(requestedFormat));
        }

        if (availableFormats == null) throw new ArgumentNullException(nameof(availableFormats));
        if (loadedPlugins == null) throw new ArgumentNullException(nameof(loadedPlugins));

        var orderedNegotiators = loadedPlugins
            .OfType<IExportNegotiationPlugin>()
            .OrderBy(p => p.Order)
            .ThenBy(p => p.Id, StringComparer.Ordinal);

        foreach (var negotiator in orderedNegotiators)
        {
            var chosen = negotiator.Negotiate(requestedFormat, availableFormats);
            if (!string.IsNullOrWhiteSpace(chosen))
            {
                return chosen;
            }
        }

        return requestedFormat;
    }

    /// <summary>
    /// Runs artifact post-processors in deterministic order.
    /// </summary>
    public static async Task<byte[]> PostProcessArtifactAsync(
        string formatId,
        byte[] artifact,
        IReadOnlyList<IPlugin> loadedPlugins,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(formatId))
        {
            throw new ArgumentException("Format ID cannot be null or empty.", nameof(formatId));
        }

        if (artifact == null) throw new ArgumentNullException(nameof(artifact));
        if (loadedPlugins == null) throw new ArgumentNullException(nameof(loadedPlugins));

        var orderedPostProcessors = loadedPlugins
            .OfType<IExportArtifactPostProcessorPlugin>()
            .OrderBy(p => p.Order)
            .ThenBy(p => p.Id, StringComparer.Ordinal);

        var current = artifact;
        foreach (var postProcessor in orderedPostProcessors)
        {
            current = await postProcessor.ProcessArtifactAsync(formatId, current, cancellationToken)
                .ConfigureAwait(false);
        }

        return current;
    }
}
