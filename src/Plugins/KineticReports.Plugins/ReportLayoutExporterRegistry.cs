namespace KineticReports.Plugins;

/// <summary>
/// Default implementation of <see cref="IReportLayoutExporterRegistry"/>.
/// </summary>
public sealed class ReportLayoutExporterRegistry : IReportLayoutExporterRegistry
{
    private readonly IReadOnlyDictionary<string, IReportLayoutExporter> _exporters;
    private readonly IReadOnlyList<ExportFormatDescriptor> _formats;

    /// <summary>
    /// Initializes a new <see cref="ReportLayoutExporterRegistry"/>.
    /// </summary>
    public ReportLayoutExporterRegistry(IEnumerable<IReportLayoutExporter> exporters)
    {
        if (exporters == null) throw new ArgumentNullException(nameof(exporters));

        // Resolve duplicates deterministically: format ID, then concrete type name.
        var ordered = exporters
            .OrderBy(e => e.Format.FormatId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.GetType().FullName, StringComparer.Ordinal)
            .ToArray();

        _exporters = ordered
            .GroupBy(e => e.Format.FormatId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        _formats = _exporters.Values
            .Select(e => e.Format)
            .OrderBy(f => f.FormatId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExportFormatDescriptor> GetAvailableFormats() => _formats;

    /// <inheritdoc/>
    public bool TryGetExporter(string formatId, out IReportLayoutExporter? exporter)
    {
        if (string.IsNullOrWhiteSpace(formatId))
        {
            exporter = null;
            return false;
        }

        return _exporters.TryGetValue(formatId, out exporter);
    }
}
