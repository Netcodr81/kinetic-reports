namespace KineticReports.Layout;

using KineticReports.Core.Layout;

/// <summary>
/// Runs the Measure, Arrange, and Pagination passes on a list of
/// <see cref="BandElement"/> objects and returns the resulting immutable
/// <see cref="LayoutTree"/>.
/// </summary>
public interface ILayoutEngine
{
    /// <summary>
    /// Lays out the supplied bands and produces an immutable <see cref="LayoutTree"/>.
    /// </summary>
    /// <param name="bands">
    /// The ordered list of bands to lay out. The engine automatically separates
    /// <see cref="BandKind.PageHeader"/> and <see cref="BandKind.PageFooter"/> bands
    /// from the body bands used for content pagination.
    /// </param>
    /// <param name="options">Page dimensions and margin configuration.</param>
    /// <param name="context">Font metrics and image-resolution services.</param>
    /// <returns>The fully laid-out, immutable <see cref="LayoutTree"/>.</returns>
    LayoutTree Layout(IReadOnlyList<BandElement> bands, LayoutOptions options, IMeasureContext context);
}
