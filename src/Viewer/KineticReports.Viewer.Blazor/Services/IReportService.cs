namespace KineticReports.Viewer.Blazor.Services;

using KineticReports.Core.Definition;

/// <summary>
/// Contract for report execution and export operations in Blazor components.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Renders a report as HTML for viewer preview.
    /// </summary>
    Task<string> RenderHtmlAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default);

    /// <summary>
    /// Gets available export formats.
    /// </summary>
    IReadOnlyList<ReportViewerExportFormat> GetAvailableExportFormats();

    /// <summary>
    /// Exports a report definition using the requested format.
    /// </summary>
    Task<ReportViewerExportResult> ExportAsync(
        ReportDefinition definition,
        string formatId,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken ct = default);

    /// <summary>
    /// Gets trace entries from the most recent render/export request.
    /// </summary>
    IReadOnlyList<string> GetLatestTrace();
}

/// <summary>
/// Describes an export format option surfaced in the report viewer.
/// </summary>
/// <param name="FormatId">Stable format identifier (for example, html or markdown).</param>
/// <param name="DisplayName">Display name for UI selection.</param>
/// <param name="MimeType">MIME type used for downloads.</param>
/// <param name="FileExtension">Default file extension for downloads.</param>
public sealed record ReportViewerExportFormat(string FormatId, string DisplayName, string MimeType, string FileExtension);

/// <summary>
/// Represents a completed viewer export artifact.
/// </summary>
/// <param name="FormatId">Final format id used for export.</param>
/// <param name="MimeType">MIME type of the content.</param>
/// <param name="FileExtension">Default file extension for downloads.</param>
/// <param name="Content">Raw exported bytes.</param>
public sealed record ReportViewerExportResult(string FormatId, string MimeType, string FileExtension, byte[] Content);
