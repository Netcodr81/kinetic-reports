namespace KineticReports.Core.Authoring.Serialization;

using KineticReports.Core.Authoring.Documents;

/// <summary>
/// Serializes and deserializes <see cref="ReportDesignerDocument"/> to and from JSON.
/// </summary>
public interface IReportDesignerDocumentSerializer
{
    /// <summary>Serializes a designer document to JSON.</summary>
    string Serialize(ReportDesignerDocument document);

    /// <summary>Deserializes a designer document from JSON.</summary>
    ReportDesignerDocument Deserialize(string json);
}