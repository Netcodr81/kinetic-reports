# Core Concepts

**Objective**: Understand the fundamental building blocks and terminology of KineticReports.

---

## Device Independent Pixels (DIP)

### Definition
A unit of measurement where **1 DIP = 1/96 inch**.

### Why DIPs?
- Independent of screen resolution
- Portable across devices
- Matches industry standards (Windows, web, PDF)
- Human-readable (easy to convert: 96 pixels = 1 inch)

### Examples
```
96 DIPs = 1 inch = 25.4 mm
72 DIPs = 0.75 inch = 19.05 mm
144 DIPs = 1.5 inches = 38.1 mm
```

### In Practice
```json
{
  "x": 96,      // 1 inch from left
  "y": 144,     // 1.5 inches from top
  "width": 480, // 5 inches wide
  "height": 240 // 2.5 inches tall
}
```

All measurements in KineticReports use DIPs internally and externally.

---

## Styles and Style Cascade

### What is a Style?
A collection of visual properties applied to an element:
- Font (family, size, bold, italic, underline)
- Color (text, background, border)
- Spacing (padding, margin)
- Border (style, width, color)
- Alignment (horizontal, vertical)

### Style Cascade (5 Levels)

```
Level 1: Theme (default for all reports)
  ↓ (if not specified)
Level 2: Report Defaults
  ↓ (if not specified)
Level 3: Named Styles (reusable style definitions)
  ↓ (if not specified)
Level 4: Parent Inheritance (container styles flow to children)
  ↓ (if not specified)
Level 5: Local Override (inline style on element)
  ↓
ResolvedStyle (final, immutable result)
```

### Example

```
Theme:
  Font=Arial, Size=12, Color=Black, Bold=false

Report Default:
  Size=14

Named Style "Heading1":
  Font=Courier, Bold=true

Parent Container:
  Color=Blue

Local (on Text element):
  Size=16

───────────────────────────────────────────
ResolvedStyle:
  Font=Courier (from named style)
  Size=16 (from local)
  Bold=true (from named style)
  Color=Blue (from parent)
```

### Defining Styles

```json
{
  "styles": [
    {
      "id": "Heading1",
      "font": {
        "family": "Arial",
        "size": 24,
        "bold": true,
        "italic": false
      },
      "color": "#003366",
      "padding": {
        "top": 12,
        "bottom": 12,
        "left": 0,
        "right": 0
      }
    },
    {
      "id": "BodyText",
      "font": {
        "family": "Calibri",
        "size": 11
      }
    }
  ]
}
```

### Using Styles

```json
{
  "id": "title",
  "type": "Text",
  "content": "My Report",
  "style": "Heading1"  // Reference by ID
}
```

---

## Layout Elements

KineticReports supports **12 built-in element types**:

### 1. Container
Invisible grouping element for organizing content.
```json
{
  "type": "Container",
  "children": [...]  // Can contain other elements
}
```

### 2. Text
Displays text content.
```json
{
  "type": "Text",
  "content": "Hello, World!"
}
```

### 3. Image
Displays an image from URL or embedded data.
```json
{
  "type": "Image",
  "source": "https://example.com/logo.png",
  "width": 100,
  "height": 100
}
```

### 4. Table
Displays tabular data.
```json
{
  "type": "Table",
  "columns": [...],
  "rows": [...]
}
```

### 5. Band
Repeating section for hierarchical data (groups).
```json
{
  "type": "Band",
  "datasetName": "Customers",
  "children": [...]  // Repeats for each row
}
```

### 6. Chart
Displays data visualization (bar, line, pie, etc.).
```json
{
  "type": "Chart",
  "chartType": "Bar",
  "datasetName": "Sales"
}
```

### 7. Shape
Draws geometric shapes (rectangles, circles, polygons).
```json
{
  "type": "Shape",
  "shapeType": "Rectangle"
}
```

