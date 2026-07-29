# KineticReports.Core Simplification & Pain Point Fix Implementation Plan

**Created:** 2026-07-29  
**Estimated Total Effort:** 12-16 hours (~1.5-2 developer days)  
**Status:** Planning Phase

---

## Overview

This plan addresses all 4 pain points and implements 4 of 5 simplification opportunities from the Architecture Review (Part 1 & 2).

### Breaking Changes Summary
🟢 **Phase 1 & 2:** Non-breaking (safe to deploy)  
🔴 **Phase 3 (Optional):** Breaking change requires user approval

---

## Phase 1: Quick Wins (Non-Breaking) — 4-6 Hours

### Goal
Reduce cognitive load with documentation, guides, and sensible defaults.

### Change 1.1: Enhance ExpressionContext Documentation
**File:** `src/Core/KineticReports.Core/Engine/Expressions/ExpressionContext.cs`  
**Breaking:** ❌ No  
**Effort:** 1-2 hours

**What to Do:**
1. Read current ExpressionContext definition
2. Add comprehensive XML doc comments showing:
   - All 5 available context fields (Row, Parameters, PageNumber, TotalPages, RowNumber)
   - Example expressions for each
   - Fallback behavior (returns null on missing field)
   - When each field is available (e.g., PageNumber/TotalPages only post-pagination)

**Deliverable:**
```csharp
/// <summary>
/// The evaluation context for a single expression evaluation.
/// Supplies the current data row, report parameters, and page/group state.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Available in expressions:</strong>
/// <list type="bullet">
/// <item>
///   <term>Data fields:</term>
///   <description>Use <c>{FieldName}</c> syntax to reference current row.
///   Example: <c>{OrderDate}</c>, <c>{Customer}</c>
///   </description>
/// </item>
/// <item>
///   <term>Parameters:</term>
///   <description>Use <c>{ParameterName}</c> syntax.
///   Example: <c>{StartDate}</c>, <c>{ReportTitle}</c>
///   </description>
/// </item>
/// <item>
///   <term>Page numbers:</term>
///   <description><c>{PageNumber}</c> (current, 1-indexed),
///   <c>{TotalPages}</c> (total, available after pagination only)
///   </description>
/// </item>
/// <item>
///   <term>Row tracking:</term>
///   <description><c>{RowNumber}</c> (1-indexed within current data source)
///   </description>
/// </item>
/// </list>
/// </para>
/// <para>
/// <strong>Behavior:</strong>
/// If a field is not found, the expression evaluator returns <see langword="null"/>
/// (silent failure—design pattern choice). Use IExpressionValidator for compile-time checks.
/// </para>
/// </remarks>
public sealed record ExpressionContext
{
    /// <summary>Gets the current row data as a name-to-value dictionary.</summary>
    public required IReadOnlyDictionary<string, object?> Row { get; init; }

    /// <summary>Gets the report parameters as a name-to-value dictionary.</summary>
    public required IReadOnlyDictionary<string, object?> Parameters { get; init; }

    /// <summary>Gets the current page number (1-indexed). Available only after pagination.</summary>
    public required int PageNumber { get; init; }

    /// <summary>Gets the total page count. Available only after pagination.</summary>
    public required int TotalPages { get; init; }

    /// <summary>Gets the current row number within the data source (1-indexed).</summary>
    public required int RowNumber { get; init; }
}
```

**Tests to Add:**  
File: `tests/unit-tests/KineticReports.Engine.Tests/Expressions/ExpressionContextTests.cs`
```csharp
[Theory]
[InlineData("{OrderDate}", "OrderDate", "2026-01-01")]
[InlineData("{Customer}", "Customer", "Acme Corp")]
[InlineData("{ReportTitle}", "ReportTitle", "Q1 Summary")]
[InlineData("{PageNumber}", "PageNumber", "1")]
[InlineData("{RowNumber}", "RowNumber", "5")]
public void ExpressionContext_ContainsAllFiveContextTypes(string expressionHint, string fieldName, string expectedValue)
{
    // Demonstrate that all five context types are accessible and documented
}
```

---

### Change 1.2: Add BlockTypeHierarchyGuide Static Class
**File:** `src/Core/KineticReports.Core/Layout/BlockTypeHierarchyGuide.cs` (NEW)  
**Breaking:** ❌ No  
**Effort:** 1 hour

