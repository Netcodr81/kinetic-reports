namespace KineticReports.Core.Data;

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// In-memory <see cref="IDataProvider"/> that materializes rows from POCO objects.
/// </summary>
/// <remarks>
/// This provider is designed for scenarios where callers compose or aggregate data
/// from multiple upstream systems and pass the resulting objects directly to the
/// reporting pipeline.
///
/// Data can be supplied in two ways:
/// <list type="number">
///   <item><description>Pre-registered named datasets via constructors or <see cref="SetDataSource"/>.</description></item>
///   <item><description>Per-request inline data using request parameter key <c>Data</c>.</description></item>
/// </list>
///
/// For typed in-memory scenarios, callers can also execute LINQ expression pipelines directly
/// via <see cref="ExecuteAsync{TSource, TResult}(IEnumerable{TSource}, Expression{Func{IQueryable{TSource}, IQueryable{TResult}}}, CancellationToken)"/>.
///
/// Supported row shapes:
/// <list type="bullet">
///   <item><description>POCO/anonymous objects (public readable properties become columns).</description></item>
///   <item><description><see cref="IDictionary{TKey, TValue}"/> with string keys.</description></item>
///   <item><description>Non-generic <see cref="IDictionary"/> with string-convertible keys.</description></item>
///   <item><description>Primitive/scalar values (single <c>Value</c> column).</description></item>
/// </list>
/// </remarks>
public sealed class PocoDataProvider : IDataProvider, ILinqDataProvider
{
    /// <summary>
    /// Parameter key used to provide inline in-memory objects for a request.
    /// </summary>
    public const string InlineDataParameterName = "Data";

    private readonly Dictionary<string, IReadOnlyList<object?>> _dataSources =
        new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public string Name => "POCO In-Memory";

    /// <summary>
    /// Initializes an empty provider. Supply data via <see cref="SetDataSource"/>
    /// or per request with <see cref="InlineDataParameterName"/>.
    /// </summary>
    public PocoDataProvider()
    {
    }

    /// <summary>
    /// Initializes the provider with a single dataset.
    /// </summary>
    /// <param name="datasetName">Dataset name used by <see cref="QueryRequest.DatasetName"/>.</param>
    /// <param name="items">In-memory objects for that dataset.</param>
    public PocoDataProvider(string datasetName, IEnumerable<object?> items)
    {
        SetDataSource(datasetName, items);
    }

    /// <summary>
    /// Initializes the provider with multiple datasets.
    /// </summary>
    /// <param name="dataSources">Dataset map keyed by dataset name.</param>
    public PocoDataProvider(IReadOnlyDictionary<string, IEnumerable<object?>> dataSources)
    {
        ArgumentNullException.ThrowIfNull(dataSources);

        foreach (var entry in dataSources)
            SetDataSource(entry.Key, entry.Value);
    }

    /// <summary>
    /// Adds or replaces a named in-memory dataset.
    /// </summary>
    /// <param name="datasetName">Dataset name.</param>
    /// <param name="items">Objects to expose for that dataset.</param>
    public void SetDataSource(string datasetName, IEnumerable<object?> items)
    {
        if (string.IsNullOrWhiteSpace(datasetName))
            throw new ArgumentException("Dataset name is required.", nameof(datasetName));

        ArgumentNullException.ThrowIfNull(items);

        _dataSources[datasetName] = items.ToList();
    }

    /// <summary>
    /// Removes a previously registered dataset.
    /// </summary>
    /// <param name="datasetName">Dataset name.</param>
    /// <returns><see langword="true"/> when removed; otherwise <see langword="false"/>.</returns>
    public bool RemoveDataSource(string datasetName)
    {
        if (string.IsNullOrWhiteSpace(datasetName))
            return false;

        return _dataSources.Remove(datasetName);
    }

    /// <inheritdoc/>
    public Task<QueryResult> ExecuteAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var stopwatch = Stopwatch.StartNew();

        var sourceItems = ResolveItems(request);
        var (schema, rows) = BuildTable(sourceItems);

        stopwatch.Stop();

