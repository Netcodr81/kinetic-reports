namespace KineticReports.Engine.Building;

using KineticReports.Core.Layout;
using KineticReports.Engine.Data;
using KineticReports.Engine.Expressions;

/// <summary>
/// Builds report blocks from a data context and an expression evaluator.
/// Implementations are responsible for translating the report definition, iterating
/// data rows, evaluating field expressions, and producing ordered <see cref="ReportBlock"/>
/// objects ready for the layout engine.
/// </summary>
public interface IReportBuilder
{
    /// <summary>
    /// Constructs the ordered list of blocks that represent the full data-bound
    /// report content.
    /// </summary>
    /// <param name="dataContext">The fully resolved data for this report run.</param>
    /// <param name="evaluator">Expression evaluator used to resolve field references.</param>
    /// <returns>
    /// An ordered list of <see cref="ReportBlock"/> objects — including page-header,
    /// report-header, detail, and footer blocks — ready to be passed to
    /// <see cref="ILayoutEngine"/>.
    /// </returns>
    IReadOnlyList<ReportBlock> Build(DataContext dataContext, IExpressionEvaluator evaluator);
}
