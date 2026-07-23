namespace KineticReports.Core.LayoutEngine;

using KineticReports.Core.Layout;

/// <summary>
/// Runs the LayoutSizing, Arrange, and Pagination passes on a list of
/// <see cref="ReportBlock"/> objects and returns the resulting immutable
/// <see cref="ReportDocument"/>.
/// </summary>
public interface ILayoutEngine
{
    /// <summary>
    /// Lays out the supplied blocks and produces an immutable <see cref="ReportDocument"/>.
    /// </summary>
    /// <param name="blocks">
    /// The ordered list of blocks to lay out. The engine automatically separates
    /// <see cref="BlockType.PageHeader"/> and <see cref="BlockType.PageFooter"/> blocks
    /// from the body blocks used for content pagination.
    /// </param>
    /// <param name="options">Page dimensions and margin configuration.</param>
    /// <param name="context">Text layout and image-resolution services.</param>
    /// <returns>The fully laid-out, immutable <see cref="ReportDocument"/>.</returns>
    ReportDocument Layout(IReadOnlyList<ReportBlock> blocks, LayoutOptions options, ILayoutSizingContext context);
}
