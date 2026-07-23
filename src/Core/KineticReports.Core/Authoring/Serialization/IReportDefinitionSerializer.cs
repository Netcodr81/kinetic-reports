namespace KineticReports.Core.Authoring.Serialization;

using KineticReports.Core.Definition;

/// <summary>
/// Serializes and deserializes <see cref="ReportDefinition"/> to and from JSON.
/// </summary>
public interface IReportDefinitionSerializer
{
    /// <summary>Serializes a report definition to JSON.</summary>
    string Serialize(ReportDefinition definition);

    /// <summary>Deserializes a report definition from JSON.</summary>
    ReportDefinition Deserialize(string json);
}
