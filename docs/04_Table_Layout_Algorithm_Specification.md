# Table Layout Algorithm Specification

**Status:** Draft v0.1

## Purpose

This specification defines the deterministic table layout algorithm used
by the reporting engine. Every renderer relies on the computed layout
produced by this algorithm.

------------------------------------------------------------------------

# Goals

-   Deterministic output
-   Stable pagination
-   Efficient for large datasets
-   Nested table support
-   Device independent

------------------------------------------------------------------------

# Terminology

-   Table
-   Column
-   Row
-   Cell
-   Grid
-   Span
-   Header
-   Footer
-   Detail

------------------------------------------------------------------------

# Processing Pipeline

``` text
Logical Table
    ↓
Resolve Columns
    ↓
Measure Cells
    ↓
Resolve Spans
    ↓
Calculate Row Heights
    ↓
Paginate
    ↓
Immutable TableLayout
```

------------------------------------------------------------------------

# Column Width Modes

## Fixed

Uses an explicit width.

## Auto

Determined by the maximum intrinsic width of visible cell contents.

## Percentage

Consumes a percentage of the available table width.

## Star (Weighted)

Consumes remaining space after Fixed and Auto columns have been
resolved.

Example:

  Column   Mode          Width
  -------- ------- -----------
  A        Fixed           120
  B        Auto      Intrinsic
  C        30%        Relative
  D        2\*       Remaining

------------------------------------------------------------------------

# Width Resolution Order

1.  Fixed
2.  Auto minimum
3.  Percentage
4.  Star allocation
5.  Constraint validation

If overflow occurs:

-   shrink Auto
-   shrink Star
-   report overflow if fixed constraints cannot be satisfied

------------------------------------------------------------------------

# Cell Measurement

Each cell computes:

-   Minimum width
-   Preferred width
-   Minimum height
-   Preferred height
-   Baseline

Inputs:

-   Font metrics
-   Padding
-   Borders
-   Wrapping
-   Images
-   Nested controls

------------------------------------------------------------------------

# Row Height Algorithm

Row height equals the maximum measured height of participating cells
after spans are resolved.

Pseudo-process:

1.  Measure all cells.
2.  Apply RowSpan constraints.
3.  Expand affected rows.
4.  Recompute dependent spans.

------------------------------------------------------------------------

# ColSpan

Algorithm:

-   Merge participating columns.
-   Sum available width.
-   Measure against merged width.
-   Distribute overflow proportionally.

------------------------------------------------------------------------

# RowSpan

Algorithm:

-   Measure total required height.
-   Distribute additional height across participating rows.
-   Preserve minimum heights.

------------------------------------------------------------------------

# Nested Tables

Rules:

-   Measured recursively.
-   Parent width constrains child width.
-   Child overflow bubbles upward.
-   Pagination remains independent.

------------------------------------------------------------------------

# Pagination

Rows are processed sequentially.

Decision matrix:

  Condition                    Action
  ---------------------------- -------------------
  Fits page                    Render
  Doesn't fit & splittable     Split
  Doesn't fit & KeepTogether   Move to next page
  Header                       Repeat

------------------------------------------------------------------------

# Repeating Headers

Header rows repeat on every page after the first occurrence.

Rules:

-   Preserve style.
-   Preserve merged cells.
-   Preserve calculated widths.

------------------------------------------------------------------------

# KeepTogether

Applies to:

-   Row
-   Group
-   Table

Hierarchy:

Table → Group → Row

------------------------------------------------------------------------

# Text Wrapping

Modes:

-   None
-   Word
-   Character
-   Ellipsis

Height is calculated after wrapping.

------------------------------------------------------------------------

# Alignment

Horizontal:

-   Left
-   Center
-   Right
-   Justify

Vertical:

-   Top
-   Middle
-   Bottom

------------------------------------------------------------------------

# Border Resolution

Adjacent borders collapse according to precedence:

1.  Explicit cell border
2.  Row border
3.  Column border
4.  Table border

------------------------------------------------------------------------

# Complexity Targets

  Operation             Target
  --------------------- --------
  Width resolution      O(c)
  Cell measurement      O(n)
  Pagination            O(r)
  Rendering traversal   O(n)

Where:

-   c = columns
-   r = rows
-   n = cells

------------------------------------------------------------------------

# Memory Model

The immutable TableLayout contains:

-   Resolved column widths
-   Resolved row heights
-   Cell rectangles
-   Pagination metadata

No mutable state is retained after layout.

------------------------------------------------------------------------

# Testing Matrix

Scenarios:

-   Empty table
-   Large table
-   Nested tables
-   RowSpan
-   ColSpan
-   Images
-   RTL text
-   Variable fonts
-   Page breaks
-   Thousands of rows

------------------------------------------------------------------------

# ADRs

ADR-026 Width resolution precedes height calculation.

ADR-027 Pagination occurs after all measurements.

ADR-028 Nested tables use recursive layout.

ADR-029 Layout output is immutable.

ADR-030 Column widths are stable across page breaks.

------------------------------------------------------------------------

# Future Work

-   Virtualized measurement
-   Multi-column reports
-   Adaptive table layout
-   Constraint solver
-   CSS Grid-inspired containers
-   Incremental re-layout
