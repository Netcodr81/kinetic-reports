namespace KineticReports.Core.Visual;

using System.Text.Json;

/// <summary>
/// System.Text.Json implementation of <see cref="IVisualDocumentSerializer"/>.
/// </summary>
public sealed class JsonVisualDocumentSerializer : IVisualDocumentSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    /// <inheritdoc/>
    public string Serialize(VisualDocument visualDocument)
    {
        if (visualDocument == null) throw new ArgumentNullException(nameof(visualDocument));

        return JsonSerializer.Serialize(visualDocument, SerializerOptions);
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(VisualDocument visualDocument, Stream output, CancellationToken ct = default)
    {
        if (visualDocument == null) throw new ArgumentNullException(nameof(visualDocument));
        if (output == null) throw new ArgumentNullException(nameof(output));

        await JsonSerializer.SerializeAsync(output, visualDocument, SerializerOptions, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public VisualDocument Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON payload is required.", nameof(json));

        var document = JsonSerializer.Deserialize<VisualDocument>(json, SerializerOptions);
        if (document == null)
            throw new JsonException("VisualDocument payload could not be deserialized.");

        return document;
    }

    /// <inheritdoc/>
    public async Task<VisualDocument> DeserializeAsync(Stream input, CancellationToken ct = default)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        var document = await JsonSerializer.DeserializeAsync<VisualDocument>(input, SerializerOptions, ct).ConfigureAwait(false);
        if (document == null)
            throw new JsonException("VisualDocument payload could not be deserialized.");

        return document;
    }
}
