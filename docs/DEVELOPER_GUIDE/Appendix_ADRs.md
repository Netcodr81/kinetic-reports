# Appendix: Architectural Decision Records (ADRs)

**Purpose**: Document major design decisions, their rationale, and alternatives considered.

Each ADR follows the format: **Title → Problem → Decision → Rationale → Alternatives**

---

## ADR-001: Device Independent Pixels (DIP)

**Problem**: Different devices have different screen densities (96 DPI, 150 DPI, 300 DPI, etc.). How should coordinates be specified to ensure consistent output?

**Decision**: All measurements use Device Independent Pixels where 1 DIP = 1/96 inch.

**Rationale**:
- ✅ Industry standard (Windows Forms, WPF, CSS)
- ✅ Predictable across screens and printers
- ✅ Human-readable (1 inch = 96 pixels)
- ✅ Matches CSS/web standards
- ✅ Easy conversion to physical units (mm, cm, pt, in)

**Alternatives Considered**:
- Pixels (device-dependent) — Would vary by DPI
- Millimeters (physical) — Would lose precision on screen
- Screen coordinates — Not portable across devices

**Impact**: All layout calculations use DIPs internally and externally.

---

## ADR-002: JSON as Report Definition Format

**Problem**: How should report definitions be stored and transmitted?

**Decision**: Use JSON as the only persisted report format.

**Rationale**:
- ✅ Language-neutral (can be created/consumed by any language)
- ✅ Human-readable (can edit in text editor)
- ✅ Web-friendly (standard `application/json` MIME type)
- ✅ Version-friendly (easy to diff in version control)
- ✅ Tool support (JSON Schema, validators, editors)
- ✅ No lock-in (no proprietary binary format)

**Alternatives Considered**:
- XML — Too verbose, harder to read
- Binary format — Not human-readable, harder to version
- YAML — Not standard for .NET ecosystem
- Protocol Buffers — Overkill for this use case

**Impact**: All reports are `.json` files. Designers create JSON directly or via tools.

---

## ADR-003: Closed Set of Layout Element Types

**Problem**: Should KineticReports support unlimited custom element types?

**Decision**: Support a fixed set of 12 element types. No custom elements.

**The 12 Elements**:
1. Page
2. Container
3. Text
4. Image
5. Table
6. Band
7. Chart
8. Shape
9. Line
10. Rectangle
11. Ellipse
12. Path

**Rationale**:
- ✅ Covers 99% of reporting use cases
- ✅ Simplifies the layout engine
- ✅ Makes exporters simpler to write
- ✅ Ensures cross-platform compatibility
- ✅ Easier to test and maintain

**Alternatives Considered**:
- Infinite custom elements — Would require plugin architecture for layout
- Nested containers only — Too limiting

**Impact**: Reports use combinations of these 12 types. Use containers and groups for complex layouts.

---

## ADR-004: Immutability After Layout

**Problem**: After layout is complete, should elements be mutable?

**Decision**: LayoutTree and all elements are immutable after Arrange pass.

**Rationale**:
- ✅ Thread-safe (can cache and share across threads)
- ✅ Reproducible (same input always produces same output)
- ✅ Verifiable (can exhaustively test)
- ✅ Enables parallel rendering
- ✅ Facilitates caching strategies

**Alternatives Considered**:
- Mutable — Would require synchronization, complicate caching
- Copy-on-write — Adds complexity without clear benefit

**Implementation**:
```csharp
public sealed record LayoutTree
{
    public required IReadOnlyList<LayoutElement> Elements { get; init; }
    // All properties are readonly/init-only
}
```

**Impact**: Exporters never modify LayoutTree. Layout engine is the only producer.

---

## ADR-005: Expression Language Features

**Problem**: What subset of language features should be supported in expressions?

**Decision**: Support property access, operators, and a small set of built-in functions.

**Supported**:
- ✅ Property access: `{Customer.Name}`
- ✅ Arithmetic: `=Total * 1.1`
- ✅ Comparison: `=Amount > 100`
- ✅ String concat: `=FirstName + " " + LastName`
- ✅ Functions: `SUM()`, `AVG()`, `TODAY()`, `IF()`, etc.

**Not Supported**:
- ❌ Method calls: `{Customer.GetName()}`
- ❌ Complex control flow: `for`, `while`, `switch`
- ❌ Lambda expressions
- ❌ LINQ queries

**Rationale**:
- ✅ Ensures deterministic evaluation
- ✅ Prevents infinite loops or side effects
- ✅ Easy to validate and optimize
- ✅ Portable across platforms
- ✅ Straightforward parsing/evaluation

