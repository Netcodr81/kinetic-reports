# Plugin System Guide

**Objective**: Understand how to build, load, and use plugins in KineticReports.

This guide references the full documentation in [12_Plugin_Manager_Implementation.md](../12_Plugin_Manager_Implementation.md).

---

## What Are Plugins?

Plugins are dynamically-loadable extensions that can:
- ✅ Provide custom data sources (IDataProvider)
- ✅ Add new export formats (IRenderer)
- ✅ Implement custom rendering backends
- ✅ Execute arbitrary initialization logic
- ✅ Access application services via DI

Plugins are loaded **at runtime** from a designated directory without requiring application recompile.

---

## Plugin Lifecycle

### 1. Discovery (Startup)
```
Application starts
  ↓
PluginLoaderHostedService.StartAsync()
  ↓
Scan pluginDirectory for .dll files
  ↓
For each DLL:
  ├─ Assembly.LoadFrom(path)
  ├─ Find IPlugin implementations
  ├─ Create instances
  └─ Add to LoadedPlugins
```

### 2. Initialization
```
For each plugin instance:
  ├─ Call plugin.InitializeAsync(ServiceProvider)
  ├─ Plugin registers custom services
  └─ Logging and diagnostics
```

### 3. Usage
```
Application runs
  ├─ Use custom IDataProvider registered by plugin
  ├─ Generate reports with plugin-provided data
  └─ Export reports using plugin-provided renderers
```

### 4. Unloading (Shutdown)
```
Application shuts down
  ↓
PluginLoaderHostedService.StopAsync()
  ↓
For each loaded plugin:
  ├─ Call plugin.UnloadAsync()
  └─ Clean up resources
```

---

## Creating a Plugin

### Step 1: Create Class Library Project

```bash
dotnet new classlib -n MyAwesomePlugin -f net10.0
cd MyAwesomePlugin
dotnet add package KineticReports.Core
dotnet add package KineticReports.Plugins
```

### Step 2: Implement IPlugin

```csharp
using KineticReports.Plugins;
using System.Diagnostics;

public class MyAwesomePlugin : PluginBase
{
    /// <summary>
    /// Unique identifier for the plugin.
    /// Format: organization.feature.plugin (e.g., acme.reporting.custom)
    /// </summary>
    public override string Id => "acme.awesome.plugin";
    
    /// <summary>
    /// Display name shown to users.
    /// </summary>
    public override string Name => "My Awesome Plugin";
    
    /// <summary>
    /// Version string following semantic versioning.
    /// </summary>
    public override string Version => "1.0.0";
    
    /// <summary>
    /// Organization or developer name.
    /// </summary>
    public override string? Author => "ACME Corporation";
    
    /// <summary>
    /// Brief description of what the plugin does.
    /// </summary>
    public override string? Description => 
        "Adds custom data providers and rendering capabilities";
    
    /// <summary>
    /// Called when the plugin is loaded into the application.
    /// Use this to register services, initialize data, etc.
    /// </summary>
    protected override async Task OnInitializeAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            Debug.WriteLine($"[{Name}] Initializing...");
            
            // ServiceProvider is set by the base class
            // Use it to access application services
            var logger = ServiceProvider
                .GetService<ILogger<MyAwesomePlugin>>();
            
            var services = ServiceProvider
                .GetService<IServiceCollection>();
            
            // Register your custom services
            if (services != null)
            {
                services.AddSingleton<IDataProvider>(
                    new MyCustomDataProvider());
                
                services.AddSingleton<IRenderer>(
                    new MyCustomRenderer());
                
                logger?.LogInformation(
                    "Plugin {PluginName} initialized successfully",
                    Name);
            }
            
            Debug.WriteLine($"[{Name}] Initialization complete");
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[{Name}] Initialization failed: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Called when the plugin is being unloaded.
    /// Use this to clean up resources, close connections, etc.
    /// </summary>
    protected override async Task OnUnloadAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            Debug.WriteLine($"[{Name}] Unloading...");
            
            // Clean up resources here
            
            Debug.WriteLine($"[{Name}] Unload complete");
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[{Name}] Unload failed: {ex.Message}");
        }
    }
}
```

### Step 3: Implement Custom Services

#### Custom Data Provider

