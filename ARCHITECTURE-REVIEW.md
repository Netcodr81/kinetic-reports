# KineticReports.Core Architecture Review
**Prepared for:** Standalone/Web Report Designer Planning  
**Date:** 2026-07-29  
**Scope:** Maintainability, Cognitive Complexity, and Designer Readiness

---

## Executive Summary

**Overall Assessment:** ⭐⭐⭐⭐ (4/5 stars)

KineticReports.Core is well-architected with excellent separation of concerns and clean abstractions. The codebase is **maintainable and extensible**, but exhibits **moderate-to-high cognitive complexity** in three areas:

1. **Expression evaluation and data binding** (implicit context threading)
2. **Block type proliferation** (12 concrete types, multiple inheritance patterns)
3. **Styling cascade resolution** (6-step priority order is correct but not self-documenting)

The foundation is **ready for designer integration**, with existing authoring infrastructure already in place (`ReportComponentCatalog`, `DesignerDocumentCompiler`). However, several **critical features are missing** for a production report designer.

---

## Part 1: Maintainability & Cognitive Complexity Assessment

### 1.1 Strengths 🟢

#### Clean Layered Architecture
| Layer | Purpose | Quality |
|-------|---------|---------|
| Definition | Immutable models (ReportDefinition, DataSourceDefinition, etc.) | Excellent - record types, clear contracts |
| Engine | Orchestration and data resolution | Very Good - IReportEngine abstraction, fluent builder |
| Layout | Measurement and arrangement passes | Excellent - Measure/Arrange pattern, immutable results (ADR-011) |
| Rendering | Output generation | Very Good - IRenderer abstraction, multiple backends |
| Authoring | Design-time compilation and visual representation | Good - Component catalog, compilation pipeline |
| Styling | Cascade resolution and typography | Good - AppliedStyle immutability, but cascade is implicit |

**Key Advantage:** Each layer is independently testable and replaceable.

#### Strong Abstraction Boundaries
- `IReportEngine` (single responsibility: orchestration)
- `IRenderer` (single responsibility: rendering, no layout)
- `IDataResolver` (single responsibility: data fetching)
- `IExpressionEvaluator` (single responsibility: expression evaluation)
- `ILayoutEngine` (single responsibility: measurement/arrangement)

Each abstraction is narrow, focused, and easy to mock in tests.

#### Immutability Guarantees
- `ReportDefinition` is sealed record (immutable by design)
- `ReportDocument` is immutable (output of layout engine)
- `LayoutBlock` and subclasses are sealed (immutable after Arrange pass)
- `AppliedStyle` is immutable (reduces styling bugs)

**Result:** Thread-safe by default, fewer race conditions, easier reasoning about state.

---

### 1.2 Cognitive Complexity Pain Points 🟠

#### Pain Point 1: Expression Context Threading (High Complexity)

**Where:** `IExpressionEvaluator.Evaluate(expression, ExpressionContext context)`

**Problem:** The `ExpressionContext` is implicitly threaded through the execution pipeline. To understand how a binding works, you must trace:
1. Where context is created
2. Where context is passed to the evaluator
3. What fields are in context (current row, parameters, page state)
4. What syntax is supported (`{FieldName}`, literals, etc.)

**Code Path Example:**
```
DefaultReportBuilder.AddDataSourceTable()
  → AddStep(DataContext, IExpressionEvaluator)
    → Evaluate(expression, new ExpressionContext(row, parameters, ...))
      → Match {FieldName}, return row[FieldName]
```

**Why It's Complex:**
- Context is built in multiple places (ProviderDataSourceResolver, DefaultReportBuilder, ExpressionContext constructor)
- No single place shows all available context fields
- Implicit parameter passing (not via constructor injection)
- Error handling is deferred (returns null on failure, not exception)

**Cognitive Load:** **Medium-High** (requires understanding 5+ code paths)

**Recommendation:** See Section 2.2 (Simplification) for mitigation.

---

#### Pain Point 2: Block Type Proliferation (Medium Complexity)

**Where:** `Layout/` namespace contains 12 concrete block types

**The Hierarchy:**
```
LayoutBlock (abstract)
├── TextBlock
├── ImageBlock
├── TableBlock
├── ContainerBlock
├── RowBlock
├── CellBlock
├── PageBlock
├── PageSectionBlock
├── ShapeBlock (Rectangle, Ellipse, Line via PathGeometry)
├── ChartBlock
├── BarcodeBlock
└── ReportBlock (abstract)
    ├── HeaderBlock
    ├── DetailBlock
    ├── FooterBlock
    ├── PageHeaderBlock
    ├── PageFooterBlock
    ├── GroupHeaderBlock
    └── GroupFooterBlock
```