### 8. Line
Draws a line between two points.
```json
{
  "type": "Line",
  "startX": 0,
  "startY": 0,
  "endX": 100,
  "endY": 100
}
```

### 9. Rectangle
Draws a filled rectangle.
```json
{
  "type": "Rectangle",
  "width": 200,
  "height": 100
}
```

### 10. Ellipse
Draws a filled ellipse/circle.
```json
{
  "type": "Ellipse",
  "width": 100,
  "height": 100
}
```

### 11. Path
Draws a custom vector path.
```json
{
  "type": "Path",
  "pathData": "M 10 10 L 100 100 Q 200 10 300 100"
}
```

### 12. Page
Container for a single page in the report.
```json
{
  "type": "Page",
  "width": 612,   // 8.5 inches (Letter width)
  "height": 792,  // 11 inches (Letter height)
  "body": {...}   // Page content
}
```

---

## LayoutTree

### What is it?
The **immutable output** of the layout engine.

Contains all positioned elements ready to render.

### Properties

```csharp
public sealed record LayoutTree
{
    // All positioned elements
    public required IReadOnlyList<LayoutElement> Elements { get; init; }
    
    // Page dimensions
    public required double DocumentWidth { get; init; }
    public required double DocumentHeight { get; init; }
    
    // Page assignments
    public required IReadOnlyList<PageInfo> Pages { get; init; }
}
```

### What It Contains
- ✅ Position (x, y) — Where element is placed
- ✅ Size (width, height) — Measured size
- ✅ Page assignment — Which page element is on
- ✅ Resolved styles — Final colors, fonts, borders
- ✅ Clipping info — What's visible vs overflowing

### What It Does NOT Contain
- ❌ Layout engine state — Already calculated
- ❌ Unmeasured content — All sizes are final
- ❌ Unresolved styles — All styles are resolved
- ❌ Raw data — Only laid-out elements

### Why Immutable?
```csharp
// Renderers can process in parallel
Task.Run(() => htmlRenderer.Render(layoutTree));
Task.Run(() => pdfRenderer.Render(layoutTree));
Task.Run(() => excelRenderer.Render(layoutTree));
// No synchronization needed — tree never changes
```

---

## Data Binding and Expressions

### Binding Syntax
Reference data from queries using curly braces:

```json
{
  "type": "Text",
  "content": "{Customer.Name}"
}
```

At runtime, replaces `{Customer.Name}` with actual customer name from data.

### Expression Syntax
Evaluate expressions using `=` prefix:

```json
{
  "type": "Text",
  "content": "=Customer.FirstName + \" \" + Customer.LastName"
}
```

Supports:
- Property access: `Customer.Name`
- Arithmetic: `Total * 1.1`, `100 - Tax`
- Comparison: `Amount > 1000`, `Status == "Active"`
- Functions: `SUM(Invoice.Total)`, `TODAY()`

### Context
Expressions evaluate against the current data context:

```
If processing Band "Customers":
  Current context is one Customer row
  {FirstName} resolves to current customer's first name
  
If processing Text element:
  Context is parent Band or page context
```

---

## Pagination

### What is Pagination?
Splitting content across multiple pages based on page size.

### How It Works
1. **Layout Engine** positions all elements
2. **Pagination Engine** checks which page each element fits on
3. **Page Breaks** split overflowing content
4. **LayoutTree** assigns each element to a page

### Page Size Examples
```
Letter:        612 × 792 DIPs (8.5" × 11")
A4:            595 × 842 DIPs (210mm × 297mm)
Legal:         612 × 1008 DIPs (8.5" × 14")
Tabloid:       792 × 1224 DIPs (11" × 17")
Custom:        Any width and height
```

### Page Breaks
```json
{
  "type": "Container",
  "breakBefore": true,    // Force page break before element
  "breakAfter": true,     // Force page break after element
  "orphanControl": true,  // Keep at least 2 lines on new page
  "widowControl": true    // Don't split content across pages
}
```

