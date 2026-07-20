namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>Specifies the functional role of a <see cref="BandElement"/>.</summary>
public enum BandKind
{
    /// <summary>Page header — repeated at the top of every page.</summary>
    PageHeader,

    /// <summary>Report header — rendered once at the beginning of the report.</summary>
    ReportHeader,

    /// <summary>Group header — rendered at the start of each data group.</summary>
    GroupHeader,

    /// <summary>Detail — rendered once per data row.</summary>
    Detail,

    /// <summary>Group footer — rendered at the end of each data group.</summary>
    GroupFooter,

    /// <summary>Report footer — rendered once at the end of the report.</summary>
    ReportFooter,

    /// <summary>Page footer — repeated at the bottom of every page.</summary>
    PageFooter,
}

/// <summary>
/// Represents a data-driven horizontal band in the report layout.
/// Bands are the primary mechanism for repeating data rows and group summaries.
/// </summary>
public sealed class BandElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Band;

    /// <summary>Gets the functional role of this band.</summary>
    public BandKind Kind { get; init; }

    /// <summary>
    /// Gets a value indicating whether a page break is forced before this band.
    /// </summary>
    public bool ForcePageBreakBefore { get; init; }

    /// <summary>
    /// Gets a value indicating whether this band must not be split across pages.
    /// </summary>
    public bool KeepTogether { get; init; }

    /// <summary>Gets the child elements within this band.</summary>
    public IReadOnlyList<LayoutElement> Children { get; init; } = [];

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        float totalHeight = 0f;

        foreach (var child in Children)
        {
            child.Measure(availableSize, context);
            totalHeight += child.DesiredSize.Height;
        }

        DesiredSize = new Size(availableSize.Width, totalHeight);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
        float y = finalRect.Y;

        foreach (var child in Children)
        {
            child.Arrange(new Rect(finalRect.X, y, finalRect.Width, child.DesiredSize.Height));
            y += child.DesiredSize.Height;
        }
    }
}