**Alternatives Considered**:
- Full C# — Would be unsafe and non-deterministic
- JavaScript-like syntax — Adds complexity

**Impact**: Report designers use expressions for bindings, computed fields, and conditionals.

---

## ADR-006: Separated Layout and Rendering

**Problem**: Should layout and rendering be combined or separate?

**Decision**: Completely separate concerns.

**Layout Engine Responsibility**:
- Measure text size
- Position elements
- Handle pagination
- Output: LayoutTree (immutable, abstract)

**Renderer Responsibility**:
- Convert LayoutTree to output format
- Apply format-specific optimizations
- Output: HTML, PDF, Excel, etc.

**Rationale**:
- ✅ Same report can output to multiple formats without recalculating layout
- ✅ Renderers don't need to know about layout algorithm
- ✅ Layout engine doesn't know about output formats
- ✅ Easy to add new renderers
- ✅ Easier to test each component independently

**Alternatives Considered**:
- Combined — Would require each renderer to implement layout

**Impact**: LayoutTree is the contract between layout and rendering.

---

## ADR-007: Deterministic Output

**Problem**: Should report output be deterministic (same input = same output)?

**Decision**: Yes. Determinism is a core requirement.

**Guarantees**:
- ✅ Same report definition + same data = bit-for-bit identical output
- ✅ No random variation
- ✅ No floating-point errors
- ✅ No timing dependencies

**Implications**:
- All calculations use deterministic algorithms
- No DateTime.Now (use provided TimeProvider)
- No random number generation
- All fonts pre-specified (no system font fallbacks)
- Coordinate calculations use fixed-point arithmetic where needed

**Rationale**:
- ✅ Enables automated testing
- ✅ Enables output caching
- ✅ Enables reproducible bug fixes
- ✅ Meets compliance requirements
- ✅ Facilitates version control of outputs

**Alternatives Considered**:
- Non-deterministic — Would be unpredictable, hard to test

**Impact**: Developers must be careful with randomness, dates, file I/O, etc.

---

## ADR-008: Plugin Architecture

**Problem**: How should extensions be loaded and managed?

**Decision**: Use a plugin architecture with IPlugin interface and PluginManager.

