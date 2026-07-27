namespace KineticReports.Core.Tests.Engine.Expressions;

using KineticReports.Core.Engine.Expressions;

public class DefaultExpressionValidatorTests
{
    private readonly DefaultExpressionValidator _sut = new();

    [Theory]
    [InlineData("Sum([1,2,3])")]
    [InlineData("If(true, 'ok', 'no')")]
    [InlineData("Count(Where(Parameter('items'), 'Amount', '>', 10))")]
    [InlineData("{CustomerName}")]
    [InlineData("Invoice {PageNumber}")]
    public void Validate_WithSupportedExpressions_ReturnsSuccess(string expression)
    {
        var result = _sut.Validate(expression);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Validate_WithUnknownFunction_ReturnsError()
    {
        var result = _sut.Validate("Mystery(1)");

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain("Unknown function 'Mystery'.");
    }

    [Fact]
    public void Validate_WithWrongArity_ReturnsError()
    {
        var result = _sut.Validate("Round(1,2,3)");

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain("Function 'Round' expects 1 to 2 arguments, but received 3.");
    }

    [Fact]
    public void Validate_WithMalformedFunctionSyntax_ReturnsError()
    {
        var result = _sut.Validate("Sum([1,2,3]");

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void Validate_WithAnyInvalidArity_ReturnsError()
    {
        var result = _sut.Validate("Any(Parameter('items'), 'Amount')");

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain("Function 'Any' expects either 1 or 4 arguments, but received 2.");
    }

    [Fact]
    public void Validate_WithUnmatchedTokenBraces_ReturnsError()
    {
        var result = _sut.Validate("Total: {Amount");

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain("Expression contains unmatched token braces.");
    }
}