**What to Do:**
1. Create new file `BlockTypeHierarchyGuide.cs`
2. Add a static class with:
   - XML doc showing the 12 block types
   - Decision tree (when to use which type)
   - Example scenarios
   - Links to each block class

**Deliverable:**
```csharp
namespace KineticReports.Core.Layout;

/// <summary>
/// Quick reference guide for choosing the correct block type for your layout.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Block Type Decision Tree:</strong>
/// </para>
/// <list type="table">
/// <listheader>
///   <term>Scenario</term>
///   <description>Block Type</description>
/// </listheader>
/// <item>
///   <term>Display static or data-bound text</term>
///   <description><see cref="TextBlock"/></description>
/// </item>
/// <item>
///   <term>Display image or chart</term>
///   <description><see cref="ImageBlock"/> or <see cref="ChartBlock"/></description>
/// </item>
/// <item>
///   <term>Display barcodes or QR codes</term>
///   <description><see cref="BarcodeBlock"/></description>
/// </item>
/// <item>
///   <term>Create a table grid with rows/columns</term>
///   <description><see cref="TableBlock"/> containing <see cref="RowBlock"/> → <see cref="CellBlock"/></description>
/// </item>
/// <item>
///   <term>Repeat child blocks for each data row (non-tabular)</term>
///   <description><see cref="ContainerBlock"/> with data-row repetition, or <see cref="RowBlock"/> outside table</description>
/// </item>
/// <item>
///   <term>Add decorative shapes (rectangles, ellipses, lines)</term>
///   <description><see cref="ShapeBlock"/> with <see cref="Rendering.PathGeometry"/></description>
/// </item>
/// <item>
///   <term>Report sections (header, detail, footer)</term>
///   <description><see cref="HeaderBlock"/>, <see cref="DetailBlock"/>, <see cref="FooterBlock"/> (via <see cref="ReportBlock"/>)</description>
/// </item>
/// <item>
///   <term>Page-level header/footer (on every page)</term>
///   <description><see cref="PageHeaderBlock"/> or <see cref="PageFooterBlock"/> (via <see cref="ReportBlock"/>)</description>
/// </item>
/// <item>
///   <term>Grouping sections within detail</term>
///   <description><see cref="GroupHeaderBlock"/> or <see cref="GroupFooterBlock"/> (via <see cref="ReportBlock"/>)</description>
/// </item>
/// <item>
///   <term>Page boundaries (automatic)</term>
///   <description><see cref="PageBlock"/> — created by layout engine, not direct use</description>
/// </item>
/// <item>
///   <term>Manual section boundary</term>
///   <description><see cref="PageSectionBlock"/></description>
/// </item>
/// </list>
/// <para>
/// <strong>Notes:</strong>
/// <list type="bullet">
/// <item>All block types inherit from <see cref="LayoutBlock"/> and use Measure/Arrange pattern.</item>
/// <item><see cref="ReportBlock"/> is a container type for header/footer/group semantics; most content uses other types.</item>
/// <item><see cref="TableBlock"/>, <see cref="RowBlock"/>, <see cref="CellBlock"/> work together; use CellBlock only inside RowBlock.</item>
/// <item><see cref="ContainerBlock"/> is flexible for non-table layouts.</item>
/// </list>
/// </para>
/// </remarks>
public static class BlockTypeHierarchyGuide
{
    // Marker class for documentation and discoverability
    // No runtime behavior needed
}
```

---

### Change 1.3: Add LayoutOptions.Presets Factory Methods
**File:** `src/Core/KineticReports.Core/LayoutEngine/LayoutOptions.cs`  
**Breaking:** ❌ No (additive only)  
**Effort:** 1-2 hours

**What to Do:**
1. Locate `LayoutOptions` class
2. Add static `Presets` class with:
   - StandardLetter() — 8.5" × 11", 0.5" margins
   - StandardA4() — 210 × 297 mm, 20 mm margins
   - WebScreen() — 1024 × 768, no margins
   - Add XML docs explaining DIP calculations

