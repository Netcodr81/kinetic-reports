# Plugin Manager Implementation Guide

## Overview

The **Plugin Manager** (`DefaultPluginManager`) is a production-ready implementation of `IPluginManager` that enables ASP.NET Core applications to dynamically discover, load, and manage plugins at runtime.

## Features

### Core Functionality
- ✅ Dynamic assembly loading and discovery
- ✅ Reflection-based `IPlugin` implementation discovery
- ✅ Thread-safe plugin management
- ✅ Async/await pattern with cancellation support
- ✅ Dependency Injection integration
- ✅ Automatic plugin lifecycle management
- ✅ Error handling and logging

### ASP.NET Core Integration
- ✅ Easy registration via extension methods
- ✅ Hosted service for automatic plugin loading on startup
- ✅ Automatic plugin unloading on application shutdown
- ✅ Scoped and singleton service support
- ✅ Works with any ASP.NET Core framework version

## Architecture

### Components

```
DefaultPluginManager
├── LoadedPlugins (thread-safe list)
├── DiscoverAndLoadPluginsAsync()
│   └── Scans directory for .dll files
│       └── LoadPluginAsync() for each assembly
├── LoadPluginAsync()
│   └── Load assembly → Find IPlugin type → Create instance → Initialize
├── UnloadPluginAsync()
│   └── Find plugin → Call UnloadAsync → Remove from list
├── GetPluginById()
│   └── Thread-safe lookup by plugin ID
└── UnloadAllAsync()
    └── Cleanup all plugins

PluginManagerExtensions
├── AddPluginManager() - Register default manager
├── AddPluginManager<T>() - Register custom implementation
├── AddPluginManager(directory) - Register with auto-load
└── AddPluginManager<T>(directory) - Custom with auto-load

PluginLoaderHostedService
├── Loads plugins on app startup
└── Unloads plugins on app shutdown
```

## ASP.NET Core Registration

### Basic Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register plugin manager
builder.Services.AddPluginManager();

var app = builder.Build();
app.Run();
```

### With Auto-Loading

Plugins are automatically discovered and loaded from a directory:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure plugin directory
var pluginDirectory = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "plugins");

// Register with auto-loading
builder.Services.AddPluginManager(pluginDirectory);

var app = builder.Build();
// Plugins are loaded automatically on startup
app.Run();
```

### With Custom Implementation

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register custom plugin manager
builder.Services.AddPluginManager<MyCustomPluginManager>();

var app = builder.Build();
app.Run();
```

### With Custom Implementation and Auto-Loading

```csharp
var builder = WebApplication.CreateBuilder(args);

var pluginDirectory = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "plugins");

builder.Services.AddPluginManager<MyCustomPluginManager>(pluginDirectory);

var app = builder.Build();
app.Run();
```

## Usage Examples

### Injecting Plugin Manager

```csharp
public class ReportController : ControllerBase
{
    private readonly IPluginManager _pluginManager;

    public ReportController(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    [HttpGet("plugins")]
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
            })
            .ToList();

        return Ok(plugins);
    }
}
```

### Loading Plugins Dynamically

```csharp
public class PluginService
{
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;

    public PluginService(IPluginManager pluginManager, IServiceProvider serviceProvider)
    {
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;
    }

    public async Task LoadPluginAsync(string assemblyPath)
    {
        try
        {
            var plugin = await _pluginManager.LoadPluginAsync(
                assemblyPath,
                _serviceProvider);

            Console.WriteLine($"Loaded: {plugin.Name} v{plugin.Version}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading plugin: {ex.Message}");
        }
    }

    public async Task UnloadPluginAsync(string pluginId)
    {
        try
        {
            await _pluginManager.UnloadPluginAsync(pluginId);
            Console.WriteLine($"Unloaded plugin: {pluginId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unloading plugin: {ex.Message}");
        }
    }

    public IPlugin? GetPlugin(string pluginId)
    {
        return _pluginManager.GetPluginById(pluginId);
    }
}
```

### Blazor Integration

```csharp
// Startup
builder.Services.AddPluginManager(pluginDirectory);

// Component
@page "/plugins"
@inject IPluginManager PluginManager

<h1>Loaded Plugins</h1>

