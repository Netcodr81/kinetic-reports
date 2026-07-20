# Implementing a New Data Provider for KineticReports

## Overview

This guide explains how to implement a new data provider for KineticReports. Data providers retrieve data from external sources (databases, APIs, files, etc.) and return results in a standardized format that the reporting engine can use.

## Architecture Principles

Data providers follow these core principles:

1. **Provider Agnostic**: The engine does not depend on specific backends (SQL, REST, GraphQL, etc.).
2. **Streaming First**: Large datasets should be streamed to avoid memory overload.
3. **Async by Default**: All I/O operations are asynchronous.
4. **Parameterized Execution**: All providers MUST use parameterized queries to prevent injection attacks.
5. **Immutable Results**: Query results are immutable records that cannot be modified after creation.
6. **Dependency Injection**: Providers are registered in DI containers for easy integration.

## Architecture

```
Report Execution
      ↓
IDataProvider.ExecuteAsync()
      ↓
Backend Connection (SQL Server, PostgreSQL, REST, etc.)
      ↓
QueryResult { Schema, Rows, Diagnostics }
      ↓
Layout Engine
```

## Core Contracts

### IDataProvider Interface

```csharp
public interface IDataProvider
{
    string Name { get; }
    
    Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default);
}
```

### QueryRequest

Contains:
- `DatasetName`: The dataset identifier from the report definition
- `QueryText`: The query (SQL, GraphQL string, REST path, etc.)
- `Parameters`: Dictionary of parameter name/value pairs
- `TimeoutMs`: Execution timeout in milliseconds (default 30000)

### QueryResult

Contains:
- `Schema`: List of `ColumnSchema` describing columns
- `Rows`: List of `DataRow` with column values
- `RowCount`: Total number of rows
- `ExecutionTimeMs`: How long the query took
- `DiagnosticsMessage`: Optional debug information

### ColumnSchema

Describes each column:
- `Name`: Column identifier
- `ClrType`: CLR type (typeof(int), typeof(string), etc.)
- `IsNullable`: Whether column can contain null
- `Ordinal`: Zero-based column index
- `DisplayName`: Optional human-readable name

### DataRow

Represents a single result row:
- `Values`: IReadOnlyList<object?> with values in ordinal order
- Indexer: `row[0]` accesses by ordinal
- `GetValue(schema, columnName)`: Access by name (case-insensitive)

## Project Structure

### File Organization

```
src/
  Data/
    KineticReports.Data.MyBackend/
      IMyBackendDataProvider.cs         # Optional: Format-specific interface
      MyBackendDataProvider.cs          # Concrete implementation
      KineticReports.Data.MyBackend.csproj

tests/
  unit-tests/
    KineticReports.Data.MyBackend.Tests/
      GlobalUsings.cs
      MyBackendDataProviderTests.cs     # Unit tests
      DataTypeTests.cs                  # Data contract tests
      KineticReports.Data.MyBackend.Tests.csproj
```

### Naming Convention

- **Implementation**: `{Backend}DataProvider` (e.g., `SqlServerDataProvider`, `PostgreSqlDataProvider`)
- **Optional Interface**: `I{Backend}DataProvider` (for backend-specific extensions)
- **NuGet Package**: `KineticReports.Data.{Backend}`

## Step 1: Implement the Core Provider

Create `{Backend}DataProvider.cs`:

