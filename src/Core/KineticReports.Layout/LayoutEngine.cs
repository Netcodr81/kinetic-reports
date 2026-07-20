namespace KineticReports.Layout;

using KineticReports.Core.Layout;
using KineticReports.Layout.Pagination;

/// <summary>
/// Default implementation of <see cref="ILayoutEngine"/>.
/// Delegates to <see cref="PaginationEngine"/> to run the Measure, Arrange,
/// and Pagination passes and produce an immutable <see cref="LayoutTree"/>.
/// </summary>
public sealed class LayoutEngine : ILayoutEngine
{
    /// <inheritdoc/>
    public LayoutTree Layout(IReadOnlyList<BandElement> bands, LayoutOptions options, IMeasureContext context)
    {
        var pages = new PaginationEngine().Paginate(bands, options, context);
        return new LayoutTree { Pages = pages };
    }
}