        return Task.FromResult(new QueryResult
        {
            Schema = schema,
            Rows = rows,
            RowCount = rows.Count,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            DiagnosticsMessage = $"Resolved dataset '{request.DatasetName}' from in-memory objects. Rows: {rows.Count}."
        });
    }

    /// <summary>
    /// Executes a LINQ query expression against an in-memory source collection.
    /// </summary>
    /// <typeparam name="TSource">Source row type.</typeparam>
    /// <typeparam name="TResult">Projected row type returned by the query expression.</typeparam>
    /// <param name="source">Input source collection.</param>
    /// <param name="queryExpression">
    /// Expression that transforms <c>IQueryable&lt;TSource&gt;</c> into
    /// <c>IQueryable&lt;TResult&gt;</c> (for example filter, project, group/aggregate).
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Materialized query result containing schema and rows.</returns>
    public Task<QueryResult> ExecuteAsync<TSource, TResult>(
        IEnumerable<TSource> source,
        Expression<Func<IQueryable<TSource>, IQueryable<TResult>>> queryExpression,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(queryExpression);
        cancellationToken.ThrowIfCancellationRequested();

        var stopwatch = Stopwatch.StartNew();

        IQueryable<TResult> queried;
        try
        {
            queried = queryExpression.Compile().Invoke(source.AsQueryable());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to execute LINQ query expression against in-memory source.", ex);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var materialized = queried.Cast<object?>().ToList();

        var (schema, rows) = BuildTable(materialized);

        stopwatch.Stop();

        return Task.FromResult(new QueryResult
        {
            Schema = schema,
            Rows = rows,
            RowCount = rows.Count,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            DiagnosticsMessage = $"Executed LINQ expression on in-memory source. Rows: {rows.Count}."
        });
    }

    /// <summary>
    /// Executes a LINQ query expression against a named in-memory dataset registered on this provider.
    /// </summary>
    /// <typeparam name="TSource">Source row type expected in the dataset.</typeparam>
    /// <typeparam name="TResult">Projected row type returned by the query expression.</typeparam>
    /// <param name="datasetName">Registered dataset name.</param>
    /// <param name="queryExpression">Query expression applied to the dataset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Materialized query result containing schema and rows.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the dataset is not registered.</exception>
    public Task<QueryResult> ExecuteAsync<TSource, TResult>(
        string datasetName,
        Expression<Func<IQueryable<TSource>, IQueryable<TResult>>> queryExpression,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(datasetName))
            throw new ArgumentException("Dataset name is required.", nameof(datasetName));

        if (!_dataSources.TryGetValue(datasetName, out var sourceItems))
            throw new InvalidOperationException($"Dataset '{datasetName}' is not registered.");

        var typedSource = sourceItems.OfType<TSource>();
        return ExecuteAsync(typedSource, queryExpression, cancellationToken);
    }

    private IReadOnlyList<object?> ResolveItems(QueryRequest request)
    {
        if (request.Parameters.TryGetValue(InlineDataParameterName, out var inlineData))
            return CoerceToObjectList(inlineData);

        if (_dataSources.TryGetValue(request.DatasetName, out var datasetItems))
            return datasetItems;

        if (!string.IsNullOrWhiteSpace(request.QueryText)
            && _dataSources.TryGetValue(request.QueryText, out var queryTextMappedItems))
        {
            return queryTextMappedItems;
        }

        return [];
    }

    private static IReadOnlyList<object?> CoerceToObjectList(object? value)
    {
        if (value is null)
            return [];

        if (value is string)
            return [value];

        if (value is IEnumerable enumerable)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
                list.Add(item);

            return list;
        }

        return [value];
    }

    private static IReadOnlyDictionary<string, object?> ToValueMap(object? item)
    {
        if (item is null)
            return new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.Ordinal));

        if (item is IReadOnlyDictionary<string, object?> readOnlyDictionary)
            return readOnlyDictionary;

        if (item is IDictionary<string, object?> dictionary)
            return new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(dictionary, StringComparer.Ordinal));

        if (item is IDictionary nonGenericDictionary)
        {
            var converted = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in nonGenericDictionary)
            {
                var key = Convert.ToString(entry.Key, System.Globalization.CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                converted[key] = entry.Value;
            }

            return new ReadOnlyDictionary<string, object?>(converted);
        }

        if (IsScalar(item.GetType()))
        {
            return new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["Value"] = item
                });
        }

        var map = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0)
                continue;

            map[property.Name] = property.GetValue(item);
        }

        return new ReadOnlyDictionary<string, object?>(map);
    }

    private static IReadOnlyList<ColumnSchema> BuildSchema(IReadOnlyList<IReadOnlyDictionary<string, object?>> maps)
    {
        var columnNames = new List<string>();
        var valuesByColumn = new Dictionary<string, List<object?>>(StringComparer.Ordinal);

        foreach (var map in maps)
        {
            foreach (var kvp in map)
            {
                if (!valuesByColumn.ContainsKey(kvp.Key))
                {
                    columnNames.Add(kvp.Key);
                    valuesByColumn[kvp.Key] = [];
                }

                valuesByColumn[kvp.Key].Add(kvp.Value);
            }

            foreach (var existing in columnNames)
            {
                if (map.ContainsKey(existing))
                    continue;

                valuesByColumn[existing].Add(null);
            }
        }

        var schema = new List<ColumnSchema>(columnNames.Count);

        for (var index = 0; index < columnNames.Count; index++)
        {
            var name = columnNames[index];
            var values = valuesByColumn[name];
            var firstNonNull = values.FirstOrDefault(static value => value is not null);

            schema.Add(new ColumnSchema
            {
                Name = name,
                Ordinal = index,
                ClrType = firstNonNull?.GetType() ?? typeof(object),
                IsNullable = values.Any(static value => value is null),
                DisplayName = name
            });
        }

        return schema;
    }

    private static IReadOnlyList<DataRow> BuildRows(
        IReadOnlyList<ColumnSchema> schema,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> maps)
    {
        if (maps.Count == 0)
            return [];

        var rows = new List<DataRow>(maps.Count);

        foreach (var map in maps)
        {
            var values = new object?[schema.Count];

            foreach (var column in schema)
            {
                map.TryGetValue(column.Name, out var value);
                values[column.Ordinal] = value;
            }

            rows.Add(new DataRow { Values = values });
        }

        return rows;
    }

    private static (IReadOnlyList<ColumnSchema> Schema, IReadOnlyList<DataRow> Rows) BuildTable(
        IReadOnlyList<object?> sourceItems)
    {
        var rowMaps = sourceItems
            .Select(ToValueMap)
            .ToList();

        var schema = BuildSchema(rowMaps);
        var rows = BuildRows(schema, rowMaps);
        return (schema, rows);
    }

    private static bool IsScalar(Type type)
    {
        var nonNullable = Nullable.GetUnderlyingType(type) ?? type;

        return nonNullable.IsPrimitive
            || nonNullable.IsEnum
            || nonNullable == typeof(string)
            || nonNullable == typeof(decimal)
            || nonNullable == typeof(DateTime)
            || nonNullable == typeof(DateTimeOffset)
            || nonNullable == typeof(Guid)
            || nonNullable == typeof(TimeSpan);
    }
}
