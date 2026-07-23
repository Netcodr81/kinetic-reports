namespace KineticReports.Engine.Expressions;

/// <summary>
/// Evaluates a report expression string within a given <see cref="ExpressionContext"/>.
/// </summary>
/// <remarks>
/// Expressions may reference data fields using <c>{FieldName}</c> syntax,
/// report parameters, or may be plain literal strings.
/// See the Expression Language Specification (doc 05) for the full grammar.
/// </remarks>
public interface IExpressionEvaluator
{
    /// <summary>
    /// Evaluates the expression and returns the result.
    /// </summary>
    /// <param name="expression">
    /// The expression string to evaluate, e.g. <c>{OrderDate}</c> or <c>Hello, World</c>.
    /// </param>
    /// <param name="context">
    /// The evaluation context supplying the current data row, parameters, and page state.
    /// </param>
    /// <returns>
    /// The evaluated value, or <see langword="null"/> when the expression cannot be resolved.
    /// </returns>
    object? Evaluate(string expression, ExpressionContext context);
}
