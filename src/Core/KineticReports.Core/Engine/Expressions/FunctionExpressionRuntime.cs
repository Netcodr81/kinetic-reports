namespace KineticReports.Core.Engine.Expressions;

using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

internal static class FunctionExpressionRuntime
{
    private sealed record FunctionSignature(int MinArgs, int MaxArgs, bool AllowTwoArityModes = false);

    private static readonly IReadOnlyDictionary<string, FunctionSignature> SupportedFunctions =
        new Dictionary<string, FunctionSignature>(StringComparer.OrdinalIgnoreCase)
        {
            ["ABS"] = new(1, 1),
            ["ROUND"] = new(1, 2),
            ["CEILING"] = new(1, 1),
            ["FLOOR"] = new(1, 1),
            ["SUM"] = new(1, 2),
            ["AVERAGE"] = new(1, 2),
            ["COUNT"] = new(1, 1),
            ["MIN"] = new(1, 2),
            ["MAX"] = new(1, 2),

            ["LENGTH"] = new(1, 1),
            ["SUBSTRING"] = new(2, 3),
            ["CONTAINS"] = new(2, 2),
            ["STARTSWITH"] = new(2, 2),
            ["ENDSWITH"] = new(2, 2),
            ["REPLACE"] = new(3, 3),
            ["TRIM"] = new(1, 1),
            ["UPPER"] = new(1, 1),
            ["LOWER"] = new(1, 1),

            ["NOW"] = new(0, 0),
            ["TODAY"] = new(0, 0),
            ["YEAR"] = new(1, 1),
            ["MONTH"] = new(1, 1),
            ["DAY"] = new(1, 1),
            ["ADDDAYS"] = new(2, 2),
            ["ADDMONTHS"] = new(2, 2),
            ["DATEDIFF"] = new(2, 3),

            ["IF"] = new(3, 3),
            ["COALESCE"] = new(1, int.MaxValue),

            ["WHERE"] = new(4, 4),
            ["SELECT"] = new(2, 2),
            ["ORDERBY"] = new(2, 3),
            ["GROUPBY"] = new(2, 2),
            ["TAKE"] = new(2, 2),
            ["SKIP"] = new(2, 2),

            ["FIRST"] = new(1, 2),
            ["LAST"] = new(1, 2),
            ["ANY"] = new(1, 4, AllowTwoArityModes: true),
            ["ALL"] = new(1, 4, AllowTwoArityModes: true),
            ["DISTINCTCOUNT"] = new(1, 2),

            ["FORMAT"] = new(2, 2),
            ["FORMATCURRENCY"] = new(1, 2),
            ["FORMATDATE"] = new(1, 2),

            ["PAGENUMBER"] = new(0, 0),
            ["TOTALPAGES"] = new(0, 0),
            ["ROWNUMBER"] = new(0, 0),
            ["FIELD"] = new(1, 1),
            ["PARAMETER"] = new(1, 1)
        };

    private static readonly Regex FunctionLikePattern = new(
        "^[A-Za-z_][A-Za-z0-9_]*\\(.*\\)$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    internal static bool LooksLikeFunctionExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        var trimmed = expression.Trim();
        return FunctionLikePattern.IsMatch(trimmed);
    }

    internal static object? Evaluate(string expression, ExpressionContext context)
    {
        var parser = new Parser(expression);
        var root = parser.ParseExpression();
        parser.SkipWhitespace();

        if (!parser.IsAtEnd)
            throw new InvalidOperationException($"Invalid expression '{expression}'. Unexpected token near index {parser.Position}.");

        return EvaluateNode(root, context);
    }

    internal static ExpressionValidationResult ValidateFunctionExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return ExpressionValidationResult.Success;

