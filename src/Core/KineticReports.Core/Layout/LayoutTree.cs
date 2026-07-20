namespace KineticReports.Core.Layout;

/// <summary>
/// Represents the complete, immutable layout tree produced by the layout engine.
/// The layout tree is the sole input to all renderers and exporters (ADR-013).
/// </summary>
public sealed class LayoutTree
{
    /// <summary>Gets the pages in document order.</summary>
    public required IReadOnlyList<PageElement> Pages { get; init; }

    /// <summary>Gets the total number of pages in the document.</summary>
    public int PageCount => Pages.Count;
}