**Why It's Complex:**
- ReportBlock adds another inheritance layer (why not just LayoutBlock types?)
- Shapes are consolidated into ShapeBlock but use PathGeometry discriminator
- Charts and Barcodes are separate types despite sharing container mechanics
- New block type = 3+ files (definition, layout, rendering) + tests

**Cognitive Load:** **Medium** (requires understanding 3-level hierarchy, 12 types, and their Measure/Arrange contracts)

**Recommendation:** See Section 2.3 (Simplification) for consolidation strategy.

---

#### Pain Point 3: Styling Cascade (Medium Complexity)

**Where:** `Styling/` namespace, style resolution in DefaultReportBuilder

**The 6-Step Cascade (spec §10):**
1. Theme defaults (not yet implemented)
2. Report defaults (DefaultReportBuilder styles)
3. Named style reference (StyleDefinition.Id)
4. Parent inheritance (from container)
5. Local override (block-specific AppliedStyle)
6. Final AppliedStyle (immutable result)

**Why It's Complex:**
- The cascade is documented in spec §10, not in code comments
- No single class orchestrates cascade resolution
- Cascade order is enforced implicitly across multiple methods
- Adding a new cascade level requires changes in multiple places
- Style inheritance is not type-safe (string-based style ID references)

**Cognitive Load:** **Medium** (requires understanding order + looking up spec)

**Recommendation:** See Section 2.4 (Simplification) for StyleCascadeResolver pattern.

---

#### Pain Point 4: Builder API Fluency (Low-Medium Complexity)

**Where:** `DefaultReportBuilder` (15 fluent methods)

**Issue:** Methods have varied signatures:
- `AddTextRegion(id, text, style?)` — style is optional
- `AddDataSourceTable(id, dataSourceId, columns, style?)` — columns required
- `AddStep(func)` — callback-based, requires DataContext + IExpressionEvaluator knowledge

New developers don't immediately know:
- Which methods auto-create styles vs. require explicit styles
- How AddStep interacts with data context
- When to use AddBlock vs. AddRegion methods

**Cognitive Load:** **Low-Medium** (learning curve ~1-2 hours)

**Recommendation:** Document with examples; see Section 2.5 for builder documentation template.

---

### 1.3 Code Organization Quality

| Aspect | Rating | Notes |
|--------|--------|-------|
| Namespace hierarchy | ⭐⭐⭐⭐⭐ | Clear domain layers: Definition, Engine, Layout, Rendering, Styling, etc. |
| File organization | ⭐⭐⭐⭐ | Each class in its own file; exception: Shapes consolidated in ShapeBlock |
| API surface | ⭐⭐⭐⭐ | Good use of sealed classes, record types, required properties |
| Discoverability | ⭐⭐⭐ | Requires documentation; interfaces are well-named but contracts need examples |
| Test coverage | ⭐⭐⭐⭐ | Comprehensive unit tests for core types (geometry, styling, layout) |
| Documentation | ⭐⭐⭐ | XML doc comments present; high-level architecture docs good; low-level examples rare |

**Verdict:** Well-organized, but **lacks inline examples and cookbook patterns**.

---

## Part 2: Simplification Opportunities

### 2.1 Priority Matrix

| Simplification | Effort | Impact | Priority | Type |
|---|---|---|---|---|
| StyleCascadeResolver class | Medium | High | 1 | Refactor |
| ExpressionContext documentation + examples | Low | Medium | 2 | Documentation |
| Consolidate shapes into ShapeType enum | Medium | Medium | 3 | Refactor |
| Expose common block patterns as helpers | Low | High | 4 | Additions |
| Add DefaultLayoutOptions factory | Low | Medium | 5 | Addition |

---

### 2.2 Simplification #1: ExpressionContext Transparency

**Current State:**
Expression evaluation is "magic" — context is threaded implicitly, field resolution is not self-documenting.

**Goal:**
Make expression binding obvious to new developers.

**Changes:**

1. **Document ExpressionContext** (in existing file or new XML doc):
```csharp
namespace KineticReports.Core.Engine.Expressions;

/// <summary>
/// The evaluation context for a single expression evaluation.
/// Supplies the current data row, report parameters, and page/group state.
/// </summary>
/// <remarks>
/// Available to expressions:
/// - Data fields: Use {FieldName} syntax to reference current row values
///   Example: {OrderDate}, {Customer.Name}
/// - Parameters: Use {ParameterName} to reference report parameters
///   Example: {StartDate}, {ReportTitle}
/// - Special fields:
///   - {PageNumber}: Current page number (1-indexed)
///   - {TotalPages}: Total page count (available after pagination)
///   - {RowNumber}: Row index in current data source (1-indexed)
/// 
/// Fallback: If a field is not found, the expression evaluator returns null.
/// </remarks>
public sealed record ExpressionContext
{
    public required IReadOnlyDictionary<string, object?> Row { get; init; }
    public required IReadOnlyDictionary<string, object?> Parameters { get; init; }
    public required int PageNumber { get; init; }
    public required int TotalPages { get; init; }
    public required int RowNumber { get; init; }
}
```