**Deliverable:**
```csharp
public sealed class LayoutOptions
{
    // ... existing properties

    /// <summary>
    /// Predefined layout option presets for common page formats.
    /// </summary>
    public static class Presets
    {
        /// <summary>
        /// Standard US Letter size (8.5" × 11") with 0.5" margins.
        /// Dimensions: 816 × 1056 DIPs @ 96 DPI, margins 48 DIPs.
        /// </summary>
        /// <returns>Configured <see cref="LayoutOptions"/> for letter-size reports.</returns>
        public static LayoutOptions StandardLetter() => new()
        {
            PageWidth = 816,   // 8.5" × 96 DPI
            PageHeight = 1056, // 11" × 96 DPI
            MarginLeft = 48,   // 0.5" × 96 DPI
            MarginRight = 48,
            MarginTop = 48,
            MarginBottom = 48,
        };

        /// <summary>
        /// ISO A4 size (210 × 297 mm) with 20 mm margins.
        /// Dimensions: 794 × 1123 DIPs @ 96 DPI, margins 75 DIPs.
        /// </summary>
        /// <returns>Configured <see cref="LayoutOptions"/> for A4 reports.</returns>
        public static LayoutOptions StandardA4() => new()
        {
            PageWidth = 794,   // 210 mm × 96 DPI ÷ 25.4
            PageHeight = 1123, // 297 mm × 96 DPI ÷ 25.4
            MarginLeft = 75,   // 20 mm × 96 DPI ÷ 25.4
            MarginRight = 75,
            MarginTop = 75,
            MarginBottom = 75,
        };

        /// <summary>
        /// Web/screen display (1024 × 768) with no margins.
        /// Use for on-screen HTML or interactive viewers.
        /// </summary>
        /// <returns>Configured <see cref="LayoutOptions"/> for web display.</returns>
        public static LayoutOptions WebScreen() => new()
        {
            PageWidth = 1024,
            PageHeight = 768,
            MarginLeft = 0,
            MarginRight = 0,
            MarginTop = 0,
            MarginBottom = 0,
        };

        /// <summary>
        /// Custom preset factory for non-standard dimensions.
        /// </summary>
        /// <param name="widthDips">Page width in DIPs (Device Independent Pixels).</param>
        /// <param name="heightDips">Page height in DIPs.</param>
        /// <param name="marginAllDips">Margin on all sides in DIPs.</param>
        /// <returns>Configured <see cref="LayoutOptions"/>.</returns>
        public static LayoutOptions Custom(double widthDips, double heightDips, double marginAllDips = 0) =>
            new()
            {
                PageWidth = widthDips,
                PageHeight = heightDips,
                MarginLeft = marginAllDips,
                MarginRight = marginAllDips,
                MarginTop = marginAllDips,
                MarginBottom = marginAllDips,
            };
    }
}
```

**Tests to Add:**
```csharp
[Fact]
public void Presets_StandardLetter_ReturnsLetterDimensions()
{
    var opts = LayoutOptions.Presets.StandardLetter();
    Assert.Equal(816, opts.PageWidth);
    Assert.Equal(1056, opts.PageHeight);
    Assert.Equal(48, opts.MarginLeft);
    Assert.Equal(48, opts.MarginTop);
}

[Fact]
public void Presets_StandardA4_ReturnsA4Dimensions()
{
    var opts = LayoutOptions.Presets.StandardA4();
    Assert.Equal(794, opts.PageWidth);
    Assert.Equal(1123, opts.PageHeight);
    Assert.Equal(75, opts.MarginLeft);
}

[Fact]
public void Presets_WebScreen_ReturnsWebDimensions()
{
    var opts = LayoutOptions.Presets.WebScreen();
    Assert.Equal(1024, opts.PageWidth);
    Assert.Equal(768, opts.PageHeight);
    Assert.Equal(0, opts.MarginLeft);
}
```

---

### Change 1.4: Create Getting Started Cookbook (Documentation)
**File:** `docs/COMPREHENSIVE_DEVELOPER_GUIDE.md` (UPDATE)  
**Breaking:** ❌ No  
**Effort:** 1.5-2 hours

**What to Do:**
1. Add a "Getting Started Cookbook" section to `docs/COMPREHENSIVE_DEVELOPER_GUIDE.md`
2. Add 5 common patterns with full code examples:
   - Pattern 1: Simple report (title + text regions)
   - Pattern 2: Data table (table with data source)
   - Pattern 3: Multi-section report (header, details, footer)
   - Pattern 4: Using parameters in expressions
   - Pattern 5: Custom styling

**Structure:**
```markdown
# Getting Started Cookbook

## Pattern 1: Simple Report (Static Content)
[Complete working example with code]

## Pattern 2: Data-Driven Table
[Complete working example with code]

## Pattern 3: Multi-Section Report
[Complete working example with code]

## Pattern 4: Using Parameters
[Complete working example with code]

## Pattern 5: Custom Styling
[Complete working example with code]

## Common Gotchas
- Expression evaluation is case-sensitive
- RowNumber is 1-indexed
- Parameters vs. Row fields require different syntax
- etc.
```