```csharp
namespace KineticReports.Data.MyBackend;

using System.Diagnostics;
using KineticReports.Core.Data;

/// <summary>
/// Provides data from MyBackend (e.g., REST API, MongoDB, GraphQL).
/// </summary>
public sealed class MyBackendDataProvider : IDataProvider
{
    private readonly string _connectionInfo;
    private readonly HttpClient? _httpClient; // For REST backends
    private readonly ILogger<MyBackendDataProvider> _logger;

    /// <summary>
    /// Initializes a new instance with connection information.
    /// </summary>
    public MyBackendDataProvider(string connectionInfo, ILogger<MyBackendDataProvider> logger)
    {
        if (string.IsNullOrEmpty(connectionInfo))
            throw new ArgumentException("Connection info required.", nameof(connectionInfo));

        _connectionInfo = connectionInfo;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "MyBackend";

    public async Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation($"Executing query: {request.DatasetName}");

            // Call backend to fetch data
            var (schema, rows) = await FetchDataAsync(request, cancellationToken);

            stopwatch.Stop();

            return new QueryResult
            {
                Schema = schema,
                Rows = rows,
                RowCount = rows.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                DiagnosticsMessage = $"Dataset: {request.DatasetName}, Rows: {rows.Count}"
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("Query cancelled");
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError($"Query failed: {ex.Message}");
            throw new InvalidOperationException(
                $"MyBackend query execution failed for '{request.DatasetName}': {ex.Message}", ex);
        }
    }

    private async Task<(IReadOnlyList<ColumnSchema>, IReadOnlyList<DataRow>)> FetchDataAsync(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Implement backend-specific logic
        // Examples:
        //
        // For SQL: Parse QueryText as SQL, prepare statement, bind parameters
        // For REST: Use QueryText as endpoint path, pass parameters as query string
        // For GraphQL: Use QueryText as GraphQL query, pass parameters to variables
        // For MongoDB: Parse QueryText as filter spec, execute find()
        // For CSV: Load file, filter by parameters

        // Return schema and rows
        var schema = new List<ColumnSchema>();
        var rows = new List<DataRow>();

        return (schema, rows);
    }
}
```

## Step 2: Example: REST API Data Provider

Here's a concrete example of a REST API provider:

```csharp
namespace KineticReports.Data.RestApi;

using KineticReports.Core.Data;
using System.Text.Json;

public sealed class RestApiDataProvider : IDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public RestApiDataProvider(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    public string Name => "REST API";

    public async Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Build URL from QueryText (path) and parameters (query string)
            var url = BuildUrl(request);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Parse JSON response
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            // Transform JSON to schema + rows
            var (schema, rows) = ParseJsonArray(doc.RootElement);

            stopwatch.Stop();

            return new QueryResult
            {
                Schema = schema,
                Rows = rows,
                RowCount = rows.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"REST API call failed: {ex.Message}", ex);
        }
    }

    private string BuildUrl(QueryRequest request)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/{request.QueryText.TrimStart('/')}";

        // Add parameters as query string
        var queryParams = request.Parameters
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString()!)}")
            .ToList();

        if (queryParams.Any())
            url += "?" + string.Join("&", queryParams);

        return url;
    }

    private (IReadOnlyList<ColumnSchema>, IReadOnlyList<DataRow>) ParseJsonArray(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Expected JSON array response");

        var schema = new List<ColumnSchema>();
        var rows = new List<DataRow>();
        var ordinal = 0;

        foreach (var item in root.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            // Build schema from first row
            if (ordinal == 0)
            {
                foreach (var prop in item.EnumerateObject())
                {
                    schema.Add(new ColumnSchema
                    {
                        Name = prop.Name,
                        ClrType = GetClrType(prop.Value),
                        Ordinal = schema.Count,
                        IsNullable = prop.Value.ValueKind == JsonValueKind.Null
                    });
                }
            }

            // Build row
            var values = new object?[schema.Count];
            foreach (var prop in item.EnumerateObject())
            {
                var colIndex = schema.FindIndex(s => s.Name == prop.Name);
                if (colIndex >= 0)
                    values[colIndex] = GetValue(prop.Value);
            }

            rows.Add(new DataRow { Values = values });
            ordinal++;
        }

        return (schema, rows);
    }

    private Type GetClrType(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => typeof(string),
        JsonValueKind.Number => typeof(double),
        JsonValueKind.True or JsonValueKind.False => typeof(bool),
        JsonValueKind.Null => typeof(object),
        _ => typeof(object)
    };

    private object? GetValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => element.GetRawText()
    };
}
```

