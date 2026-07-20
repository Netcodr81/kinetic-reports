namespace KineticReports.Core.Data;

using KineticReports.Core.Definition;

/// <summary>
/// Represents a request to execute a query against a data source.
/// </summary>
public sealed record QueryRequest
{
    /// <summary>Gets the name of the dataset referenced in the report definition.</summary>
    public required string DatasetName { get; init; }

    /// <summary>Gets the query text (SQL, GraphQL, REST path, etc.).</summary>
    public required string QueryText { get; init; }

    /// <summary>Gets the parameter values to bind to the query.</summary>
    public IReadOnlyDictionary<string, object?> Parameters { get; init; } = new Dictionary<string, object?>();

    /// <summary>Gets the execution timeout in milliseconds. Default is 30000 (30 seconds).</summary>
    public int TimeoutMs { get; init; } = 30000;
}
