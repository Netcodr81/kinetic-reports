namespace KineticReports.Core.Data.SqlServer;

using System.Diagnostics;
using Microsoft.Data.SqlClient;
using KineticReports.Core.Data;

/// <summary>
/// SQL Server implementation of IDataProvider for executing parameterized queries.
/// </summary>
public sealed class SqlServerDataProvider : IDataProvider
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDataProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <exception cref="ArgumentException">Thrown when connectionString is null or empty.</exception>
    public SqlServerDataProvider(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    /// <inheritdoc/>
    public string Name => "SQL Server";

    /// <inheritdoc/>
    public async Task<QueryResult> ExecuteAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = new SqlCommand(request.QueryText, connection)
            {
                CommandTimeout = request.TimeoutMs / 1000
            };

            // Add parameters from request
            foreach (var kvp in request.Parameters)
            {
                var parameter = command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                parameter.ParameterName = $"@{kvp.Key}";
            }

            using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            // Build schema from reader
            var schemaBuilder = new List<ColumnSchema>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                schemaBuilder.Add(new ColumnSchema
                {
                    Name = reader.GetName(i),
                    ClrType = reader.GetFieldType(i),
                    Ordinal = i,
                    IsNullable = true // SQL Server doesn't easily expose nullability from reader
                });
            }

            // Read rows
            var rows = new List<DataRow>();
            while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false))
            {
                while (reader.Read())
                {
                    var values = new object?[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }

                    rows.Add(new DataRow { Values = values });
                }
            }

            stopwatch.Stop();

            return new QueryResult
            {
                Schema = schemaBuilder,
                Rows = rows,
                RowCount = rows.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                DiagnosticsMessage = $"Executed: {request.DatasetName}, Rows: {rows.Count}"
            };
        }
        catch (SqlException ex)
        {
            stopwatch.Stop();
            throw new InvalidOperationException($"SQL Server query execution failed for dataset '{request.DatasetName}': {ex.Message}", ex);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            throw;
        }
    }
}
