namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>
/// Represents a single rendered page in the report layout.
/// A page contains optional header and footer sections plus the body content.
/// </summary>
public sealed class PageElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Page;

    /// <summary>Gets the page width in DIPs.</summary>
    public float PageWidth { get; init; }

    /// <summary>Gets the page height in DIPs.</summary>
    public float PageHeight { get; init; }

    /// <summary>Gets the one-based page number within the document.</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the optional header section for this page.</summary>
    public SectionElement? Header { get; init; }

    /// <summary>Gets the optional footer section for this page.</summary>
    public SectionElement? Footer { get; init; }

    /// <summary>Gets the body elements rendered between the header and footer.</summary>
    public IReadOnlyList<LayoutElement> Children { get; init; } = [];

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        DesiredSize = new Size(PageWidth, PageHeight);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }
}
