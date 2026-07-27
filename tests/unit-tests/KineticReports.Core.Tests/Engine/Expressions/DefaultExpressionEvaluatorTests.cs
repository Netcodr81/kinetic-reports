namespace KineticReports.Core.Tests.Engine.Expressions;

using KineticReports.Core.Engine.Expressions;

public class DefaultExpressionEvaluatorTests
{
    private readonly DefaultExpressionEvaluator _sut = new();

    [Fact]
    public void Evaluate_MathFunctions_ReturnExpectedValues()
    {
        _sut.Evaluate("Abs(-12)", ExpressionContext.Empty).ShouldBe(12m);
        _sut.Evaluate("Round(12.345, 2)", ExpressionContext.Empty).ShouldBe(12.35m);
        _sut.Evaluate("Ceiling(1.2)", ExpressionContext.Empty).ShouldBe(2m);
        _sut.Evaluate("Floor(1.9)", ExpressionContext.Empty).ShouldBe(1m);
        _sut.Evaluate("Sum([1,2,3])", ExpressionContext.Empty).ShouldBe(6m);
        _sut.Evaluate("Average([2,4,6])", ExpressionContext.Empty).ShouldBe(4m);
        _sut.Evaluate("Count([2,4,6])", ExpressionContext.Empty).ShouldBe(3);
        _sut.Evaluate("Min([9,3,7])", ExpressionContext.Empty).ShouldBe(3m);
        _sut.Evaluate("Max([9,3,7])", ExpressionContext.Empty).ShouldBe(9m);
    }

    [Fact]
    public void Evaluate_StringFunctions_ReturnExpectedValues()
    {
        _sut.Evaluate("Length('hello')", ExpressionContext.Empty).ShouldBe(5);
        _sut.Evaluate("Substring('abcdef', 1, 3)", ExpressionContext.Empty).ShouldBe("bcd");
        _sut.Evaluate("Contains('abcdef', 'bcd')", ExpressionContext.Empty).ShouldBe(true);
        _sut.Evaluate("StartsWith('abcdef', 'abc')", ExpressionContext.Empty).ShouldBe(true);
        _sut.Evaluate("EndsWith('abcdef', 'def')", ExpressionContext.Empty).ShouldBe(true);
        _sut.Evaluate("Replace('abcabc', 'ab', 'xy')", ExpressionContext.Empty).ShouldBe("xycxyc");
        _sut.Evaluate("Trim('  hi  ')", ExpressionContext.Empty).ShouldBe("hi");
        _sut.Evaluate("Upper('hello')", ExpressionContext.Empty).ShouldBe("HELLO");
        _sut.Evaluate("Lower('HELLO')", ExpressionContext.Empty).ShouldBe("hello");
    }

    [Fact]
    public void Evaluate_DateFunctions_ReturnExpectedValues()
    {
        var now = _sut.Evaluate("Now()", ExpressionContext.Empty);
        now.ShouldBeOfType<DateTime>();

        var today = _sut.Evaluate("Today()", ExpressionContext.Empty).ShouldBeOfType<DateTime>();
        _sut.Evaluate("Year(Today())", ExpressionContext.Empty).ShouldBe(today.Year);
        _sut.Evaluate("Month(Today())", ExpressionContext.Empty).ShouldBe(today.Month);
        _sut.Evaluate("Day(Today())", ExpressionContext.Empty).ShouldBe(today.Day);

        _sut.Evaluate("DateDiff('day', '2025-01-01', '2025-01-11')", ExpressionContext.Empty).ShouldBe(10);
        _sut.Evaluate("FormatDate(AddDays('2025-01-01', 2), 'yyyy-MM-dd')", ExpressionContext.Empty).ShouldBe("2025-01-03");
        _sut.Evaluate("FormatDate(AddMonths('2025-01-15', 2), 'yyyy-MM-dd')", ExpressionContext.Empty).ShouldBe("2025-03-15");
    }

