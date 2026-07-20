# Architecture Overview

**Objective**: Understand how KineticReports works, how pieces fit together, and where you can extend it.

---

## System Architecture

### High-Level Data Flow

```
┌─────────────────────────────────────────────────────┐
│  Application Layer                                  │
│  (Your code: Console, Web API, Blazor, Desktop)    │
└───────────────────┬─────────────────────────────────┘
                    │
        ┌───────────▼───────────┐
        │  ReportEngine API     │  Entry Point
        │  (GenerateAsync)      │
        └───────────┬───────────┘
                    │
    ┌───────────────┼──────────────────┐
    │               │                  │
    ▼               ▼                  ▼
┌─────────────┐ ┌──────────────┐ ┌────────────────┐
│ Definition  │ │ Data Source  │ │ Style Cascade  │
│ Parser      │ │ (IDataProvider)  │ (Theme → Local)│
└──────┬──────┘ └──────┬───────┘ └────────┬────────┘
       │               │                 │
       └───────────────┼─────────────────┘
                       │
        ┌──────────────▼──────────────┐
        │  Layout Engine              │
        │  (Measure → Arrange Pass)   │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │  Pagination Engine          │
        │  (Split into pages)         │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │  LayoutTree (Immutable)     │
        │  - Positioned elements      │
        │  - Resolved styles          │
        │  - Page assignments         │
        └──────────────┬──────────────┘
                       │
    ┌──────────────────┼──────────────────┐
    │                  │                  │
    ▼                  ▼                  ▼
┌─────────┐       ┌────────┐          ┌────────┐
│   HTML  │       │  Excel │          │  PDF   │
│Renderer │       │Renderer│          │Renderer│
│(IRenderer)      │(IRenderer)        │(IRenderer)
└────┬────┘       └───┬────┘          └───┬────┘
     │                │                   │
     └────────────────┼───────────────────┘
                      │
        ┌─────────────▼──────────┐
        │ Output Stream/File     │
        │ (.html, .xlsx, .pdf)   │
        └────────────────────────┘
```

---

## Core Concepts

### 1. Report Definition (JSON)

**What it is**: A declarative document describing the structure, layout, and appearance of a report.

**Format**: 
```json
{
  "schemaVersion": "1.0",
  "id": "invoice",
  "name": "Invoice",
  "parameters": [],
  "dataSources": [],
  "styles": [],
  "pages": []
}
```

**Key characteristics**:
- ✅ Language-neutral (pure JSON)
- ✅ Human-readable and editable
- ✅ Versioned schema for forward compatibility
- ✅ Declarative (not procedural)

**Analogy**: Think of it like an HTML page structure, but designed for reports.

### 2. Device Independent Pixels (DIP)

**What it is**: A unit of measurement independent of screen resolution.

**Value**: 1 DIP = 1/96 inch (standard since Windows Forms)

**Why**: Ensures pixel-perfect output regardless of screen DPI or printer resolution.

**Example**:
```json
{
  "x": 96,      // 1 inch from left
  "y": 144,     // 1.5 inches from top
  "width": 480, // 5 inches wide
  "height": 240 // 2.5 inches tall
}
```

### 3. Style Cascade

**How styles are resolved** (in priority order):

1. **Theme** — Default styles for entire report
2. **Report Defaults** — Report-level style overrides
3. **Named Styles** — Defined styles you can reference
4. **Parent Inheritance** — Container styles flow down
5. **Local Override** — Inline style on specific element

**Result**: One `ResolvedStyle` per element (immutable)

**Example**:
```
Theme:          Font=Arial, Size=12, Color=Black
  ↓
Report Default: Size=14
  ↓
Named Style:    Font=Courier, Bold=True
  ↓
Parent:         Color=Blue
  ↓
Local:          Size=16
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Result:         Font=Courier, Size=16, Bold=True, Color=Blue
```

### 4. Layout Tree

**What it is**: The immutable output of the layout engine.

**Contains**:
- Positioned elements (x, y, width, height known)
- Resolved styles (colors, fonts, borders)
- Page assignments (which page each element is on)
- Clipping information (for overflowing content)

