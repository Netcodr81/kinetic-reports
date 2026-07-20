namespace KineticReports.Engine.Expressions;

/// <summary>
/// A simple <see cref="IExpressionEvaluator"/> that resolves field references
/// (<c>{FieldName}</c>) from the current data row and parameters, and returns
/// all other input as a literal string.
/// </summary>
/// <remarks>
/// This evaluator handles the two most common expression forms:
/// <list type="bullet">
///   <item><description><c>{FieldName}</c> — resolved from the current row, then parameters.</description></item>
///   <item><description>Any other string — returned unchanged as a literal.</description></item>
/// </list>
/// For full expression language support (arithmetic, functions, conditionals),
/// replace this with a complete evaluator backed by the grammar in doc 05.
/// </remarks>
public sealed class LiteralEvaluator : IExpressionEvaluator
{
    /// <inheritdoc/>
    public object? Evaluate(string expression, ExpressionContext context)
    {
        if (string.IsNullOrEmpty(expression))
            return null;

        // Field reference: {FieldName}
        if (expression.Length > 2 && expression[0] == '{' && expression[^1] == '}')
        {
            var fieldName = expression[1..^1];

            if (context.CurrentRow.TryGetValue(fieldName, out var rowValue))
                return rowValue;

            if (context.Parameters.TryGetValue(fieldName, out var paramValue))
                return paramValue;

            return null;
        }

        // Literal string
        return expression;
    }
}
