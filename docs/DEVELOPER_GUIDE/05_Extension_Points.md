# Extension Points

**Objective**: Understand where and how to extend KineticReports for your custom needs.

---

## Overview

KineticReports is designed to be extended at multiple levels. Each extension point serves a different purpose and has different trade-offs.

```
┌────────────────────────────────────────────────────────┐
│  Your Application                                      │
└────────────────────────┬───────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
    ┌────────┐      ┌─────────┐      ┌─────────┐
    │Plugin  │      │Direct DI│      │Inherit  │
    │Manager │      │Register │      │Base     │
    └────────┘      └─────────┘      └─────────┘
        │                │                │
        │ Runtime        │ Startup        │ Compile
        │ Loading        │ Register       │ Time
        │                │                │
        └────────────────┼────────────────┘
                         │
           Most Flexible ▼ Least Flexible
```

---

## Extension Level 1: Plugin Manager (Runtime)

**When to use**: You want dynamic, runtime-loadable extensions.

**Best for**: 
- ✅ Modular architecture
- ✅ Third-party plugins
- ✅ Hot-loading custom features
- ✅ Marketplace of plugins

### How It Works

Plugins are discovered and loaded at runtime from a directory:

```csharp
// ASP.NET Core startup
builder.Services.AddPluginManager(pluginDirectory: @"C:\plugins");

// Automatically:
// 1. Scans for .dll files in pluginDirectory
// 2. Loads assemblies
// 3. Finds IPlugin implementations
// 4. Creates instances
// 5. Calls InitializeAsync()
// 6. Makes available via IPluginManager
```

### Implementing a Plugin

```csharp
// File: MyCustomDataPlugin.cs
// Project: MyCustomDataPlugin.csproj
// Output: MyCustomDataPlugin.dll → placed in plugins folder

using KineticReports.Plugins;
using KineticReports.Core.Data;
using System.Collections.Generic;

public class MyCustomDataPlugin : PluginBase
{
    // Required: unique identifier
    public override string Id => "my.custom.data";
    
    // Required: display name
    public override string Name => "My Custom Data Provider";
    
    // Required: version
    public override string Version => "1.0.0";
    
    // Optional: author info
    public override string? Author => "Your Organization";
    
    // Optional: description
    public override string? Description => 
        "Provides data from our custom API";
    
    // Called when plugin is loaded
    protected override async Task OnInitializeAsync(
        CancellationToken cancellationToken)
    {
        // Register custom data provider
        var services = ServiceProvider
            .GetRequiredService<IServiceCollection>();
        
        services.AddSingleton<IDataProvider>(
            new MyCustomDataProvider());
        
        await Task.CompletedTask;
    }
    
    // Called when plugin is unloaded
    protected override async Task OnUnloadAsync(
        CancellationToken cancellationToken)
    {
        // Clean up resources
        Debug.WriteLine($"Unloading {Name}");
        await Task.CompletedTask;
    }
}

// Implement your custom data provider
public class MyCustomDataProvider : IDataProvider
{
    public Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        // Fetch from your API, database, etc.
        // See 07_Data_Providers.md for complete example
    }
}
```

### Deploying a Plugin

```bash
# Build your plugin project
dotnet build MyCustomDataPlugin.csproj -c Release

# Copy DLL to plugins folder
copy bin\Release\net10.0\MyCustomDataPlugin.dll C:\plugins\

# Application starts
dotnet run

# Plugin is automatically loaded and initialized
```

### Plugin Lifecycle

```
Application Start
    ↓
PluginLoaderHostedService.StartAsync()
    ↓
DiscoverAndLoadPluginsAsync(pluginDirectory)
    ↓
For each .dll in directory:
    ├─ Assembly.LoadFrom(path)
    ├─ Find IPlugin implementations
    ├─ Activator.CreateInstance()
    ├─ plugin.InitializeAsync(ServiceProvider)
    └─ Add to LoadedPlugins
    ↓
Application Running
    ↓
IPluginManager.LoadedPlugins contains your plugins
    ↓
Your plugin can be used (e.g., IDataProvider is registered)
    ↓
Application Shutdown
    ↓
PluginLoaderHostedService.StopAsync()
    ↓
For each loaded plugin:
    ├─ plugin.UnloadAsync()
    └─ Remove from LoadedPlugins
    ↓
Application Exits
```

