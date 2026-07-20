# Expression Language Specification

**Specification Version:** 0.1 **Status:** Draft

# 1. Purpose

The Expression Language (RXL) provides calculated values, conditional
formatting, filtering, grouping, sorting, parameters, and aggregate
functions.

Design goals:

-   Safe by default
-   Deterministic
-   Fast
-   Cacheable
-   Extensible
-   Familiar to C# developers

------------------------------------------------------------------------

# 2. Architecture

``` text
Expression Text
      │
      ▼
Tokenizer
      ▼
Parser
      ▼
Abstract Syntax Tree (AST)
      ▼
Semantic Binder
      ▼
Optimizer
      ▼
Compiled Delegate
      ▼
Execution
```

Expressions are compiled once and reused.

------------------------------------------------------------------------

# 3. Language Features

Supported:

-   Arithmetic
-   Boolean logic
-   String operations
-   Date/Time
-   Null coalescing
-   Conditional expressions
-   Collection functions
-   Aggregates
-   Parameters
-   Variables

Future:

-   Pattern matching
-   User-defined functions
-   Lambda expressions

------------------------------------------------------------------------

# 4. Syntax

Examples

``` text
=Customer.Name

=Order.Total * 0.08

=Sum(LineItems.Amount)

=If(Order.Total > 1000, "Large", "Normal")

=Now()

=Parameter("StartDate")
```

------------------------------------------------------------------------

# 5. Primitive Types

-   String
-   Boolean
-   Int32
-   Int64
-   Decimal
-   Double
-   DateTime
-   Guid
-   TimeSpan

Nullable versions supported.

------------------------------------------------------------------------

# 6. Type Conversion

Implicit:

-   Int → Decimal
-   Int → Double

Explicit:

-   String → Int
-   String → DateTime

Invalid conversions raise compile-time diagnostics.

------------------------------------------------------------------------

# 7. Variables

Scopes:

Global

↓

Report

↓

Page

↓

Group

↓

Row

Inner scopes shadow outer scopes.

------------------------------------------------------------------------

# 8. Built-in Functions

Math

-   Abs
-   Round
-   Ceiling
-   Floor
-   Min
-   Max

String

-   Length
-   Upper
-   Lower
-   Trim
-   Replace
-   Substring

Date

-   Today
-   Now
-   AddDays
-   Year
-   Month
-   Day

Collections

-   Count
-   Sum
-   Average
-   Min
-   Max
-   First
-   Last

Logical

-   If
-   Coalesce
-   IsNull
-   IsEmpty

------------------------------------------------------------------------

# 9. Aggregates

Supported scopes:

-   Group
-   Page
-   Report

Aggregates are evaluated after data binding.

------------------------------------------------------------------------

# 10. Compilation

Pipeline

``` text
Source
 ↓
Lexer
 ↓
Parser
 ↓
AST
 ↓
Binding
 ↓
Optimization
 ↓
Compilation
 ↓
Cached Delegate
```

------------------------------------------------------------------------

# 11. Caching

Compiled expressions are cached using:

Hash(Source + Context + Version)

Cache is thread-safe.

------------------------------------------------------------------------

# 12. Security

The expression runtime MUST NOT permit:

-   Reflection
-   File access
-   Networking
-   Environment variables
-   Arbitrary assembly loading
-   Process execution

Only approved functions are available.

------------------------------------------------------------------------

# 13. Extensibility

Plugins may register:

-   Functions
-   Constants
-   Type converters
-   Validators

Interface:

``` csharp
IExpressionFunction
```

------------------------------------------------------------------------

# 14. Diagnostics

Compilation errors include:

-   Code
-   Severity
-   Message
-   Source location
-   Suggested fix

------------------------------------------------------------------------

# 15. Performance Targets

Compile once.

Reuse many times.

Execution should avoid allocations.

Expression evaluation should not dominate report generation time.

------------------------------------------------------------------------

# 16. Testing

Unit tests:

-   Parsing
-   Binding
-   Type checking
-   Compilation
-   Aggregates
-   Null propagation

Golden tests:

-   Cached compilation
-   Thread safety

------------------------------------------------------------------------

# ADRs

ADR-031 Expressions compile before execution.

ADR-032 Roslyn-inspired AST architecture.

ADR-033 Sandboxed runtime.

ADR-034 Plugin functions are explicitly registered.

ADR-035 Compiled delegates are immutable.

------------------------------------------------------------------------

# Open Questions

-   Roslyn scripting vs custom parser
-   Generic functions
-   Async functions
-   Localization-aware formatting
-   Expression debugging

------------------------------------------------------------------------

# Next Specification

Style System Specification

Focus:

-   Cascading
-   Theme inheritance
-   Font resolution
-   Borders
-   Brushes
-   Color management
-   Conditional formatting
