namespace KineticReports.Visual;

/// <summary>
/// Builds text search indices from visual documents.
/// </summary>
public sealed class VisualTextSearchIndexBuilder
{
    /// <summary>
    /// Builds a text search index from a visual document.
    /// </summary>
    /// <param name="visualDocument">Visual document to index.</param>
    /// <returns>Built text search index.</returns>
    public VisualTextSearchIndex Build(VisualDocument visualDocument)
    {
        if (visualDocument == null) throw new ArgumentNullException(nameof(visualDocument));

        var entries = new List<VisualTextSearchEntry>();

        foreach (var page in visualDocument.Pages)
        {
            foreach (var layer in page.Layers)
            {
                foreach (var element in layer.Elements)
                    Flatten(page.PageNumber, layer.Name, element, entries);
            }
        }

        return new VisualTextSearchIndex(entries);
    }

    private static void Flatten(
        int pageNumber,
        string layerName,
        VisualElement element,
        List<VisualTextSearchEntry> entries)
    {
        if (element is VisualText text)
        {
            entries.Add(new VisualTextSearchEntry
            {
                PageNumber = pageNumber,
                LayerName = layerName,
                ElementId = text.Id,
                Bounds = text.Bounds,
                Text = text.Text
            });
        }

        if (element is not VisualContainer container)
            return;

        foreach (var child in container.Children)
            Flatten(pageNumber, layerName, child, entries);
    }
}
