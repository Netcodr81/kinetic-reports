namespace KineticReports.Core.Authoring.Compilation;

using KineticReports.Core.Authoring.Documents;
using KineticReports.Core.Definition;

/// <summary>
/// Provides a one-call pipeline for compiling authoring documents into runtime definitions.
/// </summary>
public interface IDesignerDocumentCompilationService
{
    /// <summary>
    /// Compiles a designer document JSON payload into a runtime <see cref="ReportDefinition"/>.
    /// </summary>
    /// <param name="designerDocumentJson">Serialized <see cref="ReportDesignerDocument"/> JSON.</param>
    /// <returns>The compiled runtime report definition.</returns>
    ReportDefinition CompileFromJson(string designerDocumentJson);

    /// <summary>
    /// Compiles a designer document instance into a runtime <see cref="ReportDefinition"/>.
    /// </summary>
    /// <param name="document">The authoring document to compile.</param>
    /// <returns>The compiled runtime report definition.</returns>
    ReportDefinition Compile(ReportDesignerDocument document);
}