2. **Add built-in expression evaluator tests** showing all 5 contexts:
```csharp
[Theory]
[InlineData("{OrderDate}", new { OrderDate = "2026-01-01" }, null, "2026-01-01")]
[InlineData("{StartDate}", null, new { StartDate = "2026-01-01" }, "2026-01-01")]
[InlineData("{PageNumber}", null, null, "1")]
[InlineData("{TotalPages}", null, null, "10")]
public void Evaluate_WithVariousContextFields_ResolvesCorrectly(
    string expression, IDictionary<string, object> row, IDictionary<string, object> parameters, string expected)
{
    // Test...
}
```

3. **Create IExpressionContext interface** (optional but helpful):
```csharp
public interface IExpressionContextFactory
{
    ExpressionContext CreateFromDataRow(IReadOnlyDictionary<string, object?> row, 
        IReadOnlyDictionary<string, object?> parameters, int pageNumber, int totalPages, int rowNumber);
}
```

**Impact:** New developers understand data binding in 5 minutes instead of 1 hour.

---

### 2.3 Simplification #2: Consolidate Block Types via Strategy Pattern

**Current State:**
- 12 concrete block types, some with overlapping concerns
- ShapeBlock uses PathGeometry discriminator (not discoverable)
- ReportBlock adds unnecessary inheritance layer

**Goal:**
Reduce cognitive load without losing functionality.

**Option A: Block Type Consolidator (Recommended)**

Create a single `ContentBlock : LayoutBlock` that uses a `BlockContentType` enum:

```csharp
public enum BlockContentType
{
    Text,
    Image,
    Table,
    Container,
    Row,
    Cell,
    Chart,
    Barcode,
    Rectangle,
    Ellipse,
    Line,
}

public sealed class ContentBlock : LayoutBlock
{
    public required BlockContentType ContentType { get; init; }
    public object? Content { get; init; }  // TextContent, ImageContent, etc.
    
    public override void LayoutSize(Size availableSize, ILayoutSizingContext context)
    {
        switch (ContentType)
        {
            case BlockContentType.Text:
                MeasureTextContent();
                break;
            case BlockContentType.Chart:
                MeasureChartContent();
                break;
            // ...
        }
    }
}
```

**Pros:**
- Single class to understand instead of 12
- Type-safe discriminator (enum vs. string)
- Easy to add new block types (one enum value + one switch case)

**Cons:**
- Large switch statements in Measure/Arrange
- Less type safety at compile time (all content in `object?`)
- Breaking change for existing code

**Feasibility:** Medium effort; use feature flag to roll out gradually.

---

**Option B: Keep Current Hierarchy, Add Documentation**

If consolidation is too risky, add a BlockTypeHierarchyGuide:

```csharp
/// <summary>
/// Quick reference for choosing the right block type.
/// </summary>
/// <remarks>
/// | Scenario | Use |
/// |----------|-----|
/// | Display text | TextBlock |
/// | Display image or chart | ImageBlock or ChartBlock |
/// | Repeat rows for data | RowBlock (inside TableBlock) or ContainerBlock (for lists) |
/// | Create layout grid | TableBlock with CellBlocks |
/// | Add visual decoration | ShapeBlock (with PathGeometry type) |
/// | Page boundaries | PageBlock (automatic), PageSectionBlock (manual) |
/// | Report sections | HeaderBlock, DetailBlock, FooterBlock (via ReportBlock) |
/// </remarks>
public static class BlockTypeHierarchyGuide { }
```

**Feasibility:** Low effort, high value.

---

### 2.4 Simplification #3: Explicit Style Cascade Resolver

**Current State:**
Style cascade is distributed across DefaultReportBuilder and multiple methods.

**Goal:**
Centralize cascade resolution so new developers can understand it in 10 minutes.

**New Class:**

