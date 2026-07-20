# Reporting Platform Architecture Handbook

**Version:** 0.1 (Living Document)

## Purpose

This handbook defines the architecture, engineering standards, and
implementation roadmap for a modern, cross-platform reporting platform
targeting .NET 10 and beyond. It is intended to serve as the project's
primary technical specification.

------------------------------------------------------------------------

# Table of Contents

1.  Vision and Goals
2.  Non-Goals
3.  Product Principles
4.  High-Level Architecture
5.  Solution Structure
6.  Domain Model
7.  Report Definition Schema
8.  Fluent Builder
9.  Designer Architecture
10. Layout Engine
11. Rendering Engine
12. Graphics Abstraction
13. Export Architecture
14. Data Provider Architecture
15. Expression Engine
16. Style System
17. Pagination Algorithms
18. Tables and Crosstabs
19. Charts and Barcodes
20. Viewer Architecture
21. Plugin SDK
22. Public API Design
23. Versioning Strategy
24. Performance Targets
25. Testing Strategy
26. Accessibility
27. Security
28. CI/CD
29. Coding Standards
30. Architectural Decision Records
31. Phased Roadmap

------------------------------------------------------------------------

# 1. Vision and Goals

The platform should be:

-   Cross-platform
-   UI framework agnostic
-   Data source agnostic
-   Rendering agnostic
-   Exporter agnostic
-   Extensible through plugins
-   Open-source friendly

The canonical artifact is a versioned JSON report definition.

------------------------------------------------------------------------

# 2. Non-Goals

-   Windows-only APIs
-   GDI+-based rendering
-   Tight coupling between designer and runtime
-   XML as the primary persistence format

------------------------------------------------------------------------

# 3. Product Principles

-   Separation of concerns
-   Immutable report definitions during execution
-   Deterministic layout
-   Predictable rendering
-   Dependency injection everywhere
-   Plugin-first architecture

------------------------------------------------------------------------

# 4. High-Level Architecture

Designer / Fluent Builder ↓ ReportDefinition (JSON) ↓ Engine ↓ Layout
Tree ↓ Renderer ↓ Exporters / Viewers

------------------------------------------------------------------------

# 5. Solution Structure

Describe each project:

-   Reporting.Core
-   Reporting.Engine
-   Reporting.Layout
-   Reporting.Rendering
-   Reporting.Rendering.Skia
-   Reporting.Export.Html
-   Reporting.Export.Excel
-   Reporting.Export.Pdf
-   Reporting.Data.SqlServer
-   Reporting.Data.Rest
-   Reporting.Viewer.Web
-   Reporting.Viewer.Blazor
-   Reporting.Designer.Uno
-   Reporting.Plugins
-   Samples

Each project owns a single responsibility.

------------------------------------------------------------------------

# 6. Domain Model

Core entities:

-   Report
-   Page
-   Section
-   Band
-   Table
-   Cell
-   Text
-   Image
-   Shape
-   Chart
-   Barcode
-   Parameter
-   DataSource
-   Style

Every object has: - Stable identifier - Version support - Metadata bag -
Serialization contract

------------------------------------------------------------------------

# 7. JSON Report Definition

The JSON schema is the platform contract.

Goals:

-   Human readable
-   Version tolerant
-   Language neutral
-   Backward compatible

Future work: - Publish JSON Schema - Automatic migration tooling

------------------------------------------------------------------------

# 8. Fluent Builder

Provide a strongly typed builder using lambda expressions.

Goals: - IntelliSense friendly - Compile-time validation where
possible - Same output as designer

------------------------------------------------------------------------

# 9. Designer Architecture

Uno Platform desktop application.

Subsystems:

-   Canvas
-   Toolbox
-   Property Grid
-   Layer Explorer
-   Alignment Guides
-   Selection Model
-   Clipboard
-   Undo/Redo
-   Zoom
-   Snap
-   Serialization

Designer edits JSON only.

------------------------------------------------------------------------

# 10. Layout Engine

Pipeline:

Measure Arrange Paginate Render

No exporter performs layout.

------------------------------------------------------------------------

# 11. Rendering Engine

Consumes layout tree only.

Provides drawing primitives:

-   DrawText
-   DrawImage
-   DrawPath
-   DrawRectangle
-   Clip
-   Transform

------------------------------------------------------------------------

# 12. Graphics Abstraction

Primary backend: - SkiaSharp

Future: - Canvas2D - SVG - GPU acceleration

------------------------------------------------------------------------

# 13. Export Architecture

Interfaces:

-   IReportExporter
-   IExportContext