### Using Plugin Manager from Code

```csharp
[ApiController]
[Route("api/[controller]")]
public class PluginController : ControllerBase
{
    private readonly IPluginManager _pluginManager;
    
    public PluginController(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }
    
    [HttpGet("list")]
    public IActionResult ListPlugins()
    {
        var plugins = _pluginManager.LoadedPlugins
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Version,
                p.Author,
                p.Description
            });
        
        return Ok(plugins);
    }
    
    [HttpPost("load")]
    public async Task<IActionResult> LoadPlugin(
        [FromQuery] string assemblyPath)
    {
        try
        {
            var plugin = await _pluginManager
                .LoadPluginAsync(assemblyPath, HttpContext.RequestServices);
            
            return Ok(new { plugin.Id, plugin.Name });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("unload/{pluginId}")]
    public async Task<IActionResult> UnloadPlugin(string pluginId)
    {
        try
        {
            await _pluginManager.UnloadPluginAsync(pluginId);
            return Ok(new { message = "Plugin unloaded" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

---

## Extension Level 2: Direct DI Registration (Startup)

**When to use**: You have custom implementations you want to inject at startup.

**Best for**:
- ✅ Internal/proprietary implementations
- ✅ Tightly integrated extensions
- ✅ Testing and mocking
- ✅ Compile-time known implementations

### How It Works

Register implementations directly in DI container:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register custom data provider
builder.Services.AddSingleton<IDataProvider>(
    new MyCustomDataProvider("connection-string"));

// Register custom renderer
builder.Services.AddSingleton<IRenderer>(
    new MyCustomRenderer());

var app = builder.Build();
```

### Implementing via Interfaces

```csharp
// Implement IDataProvider
public class MyDataProvider : IDataProvider
{
    public async Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        // Your implementation
    }
}

// Implement IRenderer
public class MyRenderer : IRenderer
{
    public void Render(LayoutTree tree)
    {
        // Your rendering logic
    }
}

// Register at startup
builder.Services.AddSingleton<IDataProvider>(new MyDataProvider());
builder.Services.AddSingleton<IRenderer>(new MyRenderer());
```

### Example: Custom Data Provider via DI

```csharp
public class ApiDataProvider : IDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiDataProvider> _logger;
    
    public ApiDataProvider(
        HttpClient httpClient,
        ILogger<ApiDataProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://api.example.com/data/{request.DatasetName}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            // Parse response into QueryResult
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            
            return new QueryResult
            {
                Schema = BuildSchema(data),
                Rows = data.Select(d => new DataRow(d)).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch data for {Dataset}", 
                request.DatasetName);
            throw;
        }
    }
    
    private List<ColumnSchema> BuildSchema(
        List<Dictionary<string, object>> data)
    {
        // Extract column definitions from data
    }
}

// Register in startup
builder.Services.AddHttpClient<IDataProvider, ApiDataProvider>();
```

### Advantages

| Aspect | Benefit |
|--------|---------|
| **Testability** | Easy to mock for unit tests |
| **Flexibility** | Swap implementations without recompile |
| **Composition** | Combine with dependency injection |
| **Performance** | No reflection/assembly loading overhead |

---

## Extension Level 3: Inheritance & Overrides (Compile-Time)

**When to use**: You want to extend base classes with custom behavior.

**Best for**:
- ✅ Framework contributions
- ✅ Specialized behaviors
- ✅ Type-safe extensions
- ✅ Compile-time validation

### Extending PluginBase

```csharp
public class MyPlugin : PluginBase
{
    private ILogger<MyPlugin>? _logger;
    private IConfiguration? _config;
    
    // Implement required properties
    public override string Id => "my.plugin";
    public override string Name => "My Plugin";
    public override string Version => "1.0.0";
    
    // Override optional properties
    public override string? Author => "Your Name";
    public override string? Description => "Does something cool";
    
    // Override initialization
    protected override async Task OnInitializeAsync(
        CancellationToken cancellationToken)
    {
        // Get services from ServiceProvider (set by base class)
        _logger = ServiceProvider
            .GetService<ILogger<MyPlugin>>();
        
        _config = ServiceProvider
            .GetService<IConfiguration>();
        
        _logger?.LogInformation("Plugin initializing");
        
        // Your initialization logic
        await Task.CompletedTask;
    }
    
    // Override cleanup
    protected override async Task OnUnloadAsync(
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Plugin unloading");
        await Task.CompletedTask;
    }
}
```