**Properties**:
- ✅ Immutable (can't change after creation)
- ✅ Thread-safe (can be cached and shared)
- ✅ Renderable (exporters consume it)
- ✅ Verifiable (same input → same output)

**Never contains**: Layout engine state, unsized elements, or unresolved styles.

---

## Processing Pipeline: Detailed

### Stage 1: Definition Loading & Parsing

```csharp
// Input: JSON file or string
var json = File.ReadAllText("report.json");

// Parse into ReportDefinition object
var definition = JsonSerializer.Deserialize<ReportDefinition>(json);

// Validate schema version, required fields, etc.
```

**Responsibilities**:
- ✅ Parse JSON syntax
- ✅ Validate schema version compatibility
- ✅ Deserialize into strongly-typed objects
- ✅ Throw clear errors for invalid definitions

### Stage 2: Data Source Resolution

```csharp
var queryRequest = new QueryRequest
{
    DatasetName = "Customers",
    QueryText = "SELECT * FROM Customers WHERE Country = @country",
    Parameters = new Dictionary<string, object>
    {
        { "country", "USA" }
    },
    TimeoutMs = 30000
};

// IDataProvider executes query
var result = await dataProvider.ExecuteAsync(queryRequest);

// Result contains schema and rows
// Schema: columns, types, nullability
// Rows: actual data
```

**Responsibilities**:
- ✅ Accept any data source (SQL, REST, files, etc.)
- ✅ Execute queries with parameters
- ✅ Return structured data with schema
- ✅ Handle errors and timeouts

**Extension Point**: Implement `IDataProvider` for custom sources

### Stage 3: Expression Evaluation

```csharp
// Elements can contain expressions
{
  "content": "{Customer.Name}",           // Binding
  "visible": "=SUM(Invoice.Total) > 0",  // Conditional
  "value": "=TODAY() + 30"                // Computed
}

// IExpressionEvaluator resolves expressions
var customerName = evaluator.Evaluate("{Customer.Name}", context);
var isVisible = evaluator.Evaluate("=SUM(Invoice.Total) > 0", context);
```

**Responsibilities**:
- ✅ Parse expression syntax
- ✅ Evaluate against data context
- ✅ Return results
- ✅ Handle type coercion

### Stage 4: Layout Engine (Measure & Arrange)

**This is the core of KineticReports.** Two-pass algorithm:

#### Pass 1: Measure
```
For each element (depth-first):
  1. Measure content (text, images, etc.)
  2. Determine how much space it needs
  3. Update element size based on content + padding
```

**Purpose**: Determine natural size of elements

#### Pass 2: Arrange
```
For each element (top-down):
  1. Position element at assigned location
  2. Clip to available space
  3. Resolve and apply styles
  4. Break element if it exceeds page bounds
```

**Purpose**: Position elements and handle page breaks

**Example**:

```
Measure Pass:
  TextElement: "Hello World"
    ↓ Measure text with Arial 12pt
    ↓ Result: 89 pixels wide, 16 pixels tall
    
Arrange Pass:
  TextElement at position (100, 50)
    ↓ Text fits on page 1
    ↓ Apply resolved style (Arial, 12pt, Black)
    ↓ Store positioned element in LayoutTree
```

### Stage 5: Pagination

```csharp
// Engine identifies page breaks
var pages = layoutEngine.GetPages();

// Result: Each page contains its elements
// Page 1: Elements that fit
// Page 2: Overflow elements
// Page 3: Continued elements
// ...
```

**Responsibilities**:
- ✅ Determine where content breaks across pages
- ✅ Handle `PageBreakBefore` and `PageBreakAfter`
- ✅ Assign elements to pages
- ✅ Track page numbers for headers/footers

### Stage 6: Rendering (Exporting)

```csharp
// Input: LayoutTree (immutable)
var renderer = new HtmlRenderer();

// Process each element
foreach (var element in layoutTree.Elements)
{
    switch (element)
    {
        case TextElement text:
            output += $"<p>{text.Content}</p>";
            break;
        case ImageElement img:
            output += $"<img src='{img.Source}' />";
            break;
        case TableElement table:
            output += RenderTable(table);
            break;
        // ... other element types
    }
}

// Output: String, bytes, or stream
return output;
```

**Responsibilities**:
- ✅ Consume LayoutTree elements
- ✅ Convert to target format
- ✅ Handle pagination (page breaks, headers, footers)
- ✅ Apply element styles to output format

**Extension Point**: Implement `IRenderer` for custom formats

---

## Component Breakdown

### KineticReports.Core
**Purpose**: Domain model and contracts

**Contains**:
- `Point`, `Size`, `Rect`, `Thickness` — Geometry
- `Color`, `Border`, `Typography` — Styling
- `LayoutElement` base + 12 element types
- `IDataProvider` contract
- `IRenderer` contract
- `ReportDefinition` — Top-level model

**Depends on**: Nothing (no external dependencies)

### KineticReports.Engine
**Purpose**: Orchestration and layout calculation

**Contains**:
- `ReportEngine` — Main orchestrator
- Layout engine (Measure/Arrange passes)
- Pagination engine
- Expression evaluator
- Data resolver

**Depends on**: Core

### KineticReports.Layout
**Purpose**: Layout algorithm implementation

**Contains**:
- `LayoutTree` — Output of layout engine
- Position calculation
- Page break detection
- Element clipping

**Depends on**: Core, Engine

### KineticReports.Rendering
**Purpose**: Abstract rendering base

**Contains**:
- `IRenderer` interface
- `TextRun` — Text measurement
- `ImageReference` — Image handling
- Common rendering utilities

**Depends on**: Core

### KineticReports.Rendering.Skia
**Purpose**: SkiaSharp-based renderer (PDF, PNG, etc.)

**Contains**:
- PDF rendering
- PNG/image rendering
- Font metrics via Skia
- Graphics context

**Depends on**: Core, Rendering, SkiaSharp v4.x

### KineticReports.Export.Html
**Purpose**: HTML exporter

**Contains**:
- HTML generation
- CSS styling
- DOM structure
- Bootstrap integration

**Depends on**: Core

### KineticReports.Export.Excel
**Purpose**: Excel exporter

**Contains**:
- .xlsx generation (Open XML format)
- Cell styling
- Sheet management
- Formulas support

**Depends on**: Core

### KineticReports.Data.SqlServer
**Purpose**: SQL Server data provider

**Contains**:
- SqlServerDataProvider implementation
- Query execution
- Parameter binding
- Type mapping

**Depends on**: Core, Microsoft.Data.SqlClient

### KineticReports.Plugins
**Purpose**: Plugin SDK and manager

**Contains**:
- `IPlugin` interface
- `PluginBase` abstract class
- `DefaultPluginManager` — Runtime plugin loading
- DI registration helpers
- HostedService for auto-load

**Depends on**: Core

---

## Extension Points

### Level 1: Data Providers

**Where**: Provide data to reports

**Implement**: `IDataProvider` interface

```csharp
public interface IDataProvider
{
    Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken);
}
```

**Examples**: SQL Server, PostgreSQL, REST API, MongoDB, CSV, Excel

**Register**:
```csharp
builder.Services.AddSingleton<IDataProvider>(new MyDataProvider());
```

**Best for**: Custom data sources

### Level 2: Renderers (Exporters)

**Where**: Convert LayoutTree to output format

**Implement**: `IRenderer` interface

```csharp
public interface IRenderer
{
    void Render(LayoutTree tree);
}
```

**Examples**: HTML, PDF, Excel, Word, JSON, XML, CSV

**Register**:
```csharp
builder.Services.AddSingleton<IRenderer>(new MyRenderer());
```

**Best for**: New output formats

### Level 3: Plugins

**Where**: Load custom code at runtime

**Implement**: `IPlugin` interface (or extend `PluginBase`)

```csharp
public interface IPlugin
{
    string Id { get; }
    Task InitializeAsync(IServiceProvider services);
    Task UnloadAsync();
}
```

**Features**:
- ✅ Dynamic loading from plugin directory
- ✅ Automatic lifecycle management
- ✅ Access to application services
- ✅ Can provide data providers, renderers, or custom logic

**Register**:
```csharp
var pluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
builder.Services.AddPluginManager(pluginDir);
```

**Best for**: Packaged features, third-party extensions, modular architecture

---

## Design Principles

### 1. Immutability After Layout
Once `LayoutTree` is created, it cannot change. This ensures:
- ✅ Thread safety (can be cached, shared)
- ✅ Reproducibility (same input → same output)
- ✅ Verifiability (can be tested exhaustively)

### 2. Separation of Concerns
- **Definition**: What to report (JSON)
- **Engine**: How to lay it out (layout algorithm)
- **Renderer**: How to output it (format-specific)

Each can be changed independently.

### 3. Language Neutral
- Report definitions are JSON (not C#)
- Can be created by any tool (designer UI, APIs, etc.)
- Can be consumed by any language (.NET, JavaScript, etc.)

### 4. Data Source Agnostic
- Engine doesn't care where data comes from
- Same report can use different data providers
- Supports SQL, REST, files, in-memory, etc.

### 5. Output Format Agnostic
- Same report definition can export to HTML, PDF, Excel, etc.
- Exporters are pluggable
- Easy to add new formats

---

## Execution Model

### Single-Threaded Execution

```
1. ReportEngine.GenerateAsync(definition, dataProvider)
   └─ Sequential processing
      ├─ Load definition
      ├─ Resolve data
      ├─ Evaluate expressions
      ├─ Layout
      ├─ Paginate
      └─ Return LayoutTree
      
2. Renderer.Render(layoutTree)
   └─ Sequential rendering
      └─ Return output (HTML/PDF/Excel)
```

### Multi-Report Concurrency

```
Each report gets its own task:

Task 1: Report A
  └─ Thread pool thread

Task 2: Report B
  └─ Thread pool thread

Task 3: Report C
  └─ Thread pool thread

LayoutTree is immutable, so can be cached/shared
```

### Streaming Output

```
Large reports can stream output:

ReportEngine produces LayoutTree
  ↓
Renderer writes to stream as it processes elements
  ↓
Client receives data chunk-by-chunk
  ↓
Browsers render as data arrives (progressive)
```

---

## Error Handling Strategy

### Three Tiers

**Tier 1: Validation Errors**
- Schema mismatch
- Invalid JSON syntax
- Missing required fields

**Tier 2: Runtime Errors**
- Data source unavailable
- Query timeout
- Invalid expression

**Tier 3: Rendering Errors**
- Unsupported element type
- Font not found
- Image missing

**Handling**: Each tier logs with context, throws exception or gracefully degrades

---

## Performance Characteristics

### Time Complexity

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| Parse JSON | O(n) | n = JSON file size |
| Measure Pass | O(e×m) | e = elements, m = measurements per element |
| Arrange Pass | O(e) | e = elements |
| Pagination | O(p) | p = page count |
| Render HTML | O(e) | e = elements |
| Render PDF | O(e×f) | e = elements, f = font operations |

### Space Complexity

| Component | Space | Notes |
|-----------|-------|-------|
| Definition | O(d) | d = definition size |
| LayoutTree | O(e) | e = positioned elements |
| Page Buffer | O(w×h) | w×h = page dimensions |
| Render Output | O(o) | o = output size |

### Optimization Strategies

1. **Lazy evaluation** — Only process visible elements
2. **Streaming** — Process elements as they're ready
3. **Caching** — Store LayoutTree for repeated rendering
4. **Async I/O** — Don't block on data sources

---

## Thread Safety

### Safe Operations

```csharp
// ReportEngine is stateless (thread-safe)
var engine = new ReportEngine();
Task.Run(() => engine.GenerateAsync(...));  // OK
Task.Run(() => engine.GenerateAsync(...));  // OK

// LayoutTree is immutable (thread-safe)
var task1 = RenderAsync(layoutTree);  // OK
var task2 = RenderAsync(layoutTree);  // OK
```

### Unsafe Operations

```csharp
// ReportDefinition is mutable (not thread-safe for concurrent modification)
definition.Name = "New Name";  // OK for single-threaded
// ... in other thread
var engine = new ReportEngine();
engine.GenerateAsync(definition);  // Unsafe!

// Solution: Clone definition or use immutable patterns
```

---

## Diagrams: Component Interactions

### Basic Flow

```
Application
    │
    ├─ Load ReportDefinition (JSON)
    │
    ├─ Provide IDataProvider
    │
    └─ Call ReportEngine.GenerateAsync()
              │
              ├─ Resolve data
              │
              ├─ Calculate layout
              │
              ├─ Paginate
              │
              └─ Return LayoutTree
                     │
    ┌────────────────┼────────────────┐
    │                │                │
    ▼                ▼                ▼
HtmlRenderer    PdfRenderer     ExcelRenderer
    │                │                │
    ▼                ▼                ▼
output.html    output.pdf      output.xlsx
```

### Plugin Architecture

```
Application Startup
    │
    └─ AddPluginManager(pluginDirectory)
          │
          └─ PluginLoaderHostedService.StartAsync()
               │
               ├─ Scan pluginDirectory for .dll
               │
               ├─ Load each assembly
               │
               ├─ Find IPlugin implementations
               │
               ├─ Create instances
               │
               ├─ Call InitializeAsync()
               │
               └─ Add to LoadedPlugins

Application Running
    │
    └─ Use plugins via IPluginManager
          │
          ├─ Get custom IDataProvider from plugin
          │
          ├─ Use in ReportEngine.GenerateAsync()
          │
          └─ Report uses custom data source

Application Shutdown
    │
    └─ PluginLoaderHostedService.StopAsync()
         │
         ├─ Call UnloadAsync() on each plugin
         │
         └─ Clean up resources
```

---

## Summary

KineticReports is a **multi-stage pipeline**:

```
Definition → Parse → Data → Layout → LayoutTree → Render → Output
  (JSON)     Engine   Resolve  Engine    (immutable)  Engine   (HTML/PDF/Excel)
```

Each stage is independently extensible via:
- **Plugins** — Runtime-loaded custom code
- **DI Registration** — Injected implementations
- **Inheritance** — Extend base classes

The architecture ensures:
- ✅ Separation of concerns
- ✅ Testability
- ✅ Extensibility
- ✅ Deterministic output
- ✅ High performance

---

**Time to read**: ~20 minutes  
**Difficulty**: Intermediate  
**Next guide**: [Core Concepts](03_Core_Concepts.md)
