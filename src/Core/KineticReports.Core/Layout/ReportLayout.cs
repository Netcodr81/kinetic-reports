namespace KineticReports.Core.Layout;

/// <summary>
/// Represents the complete, immutable report layout produced by the layout engine.
/// The report layout is the sole input to all renderers and exporters (ADR-013).
/// </summary>
public sealed class ReportLayout
{
    /// <summary>Gets the pages in document order.</summary>
    public required IReadOnlyList<PageElement> Pages { get; init; }

    /// <summary>Gets the total number of pages in the document.</summary>
    public int PageCount => Pages.Count;
}
