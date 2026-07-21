namespace KineticReports.Authoring.Compilation;

using KineticReports.Authoring.Documents;
using KineticReports.Core.Definition;

/// <summary>
/// Compiles a designer document into an executable <see cref="ReportDefinition"/>.
/// </summary>
public interface IDesignerDocumentCompiler
{
    /// <summary>
    /// Compiles the input designer document to a canonical report definition.
    /// </summary>
    /// <param name="document">The source designer document.</param>
    /// <returns>The compiled report definition.</returns>
    ReportDefinition Compile(ReportDesignerDocument document);
}
