# Data Provider SDK Specification

**Specification Version:** 0.1 **Status:** Draft

# Purpose

The Data Provider SDK defines how data is retrieved for reports. Every
provider, regardless of backend technology, exposes a consistent
asynchronous streaming interface to the reporting runtime.

------------------------------------------------------------------------

# Design Principles

-   Provider agnostic
-   Streaming first
-   Async by default
-   Parameterized queries
-   Immutable results
-   Dependency Injection based

------------------------------------------------------------------------

# Architecture

``` text
Report
  │
  ▼
Dataset
  │
  ▼
IDataProvider
  │
  ▼
Provider Adapter
  │
  ▼
Backend

SQL Server
PostgreSQL
SQLite
REST
GraphQL
CSV
Excel
MongoDB
Custom
```

------------------------------------------------------------------------

# Core Contracts

## IDataProvider

``` csharp
public interface IDataProvider
{
    string Name { get; }

    Task<QueryResult> ExecuteAsync(
        QueryRequest request,
        ReportRuntimeContext context);
}
```

## QueryRequest

Contains:

-   Dataset name
-   Query text
-   Parameters
-   Timeout
-   CancellationToken

## QueryResult

Contains:

-   Schema
-   Rows
-   Diagnostics
-   Execution statistics

Rows SHOULD be streamed whenever practical.

------------------------------------------------------------------------

# Dataset Lifecycle

``` text
Resolve Provider
      ↓
Bind Parameters
      ↓
Validate
      ↓
Execute
      ↓
Stream Rows
      ↓
Complete
```

------------------------------------------------------------------------

# Parameter Binding

Supported types:

-   String
-   Numeric
-   Boolean
-   DateTime
-   Guid
-   Collections

Providers MUST use parameterized execution.

String concatenation for parameter injection is prohibited.

------------------------------------------------------------------------

# Streaming

Large datasets SHOULD use:

``` text
IAsyncEnumerable<Row>
```

The runtime MUST process rows incrementally when supported.

------------------------------------------------------------------------

# Schema

Columns include:

-   Name
-   CLR Type
-   Nullable
-   Ordinal
-   Display Name (optional)

Providers SHOULD preserve native types.

------------------------------------------------------------------------

# Authentication

Authentication is provider specific.

Examples:

-   Connection strings
-   OAuth
-   API keys
-   Managed Identity
-   Windows Authentication

Credentials SHOULD NOT be embedded in report definitions.

------------------------------------------------------------------------

# Transactions

Read-only providers ignore transactions.

Transactional providers MAY participate in ambient transactions.

------------------------------------------------------------------------

# Retry Policy

Transient failures MAY be retried.

Suggested defaults:

-   3 attempts
-   Exponential backoff
-   Provider-specific overrides

------------------------------------------------------------------------

# Caching

Optional cache levels:

-   Query plan
-   Metadata
-   Result set
-   Connection pool

The runtime controls cache lifetimes.

------------------------------------------------------------------------

# Diagnostics

Providers report:

-   Duration
-   Rows returned
-   Warnings
-   Errors

Diagnostics flow through the Runtime Kernel.

------------------------------------------------------------------------

# Security

Providers MUST:

-   Use parameterized execution
-   Validate inputs
-   Respect cancellation
-   Avoid leaking credentials

Providers MUST NOT expose secrets through diagnostics.

------------------------------------------------------------------------

# Performance Targets

-   Async I/O
-   Streaming by default
-   Minimal buffering
-   Connection reuse
-   Metadata caching

------------------------------------------------------------------------

# Built-in Providers

Phase 1

-   SQL Server
-   PostgreSQL
-   SQLite
-   Object Collection

Phase 2

-   REST
-   GraphQL
-   CSV
-   Excel

Phase 3

-   MongoDB
-   OData
-   SAP
-   Salesforce
-   Custom Providers

------------------------------------------------------------------------

# Testing

Certification tests:

-   Parameter binding
-   Streaming correctness
-   Cancellation
-   Retry behavior
-   Authentication
-   Schema fidelity
-   Concurrent execution

------------------------------------------------------------------------

# ADRs

ADR-051 Providers are asynchronous.

ADR-052 Streaming is the preferred execution model.

ADR-053 Credentials are external to report definitions.

ADR-054 Parameterized execution is mandatory.

ADR-055 Providers integrate through the Runtime Kernel.

------------------------------------------------------------------------

# Future Specifications

-   Graphics & Typography
-   Viewer Architecture
-   Uno Designer
-   Export SDK
-   Report Compiler
-   Compiled Report Format