## Step 3: Create Unit Tests

```csharp
namespace KineticReports.Data.MyBackend.Tests;

public class MyBackendDataProviderTests
{
    [Fact]
    public void Constructor_WithNullConnectionInfo_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new MyBackendDataProvider(null!, new MockLogger()));
    }

    [Fact]
    public void Name_Property_ReturnsProviderName()
    {
        // Arrange
        var provider = new MyBackendDataProvider("info", new MockLogger());

        // Act
        var name = provider.Name;

        // Assert
        name.ShouldBe("MyBackend");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new MyBackendDataProvider("info", new MockLogger());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await provider.ExecuteAsync(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsQueryResult()
    {
        // Arrange
        var provider = new MyBackendDataProvider("info", new MockLogger());
        var request = new QueryRequest
        {
            DatasetName = "Employees",
            QueryText = "SELECT * FROM Employees",
            Parameters = new Dictionary<string, object?>()
        };

        // Act
        var result = await provider.ExecuteAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Schema.ShouldNotBeEmpty();
        result.Rows.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithParameters_BindsCorrectly()
    {
        // Arrange
        var provider = new MyBackendDataProvider("info", new MockLogger());
        var request = new QueryRequest
        {
            DatasetName = "Sales",
            QueryText = "SELECT * FROM Sales WHERE Year = @Year",
            Parameters = new Dictionary<string, object?> { { "Year", 2024 } }
        };

        // Act
        var result = await provider.ExecuteAsync(request);

        // Assert
        result.ShouldNotBeNull();
    }
}
```

## Step 4: Configure Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Core\KineticReports.Core\KineticReports.Core.csproj" />
  </ItemGroup>

  <!-- Backend-specific dependencies -->
  <ItemGroup>
    <!-- For SQL Server -->
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.0" />
    
    <!-- For PostgreSQL -->
    <!-- <PackageReference Include="Npgsql" Version="8.0.2" /> -->
    
    <!-- For MongoDB -->
    <!-- <PackageReference Include="MongoDB.Driver" Version="2.26.0" /> -->
    
    <!-- For REST APIs -->
    <!-- HttpClient is built-in, but you might use: -->
    <!-- <PackageReference Include="Refit" Version="7.0.0" /> -->
  </ItemGroup>

</Project>
```

## Step 5: Register with Dependency Injection

### ASP.NET Core Integration

```csharp
// In Program.cs
builder.Services.AddScoped<IDataProvider>(sp =>
    new MyBackendDataProvider(
        configuration.GetConnectionString("MyBackend"),
        sp.GetRequiredService<ILogger<MyBackendDataProvider>>()));

// Or for multiple providers:
builder.Services.AddScoped<SqlServerDataProvider>();
builder.Services.AddScoped<PostgreSqlDataProvider>();
builder.Services.AddScoped<RestApiDataProvider>();
```

### Usage in Report Engine

```csharp
public class ReportExecutor
{
    private readonly IDataProvider _dataProvider;
    private readonly IReportEngine _engine;

    public ReportExecutor(IDataProvider dataProvider, IReportEngine engine)
    {
        _dataProvider = dataProvider;
        _engine = engine;
    }

