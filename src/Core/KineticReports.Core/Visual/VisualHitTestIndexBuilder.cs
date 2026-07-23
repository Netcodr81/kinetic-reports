namespace KineticReports.Core.Visual;

/// <summary>
/// Builds hit-test indices for visual documents.
/// </summary>
public sealed class VisualHitTestIndexBuilder
{
    /// <summary>
    /// Builds an immutable hit-test index from the visual document.
    /// </summary>
    /// <param name="visualDocument">Visual document to index.</param>
    /// <returns>Built hit-test index.</returns>
    public VisualHitTestIndex Build(VisualDocument visualDocument)
    {
        if (visualDocument == null) throw new ArgumentNullException(nameof(visualDocument));

        var entries = new List<VisualHitTestEntry>();

        foreach (var page in visualDocument.Pages)
        {
            foreach (var layer in page.Layers)
            {
                foreach (var element in layer.Elements)
                    FlattenElement(page.PageNumber, layer.Name, element, entries);
            }
        }

        return new VisualHitTestIndex(entries);
    }

    private static void FlattenElement(
        int pageNumber,
        string layerName,
        VisualElement element,
        List<VisualHitTestEntry> entries)
    {
        entries.Add(new VisualHitTestEntry
        {
            PageNumber = pageNumber,
            LayerName = layerName,
            Element = element
        });

        if (element is not VisualContainer container)
            return;

        foreach (var child in container.Children)
            FlattenElement(pageNumber, layerName, child, entries);
    }
}