        try
        {
            var parser = new Parser(expression);
            var root = parser.ParseExpression();
            parser.SkipWhitespace();

            if (!parser.IsAtEnd)
            {
                return ExpressionValidationResult.Failure(
                    $"Invalid expression '{expression}'. Unexpected token near index {parser.Position}.");
            }

            var errors = new List<string>();
            ValidateNode(root, errors);
            return errors.Count == 0
                ? ExpressionValidationResult.Success
                : new ExpressionValidationResult { IsValid = false, Errors = errors };
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return ExpressionValidationResult.Failure(ex.Message);
        }
    }

    private static void ValidateNode(Node node, List<string> errors)
    {
        switch (node)
        {
            case ArrayNode a:
                foreach (var item in a.Items)
                    ValidateNode(item, errors);
                break;

            case FunctionCallNode f:
                ValidateFunctionCall(f, errors);
                foreach (var arg in f.Args)
                    ValidateNode(arg, errors);
                break;
        }
    }

    private static void ValidateFunctionCall(FunctionCallNode call, List<string> errors)
    {
        if (!SupportedFunctions.TryGetValue(call.Name, out var signature))
        {
            errors.Add($"Unknown function '{call.Name}'.");
            return;
        }

        var count = call.Args.Count;
        if (signature.AllowTwoArityModes)
        {
            if (count != signature.MinArgs && count != signature.MaxArgs)
                errors.Add($"Function '{call.Name}' expects either {signature.MinArgs} or {signature.MaxArgs} arguments, but received {count}.");

            return;
        }

        if (count < signature.MinArgs || count > signature.MaxArgs)
        {
            var expected = signature.MinArgs == signature.MaxArgs
                ? signature.MinArgs.ToString(CultureInfo.InvariantCulture)
                : $"{signature.MinArgs} to {signature.MaxArgs}";
            errors.Add($"Function '{call.Name}' expects {expected} arguments, but received {count}.");
        }
    }

    private static object? EvaluateNode(Node node, ExpressionContext context)
    {
        return node switch
        {
            NullNode => null,
            BoolNode b => b.Value,
            NumberNode n => n.Value,
            StringNode s => s.Value,
            TokenNode t => ResolveToken(t.Name, context),
            IdentifierNode i => ResolveToken(i.Name, context),
            ArrayNode a => a.Items.Select(item => EvaluateNode(item, context)).ToList(),
            FunctionCallNode f => EvaluateFunction(f, context),
            _ => throw new InvalidOperationException("Unsupported expression node.")
        };
    }

    private static object? EvaluateFunction(FunctionCallNode call, ExpressionContext context)
    {
        var name = call.Name;
        var key = name.ToUpperInvariant();

        return key switch
        {
            // Math
            "ABS" => Math.Abs(ToDecimal(EvaluateRequiredArg(call, 0, context))),
            "ROUND" => Math.Round(
                ToDecimal(EvaluateRequiredArg(call, 0, context)),
                call.Args.Count > 1 ? ToInt(EvaluateRequiredArg(call, 1, context)) : 0,
                MidpointRounding.AwayFromZero),
            "CEILING" => Math.Ceiling(ToDecimal(EvaluateRequiredArg(call, 0, context))),
            "FLOOR" => Math.Floor(ToDecimal(EvaluateRequiredArg(call, 0, context))),
            "SUM" => AggregateNumeric(call, context, values => values.Sum()),
            "AVERAGE" => AggregateNumeric(call, context, values => values.Count == 0 ? 0m : values.Average()),
            "COUNT" => CountValues(call, context),
            "MIN" => AggregateComparable(call, context, values => values.MinBy(v => v)),
            "MAX" => AggregateComparable(call, context, values => values.MaxBy(v => v)),

            // String
            "LENGTH" => ToStringValue(EvaluateRequiredArg(call, 0, context)).Length,
            "SUBSTRING" => Substring(call, context),
            "CONTAINS" => ToStringValue(EvaluateRequiredArg(call, 0, context)).Contains(ToStringValue(EvaluateRequiredArg(call, 1, context)), StringComparison.Ordinal),
            "STARTSWITH" => ToStringValue(EvaluateRequiredArg(call, 0, context)).StartsWith(ToStringValue(EvaluateRequiredArg(call, 1, context)), StringComparison.Ordinal),
            "ENDSWITH" => ToStringValue(EvaluateRequiredArg(call, 0, context)).EndsWith(ToStringValue(EvaluateRequiredArg(call, 1, context)), StringComparison.Ordinal),
            "REPLACE" => ToStringValue(EvaluateRequiredArg(call, 0, context)).Replace(ToStringValue(EvaluateRequiredArg(call, 1, context)), ToStringValue(EvaluateRequiredArg(call, 2, context)), StringComparison.Ordinal),
            "TRIM" => ToStringValue(EvaluateRequiredArg(call, 0, context)).Trim(),
            "UPPER" => ToStringValue(EvaluateRequiredArg(call, 0, context)).ToUpperInvariant(),
            "LOWER" => ToStringValue(EvaluateRequiredArg(call, 0, context)).ToLowerInvariant(),

            // Date
            "NOW" => DateTime.UtcNow,
            "TODAY" => DateTime.UtcNow.Date,
            "YEAR" => ToDateTime(EvaluateRequiredArg(call, 0, context)).Year,
            "MONTH" => ToDateTime(EvaluateRequiredArg(call, 0, context)).Month,
            "DAY" => ToDateTime(EvaluateRequiredArg(call, 0, context)).Day,
            "ADDDAYS" => ToDateTime(EvaluateRequiredArg(call, 0, context)).AddDays(ToDouble(EvaluateRequiredArg(call, 1, context))),
            "ADDMONTHS" => ToDateTime(EvaluateRequiredArg(call, 0, context)).AddMonths(ToInt(EvaluateRequiredArg(call, 1, context))),
            "DATEDIFF" => DateDiff(call, context),

            // Conditional
            "IF" => ToBool(EvaluateRequiredArg(call, 0, context)) ? EvaluateRequiredArg(call, 1, context) : EvaluateRequiredArg(call, 2, context),
            "COALESCE" => Coalesce(call, context),

            // Collections
            "WHERE" => Where(call, context),
            "SELECT" => Select(call, context),
            "ORDERBY" => OrderBy(call, context),
            "GROUPBY" => GroupBy(call, context),
            "TAKE" => Take(call, context),
            "SKIP" => Skip(call, context),

            // Aggregates
            "FIRST" => First(call, context),
            "LAST" => Last(call, context),
            "ANY" => Any(call, context),
            "ALL" => All(call, context),
            "DISTINCTCOUNT" => DistinctCount(call, context),

            // Formatting
            "FORMAT" => Format(call, context),
            "FORMATCURRENCY" => FormatCurrency(call, context),
            "FORMATDATE" => FormatDate(call, context),

            // Report functions
            "PAGENUMBER" => context.PageNumber,
            "TOTALPAGES" => context.TotalPages ?? 0,
            "ROWNUMBER" => context.RowIndex + 1,
            "FIELD" => ResolveFieldByName(call, context),
            "PARAMETER" => ResolveParameterByName(call, context),

            _ => throw new InvalidOperationException($"Unknown function '{name}'.")
        };
    }

    private static object? EvaluateRequiredArg(FunctionCallNode call, int index, ExpressionContext context)
    {
        if (index < 0 || index >= call.Args.Count)
            throw new InvalidOperationException($"Function '{call.Name}' is missing required argument at position {index + 1}.");

        return EvaluateNode(call.Args[index], context);
    }

    private static string GetNameArg(FunctionCallNode call, int index, ExpressionContext context)
    {
        if (index < 0 || index >= call.Args.Count)
            throw new InvalidOperationException($"Function '{call.Name}' is missing required argument at position {index + 1}.");

        var arg = call.Args[index];
        return arg switch
        {
            IdentifierNode id => id.Name,
            StringNode s => s.Value,
            TokenNode t => t.Name,
            _ => ToStringValue(EvaluateNode(arg, context))
        };
    }

    private static object? Coalesce(FunctionCallNode call, ExpressionContext context)
    {
        foreach (var arg in call.Args)
        {
            var value = EvaluateNode(arg, context);
            if (value is not null)
                return value;
        }

        return null;
    }

    private static object? ResolveFieldByName(FunctionCallNode call, ExpressionContext context)
    {
        var key = GetNameArg(call, 0, context);
        return context.CurrentRow.TryGetValue(key, out var value) ? value : null;
    }

    private static object? ResolveParameterByName(FunctionCallNode call, ExpressionContext context)
    {
        var key = GetNameArg(call, 0, context);
        return context.Parameters.TryGetValue(key, out var value) ? value : null;
    }

    private static object? ResolveToken(string tokenName, ExpressionContext context)
    {
        if (context.CurrentRow.TryGetValue(tokenName, out var rowValue))
            return rowValue;

        if (context.Parameters.TryGetValue(tokenName, out var parameterValue))
            return parameterValue;

        if (string.Equals(tokenName, "PageNumber", StringComparison.OrdinalIgnoreCase))
            return context.PageNumber;

        if (string.Equals(tokenName, "TotalPages", StringComparison.OrdinalIgnoreCase))
            return context.TotalPages ?? 0;

        if (string.Equals(tokenName, "RowNumber", StringComparison.OrdinalIgnoreCase))
            return context.RowIndex + 1;

        if (string.Equals(tokenName, "CurrentDate", StringComparison.OrdinalIgnoreCase))
            return DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (string.Equals(tokenName, "CurrentTime", StringComparison.OrdinalIgnoreCase))
            return DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        return null;
    }

    private static decimal AggregateNumeric(FunctionCallNode call, ExpressionContext context, Func<List<decimal>, decimal> aggregator)
    {
        var values = ResolveProjection(call, context)
            .Select(ToDecimal)
            .ToList();

        return aggregator(values);
    }

    private static decimal AggregateComparable(FunctionCallNode call, ExpressionContext context, Func<List<decimal>, decimal> aggregator)
    {
        var values = ResolveProjection(call, context)
            .Select(ToDecimal)
            .ToList();

        if (values.Count == 0)
            return 0m;

        return aggregator(values);
    }

    private static int CountValues(FunctionCallNode call, ExpressionContext context)
    {
        if (call.Args.Count == 0)
            return 0;

        var value = EvaluateRequiredArg(call, 0, context);
        return AsEnumerable(value).Count();
    }

    private static IEnumerable<object?> ResolveProjection(FunctionCallNode call, ExpressionContext context)
    {
        if (call.Args.Count == 0)
            return [];

        if (call.Args.Count == 1)
        {
            var value = EvaluateRequiredArg(call, 0, context);
            return AsEnumerable(value);
        }

        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context));
        var selector = GetNameArg(call, 1, context);
        return source.Select(item => ResolveItemMember(item, selector));
    }

    private static object? Substring(FunctionCallNode call, ExpressionContext context)
    {
        var source = ToStringValue(EvaluateRequiredArg(call, 0, context));
        var start = ToInt(EvaluateRequiredArg(call, 1, context));

        if (call.Args.Count == 2)
            return source.Substring(start);

        var length = ToInt(EvaluateRequiredArg(call, 2, context));
        return source.Substring(start, length);
    }

    private static object DateDiff(FunctionCallNode call, ExpressionContext context)
    {
        if (call.Args.Count < 2)
            throw new InvalidOperationException("DateDiff requires at least 2 arguments.");

        DateTime start;
        DateTime end;
        string unit;

        if (call.Args.Count >= 3)
        {
            var firstArg = EvaluateRequiredArg(call, 0, context);
            if (firstArg is string s && !DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
            {
                unit = s;
                start = ToDateTime(EvaluateRequiredArg(call, 1, context));
                end = ToDateTime(EvaluateRequiredArg(call, 2, context));
            }
            else
            {
                start = ToDateTime(firstArg);
                end = ToDateTime(EvaluateRequiredArg(call, 1, context));
                unit = ToStringValue(EvaluateRequiredArg(call, 2, context));
            }
        }
        else
        {
            start = ToDateTime(EvaluateRequiredArg(call, 0, context));
            end = ToDateTime(EvaluateRequiredArg(call, 1, context));
            unit = "day";
        }

        var diff = end - start;
        return unit.Trim().ToUpperInvariant() switch
        {
            "YEAR" or "YEARS" => end.Year - start.Year,
            "MONTH" or "MONTHS" => ((end.Year - start.Year) * 12) + (end.Month - start.Month),
            "DAY" or "DAYS" => (int)diff.TotalDays,
            "HOUR" or "HOURS" => (int)diff.TotalHours,
            "MINUTE" or "MINUTES" => (int)diff.TotalMinutes,
            "SECOND" or "SECONDS" => (int)diff.TotalSeconds,
            _ => throw new InvalidOperationException($"Unsupported DateDiff unit '{unit}'.")
        };
    }

    private static object? Where(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context)).ToList();

        if (call.Args.Count < 4)
            throw new InvalidOperationException("Where requires 4 arguments: source, field, operator, value.");

        var field = GetNameArg(call, 1, context);
        var op = ToStringValue(EvaluateRequiredArg(call, 2, context));
        var compareTo = EvaluateRequiredArg(call, 3, context);

        return source
            .Where(item => Compare(ResolveItemMember(item, field), compareTo, op))
            .ToList();
    }

    private static object? Select(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context));
        var field = GetNameArg(call, 1, context);
        return source.Select(item => ResolveItemMember(item, field)).ToList();
    }

    private static object? OrderBy(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context)).ToList();
        var field = GetNameArg(call, 1, context);
        var direction = call.Args.Count > 2
            ? ToStringValue(EvaluateRequiredArg(call, 2, context))
            : "asc";

        var sorted = direction.Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? source.OrderByDescending(item => ResolveItemMember(item, field), Comparer<object?>.Create(CompareNullableObjects)).ToList()
            : source.OrderBy(item => ResolveItemMember(item, field), Comparer<object?>.Create(CompareNullableObjects)).ToList();

        return sorted;
    }

    private static object? GroupBy(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context));
        var field = GetNameArg(call, 1, context);

        return source
            .GroupBy(item => ResolveItemMember(item, field))
            .Select(group => new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Key"] = group.Key,
                ["Count"] = group.Count(),
                ["Items"] = group.ToList()
            })
            .ToList();
    }

    private static object? Take(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context));
        var count = ToInt(EvaluateRequiredArg(call, 1, context));
        return source.Take(count).ToList();
    }

    private static object? Skip(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context));
        var count = ToInt(EvaluateRequiredArg(call, 1, context));
        return source.Skip(count).ToList();
    }

    private static object? First(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context));
        var first = source.FirstOrDefault();

        if (call.Args.Count == 1)
            return first;

        var field = GetNameArg(call, 1, context);
        return ResolveItemMember(first, field);
    }

    private static object? Last(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context));
        var last = source.LastOrDefault();

        if (call.Args.Count == 1)
            return last;

        var field = GetNameArg(call, 1, context);
        return ResolveItemMember(last, field);
    }

    private static object Any(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context)).ToList();
        if (call.Args.Count == 1)
            return source.Count != 0;

        var field = GetNameArg(call, 1, context);
        var op = ToStringValue(EvaluateRequiredArg(call, 2, context));
        var compareTo = EvaluateRequiredArg(call, 3, context);
        return source.Any(item => Compare(ResolveItemMember(item, field), compareTo, op));
    }

    private static object All(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context)).ToList();
        if (call.Args.Count == 1)
            return source.All(ToBool);

        var field = GetNameArg(call, 1, context);
        var op = ToStringValue(EvaluateRequiredArg(call, 2, context));
        var compareTo = EvaluateRequiredArg(call, 3, context);
        return source.All(item => Compare(ResolveItemMember(item, field), compareTo, op));
    }

    private static object DistinctCount(FunctionCallNode call, ExpressionContext context)
    {
        var source = AsEnumerable(EvaluateRequiredArg(call, 0, context));
        if (call.Args.Count == 1)
            return source.Distinct().Count();

        var field = GetNameArg(call, 1, context);
        return source.Select(item => ResolveItemMember(item, field)).Distinct().Count();
    }

    private static object? Format(FunctionCallNode call, ExpressionContext context)
    {
        var value = EvaluateRequiredArg(call, 0, context);
        var format = ToStringValue(EvaluateRequiredArg(call, 1, context));

        return value is IFormattable formattable
            ? formattable.ToString(format, CultureInfo.InvariantCulture)
            : ToStringValue(value);
    }

    private static object FormatCurrency(FunctionCallNode call, ExpressionContext context)
    {
        var value = ToDecimal(EvaluateRequiredArg(call, 0, context));
        var cultureName = call.Args.Count > 1
            ? ToStringValue(EvaluateRequiredArg(call, 1, context))
            : CultureInfo.InvariantCulture.Name;

        var culture = string.IsNullOrWhiteSpace(cultureName)
            ? CultureInfo.InvariantCulture
            : new CultureInfo(cultureName);

        return value.ToString("C", culture);
    }

    private static object FormatDate(FunctionCallNode call, ExpressionContext context)
    {
        var value = ToDateTime(EvaluateRequiredArg(call, 0, context));
        var format = call.Args.Count > 1
            ? ToStringValue(EvaluateRequiredArg(call, 1, context))
            : "yyyy-MM-dd";

        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    private static bool Compare(object? left, object? right, string op)
    {
        return op.Trim().ToUpperInvariant() switch
        {
            "==" or "=" => Equals(left, right),
            "!=" or "<>" => !Equals(left, right),
            ">" => CompareNullableObjects(left, right) > 0,
            ">=" => CompareNullableObjects(left, right) >= 0,
            "<" => CompareNullableObjects(left, right) < 0,
            "<=" => CompareNullableObjects(left, right) <= 0,
            "CONTAINS" => ToStringValue(left).Contains(ToStringValue(right), StringComparison.Ordinal),
            "STARTSWITH" => ToStringValue(left).StartsWith(ToStringValue(right), StringComparison.Ordinal),
            "ENDSWITH" => ToStringValue(left).EndsWith(ToStringValue(right), StringComparison.Ordinal),
            _ => throw new InvalidOperationException($"Unsupported comparison operator '{op}'.")
        };
    }

    private static int CompareNullableObjects(object? left, object? right)
    {
        if (left is null && right is null) return 0;
        if (left is null) return -1;
        if (right is null) return 1;

        if (TryToDecimal(left, out var ld) && TryToDecimal(right, out var rd))
            return ld.CompareTo(rd);

        if (left is DateTime leftDate && right is DateTime rightDate)
            return leftDate.CompareTo(rightDate);

        return string.CompareOrdinal(ToStringValue(left), ToStringValue(right));
    }

    private static IEnumerable<object?> AsEnumerable(object? value)
    {
        if (value is null)
            return [];

        if (value is string)
            return [value];

        if (value is IEnumerable<object?> genericEnumerable)
            return genericEnumerable;

        if (value is IEnumerable enumerable)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
                list.Add(item);

            return list;
        }

        return [value];
    }

    private static object? ResolveItemMember(object? item, string memberName)
    {
        if (item is null)
            return null;

        if (memberName == "$")
            return item;

        if (item is IReadOnlyDictionary<string, object?> readOnlyMap)
        {
            if (readOnlyMap.TryGetValue(memberName, out var value))
                return value;

            var found = readOnlyMap.FirstOrDefault(pair => string.Equals(pair.Key, memberName, StringComparison.OrdinalIgnoreCase));
            return found.Equals(default(KeyValuePair<string, object?>)) ? null : found.Value;
        }

        if (item is IDictionary<string, object?> map)
        {
            if (map.TryGetValue(memberName, out var value))
                return value;

            var found = map.FirstOrDefault(pair => string.Equals(pair.Key, memberName, StringComparison.OrdinalIgnoreCase));
            return found.Equals(default(KeyValuePair<string, object?>)) ? null : found.Value;
        }

        var type = item.GetType();
        var property = type.GetProperty(memberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
        if (property is not null)
            return property.GetValue(item);

        var field = type.GetField(memberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
        return field?.GetValue(item);
    }

    private static bool ToBool(object? value)
    {
        if (value is null) return false;
        if (value is bool b) return b;
        if (value is string s)
            return bool.TryParse(s, out var parsed) ? parsed : !string.IsNullOrWhiteSpace(s);

        if (TryToDecimal(value, out var d))
            return d != 0m;

        return true;
    }

    private static int ToInt(object? value)
    {
        if (value is null) return 0;
        if (value is int i) return i;
        if (value is long l) return (int)l;
        if (value is double dbl) return (int)dbl;
        if (value is decimal dec) return (int)dec;
        if (value is float f) return (int)f;
        if (value is string s && int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static double ToDouble(object? value)
    {
        if (value is null) return 0d;
        if (value is double d) return d;
        if (value is float f) return f;
        if (value is decimal dec) return (double)dec;
        if (value is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    private static decimal ToDecimal(object? value)
    {
        if (TryToDecimal(value, out var result))
            return result;

        throw new InvalidOperationException($"Value '{value}' cannot be converted to decimal.");
    }

    private static bool TryToDecimal(object? value, out decimal result)
    {
        if (value is null)
        {
            result = 0m;
            return false;
        }

        switch (value)
        {
            case decimal d:
                result = d;
                return true;
            case byte b:
                result = b;
                return true;
            case sbyte sb:
                result = sb;
                return true;
            case short s:
                result = s;
                return true;
            case ushort us:
                result = us;
                return true;
            case int i:
                result = i;
                return true;
            case uint ui:
                result = ui;
                return true;
            case long l:
                result = l;
                return true;
            case ulong ul:
                result = ul;
                return true;
            case float f:
                result = (decimal)f;
                return true;
            case double dbl:
                result = (decimal)dbl;
                return true;
            case string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                result = parsed;
                return true;
            default:
                result = 0m;
                return false;
        }
    }

    private static DateTime ToDateTime(object? value)
    {
        if (value is DateTime dt)
            return dt;

        if (value is DateTimeOffset dto)
            return dto.UtcDateTime;

        var text = ToStringValue(value);
        if (DateTime.TryParseExact(
                text,
                ["yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ", "O", "s"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var exactParsed))
            return exactParsed;

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Value '{value}' cannot be converted to DateTime.");
    }

    private static string ToStringValue(object? value)
    {
        if (value is null)
            return string.Empty;

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private abstract record Node;
    private sealed record NullNode : Node;
    private sealed record BoolNode(bool Value) : Node;
    private sealed record NumberNode(decimal Value) : Node;
    private sealed record StringNode(string Value) : Node;
    private sealed record IdentifierNode(string Name) : Node;
    private sealed record TokenNode(string Name) : Node;
    private sealed record ArrayNode(IReadOnlyList<Node> Items) : Node;
    private sealed record FunctionCallNode(string Name, IReadOnlyList<Node> Args) : Node;

    private sealed class Parser
    {
        private readonly string _text;
        private int _position;

        internal Parser(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
        }

        internal int Position => _position;
        internal bool IsAtEnd => _position >= _text.Length;

        internal Node ParseExpression()
        {
            SkipWhitespace();

            if (IsAtEnd)
                return new NullNode();

            var ch = Current();

            if (ch == '\'' || ch == '"')
                return ParseString();

            if (ch == '{')
                return ParseToken();

            if (ch == '[')
                return ParseArray();

            if (char.IsDigit(ch) || ch is '-' or '+')
                return ParseNumber();

            if (IsIdentifierStart(ch))
                return ParseIdentifierOrFunction();

            throw new InvalidOperationException($"Invalid token '{ch}' at index {_position}.");
        }

        internal void SkipWhitespace()
        {
            while (!IsAtEnd && char.IsWhiteSpace(Current()))
                _position++;
        }

        private Node ParseIdentifierOrFunction()
        {
            var name = ParseIdentifier();
            SkipWhitespace();

            if (!IsAtEnd && Current() == '(')
                return ParseFunctionCall(name);

            if (string.Equals(name, "null", StringComparison.OrdinalIgnoreCase))
                return new NullNode();

            if (string.Equals(name, "true", StringComparison.OrdinalIgnoreCase))
                return new BoolNode(true);

            if (string.Equals(name, "false", StringComparison.OrdinalIgnoreCase))
                return new BoolNode(false);

            return new IdentifierNode(name);
        }

        private Node ParseFunctionCall(string name)
        {
            Expect('(');
            SkipWhitespace();

            var args = new List<Node>();
            if (!IsAtEnd && Current() != ')')
            {
                while (true)
                {
                    var arg = ParseExpression();
                    args.Add(arg);

                    SkipWhitespace();

                    if (!IsAtEnd && Current() == ',')
                    {
                        _position++;
                        SkipWhitespace();
                        continue;
                    }

                    break;
                }
            }

            SkipWhitespace();
            Expect(')');
            return new FunctionCallNode(name, args);
        }

        private Node ParseString()
        {
            var quote = Current();
            _position++;

            var sb = new StringBuilder();
            while (!IsAtEnd)
            {
                var ch = Current();
                _position++;

                if (ch == quote)
                    return new StringNode(sb.ToString());

                if (ch == '\\' && !IsAtEnd)
                {
                    var escaped = Current();
                    _position++;
                    sb.Append(escaped switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        '\\' => '\\',
                        '\'' => '\'',
                        '"' => '"',
                        _ => escaped
                    });
                    continue;
                }

                sb.Append(ch);
            }

            throw new InvalidOperationException("Unterminated string literal.");
        }

        private Node ParseToken()
        {
            Expect('{');
            var start = _position;

            while (!IsAtEnd && Current() != '}')
                _position++;

            if (IsAtEnd)
                throw new InvalidOperationException("Unterminated token expression.");

            var name = _text[start.._position].Trim();
            Expect('}');

            return new TokenNode(name);
        }

        private Node ParseArray()
        {
            Expect('[');
            SkipWhitespace();

            var items = new List<Node>();
            if (!IsAtEnd && Current() != ']')
            {
                while (true)
                {
                    items.Add(ParseExpression());
                    SkipWhitespace();

                    if (!IsAtEnd && Current() == ',')
                    {
                        _position++;
                        SkipWhitespace();
                        continue;
                    }

                    break;
                }
            }

            SkipWhitespace();
            Expect(']');
            return new ArrayNode(items);
        }

        private Node ParseNumber()
        {
            var start = _position;

            if (Current() is '-' or '+')
                _position++;

            while (!IsAtEnd && (char.IsDigit(Current()) || Current() == '.'))
                _position++;

            var text = _text[start.._position];
            if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                throw new InvalidOperationException($"Invalid numeric literal '{text}'.");

            return new NumberNode(value);
        }

        private string ParseIdentifier()
        {
            var start = _position;
            _position++;

            while (!IsAtEnd && IsIdentifierPart(Current()))
                _position++;

            return _text[start.._position];
        }

        private void Expect(char expected)
        {
            if (IsAtEnd || Current() != expected)
                throw new InvalidOperationException($"Expected '{expected}' at index {_position}.");

            _position++;
        }

        private char Current() => _text[_position];

        private static bool IsIdentifierStart(char ch) => char.IsLetter(ch) || ch == '_';

        private static bool IsIdentifierPart(char ch) => char.IsLetterOrDigit(ch) || ch == '_';
    }
}
