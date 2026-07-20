# KineticReports Blazor Sample

A comprehensive Blazor Web App (Interactive Server) demonstrating the KineticReports plugin system and sample reports with different layout components.

## Overview

This sample application showcases:

1. **Plugin System Integration** - Dynamic plugin loading and lifecycle management
2. **Sample Reports** - Multiple report definitions demonstrating different components
3. **Interactive UI** - Blazor components for plugin management and report browsing
4. **Report Components** - Examples of various report layout elements

## Project Structure

```
KineticReports.Samples.Blazor/
├── Components/
│   ├── Pages/
│   │   ├── Index.razor              - Main demo page
│   │   └── Error.razor              - Error handling page
│   ├── Layout.razor                 - Main layout component
│   └── ...
├── Reports/
│   └── SampleReports.cs             - Sample report definitions
├── Services/
│   └── PluginService.cs             - Plugin management service
├── wwwroot/
│   ├── app.css                      - Application styles
│   └── index.html                   - Static content
├── Program.cs                       - Application startup
├── App.razor                        - Root component
├── Routes.razor                     - Router configuration
└── appsettings.json                 - Configuration
```

## Running the Application

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code

### Build
```bash
cd src/Samples/KineticReports.Samples.Blazor
dotnet build
```

### Run
```bash
dotnet run
```

The application will be available at `https://localhost:5001` (or the configured URL).

## Features

### Plugin Management Panel

The left sidebar demonstrates plugin lifecycle:

- **Load Plugins** - Dynamically discovers and loads plugins from the sample plugins assembly
- **Loaded Plugins List** - Shows all loaded plugins with metadata (Name, Version, Author, Description)
- **Unload All Plugins** - Cleanly unloads all plugins using async disposal

The plugin loading demonstrates:
- Dynamic assembly discovery via reflection
- Type scanning for `IPlugin` implementations
- Plugin initialization with DI support
- Async lifecycle management

### Sample Reports

Five sample report definitions showcase different components:

#### 1. Simple Text Report
**Purpose**: Basic layout with text elements

**Features**:
- Text elements for headers and content
- Page layout with margins
- Simple styling

**Use Case**: Document generation, invoices, letters

#### 2. Sales Data Table
**Purpose**: Demonstrates tabular data

**Features**:
- Table elements with columns and rows
- Data grid layout
- Column alignment and sizing

**Use Case**: Sales reports, inventory lists, transaction records

#### 3. Customer Report (Banded)
**Purpose**: Repeating sections for hierarchical data

**Features**:
- Band elements for repeating groups
- Grouping and aggregation support
- Master-detail patterns

**Use Case**: Customer statements, order summaries, grouped reports

#### 4. Multi-Section Report
**Purpose**: Multiple logical sections in single report

**Features**:
- Section elements for logical grouping
- Page breaks and pagination
- Different content per section

**Use Case**: Multi-section documents, mixed content reports

#### 5. Styled Report
**Purpose**: Comprehensive styling demonstration

**Features**:
- Typography (fonts, sizes, weights)
- Colors and backgrounds
- Borders and spacing
- Style cascade and inheritance

**Use Case**: Branded reports, professional documents

## Plugin System

### How It Works

1. **Discovery**: The `IPluginService` scans the sample plugins assembly
2. **Loading**: Creates instances of all classes implementing `IPlugin`
3. **Initialization**: Calls `InitializeAsync()` on each plugin with DI support
4. **Storage**: Maintains a list of loaded plugin instances
5. **Unloading**: Calls `UnloadAsync()` for cleanup

### Example Plugin: WatermarkPlugin

The sample includes `WatermarkPlugin` demonstrating:

```csharp
public sealed class WatermarkPlugin : PluginBase
{
    public override string Id => "kinetic.sample.watermark";
    public override string Name => "Watermark Plugin";
    public override string Version => "1.0.0";
    
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Initialized: {Name}");
        return Task.CompletedTask;
    }
}
```

### Creating Custom Plugins

To create your own plugin:

1. Create a class extending `PluginBase`
2. Implement required properties: `Id`, `Name`, `Version`
3. Override `OnInitializeAsync` for custom initialization
4. Override `OnUnloadAsync` for cleanup
5. Register in the sample plugins assembly

## Sample Report Definitions

### Report Structure

Each `ReportDefinition` includes:

