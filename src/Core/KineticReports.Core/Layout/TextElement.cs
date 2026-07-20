namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;

/// <summary>
/// Represents a text element in the layout tree.
/// After layout, the element holds the shaped and wrapped <see cref="TextRuns"/>
/// ready for the renderer to consume.
/// </summary>
public sealed class TextElement : LayoutElement
{
    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Text;

    /// <summary>
    /// Gets the display text content (after expression evaluation has been applied).
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the shaped, word-wrapped text runs produced by the layout engine.
    /// Populated after <see cref="Measure"/> is called; empty before then.
    /// </summary>
    public IReadOnlyList<TextRun> TextRuns { get; private set; } = [];

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        DesiredSize = context.FontMetrics.MeasureText(Text, Style, availableSize.Width);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;
    }

    /// <summary>
    /// Sets the shaped text runs produced by the layout engine after word-wrapping
    /// and glyph shaping. This is called internally and is not part of the public API.
    /// </summary>
    /// <param name="runs">The computed text runs.</param>
    internal void SetTextRuns(IReadOnlyList<TextRun> runs) => TextRuns = runs;
}
