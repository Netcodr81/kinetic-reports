namespace KineticReports.Plugins;

/// <summary>
/// Describes an export format available to the host.
/// </summary>
/// <param name="FormatId">Stable format ID (e.g., html, pdf, markdown).</param>
/// <param name="DisplayName">Human-readable format name.</param>
/// <param name="MimeType">Output MIME type.</param>
/// <param name="FileExtension">Preferred file extension without leading dot.</param>
public sealed record ExportFormatDescriptor(
    string FormatId,
    string DisplayName,
    string MimeType,
    string FileExtension);
