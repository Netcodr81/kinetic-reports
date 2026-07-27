namespace KineticReports.Core.Engine.Expressions;

/// <summary>
/// Validates expression strings without executing them.
/// </summary>
public interface IExpressionValidator
{
    /// <summary>
    /// Validates a report expression and returns validation details.
    /// </summary>
    /// <param name="expression">Expression text to validate.</param>
    /// <returns>A validation result containing success state and error messages.</returns>
    ExpressionValidationResult Validate(string expression);
}
