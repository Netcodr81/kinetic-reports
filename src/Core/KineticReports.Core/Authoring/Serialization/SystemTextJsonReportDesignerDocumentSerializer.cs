namespace KineticReports.Core.Authoring.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using KineticReports.Core.Authoring.Documents;

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

    static SystemTextJsonReportDesignerDocumentSerializer()
    {
        Options.Converters.Add(new JsonStringEnumConverter());
    }

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

        JsonDuplicateKeyValidator.EnsureNoDuplicateKeys(json);

        return JsonSerializer.Deserialize<ReportDesignerDocument>(json, Options)
            ?? throw new InvalidOperationException("Failed to deserialize ReportDesignerDocument JSON.");
    }
}