```csharp
namespace KineticReports.Core.Styling;

/// <summary>
/// Resolves a block's final AppliedStyle by following the cascade priority order.
/// </summary>
/// <remarks>
/// Cascade resolution order (from lowest to highest priority):
/// 1. Theme defaults (not yet implemented)
/// 2. Report defaults (DefaultReportBuilder.WithDefaultBlockStyle)
/// 3. Named style (StyleDefinition matched by style ID)
/// 4. Parent inheritance (from container's AppliedStyle)
/// 5. Local override (block-specific AppliedStyle)
/// → Final immutable AppliedStyle
/// 
/// Each level overrides previous levels only for explicitly set properties.
/// Unset properties inherit from lower-priority levels.
/// </remarks>
public sealed class StyleCascadeResolver
{
    public StyleCascadeResolver(
        AppliedStyle? themeDefaults = null,
        AppliedStyle? reportDefaults = null,
        IReadOnlyDictionary<string, StyleDefinition>? namedStyles = null)
    {
        _themeDefaults = themeDefaults ?? CreateDefaultTheme();
        _reportDefaults = reportDefaults ?? CreateDefaultReportStyle();
        _namedStyles = namedStyles ?? new Dictionary<string, StyleDefinition>();
    }

    /// <summary>
    /// Resolves the final AppliedStyle for a block following the cascade.
    /// </summary>
    public AppliedStyle Resolve(
        string? styleId,
        AppliedStyle? parentStyle,
        AppliedStyle? localOverride)
    {
        var cascade = _themeDefaults
            .MergeWith(_reportDefaults)
            .MergeWith(styleId is not null ? _namedStyles[styleId].ToAppliedStyle() : null)
            .MergeWith(parentStyle)
            .MergeWith(localOverride);
        
        return cascade;
    }

    private static AppliedStyle CreateDefaultTheme() { /* ... */ }
    private static AppliedStyle CreateDefaultReportStyle() { /* ... */ }
}
```

**Benefits:**
- Single source of truth for cascade order
- Testable in isolation
- Easy to add theme-level defaults later
- Self-documenting (comments show priority order)

**Feasibility:** Low-medium effort; refactor DefaultReportBuilder to use it.

---

### 2.5 Simplification #4: DefaultLayoutOptions Factory & Cookbook

**Current State:**
New users don't know what LayoutOptions to pass. They see this:

```csharp
var doc = await engine.RunAsync(definition, parameters, layoutSizingContext, layoutOptions);
```

And wonder: What should `layoutOptions` be?

**Goal:**
Provide sensible defaults and common patterns.

**New Static Factory:**

```csharp
public sealed class LayoutOptions
{
    // ... existing properties

    /// <summary>
    /// Creates layout options for common scenarios.
    /// </summary>
    public static class Presets
    {
        /// <summary>Letter (8.5" × 11"), 0.5" margins</summary>
        public static LayoutOptions StandardLetter() => new()
        {
            PageWidth = 816,   // 8.5" × 96 DPI
            PageHeight = 1056, // 11" × 96 DPI
            MarginLeft = 48,   // 0.5" × 96 DPI
            MarginRight = 48,
            MarginTop = 48,
            MarginBottom = 48,
        };

        /// <summary>A4 (210 × 297 mm), 20 mm margins</summary>
        public static LayoutOptions StandardA4() => new()
        {
            PageWidth = 794,   // 210 mm × 96 DPI ÷ 25.4
            PageHeight = 1123, // 297 mm × 96 DPI ÷ 25.4
            MarginLeft = 75,   // 20 mm × 96 DPI ÷ 25.4
            MarginRight = 75,
            MarginTop = 75,
            MarginBottom = 75,
        };

        /// <summary>Web/screen (1024 × 768), no margins</summary>
        public static LayoutOptions WebScreen() => new()
        {
            PageWidth = 1024,
            PageHeight = 768,
            MarginLeft = 0,
            MarginRight = 0,
            MarginTop = 0,
            MarginBottom = 0,
        };
    }
}
```

**Usage:**
```csharp
var doc = await engine.RunAsync(
    definition,
    parameters,
    layoutSizingContext,
    LayoutOptions.Presets.StandardLetter()
);
```

**Feasibility:** Very low effort; high value for new users.

---

### 2.6 Summary of Simplifications

| Change | Effort | Value | Start |
|--------|--------|-------|-------|
| ExpressionContext documentation | 1-2 hrs | High | Now |
| BlockTypeHierarchyGuide | 1 hr | High | Now |
| StyleCascadeResolver | 4-6 hrs | High | Sprint 1 |
| LayoutOptions.Presets | 1-2 hrs | High | Now |
| Block type consolidation (Optional) | 16-20 hrs | Medium | Sprint 2+ |

**Quick Wins (< 2 hours each):**
1. Enhance ExpressionContext XML docs with examples
2. Add BlockTypeHierarchyGuide static class
3. Implement LayoutOptions.Presets
4. Add "Getting Started" section to docs with cookbook examples

**All four can be done in one sprint.**

---

## Part 3: Missing Features for Report Designer

