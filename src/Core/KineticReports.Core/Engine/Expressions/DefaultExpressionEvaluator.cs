namespace KineticReports.Core.Engine.Expressions;

using System.Text.RegularExpressions;

/// <summary>
/// Default expression evaluator used by sample and host integrations.
/// </summary>
/// <remarks>
/// Supports both direct token expressions (for example <c>{Amount}</c>) and
/// token replacement inside larger literals (for example <c>Page {PageNumber}</c>).
/// </remarks>
public sealed class DefaultExpressionEvaluator : IExpressionEvaluator
{
    private static readonly Regex TokenPattern = new("\\{(?<name>[A-Za-z0-9_]+)\\}", RegexOptions.Compiled);

    /// <inheritdoc/>
    public object? Evaluate(string expression, ExpressionContext context)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return null;

        if (expression.Length > 2 && expression[0] == '{' && expression[^1] == '}')
        {
            var tokenName = expression[1..^1];
            return ResolveToken(tokenName, context);
        }

        return TokenPattern.Replace(
            expression,
            match =>
            {
                var tokenName = match.Groups["name"].Value;
                var value = ResolveToken(tokenName, context);
                return value is null ? string.Empty : Convert.ToString(value) ?? string.Empty;
            });
    }

    private static object? ResolveToken(string tokenName, ExpressionContext context)
    {
        if (context.CurrentRow.TryGetValue(tokenName, out var rowValue))
            return rowValue;

        if (context.Parameters.TryGetValue(tokenName, out var parameterValue))
            return parameterValue;

        if (string.Equals(tokenName, "PageNumber", StringComparison.OrdinalIgnoreCase))
            return context.PageNumber;

        if (string.Equals(tokenName, "CurrentDate", StringComparison.OrdinalIgnoreCase))
            return DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (string.Equals(tokenName, "CurrentTime", StringComparison.OrdinalIgnoreCase))
            return DateTime.UtcNow.ToString("HH:mm:ss");

        return null;
    }
}