Initial exporters:

-   HTML
-   PDF
-   Excel

Future:

-   DOCX
-   PowerPoint
-   SVG
-   Markdown
-   Image

------------------------------------------------------------------------

# 14. Data Providers

Common interface:

IDataSourceProvider

Initial implementations:

-   SQL Server
-   PostgreSQL
-   SQLite
-   REST
-   IEnumerable`<T>`{=html}
-   DataTable

Future:

-   GraphQL
-   OData
-   Oracle
-   SAP
-   MongoDB

------------------------------------------------------------------------

# 15. Expression Engine

Use a restricted Roslyn evaluator.

Capabilities:

-   Calculated fields
-   Aggregates
-   Conditional formatting
-   Variables
-   Functions

------------------------------------------------------------------------

# 16. Style System

CSS-inspired cascading model.

Objects:

-   Theme
-   Style
-   Font
-   Border
-   Fill
-   Padding
-   Margin

------------------------------------------------------------------------

# 17. Pagination

Support:

-   Repeat headers
-   KeepTogether
-   KeepWithNext
-   Widow/Orphan control
-   Soft page breaks

------------------------------------------------------------------------

# 18. Tables

Features:

-   Dynamic rows
-   Nested tables
-   Grouping
-   Totals
-   Crosstabs
-   Virtualized layout

------------------------------------------------------------------------

# 19. Charts & Barcodes

Initial:

-   LiveCharts2
-   ZXing.Net
-   QRCoder

------------------------------------------------------------------------

# 20. Viewer Architecture

Preferred rendering: HTML

Hosts:

-   Blazor
-   MVC
-   Razor Pages
-   Uno WebView
-   MAUI
-   Web Component

------------------------------------------------------------------------

# 21. Plugin SDK

Extension points:

-   Exporters
-   Data Providers
-   Controls
-   Expressions
-   Themes

------------------------------------------------------------------------

# 22. Public API

Rules:

-   Async-first
-   Immutable models
-   Nullable enabled
-   Semantic Versioning

------------------------------------------------------------------------

# 23. Versioning

Semantic versioning.

Versioned report schema.

Automatic migrations.

------------------------------------------------------------------------

# 24. Performance Goals

-   First page \<250ms for typical reports
-   Streaming rendering
-   Minimal allocations
-   Incremental pagination

------------------------------------------------------------------------

# 25. Testing

-   Unit
-   Snapshot
-   Golden images
-   Performance
-   Integration
-   Cross-platform

------------------------------------------------------------------------

# 26. Accessibility

Designer: - Keyboard navigation - Screen reader support - High contrast

Viewer: - Semantic HTML - WCAG compliance

------------------------------------------------------------------------

# 27. Security

-   Parameterized SQL
-   Sandboxed expressions
-   Signed plugins
-   Export sanitization

------------------------------------------------------------------------

# 28. CI/CD

GitHub Actions:

-   Build
-   Test
-   Benchmark
-   Package
-   Publish docs

------------------------------------------------------------------------

# 29. Coding Standards

-   EditorConfig
-   Nullable enabled
-   XML docs
-   Analyzer enforcement

------------------------------------------------------------------------

# 30. Architectural Decision Records

ADR-001 JSON schema is canonical.

ADR-002 Layout separate from rendering.

ADR-003 SkiaSharp graphics backend.

ADR-004 HTML viewer.

ADR-005 Plugin-first architecture.

ADR-006 Roslyn expression engine.

ADR-007 Web Components for cross-framework viewing.

ADR-008 Dependency Injection throughout.

ADR-009 Exporters consume layout tree only.

ADR-010 Versioned report schema.

(Add additional ADRs as the platform evolves.)

------------------------------------------------------------------------

# 31. Multi-Year Roadmap

## Milestone 1

Foundation: - Core model - Serialization - Engine skeleton

## Milestone 2

Layout engine

## Milestone 3

Rendering

## Milestone 4

HTML viewer

## Milestone 5

Data providers

## Milestone 6

PDF/Excel exporters

## Milestone 7

Fluent builder

## Milestone 8

Uno designer MVP

## Milestone 9

Charts, barcodes, advanced layout

## Milestone 10

Plugin marketplace

## Milestone 11

JavaScript SDK & Web Components

## Milestone 12

Additional language support

------------------------------------------------------------------------

# Appendix A

Recommended repository conventions.

# Appendix B

JSON schema evolution guidelines.

# Appendix C

Contributor onboarding checklist.

# Appendix D

Future RFC process replacing ADRs where appropriate.
