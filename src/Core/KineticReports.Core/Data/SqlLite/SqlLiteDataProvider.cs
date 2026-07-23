namespace KineticReports.Core.Data.SqlLite;

using System.Diagnostics;
using KineticReports.Core.Data;
using Microsoft.Data.Sqlite;

/// <summary>
/// SQLite implementation of <see cref="IDataProvider"/> for executing parameterized queries.
/// </summary>
public sealed class SqlLiteDataProvider : IDataProvider
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlLiteDataProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <exception cref="ArgumentException">Thrown when connectionString is null or empty.</exception>
    public SqlLiteDataProvider(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    /// <inheritdoc/>
    public string Name => "SQLite";

    /// <inheritdoc/>
    public async Task<QueryResult> ExecuteAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var command = connection.CreateCommand();
            command.CommandText = request.QueryText;
            command.CommandTimeout = Math.Max(1, request.TimeoutMs / 1000);

            foreach (var kvp in request.Parameters)
            {
                var parameterName = kvp.Key.StartsWith('@') ? kvp.Key : $"@{kvp.Key}";
                command.Parameters.AddWithValue(parameterName, kvp.Value ?? DBNull.Value);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            var schema = new List<ColumnSchema>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                schema.Add(new ColumnSchema
                {
                    Name = reader.GetName(i),
                    ClrType = reader.GetFieldType(i),
                    Ordinal = i,
                    IsNullable = true
                });
            }

            var rows = new List<DataRow>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var values = new object?[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }

                rows.Add(new DataRow { Values = values });
            }

            stopwatch.Stop();

            return new QueryResult
            {
                Schema = schema,
                Rows = rows,
                RowCount = rows.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                DiagnosticsMessage = $"Executed: {request.DatasetName}, Rows: {rows.Count}"
            };
        }
        catch (SqliteException ex)
        {
            stopwatch.Stop();
            throw new InvalidOperationException($"SQLite query execution failed for dataset '{request.DatasetName}': {ex.Message}", ex);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            throw;
        }
    }
}
