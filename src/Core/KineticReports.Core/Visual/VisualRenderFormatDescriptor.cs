namespace KineticReports.Visual;

/// <summary>
/// Describes a concrete output format for a visual renderer.
/// </summary>
/// <param name="FormatId">Stable format identifier (for example, html or pdf).</param>
/// <param name="DisplayName">Human-friendly display name.</param>
/// <param name="MimeType">Output MIME type.</param>
/// <param name="FileExtension">Default file extension without leading dot.</param>
public sealed record VisualRenderFormatDescriptor(
    string FormatId,
    string DisplayName,
    string MimeType,
    string FileExtension);
