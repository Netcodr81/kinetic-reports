namespace KineticReports.Core.Authoring.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using KineticReports.Core.Definition;

/// <summary>
/// JSON serializer implementation for report definitions using System.Text.Json.
/// </summary>
public sealed class SystemTextJsonReportDefinitionSerializer : IReportDefinitionSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    static SystemTextJsonReportDefinitionSerializer()
    {
        Options.Converters.Add(new JsonStringEnumConverter());
    }

    /// <inheritdoc/>
    public string Serialize(ReportDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return JsonSerializer.Serialize(definition, Options);
    }

    /// <inheritdoc/>
    public ReportDefinition Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON content must be provided.", nameof(json));

        return JsonSerializer.Deserialize<ReportDefinition>(json, Options)
            ?? throw new InvalidOperationException("Failed to deserialize ReportDefinition JSON.");
    }
}