- **Id**: Unique identifier
- **Name**: Display name
- **SchemaVersion**: Report definition version
- **PageWidth/PageHeight**: Page dimensions in DIPs (Device Independent Pixels)
- **Margins**: Page margins (top, right, bottom, left)
- **Children**: Collection of report elements

### Page Dimensions

KineticReports uses DIPs (Device Independent Pixels):
- 1 DIP = 1/96 inch
- Standard letter (8.5" × 11"): 816 × 1056 DIPs
- Standard A4: 793 × 1123 DIPs

### Margins

Represented as `Thickness(top, right, bottom, left)`:
```csharp
Margins = new Thickness(48, 48, 48, 48)  // 0.5 inch margins
```

## Extending the Sample

### Add More Plugins

1. Create a new class in `KineticReports.Samples.Plugins`:
```csharp
public sealed class MyPlugin : PluginBase
{
    public override string Id => "my.custom.plugin";
    public override string Name => "My Custom Plugin";
    public override string Version => "1.0.0";
    
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        // Custom initialization logic
        return Task.CompletedTask;
    }
}
```

2. Rebuild and run - the plugin will be discovered and listed

### Add More Sample Reports

1. Add a method to `SampleReports.cs`:
```csharp
public static ReportDefinition CreateMyCustomReport()
{
    return new ReportDefinition
    {
        Id = "my-report",
        Name = "My Custom Report",
        // ... configuration
        Children = []
    };
}
```

2. Add to the `GetAllSampleReports()` collection
3. The new report appears in the UI

### Implement Report Preview

To preview reports in the browser:

1. Create a `ReportEngine` with all dependencies
2. Execute the report with sample data
3. Export to HTML using `HtmlExporter`
4. Display in an iframe or viewer component

Example (pseudo-code):
```csharp
var engine = new ReportEngine(...);
var layoutTree = await engine.RunAsync(reportDef, parameters, measureContext);
var exporter = new HtmlExporter();
var html = await exporter.ExportAsync(layoutTree);
```

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **UI**: Blazor Web App (Interactive Server mode)
- **Report Engine**: KineticReports
- **Rendering**: SkiaSharp
- **Export**: HTML
- **Styling**: Bootstrap 5.3

## Project Dependencies

- `KineticReports.Core` - Core data types and definitions
- `KineticReports.Engine` - Report execution engine
- `KineticReports.Layout` - Layout pipeline (Measure, Arrange, Pagination)
- `KineticReports.Rendering.Skia` - Text measurement via SkiaSharp
- `KineticReports.Export.Html` - HTML export functionality
- `KineticReports.Viewer.Blazor` - Blazor report viewer components
- `KineticReports.Plugins` - Plugin SDK
- `KineticReports.Samples.Plugins` - Sample plugin implementations

## Configuration

### appsettings.json

Logging configuration and host settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

## Known Limitations

1. **Report Preview**: Currently not implemented (placeholder in UI)
   - To implement: Create `ReportEngine`, execute report, export to HTML

2. **Plugin Assembly Loading**: Looks for assemblies in:
   - Application base directory
   - Assembly directory

3. **Data Binding**: Sample reports don't include data binding
   - To add: Implement `IDataProvider` and `IDataResolver`

## Performance Considerations

- **Plugin Loading**: Async operations with cancellation support
- **Report Rendering**: Deterministic output via SkiaSharp
- **HTML Export**: Streaming output to reduce memory usage

## Troubleshooting

### Plugins Not Loading
- Check that `KineticReports.Samples.Plugins.dll` exists in output directory
- Verify plugin classes implement `IPlugin`
- Check debug output for error messages

### Style Issues
- Ensure `app.css` is in `wwwroot/app.css`
- Bootstrap 5.3 is loaded via CDN
- Custom CSS is appended to bootstrap

### Build Errors
- Verify all project references are correct
- Run `dotnet restore` to update packages
- Check that .NET 10.0 SDK is installed

## Further Reading

- [KineticReports Architecture Handbook](../../docs/00_Architecture_Handbook.md)
- [How to Implement a Data Provider](../../docs/11_How_To_Implement_Data_Provider.md)
- [How to Add a New Exporter](../../docs/10_How_To_Add_New_Exporter.md)
- [Plugin SDK Specification](../../docs/08_Plugin_SDK_Specification.md)

## License

This sample is part of KineticReports and follows the same license.
