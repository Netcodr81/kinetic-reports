namespace KineticReports.Core.Layout;

using KineticReports.Core.Styling;

/// <summary>
/// Creates strongly-typed <see cref="ReportBlock"/> instances for code-first builders.
/// </summary>
public static class ReportBlockFactory
{
    /// <summary>
    /// Creates a <see cref="ReportBlock"/> from a strongly typed <see cref="BlockType"/>.
    /// </summary>
    /// <param name="blockType">Functional block role to create.</param>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved style for the block.</param>
    /// <param name="children">Optional block children.</param>
    /// <param name="forcePageBreakBefore">Whether to force a page break before this block.</param>
    /// <param name="keepTogether">Whether this block should avoid splitting across pages.</param>
    /// <returns>The concrete report block instance.</returns>
    public static ReportBlock Create(
        BlockType blockType,
        string id,
        AppliedStyle style,
        IReadOnlyList<LayoutBlock>? children = null,
        bool forcePageBreakBefore = false,
        bool keepTogether = false)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(style);

        var content = children ?? [];

        return blockType switch
        {
            BlockType.PageHeader => new PageHeaderBlock
            {
                Id = id,
                Style = style,
                Children = content,
                ForcePageBreakBefore = forcePageBreakBefore,
                KeepTogether = keepTogether,
            },
            BlockType.ReportHeader => new HeaderBlock
            {
                Id = id,
                Style = style,
                Children = content,
                ForcePageBreakBefore = forcePageBreakBefore,
                KeepTogether = keepTogether,
            },
            BlockType.GroupHeader => new GroupHeaderBlock
            {
                Id = id,
                Style = style,
                Children = content,
                ForcePageBreakBefore = forcePageBreakBefore,
                KeepTogether = keepTogether,
            },
            BlockType.Detail => new DetailBlock
            {
                Id = id,
                Style = style,
                Children = content,
                ForcePageBreakBefore = forcePageBreakBefore,
                KeepTogether = keepTogether,
            },
            BlockType.GroupFooter => new GroupFooterBlock
            {
                Id = id,
                Style = style,
                Children = content,
                ForcePageBreakBefore = forcePageBreakBefore,
                KeepTogether = keepTogether,
            },
            BlockType.ReportFooter => new FooterBlock
            {
                Id = id,
                Style = style,
                Children = content,
                ForcePageBreakBefore = forcePageBreakBefore,
                KeepTogether = keepTogether,
            },
            BlockType.PageFooter => new PageFooterBlock
            {
                Id = id,
                Style = style,
                Children = content,
                ForcePageBreakBefore = forcePageBreakBefore,
                KeepTogether = keepTogether,
            },
            _ => new DetailBlock
            {
                Id = id,
                Style = style,
                Children = content,
                ForcePageBreakBefore = forcePageBreakBefore,
                KeepTogether = keepTogether,
            }
        };
    }

    /// <summary>
    /// Creates a non-visual detail block that forces a page break before subsequent content.
    /// </summary>
    /// <param name="id">Stable block id.</param>
    /// <param name="style">Resolved style for the break marker block.</param>
    /// <returns>A detail block configured to force a page break.</returns>
    public static ReportBlock CreatePageBreak(string id, AppliedStyle style)
    {
        return Create(BlockType.Detail, id, style, forcePageBreakBefore: true);
    }
}