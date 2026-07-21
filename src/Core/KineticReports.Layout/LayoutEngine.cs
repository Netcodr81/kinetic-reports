namespace KineticReports.Layout;

using KineticReports.Core.Layout;
using KineticReports.Layout.Pagination;

/// <summary>
/// Default implementation of <see cref="ILayoutEngine"/>.
/// Delegates to <see cref="PaginationEngine"/> to run the LayoutSizing, Arrange,
/// and Pagination passes and produce an immutable <see cref="ReportLayout"/>.
/// </summary>
public sealed class LayoutEngine : ILayoutEngine
{
    /// <inheritdoc/>
    public ReportLayout Layout(IReadOnlyList<ReportBlock> blocks, LayoutOptions options, ILayoutSizingContext context)
    {
        var pages = new PaginationEngine().Paginate(blocks, options, context);
        return new ReportLayout { Pages = pages };
    }
}
