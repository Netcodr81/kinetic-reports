namespace KineticReports.Core.Engine.Expressions;

/// <summary>
/// Represents parse and semantic validation results for an expression string.
/// </summary>
public sealed record ExpressionValidationResult
{
    /// <summary>
    /// Gets a value indicating whether validation succeeded.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Gets the list of validation errors. Empty when <see cref="IsValid"/> is <see langword="true"/>.
    /// </summary>
    public required IReadOnlyList<string> Errors { get; init; }

    /// <summary>
    /// Gets a successful validation result.
    /// </summary>
    public static ExpressionValidationResult Success { get; } = new ExpressionValidationResult
    {
        IsValid = true,
        Errors = []
    };

    /// <summary>
    /// Creates a failed validation result with one or more messages.
    /// </summary>
    /// <param name="errors">Validation error messages.</param>
    /// <returns>The failed validation result.</returns>
    public static ExpressionValidationResult Failure(params string[] errors)
    {
        return new ExpressionValidationResult
        {
            IsValid = false,
            Errors = errors
        };
    }
}
