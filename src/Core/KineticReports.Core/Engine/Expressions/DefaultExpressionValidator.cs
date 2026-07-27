namespace KineticReports.Core.Engine.Expressions;

using System.Text.RegularExpressions;

/// <summary>
/// Default non-executing validator for expression syntax and supported function semantics.
/// </summary>
public sealed class DefaultExpressionValidator : IExpressionValidator
{
    private static readonly Regex TokenPattern = new("\\{(?<name>[A-Za-z0-9_]+)\\}", RegexOptions.Compiled);
    private static readonly Regex PotentialFunctionPattern = new("^[A-Za-z_][A-Za-z0-9_]*\\s*\\(", RegexOptions.Compiled);

    /// <inheritdoc/>
    public ExpressionValidationResult Validate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return ExpressionValidationResult.Success;

        var trimmed = expression.Trim();

        if (FunctionExpressionRuntime.LooksLikeFunctionExpression(trimmed) || PotentialFunctionPattern.IsMatch(trimmed))
            return FunctionExpressionRuntime.ValidateFunctionExpression(trimmed);

        if (trimmed.StartsWith('{') && !trimmed.EndsWith('}'))
            return ExpressionValidationResult.Failure("Token expression must end with '}'.");

        if (trimmed.EndsWith('}') && !trimmed.Contains('{'))
            return ExpressionValidationResult.Failure("Token expression contains unmatched closing brace '}'.");

        var openBraceCount = expression.Count(ch => ch == '{');
        var closeBraceCount = expression.Count(ch => ch == '}');
        if (openBraceCount != closeBraceCount)
            return ExpressionValidationResult.Failure("Expression contains unmatched token braces.");

        var tokens = TokenPattern.Matches(expression);
        if (openBraceCount > tokens.Count || closeBraceCount > tokens.Count)
            return ExpressionValidationResult.Failure("Expression contains malformed token placeholders.");

        return ExpressionValidationResult.Success;
    }
}
