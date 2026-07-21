namespace KineticReports.Plugins;

/// <summary>
/// Default implementation of <see cref="IReportDocumentExporterRegistry"/>.
/// </summary>
public sealed class ReportDocumentExporterRegistry : IReportDocumentExporterRegistry
{
    private readonly IReadOnlyDictionary<string, IReportDocumentExporter> _exporters;
    private readonly IReadOnlyList<ExportFormatDescriptor> _formats;

    /// <summary>
    /// Initializes a new <see cref="ReportDocumentExporterRegistry"/>.
    /// </summary>
    public ReportDocumentExporterRegistry(IEnumerable<IReportDocumentExporter> exporters)
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
    public bool TryGetExporter(string formatId, out IReportDocumentExporter? exporter)
    {
        if (string.IsNullOrWhiteSpace(formatId))
        {
            exporter = null;
            return false;
        }

        return _exporters.TryGetValue(formatId, out exporter);
    }
}
