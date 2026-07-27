namespace KineticReports.Core.Authoring.Serialization;

using System.Text.Json;

internal static class JsonDuplicateKeyValidator
{
    internal static void EnsureNoDuplicateKeys(string json)
    {
        using var document = JsonDocument.Parse(json);
        ValidateElement(document.RootElement, "$", StringComparer.Ordinal);
    }

    private static void ValidateElement(JsonElement element, string path, StringComparer comparer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    var keys = new HashSet<string>(comparer);

                    foreach (var property in element.EnumerateObject())
                    {
                        if (!keys.Add(property.Name))
                        {
                            throw new JsonException($"Duplicate JSON key '{property.Name}' found at path '{path}'.");
                        }

                        ValidateElement(property.Value, $"{path}.{property.Name}", comparer);
                    }

                    break;
                }

            case JsonValueKind.Array:
                {
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        ValidateElement(item, $"{path}[{index}]", comparer);
                        index++;
                    }

                    break;
                }
        }
    }
}
