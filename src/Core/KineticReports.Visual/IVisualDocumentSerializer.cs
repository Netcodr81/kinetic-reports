namespace KineticReports.Visual;

/// <summary>
/// Serializes and deserializes <see cref="VisualDocument"/> payloads.
/// </summary>
public interface IVisualDocumentSerializer
{
    /// <summary>
    /// Serializes a visual document to a JSON string.
    /// </summary>
    /// <param name="visualDocument">The visual document to serialize.</param>
    /// <returns>Serialized JSON payload.</returns>
    string Serialize(VisualDocument visualDocument);

    /// <summary>
    /// Serializes a visual document to a JSON stream.
    /// </summary>
    /// <param name="visualDocument">The visual document to serialize.</param>
    /// <param name="output">Destination stream.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SerializeAsync(VisualDocument visualDocument, Stream output, CancellationToken ct = default);

    /// <summary>
    /// Deserializes a visual document from a JSON string.
    /// </summary>
    /// <param name="json">Serialized JSON payload.</param>
    /// <returns>Deserialized visual document.</returns>
    VisualDocument Deserialize(string json);

    /// <summary>
    /// Deserializes a visual document from a JSON stream.
    /// </summary>
    /// <param name="input">Source stream.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Deserialized visual document.</returns>
    Task<VisualDocument> DeserializeAsync(Stream input, CancellationToken ct = default);
}