### Extending IDataProvider

```csharp
public abstract class BaseDataProvider : IDataProvider
{
    protected ILogger Logger { get; set; }
    
    public virtual async Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        // Common logic (validation, logging, caching)
        Logger.LogInformation("Executing query: {Query}", 
            request.QueryText);
        
        // Call derived implementation
        return await ExecuteQueryAsync(request, cancellationToken);
    }
    
    // Derived classes override this
    protected abstract Task<QueryResult> ExecuteQueryAsync(
        QueryRequest request,
        CancellationToken cancellationToken);
}

// Derived implementation
public class SqlServerProvider : BaseDataProvider
{
    private readonly string _connectionString;
    
    public SqlServerProvider(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    protected override async Task<QueryResult> ExecuteQueryAsync(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        // SQL Server specific logic
    }
}
```

---

## What Can You Extend?

### Data Sources (IDataProvider)
✅ **Implement** to add custom data sources

**Examples**: REST APIs, MongoDB, GraphQL, CSV, Excel, in-memory collections

→ See [Data Providers Guide](07_Data_Providers.md)

### Output Formats (IRenderer)
✅ **Implement** to add new export formats

**Examples**: Word, PowerPoint, CSV, JSON, XML, DOCX, PPTX

→ See [Exporters Guide](08_Exporters.md)

### Rendering Backends
✅ **Implement** to add custom rendering

**Examples**: Canvas, SVG, custom graphics library

→ See [Rendering Guide](09_Rendering.md)

### Plugin Features
✅ **Extend PluginBase** for custom plugin behaviors

**Examples**: Event handlers, custom processors, report transformations

### Layout Elements
❌ **Not extensible** (12 built-in types cover all needs)

Reason: ADR-003 defines a closed set. Use containers and styling to achieve custom layouts.

### Expression Language
❌ **Not extensible** (fixed set of operators and functions)

Reason: ADR-005 ensures deterministic evaluation. Use custom data providers for complex computations.

---

## Extension Decision Tree

```
Do you need a new data source?
  ├─ YES → Implement IDataProvider
  │        └─ Via Plugin or DI registration
  │
  └─ NO

Do you need a new output format?
  ├─ YES → Implement IRenderer
  │        └─ Via Plugin or DI registration
  │
  └─ NO

Do you need runtime-loadable features?
  ├─ YES → Create IPlugin (extends PluginBase)
  │        └─ Deploy as .dll in plugins folder
  │
  └─ NO → Use DI registration at startup
```

---

## Real-World Scenarios

### Scenario 1: Add PostgreSQL Support

```csharp
// PostgreSQL data provider
public class PostgreSqlDataProvider : IDataProvider
{
    public async Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        using var command = new NpgsqlCommand(
            request.QueryText, connection);
        
        // Add parameters
        foreach (var param in request.Parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value);
        }
        
        // Execute and build result
        // ... (See 07_Data_Providers.md for complete example)
    }
}

// Option A: Plugin
// - Create class library: PostgreSqlPlugin.dll
// - Deploy to plugins folder
// - Auto-loaded at startup

// Option B: Direct registration
// builder.Services.AddSingleton<IDataProvider>(
//     new PostgreSqlDataProvider(connectionString));
```

### Scenario 2: Add CSV Export

```csharp
// CSV exporter
public class CsvRenderer : IRenderer
{
    public void Render(LayoutTree tree)
    {
        var csv = new StringBuilder();
        var tables = tree.Elements.OfType<TableElement>();
        
        foreach (var table in tables)
        {
            // Write header
            csv.AppendLine(
                string.Join(",", table.Columns.Select(c => c.Name)));
            
            // Write rows
            foreach (var row in table.Rows)
            {
                csv.AppendLine(
                    string.Join(",", row.Values));
            }
        }
        
        File.WriteAllText("output.csv", csv.ToString());
    }
}

// Register
builder.Services.AddSingleton<IRenderer>(
    new CsvRenderer());
```