    [Fact]
    public void Evaluate_ConditionalFunctions_ReturnExpectedValues()
    {
        _sut.Evaluate("If(true, 'yes', 'no')", ExpressionContext.Empty).ShouldBe("yes");
        _sut.Evaluate("If(false, 'yes', 'no')", ExpressionContext.Empty).ShouldBe("no");
        _sut.Evaluate("Coalesce(null, null, 'fallback')", ExpressionContext.Empty).ShouldBe("fallback");
    }

    [Fact]
    public void Evaluate_CollectionAndAggregateFunctions_ReturnExpectedValues()
    {
        var context = new ExpressionContext
        {
            CurrentRow = new Dictionary<string, object?>(),
            Parameters = new Dictionary<string, object?>
            {
                ["items"] = new List<Dictionary<string, object?>>()
                {
                    new(StringComparer.Ordinal) { ["Name"] = "A", ["Amount"] = 10m, ["Region"] = "East" },
                    new(StringComparer.Ordinal) { ["Name"] = "B", ["Amount"] = 30m, ["Region"] = "West" },
                    new(StringComparer.Ordinal) { ["Name"] = "C", ["Amount"] = 20m, ["Region"] = "West" }
                }
            }
        };

        _sut.Evaluate("Count(Where(Parameter('items'), 'Amount', '>', 15))", context).ShouldBe(2);
        _sut.Evaluate("First(OrderBy(Parameter('items'), 'Amount'), 'Name')", context).ShouldBe("A");
        _sut.Evaluate("Last(OrderBy(Parameter('items'), 'Amount'), 'Name')", context).ShouldBe("B");
        _sut.Evaluate("Any(Parameter('items'), 'Amount', '>', 25)", context).ShouldBe(true);
        _sut.Evaluate("All(Parameter('items'), 'Amount', '>', 5)", context).ShouldBe(true);
        _sut.Evaluate("DistinctCount(Parameter('items'), 'Region')", context).ShouldBe(2);
        _sut.Evaluate("Sum(Parameter('items'), 'Amount')", context).ShouldBe(60m);
        _sut.Evaluate("Take(Select(OrderBy(Parameter('items'), 'Amount', 'desc'), 'Name'), 2)", context)
            .ShouldBeOfType<List<object?>>()
            .ShouldBe(["B", "C"]);

        var grouped = _sut.Evaluate("GroupBy(Parameter('items'), 'Region')", context)
            .ShouldBeOfType<List<Dictionary<string, object?>>>();
        grouped.Count.ShouldBe(2);
    }

    [Fact]
    public void Evaluate_FormattingFunctions_ReturnExpectedValues()
    {
        _sut.Evaluate("Format(1234.5, '0.00')", ExpressionContext.Empty).ShouldBe("1234.50");
        _sut.Evaluate("FormatCurrency(1234.5, 'en-US')", ExpressionContext.Empty).ShouldBe("$1,234.50");
        _sut.Evaluate("FormatDate('2025-02-01', 'yyyy/MM/dd')", ExpressionContext.Empty).ShouldBe("2025/02/01");
    }

    [Fact]
    public void Evaluate_ReportFunctions_ReturnExpectedValues()
    {
        var context = new ExpressionContext
        {
            CurrentRow = new Dictionary<string, object?> { ["Customer"] = "Acme" },
            Parameters = new Dictionary<string, object?> { ["Title"] = "Q1 Report" },
            RowIndex = 4,
            PageNumber = 3,
            TotalPages = 12
        };

        _sut.Evaluate("PageNumber()", context).ShouldBe(3);
        _sut.Evaluate("TotalPages()", context).ShouldBe(12);
        _sut.Evaluate("RowNumber()", context).ShouldBe(5);
        _sut.Evaluate("Field('Customer')", context).ShouldBe("Acme");
        _sut.Evaluate("Parameter('Title')", context).ShouldBe("Q1 Report");
    }

    [Fact]
    public void Evaluate_UnknownFunction_Throws()
    {
        Should.Throw<InvalidOperationException>(() => _sut.Evaluate("UnknownFunction(1)", ExpressionContext.Empty));
    }
}