### 3.1 Current Designer Infrastructure (Good Starting Point)

✅ **Exists:**
- `ReportComponentCatalog` — 20+ predefined component types
- `DesignerDocumentCompiler` — Converts designer metadata → ReportDefinition
- `VisualDocumentBuilder` — Builds visual representation from ReportDefinition
- `VisualDocument` + serialization — JSON persistence of visual state

❌ **Missing:**
- UI building blocks (no React/Vue/Blazor components for designer)
- Drag-and-drop orchestration
- Property editors (style, binding, data source)
- Real-time preview
- Undo/redo
- Template library
- Data source preview
- Expression syntax validation

---

### 3.2 Critical Missing Features (MVP for Designer)

#### Feature 1: Component Property Metadata 🔴 (Critical)

**Problem:** The designer doesn't know what properties each component has.

**Current Code:**
```csharp
public class ReportComponentDescriptor
{
    public ReportComponentType Type { get; set; }
    public string DisplayName { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public bool SupportsChildren { get; set; }
}
```

**What's Missing:**
- Editable properties (e.g., TextBlock has `Text`, `StyleId`, `Binding`)
- Property types (string, number, boolean, enum, expression)
- Constraints (required, min/max length, allowed values)
- Default values
- Whether property is expression-bindable

**Recommendation:**

```csharp
public sealed class ReportComponentDescriptor
{
    public ReportComponentType Type { get; set; }
    public string DisplayName { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public bool SupportsChildren { get; set; }
    
    // NEW:
    public IReadOnlyList<ComponentPropertyDescriptor> EditableProperties { get; set; } = [];
    public IReadOnlyList<string> AllowedParentTypes { get; set; } = [];
    public ComponentConstraints? Constraints { get; set; }
}

public sealed class ComponentPropertyDescriptor
{
    public required string Name { get; init; }
    public required PropertyValueType ValueType { get; init; }
    public required string DisplayName { get; init; }
    public string? Description { get; init; }
    public bool IsRequired { get; init; }
    public object? DefaultValue { get; init; }
    public bool IsExpressionBindable { get; init; } = false;
    public IReadOnlyList<string>? AllowedValues { get; init; }
    public NumberConstraint? NumberConstraint { get; init; }
    public StringConstraint? StringConstraint { get; init; }
}

public enum PropertyValueType
{
    String,
    Number,
    Boolean,
    Enum,
    Color,
    Expression,
}

public sealed record NumberConstraint(decimal? MinValue, decimal? MaxValue, int? DecimalPlaces);
public sealed record StringConstraint(int? MinLength, int? MaxLength, string? Pattern);
```

**Why Critical:** Without this, designer can't build a property panel. Users can't edit component settings.

---

#### Feature 2: Data Source Preview & Binding Validation 🔴 (Critical)

**Problem:** Designer doesn't know what fields are available in a data source.

**Current Code:**
No mechanism to:
- List available fields in a data source
- Validate expressions against data schema
- Preview sample data

**Recommendation:**

```csharp
namespace KineticReports.Core.Engine.Data;

/// <summary>
/// Inspects a data source schema without fetching all data.
/// </summary>
public interface IDataSourceInspector
{
    /// <summary>
    /// Returns the available fields and their types in the data source.
    /// </summary>
    Task<IReadOnlyList<DataFieldDescriptor>> InspectAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a preview of sample rows (typically first 10) for UI preview.
    /// </summary>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> PreviewAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        int limit = 10,
        CancellationToken cancellationToken = default);
}

public sealed record DataFieldDescriptor(
    string Name,
    Type ClrType,
    bool IsNullable,
    string? Description = null);
```

**Implementation Example (PocoDataProvider):**
```csharp
public class PocoDataProvider : IDataSourceInspector
{
    public async Task<IReadOnlyList<DataFieldDescriptor>> InspectAsync(
        DataSourceDefinition definition, ...)
    {
        var rows = this.Resolve(definition, ...);
        if (!rows.Any()) return [];
        
        var firstRow = rows.First();
        return firstRow.Keys.Select(k => new DataFieldDescriptor(
            Name: k,
            ClrType: firstRow[k]?.GetType() ?? typeof(object),
            IsNullable: true
        )).ToList();
    }
}
```

**Why Critical:** Enables expression validation, auto-complete, and prevents binding errors.

---

#### Feature 3: Expression Syntax Validation 🟠 (High Priority)

**Problem:** Expressions are validated at runtime only (no compile-time checking).

**Current Behavior:**
```csharp
var result = evaluator.Evaluate("{InvalidFieldName}", context);
// Returns null (silent failure, no error)
```

**Recommendation:**

