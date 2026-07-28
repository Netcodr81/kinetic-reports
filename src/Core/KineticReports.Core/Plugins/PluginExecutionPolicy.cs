namespace KineticReports.Core.Plugins;

using KineticReports.Core.Definition;

/// <summary>
/// Resolves whether a plugin should run for a specific <see cref="ReportDefinition"/>.
/// </summary>
public static class PluginExecutionPolicy
{
    /// <summary>
    /// Returns <see langword="true"/> when the plugin is enabled for the given report.
    /// </summary>
    /// <param name="definition">Report definition that may contain plugin toggles.</param>
    /// <param name="pluginId">Plugin ID to evaluate.</param>
    public static bool IsEnabled(ReportDefinition? definition, string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return false;

        if (definition?.Plugins == null || definition.Plugins.Count == 0)
            return true;

        foreach (var toggle in definition.Plugins)
        {
            if (string.Equals(toggle.PluginId, pluginId, StringComparison.OrdinalIgnoreCase))
                return toggle.Enabled;
        }

        return true;
    }
}