```csharp
using KineticReports.Core.Data;

public class MyCustomDataProvider : IDataProvider
{
    private readonly string _connectionString;
    
    public MyCustomDataProvider(string? connectionString = null)
    {
        _connectionString = connectionString ?? 
            "your-default-connection";
    }
    
    public async Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        // See 07_Data_Providers.md for complete implementation
        // Example: fetch from REST API, database, file system, etc.
        
        try
        {
            // Execute query
            var rows = await FetchDataAsync(
                request.QueryText,
                request.Parameters,
                cancellationToken);
            
            // Build schema
            var schema = BuildSchema(rows);
            
            // Return result
            return new QueryResult
            {
                Schema = schema,
                Rows = rows.ConvertAll(r => new DataRow(r)),
                RowCount = rows.Count,
                ExecutionTimeMs = 0,
                DiagnosticsMessage = "Success"
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Query execution failed: {ex.Message}", ex);
        }
    }
    
    private async Task<List<Dictionary<string, object>>> FetchDataAsync(
        string query,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        // Your implementation here
        await Task.CompletedTask;
        return [];
    }
    
    private List<ColumnSchema> BuildSchema(
        List<Dictionary<string, object>> rows)
    {
        // Your schema building logic here
        return [];
    }
}
```

#### Custom Renderer

```csharp
using KineticReports.Core.Rendering;
using KineticReports.Core.Layout;

public class MyCustomRenderer : IRenderer
{
    private readonly ILogger<MyCustomRenderer> _logger;
    
    public MyCustomRenderer(
        ILogger<MyCustomRenderer> logger)
    {
        _logger = logger;
    }
    
    public void Render(LayoutTree tree)
    {
        try
        {
            _logger.LogInformation(
                "Rendering with custom renderer");
            
            // Process each element
            foreach (var element in tree.Elements)
            {
                RenderElement(element);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rendering failed");
            throw;
        }
    }
    
    private void RenderElement(LayoutElement element)
    {
        // Your rendering logic here
    }
}
```

### Step 4: Build and Deploy

```bash
# Build in Release mode
dotnet build -c Release

# Copy DLL to plugins directory
copy bin\Release\net10.0\MyAwesomePlugin.dll C:\plugins\

# Plugin will be automatically loaded on next app start
```

---

## Registering Plugin Manager

### In ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddLogging();

// Register plugin manager with auto-loading
var pluginDirectory = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "plugins");

builder.Services.AddPluginManager(pluginDirectory);

var app = builder.Build();

// Plugins are now loaded and initialized
app.MapControllers();
app.Run();
```

### Configuration

Store plugin directory in `appsettings.json`:

```json
{
  "Plugins": {
    "Directory": "./plugins",
    "Enabled": true
  }
}
```

Use in startup:

```csharp
var config = builder.Configuration;
var pluginDir = config["Plugins:Directory"]
    ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

if (config.GetValue<bool>("Plugins:Enabled", true))
{
    builder.Services.AddPluginManager(pluginDir);
}
```

---

## Using Plugins from Code

### List Loaded Plugins

```csharp
public class PluginListController : ControllerBase
{
    private readonly IPluginManager _pluginManager;
    
    public PluginListController(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }
    