@foreach (var plugin in PluginManager.LoadedPlugins)
{
    <div class="card">
        <div class="card-body">
            <h5 class="card-title">@plugin.Name</h5>
            <p class="card-text">@plugin.Description</p>
            <small>v@plugin.Version by @plugin.Author</small>
        </div>
    </div>
}
```

## Plugin Directory Structure

Recommended directory structure for plugins:

```
/plugins/
├── MyPlugin1.dll
├── MyPlugin2.dll
├── config/
│   └── plugins.json
└── data/
    └── plugin-cache/
```

## Error Handling

### Directory Not Found

```csharp
try
{
    await pluginManager.DiscoverAndLoadPluginsAsync(pluginDir, serviceProvider);
}
catch (DirectoryNotFoundException ex)
{
    Console.WriteLine($"Plugin directory not found: {ex.Message}");
}
```

### No IPlugin Implementation

```csharp
try
{
    await pluginManager.LoadPluginAsync(assemblyPath, serviceProvider);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Invalid plugin assembly: {ex.Message}");
}
```

### Duplicate Plugin ID

```csharp
try
{
    await pluginManager.LoadPluginAsync(assemblyPath, serviceProvider);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("already loaded"))
{
    Console.WriteLine($"Plugin already loaded: {ex.Message}");
}
```

## Thread Safety

`DefaultPluginManager` uses a locking mechanism to ensure thread-safe access:

- `LoadedPlugins` property uses a lock for thread-safe enumeration
- `LoadPluginAsync()` is synchronized to prevent race conditions
- `UnloadPluginAsync()` is synchronized to prevent conflicts
- `GetPluginById()` is thread-safe

```csharp
// Thread-safe access
var plugins = pluginManager.LoadedPlugins;  // Safe to enumerate
var plugin = pluginManager.GetPluginById("plugin-id");  // Safe
```

## Lifecycle

### On Application Start

1. `PluginLoaderHostedService.StartAsync()` is called
2. Plugin directory is scanned for `.dll` files
3. For each assembly:
   - Load assembly
   - Find `IPlugin` implementations
   - Create instance
   - Call `plugin.InitializeAsync()`
   - Add to `LoadedPlugins`
4. Application is ready with plugins loaded

### On Application Stop

1. `PluginLoaderHostedService.StopAsync()` is called
2. For each loaded plugin:
   - Call `plugin.UnloadAsync()`
   - Remove from `LoadedPlugins`
3. Application shuts down cleanly

## Debugging

Debug output is written via `System.Diagnostics.Debug.WriteLine()`:

```csharp
Loaded plugin: My Plugin (ID: my.plugin, Version: 1.0.0)
Unloaded plugin: My Plugin (ID: my.plugin)
Error loading plugin from C:\plugins\bad.dll: InvalidOperationException - No IPlugin found
```

Enable debug output in Visual Studio:
1. Debug → Windows → Output
2. Filter: `System.Diagnostics`

## Best Practices

### 1. Directory Structure

```csharp
// Recommended
var baseDir = AppDomain.CurrentDomain.BaseDirectory;
var pluginDir = Path.Combine(baseDir, "plugins");
builder.Services.AddPluginManager(pluginDir);
```

### 2. Error Handling

```csharp
// Good: Handle errors gracefully
try
{
    await pluginManager.DiscoverAndLoadPluginsAsync(dir, serviceProvider);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to load plugins");
    // Application can continue without plugins
}
```

### 3. Dependency Injection

```csharp
// Good: Inject services into plugins
public class MyPlugin : PluginBase
{
    protected override async Task OnInitializeAsync(CancellationToken ct)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger>();
        logger.LogInformation("Plugin initialized");
    }
}
```

### 4. Graceful Shutdown

```csharp
// Automatic via hosted service
builder.Services.AddPluginManager(pluginDir);
// Plugins are unloaded automatically on app shutdown
```

### 5. Configuration

```csharp
// Store plugin directory in configuration
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var pluginDir = config["Plugins:Directory"] 
    ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

builder.Services.AddPluginManager(pluginDir);
```

## Integration with Different ASP.NET Core Frameworks

### Web API

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddPluginManager(pluginDir);

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Minimal APIs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPluginManager(pluginDir);

var app = builder.Build();

app.MapGet("/plugins", (IPluginManager pm) =>
    pm.LoadedPlugins.Select(p => new { p.Id, p.Name }));

app.Run();
```