```csharp
namespace KineticReports.Core.Engine.Expressions;

/// <summary>
/// Validates expression syntax and field references at design time.
/// </summary>
public interface IExpressionValidator
{
    /// <summary>
    /// Validates the expression and returns any errors/warnings.
    /// </summary>
    IReadOnlyList<ExpressionValidationResult> Validate(
        string expression,
        ExpressionValidationContext context);
}

public sealed record ExpressionValidationResult(
    ExpressionValidationLevel Level, // Error, Warning, Info
    string Message,
    int? Position = null,
    string? SuggestedFix = null);

public sealed record ExpressionValidationContext(
    IReadOnlyList<DataFieldDescriptor> AvailableFields,
    IReadOnlyList<ParameterDefinition> AvailableParameters,
    bool AllowPageNumbers = true);
```

**Example:**
```csharp
var validation = validator.Validate("{OrderDate}", context);
// ValidationResult(Error, "Field 'OrderDate' not found in available fields")
// SuggestedFix: "Did you mean 'OrderDateField'?"

var validation2 = validator.Validate("{PageNumber}", context);
// ValidationResult(Warning, "PageNumber is only available after pagination")
```

**Why High Priority:** Essential for designer UX (catch errors before publish).

---

#### Feature 4: Visual Layout Editor State 🟠 (High Priority)

**Problem:** Designer state (selected component, viewport position, zoom level) is not captured.

**Current:**
`VisualDocument` stores layout output, not design-time UI state.

**Recommendation:**

```csharp
namespace KineticReports.Core.Visual;

/// <summary>
/// Captures the designer's interactive state (selection, zoom, viewport position).
/// </summary>
public sealed record DesignerViewState
{
    /// <summary>ID of the currently selected component.</summary>
    public string? SelectedComponentId { get; set; }

    /// <summary>IDs of multi-selected components.</summary>
    public IReadOnlySet<string> MultiSelectIds { get; set; } = new HashSet<string>();

    /// <summary>Zoom level as percentage (100 = 1:1).</summary>
    public int ZoomPercentage { get; set; } = 100;

    /// <summary>Viewport scroll position in pixels.</summary>
    public (double X, double Y) ViewportOffset { get; set; }

    /// <summary>Open property editor panel (null = closed).</summary>
    public PropertyEditorPanel? OpenPanel { get; set; }

    /// <summary>Guides and snapping settings.</summary>
    public DesignerGridSettings GridSettings { get; set; } = new();
}

public sealed record PropertyEditorPanel(
    string SelectedComponentId,
    PropertyEditorMode Mode); // Design, Data, Style

public sealed record DesignerGridSettings(
    bool ShowGrid = true,
    int GridSpacing = 10,
    bool SnapToGrid = true);
```

**Usage (Designer App):**
```csharp
// Save designer state
var state = new DesignerViewState 
{ 
    SelectedComponentId = "text-1",
    ZoomPercentage = 150,
    ViewportOffset = (100, 200)
};
await storage.SaveDesignerStateAsync(reportId, state);

// Load designer state
var state = await storage.LoadDesignerStateAsync(reportId);
camera.SetZoom(state.ZoomPercentage);
// ... restore UI
```

**Why High Priority:** Without this, users lose their work context between sessions.

---

#### Feature 5: Template & Snippet Library 🟡 (Medium Priority)

**Problem:** New designers start from scratch every time.

**Recommendation:**

```csharp
namespace KineticReports.Core.Authoring.Templates;

/// <summary>
/// Predefined report templates and reusable component snippets.
/// </summary>
public interface IReportTemplateLibrary
{
    /// <summary>Gets all available templates.</summary>
    IReadOnlyList<ReportTemplate> ListTemplates();

    /// <summary>Creates a new report from a template.</summary>
    ReportDefinition CreateFromTemplate(string templateId, string newReportId);

    /// <summary>Gets reusable component snippets by category.</summary>
    IReadOnlyList<ComponentSnippet> ListSnippets(string? category = null);

    /// <summary>Inserts a snippet into the current design.</summary>
    ReportDesignerDocument InsertSnippet(ReportDesignerDocument document, string snippetId);
}

public sealed record ReportTemplate(
    string Id,
    string DisplayName,
    string Description,
    string Thumbnail, // Base64 PNG
    ReportDefinition BaseDefinition,
    string[] Tags);

public sealed record ComponentSnippet(
    string Id,
    string DisplayName,
    string Category, // "Headers", "Tables", "Charts", etc.
    ReportComponentPlacement[] Components);
```

**Built-in Templates:**
- "Simple Report" — Basic header, detail rows, footer
- "Financial Report" — Multi-section with summary tables
- "Sales Dashboard" — Charts and KPIs
- "Invoice" — Professional invoice format

