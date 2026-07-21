using KineticReports.Core.Layout;
using KineticReports.Engine.Data;
using KineticReports.Engine.Expressions;

namespace KineticReports.Engine.Building;

/// <summary>
/// Default implementation of <see cref="IReportBuilder"/> that returns an empty block list.
/// Override or replace this with a real blocks builder that constructs the report element hierarchy
/// based on the report definition and resolved data sources.
/// </summary>
public sealed class DefaultReportBuilder : IReportBuilder
{
    /// <summary>
    /// Builds report blocks from the data context and definition.
    /// </summary>
    /// <remarks>
    /// This is a placeholder implementation. The Report Builder is responsible for:
    /// 1. Taking the report definition (structure/layout)
    /// 2. Combining it with the resolved data from DataContext
    /// 3. Creating an element tree with ReportBlock, TextElement, TableElement, etc.
    /// 4. Evaluating expressions for data binding
    ///
    /// To implement this properly:
    /// - Iterate through the report definition blocks
    /// - For each block, bind it to the appropriate data source
    /// - Evaluate expressions using the provided evaluator
    /// - Create the corresponding block element list
    /// </remarks>
    public IReadOnlyList<ReportBlock> Build(
        DataContext dataContext,
        IExpressionEvaluator evaluator)
    {
        // Return an empty list for now - your implementation should build the real tree
        return Array.Empty<ReportBlock>();
    }
}