---

### Phase 1 Checklist
- [ ] Update ExpressionContext.cs with comprehensive XML docs
- [ ] Create ExpressionContextTests.cs with 5 test cases
- [ ] Create BlockTypeHierarchyGuide.cs marker class
- [ ] Extend LayoutOptions with Presets static class
- [ ] Add LayoutOptions.Presets unit tests
- [ ] Add cookbook section in docs/COMPREHENSIVE_DEVELOPER_GUIDE.md
- [ ] Build solution (verify no breaks)
- [ ] Test all examples in cookbook

**Estimated Completion:** Day 1 (4-6 hours)

---

## Phase 2: Medium Effort (Non-Breaking) — 4-6 Hours

### Goal
Centralize and document styling cascade logic.

### Change 2.1: Create StyleCascadeResolver Class
**File:** `src/Core/KineticReports.Core/Styling/StyleCascadeResolver.cs` (NEW)  
**Breaking:** ❌ No (additive, existing code unaffected)  
**Effort:** 4-6 hours

**What to Do:**
1. Create new class `StyleCascadeResolver`
2. Implement 6-level cascade resolution:
   - Theme defaults (placeholder for future)
   - Report defaults
   - Named style lookup
   - Parent inheritance
   - Local override
   - Final AppliedStyle
3. Add unit tests covering all levels
4. Add integration test showing cascade in action
5. Document in existing style docs

**Deliverable (Pseudocode):**
```csharp
namespace KineticReports.Core.Styling;

/// <summary>
/// Resolves a block's final AppliedStyle by following the 6-level cascade priority.
/// </summary>
/// <remarks>
/// <strong>Cascade Resolution Order (lowest to highest priority):</strong>
/// <list type="number">
/// <item><description>Theme defaults (future; placeholder for now)</description></item>
/// <item><description>Report defaults (from DefaultReportBuilder.WithDefaultBlockStyle)</description></item>
/// <item><description>Named style (referenced by style ID from StyleDefinition)</description></item>
/// <item><description>Parent inheritance (from container's AppliedStyle)</description></item>
/// <item><description>Local override (block-specific AppliedStyle)</description></item>
/// <item><description>Final immutable AppliedStyle</description></item>
/// </list>
/// 
/// Each level overrides previous levels only for explicitly set properties.
/// Unset properties cascade from lower-priority levels.
/// </remarks>
public sealed class StyleCascadeResolver
{
    private readonly AppliedStyle _themeDefaults;
    private readonly AppliedStyle _reportDefaults;
    private readonly IReadOnlyDictionary<string, StyleDefinition> _namedStyles;

    /// <summary>Initializes a new instance of StyleCascadeResolver.</summary>
    /// <param name="themeDefaults">Theme-level defaults (optional).</param>
    /// <param name="reportDefaults">Report-level defaults.</param>
    /// <param name="namedStyles">Map of style ID → StyleDefinition.</param>
    public StyleCascadeResolver(
        AppliedStyle? themeDefaults = null,
        AppliedStyle? reportDefaults = null,
        IReadOnlyDictionary<string, StyleDefinition>? namedStyles = null)
    {
        ArgumentNullException.ThrowIfNull(reportDefaults);
        _themeDefaults = themeDefaults ?? CreateDefaultTheme();
        _reportDefaults = reportDefaults;
        _namedStyles = namedStyles ?? new Dictionary<string, StyleDefinition>();
    }

    /// <summary>
    /// Resolves the final AppliedStyle for a block following the cascade.
    /// </summary>
    /// <param name="styleId">Named style reference (optional).</param>
    /// <param name="parentStyle">Parent container's AppliedStyle (optional).</param>
    /// <param name="localOverride">Block-specific AppliedStyle (optional).</param>
    /// <returns>Final resolved AppliedStyle.</returns>
    public AppliedStyle Resolve(
        string? styleId,
        AppliedStyle? parentStyle,
        AppliedStyle? localOverride)
    {
        // Start with theme defaults
        var result = _themeDefaults;

        // Merge report defaults
        result = MergeStyles(result, _reportDefaults);

        // Merge named style (if referenced)
        if (styleId is not null && _namedStyles.TryGetValue(styleId, out var namedStyle))
        {
            var namedApplied = namedStyle.ToAppliedStyle(); // or resolution logic
            result = MergeStyles(result, namedApplied);
        }

        // Merge parent inheritance
        if (parentStyle is not null)
        {
            result = MergeStyles(result, parentStyle);
        }

        // Merge local override (highest priority)
        if (localOverride is not null)
        {
            result = MergeStyles(result, localOverride);
        }

        return result;
    }

    /// <summary>Merges two styles, with <paramref name="overlay"/> taking priority.</summary>
    private static AppliedStyle MergeStyles(AppliedStyle baseStyle, AppliedStyle overlay)
    {
        // Merge logic: overlay's non-default properties override base
        // Implementation depends on how you want to detect "explicitly set" properties
        // Option 1: Use nullable properties (complex)
        // Option 2: Use a "was-explicitly-set" flag (cleaner)
        // Option 3: Never merge, always use highest-priority non-null (simplest, current approach)
        
        return overlay ?? baseStyle;
    }

    private static AppliedStyle CreateDefaultTheme()
    {
        return new AppliedStyle
        {
            FontFamily = "Segoe UI",
            FontSize = 11,
            TextColor = Color.Black,
            Background = Color.Transparent,
            // ... other defaults
        };
    }
}
```

