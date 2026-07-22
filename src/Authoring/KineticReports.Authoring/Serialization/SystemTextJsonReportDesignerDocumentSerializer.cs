namespace KineticReports.Authoring.Serialization;

using System.Text.Json;
using KineticReports.Authoring.Documents;

/// <summary>
/// JSON serializer implementation for designer documents using System.Text.Json.
/// </summary>
public sealed class SystemTextJsonReportDesignerDocumentSerializer : IReportDesignerDocumentSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <inheritdoc/>
    public string Serialize(ReportDesignerDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return JsonSerializer.Serialize(document, Options);
    }

    /// <inheritdoc/>
    public ReportDesignerDocument Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON content must be provided.", nameof(json));

        return JsonSerializer.Deserialize<ReportDesignerDocument>(json, Options)
            ?? throw new InvalidOperationException("Failed to deserialize ReportDesignerDocument JSON.");
    }
}