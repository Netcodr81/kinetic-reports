namespace KineticReports.Core.Visual;

using KineticReports.Core.Geometry;

/// <summary>
/// Immutable hit-test index built from a <see cref="VisualDocument"/>.
/// </summary>
public sealed class VisualHitTestIndex
{
    /// <summary>
    /// Gets flattened hit-test entries in draw order.
    /// </summary>
    public IReadOnlyList<VisualHitTestEntry> Entries { get; }

    /// <summary>
    /// Initializes a new <see cref="VisualHitTestIndex"/>.
    /// </summary>
    /// <param name="entries">Flattened draw-order entries.</param>
    public VisualHitTestIndex(IReadOnlyList<VisualHitTestEntry> entries)
    {
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
    }

    /// <summary>
    /// Returns the topmost element hit at the given point on a page.
    /// </summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="point">Page-local point in DIPs.</param>
    /// <returns>Topmost matching entry, or <see langword="null"/> if no hit exists.</returns>
    public VisualHitTestEntry? HitTest(int pageNumber, Point point)
    {
        for (var i = Entries.Count - 1; i >= 0; i--)
        {
            var entry = Entries[i];
            if (entry.PageNumber != pageNumber)
                continue;

            if (entry.Element.Bounds.Contains(point))
                return entry;
        }

        return null;
    }
}