**Tests to Add:**  
File: `tests/unit-tests/KineticReports.Core.Tests/Styling/StyleCascadeResolverTests.cs`

```csharp
[Fact]
public void Resolve_WithAllLevelsPresent_HighestPriorityWins()
{
    // Given: theme, report, named, parent, local styles all present
    // When: Resolve is called
    // Then: Local override should be in final result for all properties
}

[Fact]
public void Resolve_WithNamedStyle_NamedStylePropertiesIncluded()
{
    // Verify named style properties are included
}

[Fact]
public void Resolve_WithMissingNamedStyle_ResolvesContinues()
{
    // Verify that missing style ID doesn't break resolution
}

[Fact]
public void Resolve_WithNoOverride_CascadesCorrectly()
{
    // Verify cascade order when some levels are null
}

[Fact]
public void Resolve_AllSixLevels_CascadeOrderDocumented()
{
    // Integration test showing full cascade path
}
```

**Integration with DefaultReportBuilder:**  
Optionally refactor DefaultReportBuilder to use StyleCascadeResolver internally (optional; can be done later without breaking change).

---

### Phase 2 Checklist
- [ ] Create StyleCascadeResolver.cs with 6-level resolution logic
- [ ] Add comprehensive unit tests (5+ test cases)
- [ ] Add integration test showing full cascade
- [ ] Update style resolution documentation
- [ ] Build solution (verify no breaks)
- [ ] All tests pass

**Estimated Completion:** Day 2 (4-6 hours)

---

## Phase 3: Optional Breaking Changes

### ⚠️ DECISION POINT: Block Type Consolidation

**Current Situation:**
- 12 concrete block types (TextBlock, ImageBlock, ..., BarcodeBlock)
- Some redundancy (shapes via PathGeometry discriminator)
- Moderate cognitive load for new developers

**Proposed Change:** Consolidate into single `ContentBlock` with `BlockContentType` enum

**Impact Analysis:**
- ✅ Reduces cognitive load (1 type vs. 12 types)
- ✅ Easier to add new block types (add enum value + switch case)
- ✅ Type-safe discriminator
- ❌ **BREAKING:** Existing code using TextBlock, ImageBlock, etc. will fail
- ❌ Large switch statements (complexity trade-off)
- ❌ Loss of compile-time type safety for content

**Recommendation:** This should be skipped for now unless you're doing a major version bump.

**Decision Required:** Should we pursue this? **YES / NO**

**If NO (Recommended):**
- Skip Phase 3
- Stop here with phases 1-2 complete
- Plan consolidation for future major version (v2.0 or later)

**If YES:**
- Plan for breaking change announcement
- Provide migration guide
- Update all samples
- Increment major version number
- Proceed with Phase 3 below

---

### Phase 3.1: Block Type Consolidation (IF APPROVED)
**Breaking:** 🔴 **YES**  
**Effort:** 16-20 hours  
**Complexity:** High

**What Would Be Required:**
1. Create new `ContentBlock : LayoutBlock`
2. Create `BlockContentType` enum (12 values)
3. Move content-specific properties to content objects
4. Update LayoutSize/Arrange to use switch on BlockContentType
5. Create factory methods for backward compatibility (optional)
6. Update all tests
7. Update all samples
8. Update documentation
9. Create migration guide for consumers

