namespace KineticReports.Core.Viewer.Services;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;

/// <summary>
/// Contract for report execution and export operations in viewer hosts.
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
    /// Renders a prebuilt report document as HTML for viewer preview.
    /// </summary>
    Task<string> RenderHtmlAsync(ReportDocument reportDocument, CancellationToken ct = default);

    /// <summary>
    /// Renders a prebuilt report document as HTML for viewer preview, using an optional
    /// report definition for plugin toggle evaluation.
    /// </summary>
    Task<string> RenderHtmlAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition,
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
    /// Exports a prebuilt report document using the requested format.
    /// </summary>
    Task<ReportViewerExportResult> ExportAsync(
        ReportDocument reportDocument,
        string formatId,
        CancellationToken ct = default);

    /// <summary>
    /// Exports a prebuilt report document using the requested format, with an optional
    /// report definition for plugin toggle evaluation.
    /// </summary>
    Task<ReportViewerExportResult> ExportAsync(
        ReportDocument reportDocument,
        ReportDefinition? definition,
        string formatId,
        CancellationToken ct = default);

    /// <summary>
    /// Performs a page-local hit-test against the report visual tree.
    /// </summary>
    Task<ReportViewerHitTestResult> HitTestAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        int pageNumber,
        float x,
        float y,
        CancellationToken ct = default);

    /// <summary>
    /// Performs a page-local hit-test against a prebuilt report document.
    /// </summary>
    Task<ReportViewerHitTestResult> HitTestAsync(
        ReportDocument reportDocument,
        int pageNumber,
        float x,
        float y,
        CancellationToken ct = default);

    /// <summary>
    /// Searches report text content and returns deterministic ordered matches.
    /// </summary>
    Task<ReportViewerTextSearchResult> SearchTextAsync(
        ReportDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        string query,
        int? pageNumber = null,
        CancellationToken ct = default);

    /// <summary>
    /// Searches text content in a prebuilt report document.
    /// </summary>
    Task<ReportViewerTextSearchResult> SearchTextAsync(
        ReportDocument reportDocument,
        string query,
        int? pageNumber = null,
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

/// <summary>
/// Represents a hit-test result from an embedded viewer host.
/// </summary>
public sealed record ReportViewerHitTestResult(
    bool Hit,
    int PageNumber,
    float X,
    float Y,
    string? ElementId,
    string? LayerName,
    IReadOnlyDictionary<string, string> Metadata);

/// <summary>
/// Represents one text search match from an embedded viewer host.
/// </summary>
public sealed record ReportViewerTextSearchMatch(
    int PageNumber,
    string LayerName,
    string ElementId,
    string Text);

/// <summary>
/// Represents a text search response from an embedded viewer host.
/// </summary>
public sealed record ReportViewerTextSearchResult(
    string Query,
    int? PageNumber,
    IReadOnlyList<ReportViewerTextSearchMatch> Matches);