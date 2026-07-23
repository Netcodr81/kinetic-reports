namespace KineticReports.Visual;

/// <summary>
/// Immutable index of searchable visual text entries.
/// </summary>
public sealed class VisualTextSearchIndex
{
    /// <summary>
    /// Gets searchable entries in deterministic draw order.
    /// </summary>
    public IReadOnlyList<VisualTextSearchEntry> Entries { get; }

    /// <summary>
    /// Initializes a new <see cref="VisualTextSearchIndex"/>.
    /// </summary>
    /// <param name="entries">Searchable entries.</param>
    public VisualTextSearchIndex(IReadOnlyList<VisualTextSearchEntry> entries)
    {
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
    }

    /// <summary>
    /// Searches indexed text entries by case-insensitive substring match.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="pageNumber">Optional one-based page filter.</param>
    /// <returns>Matching entries in deterministic order.</returns>
    public IReadOnlyList<VisualTextSearchEntry> Search(string query, int? pageNumber = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var normalizedQuery = query.Trim();
        var result = Entries
            .Where(entry => (!pageNumber.HasValue || entry.PageNumber == pageNumber.Value)
                && entry.Text.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return result;
    }
}
