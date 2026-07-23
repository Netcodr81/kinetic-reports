namespace KineticReports.Visual;

using KineticReports.Core.Layout;

/// <summary>
/// Builds a renderer-agnostic <see cref="VisualDocument"/> from a semantic <see cref="ReportDocument"/>.
/// </summary>
public interface IVisualDocumentBuilder
{
    /// <summary>
    /// Builds an immutable visual scene graph for the supplied report document.
    /// </summary>
    /// <param name="reportDocument">The semantic report document.</param>
    /// <returns>The visual scene graph representation.</returns>
    VisualDocument Build(ReportDocument reportDocument);
}