### Blazor Web App

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddPluginManager(pluginDir);

var app = builder.Build();
app.MapRazorComponents<App>();
app.Run();
```

### MVC

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddPluginManager(pluginDir);

var app = builder.Build();
app.MapControllers();
app.Run();
```

### gRPC

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddPluginManager(pluginDir);

var app = builder.Build();
app.MapGrpcService<MyService>();
app.Run();
```

## Advanced Scenarios

### Multiple Plugin Directories

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPluginManager();
builder.Services.AddHostedService(sp =>
{
    var pm = sp.GetRequiredService<IPluginManager>();
    return new MultiDirectoryPluginLoader(pm, serviceProvider, new[]
    {
        "/plugins/renderers",
        "/plugins/data-providers",
        "/plugins/exporters"
    });
});
```

### Plugin Version Management

```csharp
public class PluginVersionManager
{
    private readonly IPluginManager _pluginManager;

    public async Task UpdatePluginAsync(string pluginId, string newVersion)
    {
        var existing = _pluginManager.GetPluginById(pluginId);
        if (existing != null)
        {
            await _pluginManager.UnloadPluginAsync(pluginId);
        }

        // Load new version
        var newPath = $"/plugins/{pluginId}.{newVersion}.dll";
        await _pluginManager.LoadPluginAsync(newPath, serviceProvider);
    }
}
```

### Plugin Metadata Store

```csharp
public class PluginRegistry
{
    private readonly IPluginManager _pluginManager;
    private readonly IDistributedCache _cache;

    public async Task RegisterPluginAsync(IPlugin plugin)
    {
        var metadata = new
        {
            plugin.Id,
            plugin.Name,
            plugin.Version,
            plugin.Author,
            plugin.Description,
            LoadedAt = DateTime.UtcNow
        };

        await _cache.SetAsync(
            $"plugin:{plugin.Id}",
            JsonSerializer.SerializeToUtf8Bytes(metadata));
    }
}
```

## Troubleshooting

### Plugins Not Loading

1. Check plugin directory exists: `Directory.Exists(pluginDir)`
2. Check `.dll` files are present
3. Check assembly has `IPlugin` implementations
4. Enable debug output and check for error messages

### Plugin Initialization Fails

1. Check plugin constructor is parameterless
2. Check plugin implements `IPlugin` interface
3. Check plugin dependencies are available
4. Check `OnInitializeAsync()` doesn't throw

### Plugin Not Found by ID

1. Check spelling of plugin ID
2. Check plugin is in `LoadedPlugins` list
3. Verify plugin was initialized successfully

## Testing

```csharp
[Fact]
public async Task TestPluginManager()
{
    var manager = new DefaultPluginManager();
    var serviceProvider = new ServiceCollection().BuildServiceProvider();

    // Test loading
    var plugin = await manager.LoadPluginAsync(assemblyPath, serviceProvider);
    manager.LoadedPlugins.ShouldContain(plugin);

    // Test retrieval
    var found = manager.GetPluginById(plugin.Id);
    found.ShouldBe(plugin);

    // Test unloading
    await manager.UnloadPluginAsync(plugin.Id);
    manager.LoadedPlugins.ShouldNotContain(plugin);
}
```

## Related Documentation

- [Plugin SDK Specification](../docs/08_Plugin_SDK_Specification.md)
- [PluginBase Implementation](../src/Plugins/KineticReports.Plugins/PluginBase.cs)
- [IPlugin Interface](../src/Plugins/KineticReports.Plugins/IPlugin.cs)
- [Sample Plugin](../src/Samples/KineticReports.Samples.Plugins/WatermarkPlugin.cs)

## Performance Considerations

- **Discovery**: `O(n)` where n = number of assemblies in directory
- **Loading**: Assembly loading is cached by runtime
- **Lookup**: `O(n)` where n = number of loaded plugins (typically < 100)
- **Thread Safety**: Minimal lock contention for typical usage

For high-performance scenarios, consider:
- Pre-loading plugins on startup
- Caching plugin lookups
- Using custom `IPluginManager` implementation with optimizations