    public async Task<LayoutTree> ExecuteAsync(ReportDefinition definition)
    {
        // Engine internally uses IDataProvider to resolve datasets
        return await _engine.RunAsync(definition, new Dictionary<string, object?>());
    }
}
```

## Step 6: Support Parameter Binding

Different backends require different parameter binding:

### SQL Server
```csharp
// Parameterized query
var cmd = new SqlCommand("SELECT * FROM Employees WHERE DeptId = @DeptId", connection);
cmd.Parameters.AddWithValue("@DeptId", 5);
```

### PostgreSQL
```csharp
// Parameterized query
using var cmd = new NpgsqlCommand("SELECT * FROM employees WHERE dept_id = @deptId", connection);
cmd.Parameters.AddWithValue("@deptId", 5);
```

### REST API
```csharp
// Query string parameters
var url = "https://api.example.com/employees?deptId=5&status=active";
```

### MongoDB
```csharp
// Filter document
var filter = Builders<Employee>.Filter.Eq(e => e.DepartmentId, 5);
var employees = await collection.Find(filter).ToListAsync();
```

## Testing Guidelines

1. **Unit Tests**:
   - Constructor validation (null/empty connection strings)
   - Provider name property
   - Null request handling

2. **Integration Tests** (if real backend available):
   - Execute simple query
   - Verify schema matches backend
   - Bind parameters correctly
   - Handle multiple rows
   - Handle empty result sets

3. **Mock/Stub Tests** (when backend unavailable):
   - Mock backend responses
   - Verify parameter binding behavior
   - Test error scenarios

4. **Error Cases**:
   - Connection failures
   - Query syntax errors
   - Invalid parameters
   - Timeout scenarios
   - Cancellation token

## Best Practices

1. **Parameterization**: Always use parameterized queries. Never concatenate parameters into query strings.
2. **Connection Management**: Use `using` statements to ensure connections are disposed.
3. **Streaming**: For large datasets, implement `IAsyncEnumerable<DataRow>` instead of buffering all rows.
4. **Error Messages**: Provide actionable error messages that help debugging (include dataset name, timeout, etc.).
5. **Logging**: Log query execution, timing, row counts, and errors for diagnostics.
6. **Timeouts**: Respect the `TimeoutMs` property; fail gracefully if exceeded.
7. **Cancellation**: Support `CancellationToken` for graceful shutdown during long operations.
8. **Type Fidelity**: Preserve native types (int, decimal, DateTime) from the backend; avoid converting everything to string.
9. **Nullable Columns**: Accurately report which columns can be null.
10. **Documentation**: Document backend-specific limitations, parameter syntax, and connection string format.

## Supported Backend Examples

| Backend | Complexity | Key Considerations |
|---------|-----------|-------------------|
| SQL Server | Medium | Connection strings, parameterized T-SQL, DataReader |
| PostgreSQL | Medium | npgsql NuGet, parameterized queries, type mapping |
| SQLite | Low | In-memory/file-based, single-threaded by default |
| REST API | Medium | HTTP client, JSON parsing, pagination |
| GraphQL | High | Query language, variable binding, nested responses |
| MongoDB | High | Document model, filter syntax, async cursor |
| CSV | Low | File I/O, header parsing, type inference |
| Excel | Medium | OpenXML, sheet selection, cell type detection |

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Parameters not bound | Verify parameter names match backend syntax (@, :, $, etc.) |
| Wrong data types | Ensure CLR type matches backend type; add explicit type conversion |
| Null values become empty strings | Check database nullability; distinguish DBNull from empty string |
| Query times out | Increase `TimeoutMs` in QueryRequest; optimize backend query |
| Memory exhaustion | Implement streaming (IAsyncEnumerable) instead of buffering all rows |
| Schema detection fails | Verify metadata API for backend; fallback to inferring from first row |

## Next Steps

1. Choose your backend technology
2. Create `KineticReports.Data.{Backend}` project
3. Implement `{Backend}DataProvider`
4. Write unit and integration tests
5. Add to `KineticReports.slnx`
6. Package as NuGet for distribution
7. Document backend-specific connection string format and query syntax

## See Also

- [IDataProvider Interface](../src/Core/KineticReports.Core/Data/IDataProvider.cs)
- [SQL Server Example](../src/Data/KineticReports.Data.SqlServer/SqlServerDataProvider.cs)
- [Data Provider Specification](./09_Data_Provider_SDK_Specification.md)

