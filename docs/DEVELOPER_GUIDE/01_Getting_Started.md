# Getting Started with KineticReports

**Objective**: Install KineticReports and generate your first report in 5 minutes.

---

## Prerequisites

- **.NET 10.0 SDK** or later ([download](https://dotnet.microsoft.com/download))
- A code editor (Visual Studio 2025, VS Code, Rider, etc.)
- Familiarity with C# and ASP.NET Core

---

## Step 1: Create a New Project

### Create a Console Application

```bash
dotnet new console -n MyFirstReport -f net10.0
cd MyFirstReport
```

### Add KineticReports NuGet Package

```bash
dotnet add package KineticReports.Core
dotnet add package KineticReports.Engine
dotnet add package KineticReports.Rendering.Skia
dotnet add package KineticReports.Export.Html
```

---

## Step 2: Create a Simple Report Definition

Create a file `simple-report.json`:

```json
{
  "schemaVersion": "1.0",
  "id": "hello-world",
  "name": "Hello World Report",
  "metadata": {
    "author": "Your Name",
    "description": "First KineticReports example"
  },
  "parameters": [],
  "dataSources": [],
  "styles": [
    {
      "id": "Heading1",
      "font": {
        "family": "Arial",
        "size": 24,
        "bold": true
      },
      "color": "#003366"
    },
    {
      "id": "Body",
      "font": {
        "family": "Arial",
        "size": 12
      }
    }
  ],
  "pages": [
    {
      "id": "page1",
      "type": "Page",
      "width": 612,
      "height": 792,
      "body": {
        "id": "body1",
        "type": "Container",
        "children": [
          {
            "id": "title",
            "type": "Text",
            "style": "Heading1",
            "content": "Welcome to KineticReports!"
          },
          {
            "id": "subtitle",
            "type": "Text",
            "style": "Body",
            "content": "This is your first report generated in seconds."
          }
        ]
      }
    }
  ]
}
```

---

## Step 3: Generate the Report

Update `Program.cs`:

```csharp
using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;
using KineticReports.Rendering.Skia;
using KineticReports.Export.Html;
using System.Json;
using System.Text.Json;

// Read the report definition
var json = File.ReadAllText("simple-report.json");
var document = JsonDocument.Parse(json);

// (For this simple example, we'll create a minimal report manually)
// In a real app, you'd parse the JSON and build ReportDefinition

try
{
    // Initialize font metrics (required for text measurement)
    var fontMetrics = new SkiaFontMetrics();
    
    // Create a simple text element
    var textElement = new TextElement
    {
        Id = "text1",
        Content = "Hello, KineticReports!",
        Bounds = new(100, 100, 400, 50)
    };
    
    // Wrap in a layout tree
    var layoutTree = new LayoutTree(
        elements: [textElement],
        documentWidth: 612,
        documentHeight: 792
    );
    
    // Export to HTML
    var htmlExporter = new HtmlExporter();
    var html = htmlExporter.Export(layoutTree);
    
    // Save output
    File.WriteAllText("output.html", html);
    Console.WriteLine("✓ Report generated: output.html");
    
    // Open in browser (Windows only)
    if (OperatingSystem.IsWindows())
    {
        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo("output.html")
            {
                UseShellExecute = true
            });
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
}
```

---

## Step 4: Run Your First Report

```bash
dotnet run
```

You should see:
```
✓ Report generated: output.html
```

Open `output.html` in your browser to see your report! 🎉

---

## What Just Happened?

```
1. Created a simple report definition in JSON
2. Initialized font metrics for text measurement
3. Created a TextElement with positioning
4. Wrapped it in a LayoutTree
5. Exported to HTML format
6. Saved the output
```

---

## Next Steps

### Option A: Learn the Full Workflow
Read [Architecture Overview](02_Architecture_Overview.md) to understand:
- How data flows through the system
- What ReportEngine does
- How layout and pagination work
- What exporters are available

### Option B: Build a Real Report
Follow [Report Definitions](04_Report_Definitions.md) to:
- Learn the full JSON schema
- Add tables, images, charts
- Style elements properly
- Bind data dynamically

### Option C: Integrate with ASP.NET Core
Check the [Web API Sample](../../src/Samples/KineticReports.Samples.WebApi/) to:
- Build a report API endpoint
- Handle HTTP requests
- Stream PDF/Excel output
- Integrate with databases

### Option D: Explore the Blazor Sample
Try the [Blazor Sample](../../src/Samples/KineticReports.Samples.Blazor/) to:
- See interactive plugin management
- Explore sample reports in action
- Understand Blazor integration
- View the UI framework

---

## Common Tasks

### Task: Generate PDF Instead of HTML

```csharp
// Add NuGet package
// dotnet add package KineticReports.Rendering.Skia

var renderer = new SkiaRenderer();
var bytes = renderer.RenderToPdf(layoutTree, pageSize: PageSize.Letter);
File.WriteAllBytes("output.pdf", bytes);
```

### Task: Generate Excel Instead of HTML

```csharp
// Add NuGet package
// dotnet add package KineticReports.Export.Excel

var exporter = new ExcelExporter();
var bytes = exporter.Export(layoutTree);
File.WriteAllBytes("output.xlsx", bytes);
```

### Task: Add Data from SQL Server

```csharp
// Add NuGet packages
// dotnet add package KineticReports.Data.SqlServer

var dataProvider = new SqlServerDataProvider("Server=localhost;Database=Reports;Integrated Security=true");

var queryRequest = new QueryRequest
{
    DatasetName = "Customers",
    QueryText = "SELECT CustomerId, Name, Email FROM Customers",
    Parameters = [],
    TimeoutMs = 30000
};

var result = await dataProvider.ExecuteAsync(queryRequest);
Console.WriteLine($"Loaded {result.Rows.Count} rows");
```

---

## Troubleshooting

### Error: "Unable to load native library"
**Cause**: SkiaSharp native libraries not found  
**Solution**: Ensure you're using SkiaSharp 4.x, not v2/v3
```bash
dotnet add package SkiaSharp --version 4.*
```

### Error: "Cannot find type 'TextElement'"
**Cause**: Missing using statement  
**Solution**: Add these usings to your file:
```csharp
using KineticReports.Core.Layout;
using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;
```

### Error: "Layout tree is empty"
**Cause**: No elements added to layout tree  
**Solution**: Ensure you create and add at least one element before exporting

### Everything runs but output looks wrong
**Cause**: Missing style definitions  
**Solution**: Review [Style System Specification](../06_Style_System_Specification.md)

---

## Architecture Diagram: Quick View

```
┌──────────────────────────┐
│ Your Code                │
│ (Program.cs)             │
└────────────┬─────────────┘
             │
             │ 1. Load JSON
             ▼
┌──────────────────────────┐
│ Report Definition        │
│ (JSON)                   │
└────────────┬─────────────┘
             │
             │ 2. Process
             ▼
┌──────────────────────────┐
│ ReportEngine             │
│ - Layout                 │
│ - Pagination             │
│ - Expression Eval        │
└────────────┬─────────────┘
             │
             │ 3. Create
             ▼
┌──────────────────────────┐
│ LayoutTree               │
│ (Positioned Elements)    │
└────────────┬─────────────┘
             │
             │ 4. Export
             ▼
┌──────────────────────────┐
│ Exporter                 │
│ - HTML, PDF, Excel       │
│ - Custom formats         │
└────────────┬─────────────┘
             │
             │ 5. Output
             ▼
┌──────────────────────────┐
│ File or Stream           │
│ .html, .pdf, .xlsx, etc. │
└──────────────────────────┘
```

---

## Key Files in the Sample Projects

### Console Sample
- **Location**: `src/Samples/KineticReports.Samples.Console/`
- **Shows**: Basic report generation, minimal setup
- **Best for**: Learning the fundamentals

### Web API Sample
- **Location**: `src/Samples/KineticReports.Samples.WebApi/`
- **Shows**: ASP.NET Core integration, endpoints
- **Best for**: Server-side report generation

### Blazor Sample
- **Location**: `src/Samples/KineticReports.Samples.Blazor/`
- **Shows**: Interactive UI, plugin management, Blazor components
- **Best for**: Understanding UI integration

---

## What's Next?

1. **Completed**: ✓ You've generated your first report
2. **Next**: Read [Architecture Overview](02_Architecture_Overview.md)
3. **Then**: Dive into [Report Definitions](04_Report_Definitions.md)
4. **Advanced**: Explore [Extension Points](05_Extension_Points.md)

---

**Time to complete**: ~5 minutes  
**Difficulty**: Beginner  
**Next guide**: [Architecture Overview](02_Architecture_Overview.md)