### Scenario 3: Custom Report Transformation

```csharp
// Plugin that transforms reports on load
public class ReportTransformPlugin : PluginBase
{
    public override string Id => "report.transform";
    public override string Name => "Report Transformer";
    public override string Version => "1.0.0";
    
    protected override async Task OnInitializeAsync(
        CancellationToken cancellationToken)
    {
        // Register a middleware or event handler
        // to transform reports before processing
        
        var engine = ServiceProvider
            .GetRequiredService<ReportEngine>();
        
        // Hook into engine lifecycle
        // (if engine supports hooks)
        
        await Task.CompletedTask;
    }
}
```

---

## Extension Checklist

### Before Implementing

- [ ] Identify which layer to extend (data, render, plugin)
- [ ] Check if it's already supported (built-in)
- [ ] Choose extension method (plugin, DI, inheritance)
- [ ] Review relevant spec document

### During Implementation

- [ ] Implement required interface/extend base class
- [ ] Add error handling (null checks, try-catch)
- [ ] Add logging (ILogger if available)
- [ ] Write unit tests
- [ ] Document assumptions and limitations

### Before Deploying

- [ ] Build and test locally
- [ ] Check for breaking changes
- [ ] Verify dependencies are compatible
- [ ] Test with sample data
- [ ] Document configuration/setup

### After Deploying

- [ ] Monitor for errors
- [ ] Gather usage metrics
- [ ] Collect user feedback
- [ ] Plan maintenance and updates

---

## Best Practices

### 1. Use Dependency Injection
```csharp
// ✅ Good
public class MyProvider : IDataProvider
{
    private readonly ILogger<MyProvider> _logger;
    
    public MyProvider(ILogger<MyProvider> logger)
    {
        _logger = logger;
    }
}

// ❌ Avoid
public class MyProvider : IDataProvider
{
    private ILogger _logger = LogManager.GetCurrentClassLogger();
}
```

### 2. Make Extensions Configurable
```csharp
// ✅ Good
public class MyProvider : IDataProvider
{
    public MyProvider(IConfiguration config)
    {
        _connectionString = config["ConnectionString"];
    }
}

// ❌ Avoid
public class MyProvider : IDataProvider
{
    private const string ConnectionString = 
        "Server=localhost;Database=Reports";
}
```

### 3. Handle Errors Gracefully
```csharp
// ✅ Good
try
{
    // Implementation
}
catch (TimeoutException ex)
{
    _logger.LogError(ex, "Query timeout");
    throw new InvalidOperationException("Query failed", ex);
}

// ❌ Avoid
catch { throw; }  // Swallows context
```

### 4. Version Your Extensions
```csharp
// ✅ Good
public class MyPlugin : PluginBase
{
    public override string Version => "1.2.3";
}

// ❌ Avoid
public override string Version => "1.0";  // Never changes
```

### 5. Document Configuration
```csharp
/// <summary>
/// Custom data provider connecting to our REST API.
/// 
/// Configuration:
/// {
///   "ApiUrl": "https://api.example.com",
///   "ApiKey": "your-api-key",
///   "Timeout": "00:00:30"
/// }
/// </summary>
public class ApiDataProvider : IDataProvider
{
}
```

---

## Summary

KineticReports provides **three levels of extensibility**:

| Level | Method | When | Effort |
|-------|--------|------|--------|
| **1** | Plugin Manager | Runtime, dynamic | Medium |
| **2** | DI Registration | Startup, flexible | Low |
| **3** | Inheritance | Compile-time, typed | High |

**Choose based on your needs:**
- Want to load plugins at runtime? → **Plugin Manager**
- Want to inject at startup? → **DI Registration**
- Want to extend base classes? → **Inheritance**

All three can coexist in the same application!

---

**Time to read**: ~25 minutes  
**Difficulty**: Intermediate-Advanced  
**Next guides**: [Plugin System](06_Plugin_System.md), [Data Providers](07_Data_Providers.md), [Exporters](08_Exporters.md)
