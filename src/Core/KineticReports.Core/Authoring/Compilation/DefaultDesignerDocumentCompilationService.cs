namespace KineticReports.Authoring.Compilation;

using KineticReports.Authoring.Documents;
using KineticReports.Authoring.Serialization;
using KineticReports.Core.Definition;

/// <summary>
/// Default one-call pipeline for compiling authoring JSON into runtime report definitions.
/// </summary>
public sealed class DefaultDesignerDocumentCompilationService : IDesignerDocumentCompilationService
{
    private readonly IReportDesignerDocumentSerializer _documentSerializer;
    private readonly IDesignerDocumentCompiler _documentCompiler;

    /// <summary>
    /// Initializes a new instance of the compilation service.
    /// </summary>
    public DefaultDesignerDocumentCompilationService(
        IReportDesignerDocumentSerializer documentSerializer,
        IDesignerDocumentCompiler documentCompiler)
    {
        _documentSerializer = documentSerializer ?? throw new ArgumentNullException(nameof(documentSerializer));
        _documentCompiler = documentCompiler ?? throw new ArgumentNullException(nameof(documentCompiler));
    }

    /// <inheritdoc/>
    public ReportDefinition CompileFromJson(string designerDocumentJson)
    {
        var document = _documentSerializer.Deserialize(designerDocumentJson);
        return _documentCompiler.Compile(document);
    }

    /// <inheritdoc/>
    public ReportDefinition Compile(ReportDesignerDocument document)
    {
        return _documentCompiler.Compile(document);
    }
}