**Built-in Snippets:**
- "Table with totals row"
- "Three-column header"
- "Chart + table combo"
- "Blank page section"

**Why Medium Priority:** Accelerates designer productivity but not blocking for MVP.

---

### 3.3 Enhancement Features (Nice-to-Have)

#### Feature 6: Undo/Redo Support 🟡

**Current:** No undo/redo in designer.

**Pattern (Command Pattern):**
```csharp
public interface IDesignCommand
{
    void Execute(ReportDesignerDocument document);
    void Undo(ReportDesignerDocument document);
}

public sealed class CommandHistory
{
    public void Execute(IDesignCommand command) { /* ... */ }
    public void Undo() { /* ... */ }
    public void Redo() { /* ... */ }
}
```

---

#### Feature 7: Component Dependency Graph 🟡

**Current:** No way to find what components use a style or data source.

**Recommendation:**
```csharp
public interface IReportDependencyAnalyzer
{
    /// <summary>Find all components using a style.</summary>
    IReadOnlyList<string> FindComponentsUsingSyle(
        ReportDesignerDocument document, string styleId);

    /// <summary>Find all components bound to a data source.</summary>
    IReadOnlyList<string> FindComponentsUsingDataSource(
        ReportDesignerDocument document, string dataSourceId);

    /// <summary>Find breaking changes if style is deleted.</summary>
    IReadOnlyList<string> AnalyzeImpactOfStyleDeletion(
        ReportDesignerDocument document, string styleId);
}
```

---

#### Feature 8: Real-Time Preview 🟡

**Current:** Preview requires full report engine run (~500ms - 2s).

**Recommendation:** Cache layout results, invalidate only affected regions on edits.

```csharp
public interface IPreviewCache
{
    Task<ReportDocument?> GetCachedAsync(string reportId);
    Task SetCachedAsync(string reportId, ReportDocument doc);
    void InvalidateComponent(string reportId, string componentId);
}
```

---

### 3.4 Feature Priority Matrix

| Feature | MVP | Effort | Value | Start Sprint |
|---------|-----|--------|-------|---|
| Component Property Metadata | ✅ | Medium | Critical | 1 |
| Data Source Preview/Validation | ✅ | Medium | Critical | 1 |
| Expression Syntax Validation | ✅ | Low | Critical | 1 |
| DesignerViewState | ✅ | Low | Critical | 1 |
| Template Library | ⭕ | Medium | High | 2 |
| Undo/Redo | ⭕ | Medium | High | 2 |
| Dependency Analysis | ⭕ | Low | Medium | 3 |
| Real-Time Preview Cache | ⭕ | Medium | Medium | 3 |

---

### 3.5 Summary of Missing Features

**For MVP Report Designer, Add:**
1. **ComponentPropertyDescriptor** — Property metadata (Effort: 4-6 hrs)
2. **IDataSourceInspector** — Field inspection + preview (Effort: 6-8 hrs)
3. **IExpressionValidator** — Expression validation (Effort: 4-6 hrs)
4. **DesignerViewState** — Design-time UI state capture (Effort: 2-3 hrs)

**Total MVP Effort:** 16-24 hours (~2-3 developer days)

**Post-MVP Enhancements:**
5. Template library (Effort: 8-12 hrs)
6. Undo/redo (Effort: 8-12 hrs)
7. Dependency analysis (Effort: 6-8 hrs)
8. Preview caching (Effort: 8-10 hrs)

---

## Part 4: Recommendations Summary

### 4.1 Immediate Actions (This Sprint)

**Simplifications (Quick Wins):**
1. ✅ **Enhance ExpressionContext XML docs** with field list and examples
2. ✅ **Add BlockTypeHierarchyGuide** static class
3. ✅ **Implement LayoutOptions.Presets** (Letter, A4, WebScreen)
4. ✅ **Create Getting Started cookbook** with 5 common patterns

**Designer Features (Sprint 1):**
5. ✅ **Add ComponentPropertyDescriptor** to ReportComponentCatalog
6. ✅ **Implement IDataSourceInspector** for PocoDataProvider + SqlLiteDataProvider
7. ✅ **Build IExpressionValidator** with pattern matching
8. ✅ **Add DesignerViewState** record for UI state persistence

---

### 4.2 Medium-Term Actions (Sprints 2-3)

**Refactoring:**
1. **Introduce StyleCascadeResolver** (consolidate cascade logic)
2. **Consider Block Type Consolidation** (optional; use feature flag)

**Designer Features:**
3. **Template Library** (ReportTemplateLibrary interface + built-ins)
4. **Undo/Redo** (CommandHistory + IDesignCommand)

---