    [HttpGet("api/plugins")]
    public IActionResult GetPlugins()
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
}
```

### Load Plugin Dynamically

```csharp
[HttpPost("api/plugins/load")]
public async Task<IActionResult> LoadPlugin(
    [FromQuery] string assemblyPath)
{
    try
    {
        var plugin = await _pluginManager.LoadPluginAsync(
            assemblyPath,
            HttpContext.RequestServices);
        
        return Ok(new
        {
            message = "Plugin loaded successfully",
            plugin = new { plugin.Id, plugin.Name }
        });
    }
    catch (FileNotFoundException ex)
    {
        return NotFound(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

### Unload Plugin

```csharp
[HttpDelete("api/plugins/{pluginId}")]
public async Task<IActionResult> UnloadPlugin(string pluginId)
{
    try
    {
        await _pluginManager.UnloadPluginAsync(pluginId);
        return Ok(new { message = "Plugin unloaded" });
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { error = ex.Message });
    }
}
```

---

## Plugin Directory Structure

```
plugins/
├── MyAwesomePlugin.dll
├── MyAwesomePlugin.pdb        # Optional: debug symbols
├── AnotherPlugin.dll
├── config/
│   └── plugins-config.json
└── data/
    └── cached-data/
```

### Plugin Naming Convention
```
{Organization}.{Feature}.Plugin.dll

Examples:
  Acme.Reporting.SqlServerPlugin.dll
  Acme.Reporting.RestApiPlugin.dll
  Acme.Reporting.MongoDbPlugin.dll
```

---

## Best Practices

### 1. Use Dependency Injection

```csharp
// ✅ Good: Inject services
public class MyPlugin : PluginBase
{
    protected override async Task OnInitializeAsync(
        CancellationToken cancellationToken)
    {
        var logger = ServiceProvider
            .GetRequiredService<ILogger<MyPlugin>>();
        logger.LogInformation("Plugin initialized");
    }
}

// ❌ Avoid: Static references
public class BadPlugin : PluginBase
{
    private static ILogger _logger = LogManager.GetCurrentClassLogger();
}
```

### 2. Implement Proper Error Handling

```csharp
protected override async Task OnInitializeAsync(
    CancellationToken cancellationToken)
{
    try
    {
        // Implementation
    }
    catch (Exception ex)
    {
        var logger = ServiceProvider.GetService<ILogger>();
        logger?.LogError(ex, "Plugin initialization failed");
        throw;  // Re-throw to inform caller
    }
}
```

### 3. Version Your Plugin

```csharp
public override string Version => "1.0.0";  // Semantic versioning
```

Update version when releasing new features.

### 4. Document Configuration

```csharp
/// <summary>
/// Custom SQL Server data provider plugin.
/// 
/// Configuration (in appsettings.json):
/// {
///   "Plugins": {
///     "SqlServerProvider": {
///       "ConnectionString": "Server=...;Database=...",
///       "Timeout": 30000
///     }
///   }
/// }
/// 
/// Usage:
///   Reports can now use providerType: "CustomSqlServer"
/// </summary>
public class CustomSqlServerPlugin : PluginBase
{
}
```

### 5. Use Unique IDs

```csharp
// ✅ Good: Fully qualified, reverse-domain style
public override string Id => "com.acme.reporting.datasql";

// ❌ Avoid: Generic names that might conflict
public override string Id => "sql-provider";
```

---

## Testing Plugins

### Unit Testing

```csharp
[Fact]
public void Plugin_HasUniqueId()
{
    var plugin = new MyAwesomePlugin();
    plugin.Id.Should().NotBeNullOrEmpty();
    plugin.Id.Should().Contain(".");  // Reverse-domain format
}

[Fact]
public async Task Plugin_Initializes_WithServices()
{
    var services = new ServiceCollection();
    services.AddLogging();
    var provider = services.BuildServiceProvider();
    
    var plugin = new MyAwesomePlugin();
    await plugin.InitializeAsync(provider);
    
    plugin.Name.Should().NotBeNullOrEmpty();
}

[Fact]
public async Task Plugin_Unloads_Gracefully()
{
    var plugin = new MyAwesomePlugin();
    
    // Should not throw
    await plugin.UnloadAsync();
}
```

### Integration Testing

```csharp
[Fact]
public async Task Plugin_CanBeLoaded_ByManager()
{
    var manager = new DefaultPluginManager();
    var services = new ServiceCollection()
        .BuildServiceProvider();
    
    var assemblyPath = typeof(MyAwesomePlugin)
        .Assembly.Location;
    
    var plugin = await manager.LoadPluginAsync(
        assemblyPath,
        services);
    
    manager.LoadedPlugins.Should().Contain(plugin);
    manager.GetPluginById(plugin.Id).Should().Be(plugin);
}
```

---

## Troubleshooting

### Plugin Not Loading

1. Check plugin directory exists:
```csharp
if (!Directory.Exists(pluginDirectory))
{
    Console.WriteLine($"Plugin directory not found: {pluginDirectory}");
}
```

2. Check for .dll files:
```csharp
var dlls = Directory.GetFiles(pluginDirectory, "*.dll");
Console.WriteLine($"Found {dlls.Length} plugin assemblies");
```

3. Check debug output:
```
[MyAwesomePlugin] Initializing...
[MyAwesomePlugin] Initialization complete
```

### Plugin Initialization Fails

1. Check ILogger output:
```csharp
var logger = sp.GetService<ILogger>();
logger?.LogError(ex, "Plugin load failed");
```

2. Verify services are registered:
```csharp
var services = ServiceProvider
    .GetService<IServiceProvider>();
```

3. Use try-catch in OnInitializeAsync:
```csharp
protected override async Task OnInitializeAsync(
    CancellationToken cancellationToken)
{
    try { /* ... */ }
    catch (Exception ex)
    {
        Debug.WriteLine($"Init failed: {ex}");
        throw;
    }
}
```

### Assembly Loading Issues

1. Ensure .NET 10.0 compatibility:
```bash
dotnet --version  # Should be 10.0+
```

2. Check dependency versions:
```xml
<PackageReference Include="KineticReports.Core" Version="1.0.0" />
```

3. Verify no architecture mismatches:
```bash
# For x64
dotnet build -c Release

# For ARM64
dotnet build -c Release --runtime win-arm64
```

---

## Reference Documentation

- **Full Plugin Manager Guide**: [12_Plugin_Manager_Implementation.md](../12_Plugin_Manager_Implementation.md)
- **Data Providers**: [07_Data_Providers.md](07_Data_Providers.md)
- **Custom Renderers**: [08_Exporters.md](08_Exporters.md) / [09_Rendering.md](09_Rendering.md)
- **Extension Points**: [05_Extension_Points.md](05_Extension_Points.md)

---

**Time to read**: ~30 minutes  
**Difficulty**: Intermediate-Advanced  
**Prerequisites**: C# knowledge, understanding of interfaces/DI
