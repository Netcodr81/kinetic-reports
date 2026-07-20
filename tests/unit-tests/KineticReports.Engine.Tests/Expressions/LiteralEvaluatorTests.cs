namespace KineticReports.Engine.Tests.Expressions;

using KineticReports.Engine.Expressions;

public class LiteralEvaluatorTests
{
    private readonly LiteralEvaluator _sut = new();

    // -------------------------------------------------------------------------
    // Null / empty
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_NullExpression_ReturnsNull()
    {
        var result = _sut.Evaluate(null!, ExpressionContext.Empty);
        result.ShouldBeNull();
    }

    [Fact]
    public void Evaluate_EmptyExpression_ReturnsNull()
    {
        var result = _sut.Evaluate(string.Empty, ExpressionContext.Empty);
        result.ShouldBeNull();
    }

    // -------------------------------------------------------------------------
    // Literals
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_LiteralString_ReturnsSameString()
    {
        var result = _sut.Evaluate("Hello, World", ExpressionContext.Empty);
        result.ShouldBe("Hello, World");
    }

    [Fact]
    public void Evaluate_LiteralWithBraces_NotAFieldReference_ReturnsLiteral()
    {
        // A string that starts with { but doesn't end with } is a literal.
        var result = _sut.Evaluate("{incomplete", ExpressionContext.Empty);
        result.ShouldBe("{incomplete");
    }

    // -------------------------------------------------------------------------
    // Field references
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_FieldReference_ResolvesFromCurrentRow()
    {
        var ctx = new ExpressionContext
        {
            CurrentRow = new Dictionary<string, object?> { ["Name"] = "Alice" },
            Parameters = new Dictionary<string, object?>()
        };
        var result = _sut.Evaluate("{Name}", ctx);
        result.ShouldBe("Alice");
    }

    [Fact]
    public void Evaluate_FieldReference_FallsBackToParameters()
    {
        var ctx = new ExpressionContext
        {
            CurrentRow = new Dictionary<string, object?>(),
            Parameters = new Dictionary<string, object?> { ["ReportTitle"] = "Sales Q1" }
        };
        var result = _sut.Evaluate("{ReportTitle}", ctx);
        result.ShouldBe("Sales Q1");
    }

    [Fact]
    public void Evaluate_FieldReference_RowTakesPrecedenceOverParameter()
    {
        var ctx = new ExpressionContext
        {
            CurrentRow = new Dictionary<string, object?> { ["Value"] = "from-row" },
            Parameters = new Dictionary<string, object?> { ["Value"] = "from-param" }
        };
        var result = _sut.Evaluate("{Value}", ctx);
        result.ShouldBe("from-row");
    }

    [Fact]
    public void Evaluate_UnknownFieldReference_ReturnsNull()
    {
        var result = _sut.Evaluate("{UnknownField}", ExpressionContext.Empty);
        result.ShouldBeNull();
    }

    [Fact]
    public void Evaluate_FieldReference_WithNullRowValue_ReturnsNull()
    {
        var ctx = new ExpressionContext
        {
            CurrentRow = new Dictionary<string, object?> { ["Nullable"] = null },
            Parameters = new Dictionary<string, object?>()
        };
        var result = _sut.Evaluate("{Nullable}", ctx);
        result.ShouldBeNull();
    }
}
