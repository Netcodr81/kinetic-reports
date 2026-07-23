namespace KineticReports.Core.Data;

using System.Linq.Expressions;

/// <summary>
/// Defines a contract for executing strongly typed LINQ expressions over in-memory data sources.
/// </summary>
public interface ILinqDataProvider
{
    /// <summary>
    /// Executes a LINQ query expression against an in-memory source collection.
    /// </summary>
    /// <typeparam name="TSource">Source row type.</typeparam>
    /// <typeparam name="TResult">Projected row type returned by the query expression.</typeparam>
    /// <param name="source">Input source collection.</param>
    /// <param name="queryExpression">Expression that transforms the source queryable into a projected queryable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Materialized query result containing schema and rows.</returns>
    Task<QueryResult> ExecuteAsync<TSource, TResult>(
        IEnumerable<TSource> source,
        Expression<Func<IQueryable<TSource>, IQueryable<TResult>>> queryExpression,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a LINQ query expression against a named in-memory dataset.
    /// </summary>
    /// <typeparam name="TSource">Source row type expected in the dataset.</typeparam>
    /// <typeparam name="TResult">Projected row type returned by the query expression.</typeparam>
    /// <param name="datasetName">Registered dataset name.</param>
    /// <param name="queryExpression">Expression that transforms the source queryable into a projected queryable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Materialized query result containing schema and rows.</returns>
    Task<QueryResult> ExecuteAsync<TSource, TResult>(
        string datasetName,
        Expression<Func<IQueryable<TSource>, IQueryable<TResult>>> queryExpression,
        CancellationToken cancellationToken = default);
}