### 4.3 Architecture Patterns to Adopt

**1. Descriptor Pattern (Already Used)**
Used for: Components, Properties, Fields
Benefit: Design-time metadata without runtime overhead

**2. Strategy Pattern (For Resolvers)**
Used for: DataResolver, ExpressionValidator, ExpressionEvaluator
Benefit: Swappable implementations, testable

**3. Builder Pattern (Already Used)**
Used for: DefaultReportBuilder, StyleBuilder
Benefit: Fluent API, progressive construction

**Recommended New:**

**4. Visitor Pattern (For Designer Traversal)**
```csharp
public interface IReportDesignVisitor
{
    void Visit(ReportDesignerDocument document);
    void Visit(ReportComponentPlacement component);
    void Visit(ComponentPropertyValue property);
}
```
Benefit: Safe traversal of component tree, easy to implement "find/replace", "refactor", etc.

**5. Observer Pattern (For Designer Changes)**
```csharp
public interface IDesignChangeListener
{
    void OnComponentAdded(string componentId);
    void OnPropertyChanged(string componentId, string propertyName);
    void OnComponentRemoved(string componentId);
}
```
Benefit: Notifies UI, preview, property panel without tight coupling

---

### 4.4 Documentation Improvements

| Document | Audience | Status | Effort |
|----------|----------|--------|--------|
| Expression Language Quick Ref | Designers, Developers | Partial | 2 hrs |
| Block Type Decision Tree | Developers | Missing | 1 hr |
| Style Cascade Deep Dive | Developers | Partial | 1.5 hrs |
| Designer Integration Guide | Designer Developers | Missing | 4 hrs |
| Cookbook: Common Patterns | All Users | Partial | 3 hrs |
| Component Property Reference | Designers | Missing | 2 hrs |

**Priority:** Start with Expression Language Quick Ref + Cookbook.

---

## Conclusion

### Strengths Summary
- ✅ Excellent layered architecture with clean abstractions
- ✅ Immutability guarantees reduce bugs
- ✅ Existing designer infrastructure is solid (component catalog, compilation)
- ✅ Well-organized codebase with good test coverage

### Improvement Opportunities
- 🟠 Expression context threading is implicit (fix: documentation + examples)
- 🟠 Block type proliferation adds cognitive load (fix: add guide or consolidate)
- 🟠 Styling cascade is not self-documenting (fix: StyleCascadeResolver)
- 🟠 Designer is missing critical metadata features (fix: Add 4 features in Sprint 1)

### Readiness for Designer
- 🟢 Core engine is ready
- 🟢 Authoring framework is in place
- 🟡 Missing property metadata for UI builder
- 🟡 Missing data source inspection/validation
- 🟡 Missing expression validation for designers

### Recommendation: **Go Forward with Designer**
All architectural concerns can be addressed incrementally without refactoring the core. The foundation is solid.

**Estimated MVP Timeline:** 4-6 weeks (2 developers)
- Weeks 1-2: Add 4 critical features + simplification guides
- Weeks 3-4: Build Blazor/React designer UI components
- Weeks 5-6: Integration testing + polish

---

## Appendix: Quick Reference

### Key Interfaces
| Interface | Purpose | Lifetime |
|-----------|---------|----------|
| `IReportEngine` | Orchestration | Scoped |
| `IRenderer` | Output generation | Transient |
| `IDataResolver` | Data fetching | Scoped |
| `IExpressionEvaluator` | Expression evaluation | Scoped |
| `ILayoutEngine` | Measurement/arrangement | Transient |
| `IReportBuilder` | Report construction | N/A (static) |
| `ILayoutSizingContext` | Text/image services | Scoped |

### Key Records/Classes
| Type | Role | Notes |
|------|------|-------|
| `ReportDefinition` | Immutable model | Sealed record |
| `ReportDocument` | Layout output | Immutable |
| `LayoutBlock` | Layout element base | Abstract, sealed subclasses |
| `AppliedStyle` | Resolved style | Sealed record |
| `ExpressionContext` | Binding context | Sealed record |

### Common Patterns
```csharp
// 1. Running a report
var doc = await engine.RunAsync(definition, parameters, context, LayoutOptions.Presets.StandardLetter());

// 2. Rendering output
await renderer.RenderAsync(doc, outputStream);

// 3. Building via code
var blocks = new DefaultReportBuilder()
    .AddTextRegion("title", "My Report", titleStyle)
    .AddDataSourceTable("data", "Orders", columns, tableStyle)
    .Build(dataContext, expressionEvaluator);

// 4. Loading from JSON
var definition = JsonConvert.DeserializeObject<ReportDefinition>(json);
```

---

**End of Review**  
Generated: 2026-07-29
