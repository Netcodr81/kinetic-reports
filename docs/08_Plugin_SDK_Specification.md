# Plugin SDK Specification

**Specification Version:** 0.1 **Status:** Draft

# Purpose

The Plugin SDK defines the extensibility model for the reporting
platform. Every extension---including exporters, data providers,
controls, expression functions, themes, and designer tools---uses the
same discovery and lifecycle model.

------------------------------------------------------------------------

# Design Goals

-   Stable public API
-   Versioned contracts
-   Dependency Injection first
-   Cross-platform
-   Secure by default
-   Side-by-side plugin versions where practical

------------------------------------------------------------------------

# Plugin Categories

  Category              Purpose
  --------------------- -------------------------------------
  Data Provider         Retrieve data
  Exporter              Produce output formats
  Report Object         Custom visual controls
  Expression Function   Extend expression language
  Theme                 Visual themes
  Designer Extension    Toolbox, property editors, commands
  Viewer Extension      Search providers, annotations, UI

------------------------------------------------------------------------

# Package Layout

``` text
Acme.Reporting.Excel/
 ├── Acme.Reporting.Excel.dll
 ├── plugin.json
 ├── icon.png
 ├── README.md
 └── LICENSE
```

------------------------------------------------------------------------

# Manifest

``` json
{
  "id": "Acme.Excel",
  "name": "Excel Exporter",
  "version": "1.0.0",
  "platformVersion": "1.x",
  "author": "Acme",
  "entryAssembly": "Acme.Reporting.Excel.dll",
  "services": [
    "Exporter"
  ]
}
```

------------------------------------------------------------------------

# Discovery

Sources:

-   Application plugin folder
-   NuGet package references
-   Explicit registration
-   Future online marketplace

Discovery pipeline:

``` text
Locate
  ↓
Read Manifest
  ↓
Validate
  ↓
Load Assembly
  ↓
Register Services
  ↓
Activate
```

------------------------------------------------------------------------

# Dependency Injection

Plugins register services through:

``` csharp
public interface IReportingPlugin
{
    void ConfigureServices(IServiceCollection services);

    void Configure(IPluginContext context);
}
```

Plugins MUST register abstractions.

------------------------------------------------------------------------

# Runtime Context

Every plugin receives:

-   ReportRuntimeContext
-   IServiceProvider
-   ILogger
-   CancellationToken

Plugins MUST NOT construct runtime infrastructure.

------------------------------------------------------------------------

# Version Compatibility

Compatibility levels:

-   Major
-   Minor
-   Patch

Rules:

-   Major mismatch → reject
-   Minor mismatch → warn if supported
-   Patch mismatch → allowed

------------------------------------------------------------------------

# Exporter Contract

``` csharp
public interface IReportExporter
{
    string Format { get; }

    Task ExportAsync(
        LayoutDocument document,
        Stream output,
        ReportRuntimeContext context);
}
```

------------------------------------------------------------------------

# Data Provider Contract

``` csharp
public interface IDataProvider
{
    string Name { get; }

    Task<IAsyncEnumerable<Row>> ExecuteAsync(
        QueryContext context);
}
```

Providers should stream rows where possible.

------------------------------------------------------------------------

# Report Object Contract

Each object provides:

-   Measurement
-   Arrangement
-   Rendering
-   Serialization metadata

Custom objects participate in layout exactly like built-in objects.

------------------------------------------------------------------------

# Expression Extensions

Plugins may register:

-   Scalar functions
-   Aggregate functions
-   Constants
-   Type converters

Names must be globally unique.

------------------------------------------------------------------------

# Security Model

Plugins:

-   MUST execute in managed code.
-   MUST NOT modify runtime state outside published APIs.
-   SHOULD declare permissions in the manifest.

Future:

-   Package signing
-   Trust policies
-   Sandboxed execution

------------------------------------------------------------------------

# Lifecycle

``` text
Discover
  ↓
Validate
  ↓
Register
  ↓
Initialize
  ↓
Execute
  ↓
Dispose
```

------------------------------------------------------------------------

# Diagnostics

Plugins report diagnostics through the platform sink.

Severity:

-   Info
-   Warning
-   Error
-   Critical

------------------------------------------------------------------------

# Performance Requirements

Plugins SHOULD:

-   Avoid blocking I/O
-   Cache immutable resources
-   Stream large datasets
-   Avoid unnecessary allocations

------------------------------------------------------------------------

# Testing

Plugin certification suite:

-   API compatibility
-   Serialization
-   Thread safety
-   Performance
-   Resource cleanup
-   Version negotiation

------------------------------------------------------------------------

# ADRs

ADR-046 Plugins register only through DI.

ADR-047 Public contracts are versioned.

ADR-048 Runtime context is supplied by the platform.

ADR-049 Plugin manifests are mandatory.

ADR-050 Plugin failures must be isolated and diagnosable.

------------------------------------------------------------------------

# Roadmap

Phase 1 - Exporters - Data Providers - Expression Functions

Phase 2 - Report Objects - Themes - Designer Extensions

Phase 3 - Viewer Extensions - Marketplace - Package Signing - Remote
Plugin Catalog

------------------------------------------------------------------------

# Next Specification

Data Provider SDK

Topics:

-   Query model
-   Parameter binding
-   Streaming
-   Transactions
-   Authentication
-   Connection management
-   Retry policies
-   Caching
