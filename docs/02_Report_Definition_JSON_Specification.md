This archive index reserves the position for Report_Definition_JSON_Specification.md. Use the previously generated document of this name in the matching slot.# Report Definition JSON Schema Specification

**Status:** Draft v0.1

## Purpose

The JSON Report Definition (JRD) is the canonical, versioned
representation of a report. Every producer (designer, fluent builder,
future SDKs) emits JRD. Every consumer (engine, viewer, exporters)
consumes JRD.

------------------------------------------------------------------------

# Guiding Principles

-   Language neutral
-   Human readable
-   Deterministic
-   Version tolerant
-   Forward compatible where possible
-   Stable identifiers
-   Explicit defaults

------------------------------------------------------------------------

# Top-Level Document

``` json
{
  "schemaVersion": "1.0",
  "id": "invoice",
  "name": "Invoice",
  "metadata": {},
  "parameters": [],
  "dataSources": [],
  "styles": [],
  "pages": []
}
```

## Required Properties

  Property        Description
  --------------- --------------------------
  schemaVersion   Version of the schema
  id              Stable unique identifier
  name            Display name
  pages           Page definitions

------------------------------------------------------------------------

# Object Model

Report - Metadata - Parameters - Data Sources - Styles - Pages

Page - Header - Body - Footer

Body - Containers - Tables - Text - Images - Charts - Shapes

------------------------------------------------------------------------

# Universal Base Object

Every visual element inherits:

``` json
{
  "id": "guid",
  "type": "Text",
  "name": "CustomerName",
  "bounds": {},
  "style": "Heading1",
  "metadata": {}
}
```

Rules: - id is immutable. - type is required. - metadata is an
extensibility bag.

------------------------------------------------------------------------

# Coordinate Model

All measurements use Device Independent Pixels (96 DPI).

Supported units: - px (internal) - in - cm - mm - pt

Serialized values normalize to DIPs.

------------------------------------------------------------------------

# Styles

Styles are referenced by id.

``` json
{
  "id":"Heading1",
  "font":{
    "family":"Inter",
    "size":16
  }
}
```

Style resolution:

Theme → Report Defaults → Named Style → Parent → Local Override

------------------------------------------------------------------------

# Parameters

Supported types: - String - Integer - Decimal - Boolean - DateTime -
Guid - Enum

Future: - Arrays - Complex objects

------------------------------------------------------------------------

# Data Sources

``` json
{
  "id":"Customers",
  "provider":"SqlServer",
  "connection":"Sales",
  "query":"SELECT * FROM Customers"
}
```

Future providers: - REST - GraphQL - MongoDB - OData

------------------------------------------------------------------------

# Expressions

Expressions are serialized as strings.

Examples:

    =Customer.Name
    =Sum(Orders.Total)
    =Now()

Expression language version is independent of schema version.

------------------------------------------------------------------------

# Tables

Columns Rows Cells

Each cell may contain: - Text - Image - Container - Nested table

------------------------------------------------------------------------

# Versioning Rules

Major version: - Breaking changes

Minor version: - Backward compatible additions

Patch: - Documentation / validation improvements

Migration library must support: 1. Upgrade 2. Downgrade (best effort) 3.
Validation 4. Auto-fixes

------------------------------------------------------------------------

# Validation

Validation stages:

1.  JSON syntax
2.  JSON Schema
3.  Semantic validation
4.  Engine validation

------------------------------------------------------------------------

# Serialization Guidelines

-   camelCase
-   UTF-8
-   ISO-8601
-   No duplicated state
-   No computed layout

Layout is runtime-only.

------------------------------------------------------------------------

# Extension Model

Custom objects:

``` json
{
  "type":"Acme.QRCode"
}
```

Unknown types must be preserved during round-trip serialization.

------------------------------------------------------------------------

# ADRs

ADR-016: JSON is the only persisted report format.

ADR-017: Layout data is never serialized.

ADR-018: Unknown properties are preserved.

ADR-019: Stable identifiers are mandatory.

ADR-020: Schema and expression versions evolve independently.

------------------------------------------------------------------------

# Open Questions

-   JSON Schema generation
-   Binary packaging for embedded resources
-   Localization packaging
-   Incremental loading
-   Streaming report definitions

------------------------------------------------------------------------

# Next Specification

Rendering Pipeline Specification

Topics: - Render tree - Graphics abstraction - Font metrics - Text
shaping - Image pipeline - Clipping - Layers - Transparency - Resource
caching