**Not Recommended** unless doing major version release.

---

## Implementation Sequence

### Week 1: Phase 1 (4-6 hours)
**Mon-Tue:**
- Change 1.1: ExpressionContext docs
- Change 1.2: BlockTypeHierarchyGuide
- Change 1.3: LayoutOptions.Presets

**Wed-Thu:**
- Change 1.4: Cookbook documentation
- Testing + validation

**Fri:**
- Review + polish
- Merge to main branch

### Week 2: Phase 2 (4-6 hours)
**Mon-Tue:**
- Change 2.1: StyleCascadeResolver (class + tests)

**Wed:**
- Integration testing
- Documentation updates

**Thu:**
- Review + decision on Phase 3

**Fri:**
- Merge to main branch

### Week 3 (Conditional): Phase 3
Only if **YES** decision made for block consolidation.

---

## Acceptance Criteria (All Phases)

### Phase 1 ✅
- [ ] All XML doc comments are comprehensive and include examples
- [ ] BlockTypeHierarchyGuide is discoverable via IDE intellisense
- [ ] LayoutOptions.Presets work correctly (unit tests pass)
- [ ] All 5 cookbook patterns execute without error
- [ ] No breaking changes
- [ ] Build succeeds with no warnings

### Phase 2 ✅
- [ ] StyleCascadeResolver correctly implements 6-level cascade
- [ ] All cascade combinations tested (5+ unit tests)
- [ ] Integration test shows cascade in realistic scenario
- [ ] No breaking changes
- [ ] Existing code continues to work
- [ ] Build succeeds with no warnings

### Phase 3 ✅ (If approved)
- [ ] All block types consolidated into ContentBlock
- [ ] All tests pass
- [ ] Migration guide provided
- [ ] All samples updated
- [ ] Major version number incremented

---

## Risk Assessment

| Item | Risk | Mitigation |
|------|------|-----------|
| Phase 1 documentation quality | Low | Peer review; examples tested |
| Phase 2 cascade logic correctness | Medium | Comprehensive unit tests; integration test |
| Phase 3 (if approved) breaking changes | High | Major version bump; migration guide; extensive testing |

---

## Communication Plan

### After Phase 1 Complete
- Announce "simplification documentation update" on changelog
- Point developers to cookbook and guides
- No code changes for consumers

### After Phase 2 Complete
- Announce "new StyleCascadeResolver available" (optional to use)
- Update docs pointing to new class
- No code changes for consumers

### Before Phase 3 (if approved)
- **Notify users of upcoming breaking change**
- Provide migration guide
- Set clear timeline for deprecation
- Consider providing adapter/shim library

---

## Questions for User

1. **Phase 3 Decision:** Should we pursue block type consolidation?
   - Option A: **Skip** — Keep current 12 types, revisit for v2.0 (RECOMMENDED)
   - Option B: **Proceed** — Accept breaking change, do consolidation now

2. **Documentation Priority:** For cookbook (Change 1.4), which patterns are most important?
   - All 5 listed above?
   - Specific domain patterns you care about (financial, sales, etc.)?

3. **Timeline:** Any deadline constraints? Should we batch all phases into one sprint?

---

## Files to Modify (Summary)

### Phase 1
- `src/Core/KineticReports.Core/Engine/Expressions/ExpressionContext.cs` (modify)
- `src/Core/KineticReports.Core/Layout/BlockTypeHierarchyGuide.cs` (new)
- `src/Core/KineticReports.Core/LayoutEngine/LayoutOptions.cs` (modify)
- `docs/COMPREHENSIVE_DEVELOPER_GUIDE.md` (update)
- `tests/unit-tests/KineticReports.Engine.Tests/Expressions/ExpressionContextTests.cs` (new)
- `tests/unit-tests/KineticReports.Core.Tests/LayoutEngine/LayoutOptionsPresetsTests.cs` (new)

### Phase 2
- `src/Core/KineticReports.Core/Styling/StyleCascadeResolver.cs` (new)
- `tests/unit-tests/KineticReports.Core.Tests/Styling/StyleCascadeResolverTests.cs` (new)
- `docs/COMPREHENSIVE_DEVELOPER_GUIDE.md` (modify styling section to reference resolver)

### Phase 3 (if approved)
- ~20+ files across Layout namespace
- All test files affected
- All sample projects affected
- Full major version update

---

**End of Implementation Plan**
