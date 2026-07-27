namespace KineticReports.Core.Authoring.Serialization;

/// <summary>
/// Builds image <c>SourceKey</c> values for JSON-authored report definitions.
/// </summary>
public static class ReportImageSourceKeyHelper
{
    /// <summary>
    /// Creates a data-URI <c>SourceKey</c> from image bytes.
    /// </summary>
    /// <param name="imageBytes">Raw image bytes.</param>
    /// <param name="mimeType">MIME type such as <c>image/png</c> or <c>image/jpeg</c>.</param>
    /// <returns>A data URI string suitable for <c>ReportLayoutItemDefinition.SourceKey</c>.</returns>
    public static string CreateEmbeddedSourceKey(byte[] imageBytes, string mimeType)
    {
        ArgumentNullException.ThrowIfNull(imageBytes);
        if (imageBytes.Length == 0)
            throw new ArgumentException("Image bytes must not be empty.", nameof(imageBytes));

        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("A non-empty MIME type is required.", nameof(mimeType));

        var payload = Convert.ToBase64String(imageBytes);
        return $"data:{mimeType};base64,{payload}";
    }

    /// <summary>
    /// Creates a data-URI <c>SourceKey</c> from an image file.
    /// </summary>
    /// <param name="filePath">Path to the source image file.</param>
    /// <param name="mimeType">
    /// Optional MIME type override. When omitted, MIME type is inferred from the file extension.
    /// </param>
    /// <returns>A data URI string suitable for <c>ReportLayoutItemDefinition.SourceKey</c>.</returns>
    public static string CreateEmbeddedSourceKeyFromFile(string filePath, string? mimeType = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A file path is required.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Image file was not found: {filePath}", filePath);

        var resolvedMime = string.IsNullOrWhiteSpace(mimeType)
            ? InferMimeTypeFromPath(filePath)
            : mimeType;

        if (string.IsNullOrWhiteSpace(resolvedMime))
        {
            throw new InvalidOperationException(
                $"Could not infer MIME type from file extension for '{filePath}'. Provide mimeType explicitly.");
        }

        var bytes = File.ReadAllBytes(filePath);
        return CreateEmbeddedSourceKey(bytes, resolvedMime);
    }

    /// <summary>
    /// Creates a file URI <c>SourceKey</c> from a local image path.
    /// </summary>
    /// <param name="filePath">Path to the source image file.</param>
    /// <returns>A <c>file://</c> URI string suitable for <c>ReportLayoutItemDefinition.SourceKey</c>.</returns>
    public static string CreateFileSourceKey(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A file path is required.", nameof(filePath));

        var absolutePath = Path.GetFullPath(filePath);
        return new Uri(absolutePath, UriKind.Absolute).AbsoluteUri;
    }

    private static string? InferMimeTypeFromPath(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrWhiteSpace(extension))
            return null;

        return extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => null
        };
    }
}