**Features**:
- ✅ Runtime discovery from plugin directory
- ✅ Assembly loading via reflection
- ✅ Lifecycle management (Init/Unload)
- ✅ Access to application services via IServiceProvider
- ✅ Error resilience (plugin load failures don't crash app)

**Rationale**:
- ✅ Decoupled from main application
- ✅ Third-party plugins can extend functionality
- ✅ Hot-loading possible without recompile
- ✅ Easy to distribute plugins
- ✅ Works with .NET dependency injection

**Alternatives Considered**:
- Static registration only — Less flexible
- MEF — Overkill for this use case
- Custom reflection — Reinvents the wheel

**Implementation**:
```csharp
public interface IPlugin
{
    string Id { get; }
    Task InitializeAsync(IServiceProvider services);
    Task UnloadAsync();
}

public class DefaultPluginManager : IPluginManager
{
    // Discovers and loads plugins
}
```

**Impact**: Applications can load plugins at runtime via plugin directory.

---

## ADR-009: Data Source Abstraction

**Problem**: How should data sources be abstracted to support multiple backends?

**Decision**: Use IDataProvider interface with QueryRequest/QueryResult.

**Contract**:
```csharp
public interface IDataProvider
{
    Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken);
}
```

**Rationale**:
- ✅ Supports any data source (SQL, REST, files, etc.)
- ✅ Asynchronous for scalability
- ✅ Parameterized queries for safety
- ✅ Structured results with schema
- ✅ Timeout and cancellation support

**Alternatives Considered**:
- Direct LINQ — Would be tied to in-memory data
- Raw SQL strings — Would be unsafe (SQL injection)
- Proprietary interface — Would limit adoption

**Impact**: Reports can use any data source by implementing IDataProvider.

---

## ADR-010: Export Pipeline

**Problem**: How should reports be exported to different formats?

**Decision**: Use IRenderer interface. All renderers consume immutable LayoutTree.

**Contract**:
```csharp
public interface IRenderer
{
    void Render(LayoutTree tree);
}
```

**Rationale**:
- ✅ Layout is calculated once, rendered multiple times
- ✅ Easy to add new exporters
- ✅ Exporters don't duplicate layout logic
- ✅ Supports streaming output
- ✅ Format-agnostic layout engine

**Alternatives Considered**:
- Tight coupling — Each format would implement layout
- Template-based — Limited customization

**Implementation Examples**:
- HtmlRenderer — Outputs HTML
- PdfRenderer — Uses SkiaSharp for PDF
- ExcelRenderer — Uses OpenXml for .xlsx
- Custom renderers — JSON, XML, CSV, etc.

**Impact**: Easy to add support for new output formats.

---

## ADR-011: Layout Tree Immutability

**Problem**: Should LayoutTree be mutable during rendering?

**Decision**: LayoutTree is completely immutable. Renderers cannot modify it.

**Guarantee**:
```csharp
public sealed record LayoutTree
{
    public required IReadOnlyList<LayoutElement> Elements { get; init; }
    // No setter methods, all properties are init-only or get-only
}
```

**Rationale**:
- ✅ Renderers work independently without side effects
- ✅ Multiple renderers can process same tree in parallel
- ✅ Output is verifiable and reproducible
- ✅ Caching strategies are safe

**Alternatives Considered**:
- Mutable — Would require locking, reduce parallelism
- Copy-on-write — Unnecessary complexity

**Impact**: Renderers must compute output without modifying LayoutTree.

---

## ADR-012: Pagination After Arrange

**Problem**: When should pagination (page breaks) be determined?

**Decision**: Pagination occurs after the Arrange pass, not before.

**Pipeline**:
```
Measure (determine sizes)
  ↓
Arrange (position elements)
  ↓
Paginate (split into pages)
  ↓
Render
```

**Rationale**:
- ✅ All elements positioned before pagination decisions
- ✅ Allows smarter page break logic
- ✅ Handles widow/orphan control
- ✅ Cleaner algorithm (no recursive reflows)

**Alternatives Considered**:
- Pagination before arrange — Would require multiple passes
- Pagination during arrange — Would increase complexity

**Impact**: Layout algorithm has three distinct stages. Pages are assigned during pagination, not layout.

---

## ADR-013: Exporters Consume LayoutTree Only

**Problem**: Should exporters access report definition or only LayoutTree?

**Decision**: Exporters consume only the immutable LayoutTree.

**Consequence**:
- All layout decisions are final and embedded in LayoutTree
- Exporters never reflow or re-layout content
- Exporters have no access to report definition
- Exporters have no access to raw data

**Rationale**:
- ✅ Exporters are simpler (no layout logic needed)
- ✅ Output is consistent across exporters
- ✅ Exporters can focus on format-specific details
- ✅ Easier to parallelize rendering

**Alternatives Considered**:
- Exporters get definition + tree — Would allow different layouts per format

**Implementation**:
```csharp
public class HtmlExporter
{
    public string Export(LayoutTree tree)  // Tree only, no definition
    {
        // Process tree, generate HTML
    }
}
```

**Impact**: All exporters work from the same LayoutTree.

---

## ADR-014: Coordinate System

**Problem**: What coordinate system should be used for layout?

**Decision**: Standard Cartesian coordinate system with origin at top-left (screen coordinates).

```
(0, 0) ─────────────────▶ X
  │
  │
  ▼
  Y
```

**Rationale**:
- ✅ Matches screen coordinate systems
- ✅ Matches PDF/SVG coordinate systems
- ✅ Easier for implementers to reason about
- ✅ No confusion with mathematical coordinate system

**Alternatives Considered**:
- Mathematical (origin at bottom-left) — Counterintuitive for UI
- Centered origin — Complicates calculations

**Measurements**:
- X increases left-to-right
- Y increases top-to-bottom
- Width and height are positive
- All units are DIPs (Device Independent Pixels)

**Impact**: All coordinate calculations use screen-based coordinates.

---

## ADR-015: Centralized Font Metrics

**Problem**: How should font measurements be handled across the system?

**Decision**: All font metrics are calculated centrally via IFontMetrics interface.

```csharp
public interface IFontMetrics
{
    SizeF MeasureText(string text, FontDescriptor font);
    float GetLineHeight(FontDescriptor font);
    float GetAscentHeight(FontDescriptor font);
}
```

**Implementations**:
- SkiaFontMetrics (using SkiaSharp)
- Custom implementations possible

**Rationale**:
- ✅ Consistent measurements throughout system
- ✅ Easy to swap implementations
- ✅ Testable with mock metrics
- ✅ No platform-specific code in layout engine
- ✅ Enables different rendering backends

**Alternatives Considered**:
- Embedded in renderer — Would create duplication
- Platform-specific — Would reduce portability

**Impact**: Renderers and layout engine both use same IFontMetrics for consistency.

---

## ADR-016: JSON as Only Persisted Format

**Problem**: Should KineticReports support multiple file formats?

**Decision**: JSON is the only persisted report format. No binary, XML, or proprietary formats.

**Rationale**:
- ✅ Single format to support reduces complexity
- ✅ Version control friendly
- ✅ Human readable
- ✅ Tool independent
- ✅ Minimizes test matrix

**Alternatives Considered**:
- Multiple formats — Would require converters, increase testing
- Binary format — Would lose portability benefits

**Implementation**:
- Reports stored as `.json` files
- Can be versioned in Git
- Can be edited in any text editor
- Can be generated by any tool

**Impact**: No `.krrpt` or proprietary binary format. Just JSON.

---

## ADR-017: Layout Data Never Serialized

**Problem**: Should the LayoutTree (output of layout engine) be persisted?

**Decision**: No. Layout data is never serialized to disk.

**Why?**:
- ✅ Layout is always recalculated from definition + data
- ✅ Prevents stale layout data
- ✅ Ensures deterministic output
- ✅ Reduces storage requirements

**What is Serialized**:
- ✅ ReportDefinition (JSON)
- ✅ Data (stored separately)
- ❌ LayoutTree (calculated on-demand)
- ❌ Resolved styles (calculated on-demand)

**Rationale**: Layout and data are inputs; LayoutTree is a derived artifact that should be computed fresh.

**Impact**: Reports are always rendered fresh. No caching of layout trees to disk.

---

## ADR-018: Unknown Properties Preserved

**Problem**: Should unknown JSON properties be preserved during round-trip?

**Decision**: Yes. Unknown properties are preserved during serialization/deserialization.

**Rationale**:
- ✅ Enables forward compatibility
- ✅ Allows gradual schema evolution
- ✅ Doesn't break when schema adds new fields
- ✅ Preserves custom extensions

**Implementation**:
```csharp
[JsonSerializerOptions(
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
)]
public class ReportDefinition
{
    // Fields...
}
```

**Impact**: Can upgrade library version without losing custom JSON properties.

---

## ADR-019: Stable Identifiers

**Problem**: Should elements have stable, unique identifiers?

**Decision**: Yes. All elements require a stable `id` property.

**Rules**:
- ✅ Every element has an `id`
- ✅ IDs are immutable within a report
- ✅ IDs are unique within a report
- ✅ IDs enable references and queries

**Rationale**:
- ✅ Enables element references (e.g., hyperlinks)
- ✅ Supports conditional visibility
- ✅ Enables incremental updates
- ✅ Facilitates debugging

**Implementation**:
```json
{
  "id": "customer_name",
  "type": "Text",
  "content": "{Customer.Name}"
}
```

**Impact**: Designers must assign stable IDs. Enables references between elements.

---

## ADR-020: Schema and Expression Versions Evolve Independently

**Problem**: How should versioning work for schema and expression language?

**Decision**: Schema version and expression language version evolve independently.

**Properties**:
```json
{
  "schemaVersion": "1.0",
  "expressionLanguageVersion": "1.0"
}
```

**Rationale**:
- ✅ Layout schema can evolve independently of expressions
- ✅ New element types don't require expression changes
- ✅ Allows incremental feature releases
- ✅ Clearer versioning semantics

**Alternatives Considered**:
- Single version — Would couple unrelated features

**Impact**: Report definitions can specify which schema/expression versions they require.

---

## ADR-021: Page-Based Rendering Model

**Problem**: Should KineticReports support continuous or page-based output?

**Decision**: Page-based model. All output is organized into discrete pages.

**Implications**:
- ✅ Each page has fixed dimensions (e.g., Letter: 8.5" × 11")
- ✅ Headers and footers repeat on each page
- ✅ Page numbers are calculated
- ✅ Page breaks are explicit

**Rationale**:
- ✅ Matches physical world (printer, paper)
- ✅ Supports both print and screen output
- ✅ Simpler pagination logic
- ✅ Aligns with PDF, Excel, Word models

**Alternatives Considered**:
- Continuous scroll — Would lose print compatibility
- Flexible pages — Would complicate layout

**Impact**: Reports are organized by pages. Exporters generate multi-page output.

---

## Summary: Design Pillars

These ADRs establish five core design pillars:

| Pillar | ADRs | Benefit |
|--------|------|---------|
| **Determinism** | 007 | Same input → Same output, always |
| **Immutability** | 004, 011, 017 | Thread-safe, cacheable, verifiable |
| **Separation of Concerns** | 006, 009, 010, 013 | Independent evolution of components |
| **Extensibility** | 003, 008, 009, 010, 018 | Plugins, custom providers, custom exporters |
| **Portability** | 001, 002, 005, 014 | Cross-platform, language-neutral, human-readable |

---

**Last Updated**: 2026-07-20  
**KineticReports Version**: 1.0.0-alpha  
**.NET Target**: 10.0
