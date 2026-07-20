namespace KineticReports.Core.Layout;

using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;

/// <summary>
/// Represents a text element in the layout tree.
/// After the Arrange pass, the element holds the shaped and wrapped <see cref="TextRuns"/>
/// ready for the renderer to consume.
/// </summary>
public sealed class TextElement : LayoutElement
{
    private IMeasureContext? _measureContext;

    /// <inheritdoc/>
    public override LayoutElementType ElementType => LayoutElementType.Text;

    /// <summary>
    /// Gets the display text content (after expression evaluation has been applied).
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the shaped, word-wrapped text runs produced by the Arrange pass.
    /// Populated after <see cref="Arrange"/> is called; empty before then.
    /// </summary>
    public IReadOnlyList<TextRun> TextRuns { get; private set; } = [];

    /// <inheritdoc/>
    public override void Measure(Size availableSize, IMeasureContext context)
    {
        _measureContext = context;
        DesiredSize = context.FontMetrics.MeasureText(Text, Style, availableSize.Width);
    }

    /// <inheritdoc/>
    public override void Arrange(Rect finalRect)
    {
        Bounds = finalRect;

        if (_measureContext != null && !string.IsNullOrEmpty(Text))
        {
            SetTextRuns(_measureContext.FontMetrics.ShapeText(Text, Style, finalRect));
            _measureContext = null;
        }
    }

    /// <summary>
    /// Sets the shaped text runs. Called internally by the layout pipeline.
    /// </summary>
    /// <param name="runs">The computed text runs.</param>
    internal void SetTextRuns(IReadOnlyList<TextRun> runs) => TextRuns = runs;
}