---

## Rendering and Export

### Rendering Process

```
LayoutTree
  ↓
Renderer processes each element
  ├─ Text element → "output text"
  ├─ Image element → embed image
  ├─ Table element → format rows/columns
  └─ ... (12 element types)
  ↓
Output (HTML, PDF, Excel, etc.)
```

### Common Output Formats

| Format | Exporter | Best For |
|--------|----------|----------|
| **HTML** | HtmlExporter | Web browsers, email |
| **PDF** | PdfRenderer | Print, archival, sharing |
| **Excel** | ExcelExporter | Data analysis, spreadsheets |
| **Custom** | IRenderer impl. | Domain-specific formats |

### Renderers Never Layout
Critical principle:
```
Renderer's job:
  ✅ Convert positioned elements to output format
  ❌ Does NOT calculate positions (already done)
  ❌ Does NOT make layout decisions
```

---

## Coordinates and Bounds

### Coordinate System
```
(0, 0) at top-left
X increases left to right
Y increases top to bottom
```

### Bounds Representation
```csharp
public record Bounds(double X, double Y, double Width, double Height);
```

Example:
```json
{
  "x": 100,      // 100 DIPs from left
  "y": 50,       // 50 DIPs from top
  "width": 200,  // 200 DIPs wide
  "height": 30   // 30 DIPs tall
}
```

### Nested Coordinates
Child elements have coordinates relative to parent:
```
Parent at (100, 100) size (200, 200)
  Child at (50, 50) — Absolute position is (150, 150)
  Child at (0, 0) — Absolute position is (100, 100) [top-left of parent]
```

---

## Theme and Defaults

### Theme
Global default styles for the entire report:
```json
{
  "theme": {
    "font": {
      "family": "Arial",
      "size": 12,
      "color": "#000000"
    },
    "pageSize": "Letter"
  }
}
```

All elements inherit from theme unless overridden.

### Report Defaults
Report-level style overrides:
```json
{
  "reportDefaults": {
    "fontFamily": "Calibri",
    "fontSize": 14
  }
}
```

---

## Key Terminology

| Term | Definition |
|------|-----------|
| **DIP** | Device Independent Pixel (1/96 inch) |
| **ReportDefinition** | JSON document describing report structure |
| **LayoutTree** | Immutable positioned elements ready to render |
| **Element** | Visual component (Text, Table, Image, etc.) |
| **Style** | Collection of visual properties (font, color, etc.) |
| **Binding** | Reference to data via `{DatasetName.Field}` |
| **Expression** | Computed value via `=formula` |
| **Pagination** | Splitting content across pages |
| **Renderer** | Converts LayoutTree to output format |
| **IDataProvider** | Interface for data sources |
| **IRenderer** | Interface for output formats |
| **IPlugin** | Interface for loadable extensions |

---

## Mental Models

### The Pipeline
```
Definition (JSON) + Data (IDataProvider)
  → ReportEngine
  → LayoutTree (immutable)
  → Renderer (IRenderer)
  → Output (HTML/PDF/Excel/...)
```

### The Cascade
```
Theme → Report Default → Named Style → Parent → Local
  ↓         ↓               ↓           ↓       ↓
  All    Whole Report    Reusable   Container Individual
  →  →  →  →  →  →  →  →  →  →  →  →  →  →  →
                ResolvedStyle
```

### The Tree
```
ReportDefinition
  ├─ Page 1
  │  ├─ Header
  │  ├─ Body
  │  │  ├─ Container
  │  │  │  ├─ Text
  │  │  │  ├─ Table
  │  │  │  └─ Image
  │  │  └─ Band (repeats)
  │  └─ Footer
  └─ (Pagination adds more pages)
```

---

**Time to read**: ~15 minutes  
**Difficulty**: Beginner-Intermediate  
**Next guide**: [Report Definitions](04_Report_Definitions.md)
