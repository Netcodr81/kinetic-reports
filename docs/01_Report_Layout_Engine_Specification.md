# Report Layout Engine Specification

**Status:** Draft v0.1

## Purpose

This specification defines the deterministic layout engine that all
renderers consume. It is the foundational document for the reporting
platform.

------------------------------------------------------------------------

# 1. Design Goals

-   Deterministic layout regardless of exporter
-   Device-independent rendering
-   Immutable layout tree
-   Pixel-consistent output
-   Testable and repeatable algorithms

------------------------------------------------------------------------

# 2. Pipeline

    ReportDefinition
          │
          ▼
    Data Resolution
          │
          ▼
    Expression Evaluation
          │
          ▼
    Logical Object Tree
          │
          ▼
    Measure Pass
          │
          ▼
    Arrange Pass
          │
          ▼
    Pagination
          │
          ▼
    Layout Tree
          │
          ▼
    Renderer

------------------------------------------------------------------------

# 3. Coordinate System

-   Internal unit: Device Independent Pixel (1/96 inch)
-   Floating point coordinates
-   Origin at page top-left
-   Affine transforms supported

------------------------------------------------------------------------

# 4. Layout Objects

Every visual object derives from LayoutElement.

Core types:

-   Page
-   Section
-   Band
-   Container
-   Text
-   Table
-   Row
-   Cell
-   Image
-   Shape
-   Chart
-   Barcode

Each exposes:

-   Measure()
-   Arrange()
-   Render()

------------------------------------------------------------------------

# 5. Measure Pass

Responsibilities:

-   Measure text using font metrics
-   Resolve percentage sizing
-   Resolve Auto width/height
-   Calculate intrinsic content size
-   Determine minimum and preferred sizes

No drawing occurs during measure.

------------------------------------------------------------------------

# 6. Arrange Pass

Responsibilities:

-   Assign final bounds
-   Resolve alignment
-   Apply padding and margins
-   Compute clipping regions
-   Produce immutable layout rectangles

------------------------------------------------------------------------

# 7. Pagination

Rules:

-   Repeat page headers
-   Repeat table headers
-   KeepTogether
-   KeepWithNext
-   Prevent orphan rows
-   Soft page breaks
-   Explicit page breaks

------------------------------------------------------------------------

# 8. Text Layout

Requirements:

-   Unicode
-   RTL support
-   Bidirectional text
-   Ligatures
-   Font fallback
-   Word wrapping
-   Character wrapping
-   Ellipsis
-   Vertical alignment
-   Baseline alignment

------------------------------------------------------------------------

# 9. Table Layout

Algorithm:

1.  Measure all cells
2.  Resolve column widths
3.  Resolve row heights
4.  Apply spans
5.  Detect overflow
6.  Split rows where permitted
7.  Paginate

Support:

-   RowSpan
-   ColSpan
-   Nested tables
-   Repeating headers
-   Group footers

------------------------------------------------------------------------

# 10. Style Resolution

Order:

1.  Theme
2.  Report defaults
3.  Named style
4.  Parent inheritance
5.  Local override

Resolved style becomes immutable before rendering.

------------------------------------------------------------------------

# 11. Rendering Contract

Renderers receive only:

-   Bounds
-   Text runs
-   Paths
-   Images
-   Styles

Renderers never calculate layout.

------------------------------------------------------------------------

# 12. Performance Targets

-   O(n) traversal
-   Streaming pagination
-   Cached text measurement
-   Shared font metrics
-   Object pooling where beneficial

------------------------------------------------------------------------

# 13. Testing

Golden tests:

-   Pagination
-   Text wrapping
-   Tables
-   Images
-   Charts
-   Nested containers

Snapshot testing ensures deterministic output.

------------------------------------------------------------------------

# 14. Future Enhancements

-   Multi-column layout
-   Flex/Grid-inspired containers
-   Constraints-based layout
-   Floating regions
-   Async image loading
-   Incremental re-layout
-   GPU-assisted preview

------------------------------------------------------------------------

# Initial ADRs

ADR-011: Layout is immutable after Arrange.

ADR-012: Pagination occurs after Arrange.

ADR-013: Exporters consume LayoutTree only.

ADR-014: Layout uses Device Independent Pixels.

ADR-015: Font metrics are centralized.

------------------------------------------------------------------------

# Next Specifications

1.  JSON Report Schema
2.  Table Layout Algorithm
3.  Expression Language
4.  Viewer Architecture
5.  Uno Designer
6.  Plugin SDK
7.  Export SDK
8.  Data Provider SDK

This document is intended to evolve into the authoritative specification
for the layout subsystem.
