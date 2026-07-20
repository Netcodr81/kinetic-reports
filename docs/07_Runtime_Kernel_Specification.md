# Runtime Kernel Specification

**Specification Version:** 0.1 **Status:** Draft

# Purpose

The Runtime Kernel is the execution backbone of the reporting platform.
It coordinates services, caches, plugins, diagnostics, and execution
state while keeping the layout, rendering, data, and export subsystems
decoupled.

The Runtime Kernel is **not** responsible for report layout or
rendering. It provides the shared infrastructure those systems depend
on.

------------------------------------------------------------------------

# Architectural Goals

-   Immutable execution context
-   Dependency Injection first
-   Thread-safe
-   Testable
-   Plugin friendly
-   Cross-platform
-   Minimal allocations

------------------------------------------------------------------------

# High-Level Architecture

``` text
             ReportEngine
                  │
                  ▼
        ReportRuntimeContext
                  │
 ┌────────────────┼────────────────┐
 │                │                │
 ▼                ▼                ▼
Services       Registries       Caches
 │                │                │
 ▼                ▼                ▼
Execution     Plugins        Shared Resources
```

------------------------------------------------------------------------

# Core Runtime Objects

## ReportRuntimeContext

The immutable context passed to every subsystem.

Suggested members:

``` csharp
public sealed class ReportRuntimeContext
{
    IServiceProvider Services;
    IFontManager Fonts;
    IImageManager Images;
    IThemeManager Themes;
    IExpressionEngine Expressions;
    IDataProviderRegistry DataProviders;
    IPluginRegistry Plugins;
    IResourceCache Resources;
    IDiagnosticSink Diagnostics;
    ILogger Logger;
    CancellationToken CancellationToken;
    ReportOptions Options;
}
```

This object MUST be immutable once execution begins.

------------------------------------------------------------------------

# Execution Pipeline

``` text
Load Definition
      ↓
Validate
      ↓
Create Runtime Context
      ↓
Resolve Data Sources
      ↓
Evaluate Expressions
      ↓
Build Layout Tree
      ↓
Render
      ↓
Export
```

Each stage receives the same Runtime Context.

------------------------------------------------------------------------

# Service Categories

## Infrastructure

-   Logging
-   Localization
-   Configuration
-   Licensing (future)
-   Telemetry (future)

## Runtime

-   Expression Engine
-   Data Provider Registry
-   Theme Manager
-   Resource Manager

## Rendering

-   Font Manager
-   Image Manager
-   Graphics Factory

------------------------------------------------------------------------

# Resource Management

Shared resources:

-   Fonts
-   Images
-   SVG documents
-   Color palettes
-   Brushes
-   Compiled expressions
-   Data source metadata

Resources are cached and reference counted where appropriate.

------------------------------------------------------------------------

# Registries

The runtime owns several registries.

## IDataProviderRegistry

Maps provider names to implementations.

## IExporterRegistry

Discovers exporters.

## IPluginRegistry

Discovers extensions.

## IControlRegistry

Maps report object types to runtime controls.

------------------------------------------------------------------------

# Diagnostics

All diagnostics flow through:

``` text
Diagnostic
    ↓
Diagnostic Sink
    ↓
Logger
    ↓
Designer
    ↓
Viewer
```

Diagnostic severities:

-   Information
-   Warning
-   Error
-   Critical

------------------------------------------------------------------------

# Execution State

Mutable execution state is isolated.

Contains:

-   Current page
-   Current group
-   Current row
-   Variables
-   Aggregate state
-   Parameter values

Execution state MUST NOT modify the Report Definition.

------------------------------------------------------------------------

# Concurrency Model

Goals:

-   Parallel data loading
-   Parallel image decoding
-   Parallel expression compilation
-   Sequential layout
-   Parallel rendering (future)

The Runtime Context itself remains immutable.

------------------------------------------------------------------------

# Dependency Injection

All services are registered through
Microsoft.Extensions.DependencyInjection.

Subsystems MUST request abstractions rather than concrete
implementations.

------------------------------------------------------------------------

# Plugin Lifecycle

``` text
Discover
    ↓
Validate
    ↓
Register
    ↓
Activate
    ↓
Use
    ↓
Dispose
```

Plugins execute within the Runtime Context.

------------------------------------------------------------------------

# Memory Model

Long-lived:

-   Fonts
-   Themes
-   Exporters
-   Plugins

Per execution:

-   Variables
-   Layout Tree
-   Execution State

Transient:

-   Draw commands
-   Temporary buffers

------------------------------------------------------------------------

# Performance Targets

-   Startup \< 100 ms (warm)
-   Zero allocations during steady-state rendering where practical
-   Shared immutable caches
-   Lock-free read paths

------------------------------------------------------------------------

# Error Recovery

Recoverable:

-   Missing font
-   Missing image
-   Unknown exporter
-   Unknown plugin

Fatal:

-   Invalid report schema
-   Corrupt layout tree
-   Circular expression dependencies

------------------------------------------------------------------------

# Testing

Unit:

-   Runtime context immutability
-   Registry resolution
-   Plugin discovery
-   Cache correctness

Integration:

-   End-to-end report execution
-   Multi-threaded execution
-   Large report memory usage

------------------------------------------------------------------------

# ADRs

ADR-041 Runtime Context is immutable.

ADR-042 Services are resolved through DI.

ADR-043 Execution state is isolated from report definitions.

ADR-044 Plugins execute inside the Runtime Context.

ADR-045 Shared resources are centrally cached.

------------------------------------------------------------------------

# Next Specifications

1.  Plugin SDK
2.  Data Provider SDK
3.  Graphics & Typography
4.  Viewer Architecture
5.  Uno Designer Architecture
6.  Export SDK
