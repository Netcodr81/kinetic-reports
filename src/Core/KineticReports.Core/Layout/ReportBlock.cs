namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;

/// <summary>Specifies the functional role of a <see cref="ReportBlock"/>.</summary>
public enum BlockType
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
/// Represents a data-driven horizontal report block in the report layout.
/// Blocks are the primary mechanism for repeating data rows and group summaries.
/// </summary>
public abstract class ReportBlock : LayoutBlock
{
    /// <inheritdoc/>
    public override LayoutBlockType ElementType => LayoutBlockType.Band;

    /// <summary>Gets the functional role of this block.</summary>
    public virtual BlockType Kind { get; init; } = BlockType.Detail;

    /// <summary>
    /// Gets a value indicating whether a page break is forced before this block.
    /// </summary>
    public bool ForcePageBreakBefore { get; init; }

    /// <summary>
    /// Gets a value indicating whether this block must not be split across pages.
    /// </summary>
    public bool KeepTogether { get; init; }

    /// <summary>Gets the child elements within this block.</summary>
    public IReadOnlyList<LayoutBlock> Children { get; init; } = [];

    /// <inheritdoc/>
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        var horizontalInset = Style.Padding.Horizontal + GetBorderLeftWidth() + GetBorderRightWidth();
        var verticalInset = Style.Padding.Vertical + GetBorderTopWidth() + GetBorderBottomWidth();

        var contentWidth = Math.Max(0f, availableSize.Width - horizontalInset);
        float totalHeight = 0f;

        foreach (var child in Children)
        {
            child.LayoutSize(new Size(contentWidth, availableSize.Height), context);
            totalHeight += child.DesiredSize.Height;
        }

        DesiredSize = new Size(availableSize.Width, totalHeight + verticalInset);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
        var x = finalRect.X + Style.Padding.Left + GetBorderLeftWidth();
        float y = finalRect.Y + Style.Padding.Top + GetBorderTopWidth();
        var contentWidth = Math.Max(0f, finalRect.Width - Style.Padding.Horizontal - GetBorderLeftWidth() - GetBorderRightWidth());

        foreach (var child in Children)
        {
            child.Arrange(new Rect(x, y, contentWidth, child.DesiredSize.Height));
            y += child.DesiredSize.Height;
        }
    }

    private float GetBorderTopWidth() => Style.Border?.Top?.Width ?? 0f;

    private float GetBorderRightWidth() => Style.Border?.Right?.Width ?? 0f;

    private float GetBorderBottomWidth() => Style.Border?.Bottom?.Width ?? 0f;

    private float GetBorderLeftWidth() => Style.Border?.Left?.Width ?? 0f;
}

/// <summary>Represents a report-header block rendered once at report start.</summary>
public sealed class HeaderBlock : ReportBlock
{
    /// <inheritdoc/>
    public override BlockType Kind { get; init; } = BlockType.ReportHeader;
}

/// <summary>Represents a detail block rendered for each data row.</summary>
public sealed class DetailBlock : ReportBlock
{
    /// <inheritdoc/>
    public override BlockType Kind { get; init; } = BlockType.Detail;
}

/// <summary>Represents a report-footer block rendered once at report end.</summary>
public sealed class FooterBlock : ReportBlock
{
    /// <inheritdoc/>
    public override BlockType Kind { get; init; } = BlockType.ReportFooter;
}

/// <summary>Represents a page-header block repeated at the top of each page.</summary>
public sealed class PageHeaderBlock : ReportBlock
{
    /// <inheritdoc/>
    public override BlockType Kind { get; init; } = BlockType.PageHeader;
}

/// <summary>Represents a page-footer block repeated at the bottom of each page.</summary>
public sealed class PageFooterBlock : ReportBlock
{
    /// <inheritdoc/>
    public override BlockType Kind { get; init; } = BlockType.PageFooter;
}

/// <summary>Represents a group-header block rendered at group boundaries.</summary>
public sealed class GroupHeaderBlock : ReportBlock
{
    /// <inheritdoc/>
    public override BlockType Kind { get; init; } = BlockType.GroupHeader;
}

/// <summary>Represents a group-footer block rendered at group boundaries.</summary>
public sealed class GroupFooterBlock : ReportBlock
{
    /// <inheritdoc/>
    public override BlockType Kind { get; init; } = BlockType.GroupFooter;
}
