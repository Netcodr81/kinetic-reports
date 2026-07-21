namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;

/// <summary>
/// Abstract base class for all elements in the report layout.
/// The report layout is produced by the Arrange pass and is immutable thereafter (ADR-011).
/// </summary>
public abstract class LayoutElement
{
    /// <summary>Gets the stable unique identifier of this element.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the fully resolved, immutable style for this element.</summary>
    public required ResolvedStyle Style { get; init; }

    /// <summary>
    /// Gets the final axis-aligned bounds assigned during the Arrange pass,
    /// in page-relative coordinates (DIPs).
    /// </summary>
    public Rect Bounds { get; protected set; }

    /// <summary>
    /// Gets the desired size as computed by the LayoutSizing pass.
    /// This value is set by <see cref="Measure"/> and consumed by the parent's Arrange pass.
    /// </summary>
    public Size DesiredSize { get; protected set; }

    /// <summary>Gets the concrete type of this layout element.</summary>
    public abstract LayoutElementType ElementType { get; }

    /// <summary>
    /// Performs the LayoutSizing pass, computing <see cref="DesiredSize"/> given the
    /// <paramref name="availableSize"/> constraint.
    /// </summary>
    /// <param name="availableSize">
    /// The size available to this element. Pass <see cref="Size.Infinity"/> for
    /// unconstrained measurement.
    /// </param>
    /// <param name="context">Text layout and image resolution services.</param>
    public abstract void LayoutSize(Size availableSize, ILayoutSizingContext context);

    /// <summary>
    /// Performs the Arrange pass, assigning the final <see cref="Bounds"/> within
    /// the given <paramref name="finalRect"/>.
    /// </summary>
    /// <param name="finalRect">
    /// The rectangle allocated to this element by its parent.
    /// </param>
    public abstract void Arrange(Rect finalRect);
}
