# KineticReports Developer Guide

Welcome to the comprehensive developer guide for **KineticReports** — a deterministic, cross-platform reporting engine for .NET 10+.

This guide is organized as a learning path from fundamentals to advanced extension patterns.

---

## 🚀 Quick Start (5 minutes)

**New to KineticReports?** Start here:

1. [Getting Started](01_Getting_Started.md) — Install, build first report, generate output
2. [Architecture Overview](02_Architecture_Overview.md) — Understand how pieces fit together
3. Pick a use case below

---

## 📚 Learning Path

### For Report Designers
Want to create beautiful reports?

→ **[Report Definitions](04_Report_Definitions.md)**
- Learn JSON schema for reports
- Understand styling and typography
- Master layout and pagination
- See complete examples

### For Backend Developers
Want to integrate KineticReports into your application?

→ **[Core Concepts](03_Core_Concepts.md)** (5 min)
→ **[API Reference](10_API_Reference.md)** (10 min)
→ Build a simple report generator
→ Add data binding and parameters

### For Plugin Developers
Want to extend KineticReports with custom features?

→ **[Extension Points](05_Extension_Points.md)** (overview of all extension mechanisms)
→ **[Plugin System](06_Plugin_System.md)** (how plugins work)
→ Pick your extension type:
  - Custom data provider → [Data Providers](07_Data_Providers.md)
  - Custom output format → [Exporters](08_Exporters.md)
  - Custom rendering → [Rendering](09_Rendering.md)

### For Framework Contributors
Want to contribute to KineticReports core?

→ **[Architecture Overview](02_Architecture_Overview.md)** (detailed deep dive)
→ **[Core Concepts](03_Core_Concepts.md)** (foundations)
→ **[Appendix: Architectural Decision Records](Appendix_ADRs.md)** (why things work this way)
→ Review relevant specification documents

---

## 📖 Documentation Organization

### Fundamentals
| Document | Purpose | Read Time |
|----------|---------|-----------|
| [01_Getting_Started.md](01_Getting_Started.md) | Installation, setup, first report | 5 min |
| [02_Architecture_Overview.md](02_Architecture_Overview.md) | System design, data flow | 15 min |
| [03_Core_Concepts.md](03_Core_Concepts.md) | DIPs, styles, layout, coordinates | 10 min |

### Report Development
| Document | Purpose | Read Time |
|----------|---------|-----------|
| [04_Report_Definitions.md](04_Report_Definitions.md) | JSON schema, examples, best practices | 20 min |
| [Core Styling Guide](../06_Style_System_Specification.md) | Typography, colors, layout | 15 min |
| [Expressions](../05_Expression_Language_Specification.md) | Bindings and dynamic content | 10 min |

### Integration & API
| Document | Purpose | Read Time |
|----------|---------|-----------|
| [10_API_Reference.md](10_API_Reference.md) | Main API entry points | 10 min |
| [Data Binding](../07_Runtime_Kernel_Specification.md) | Query execution, parameters | 15 min |
| [Rendering Pipeline](../03_Rendering_Pipeline_Specification.md) | Output generation | 15 min |

### Extension & Customization
| Document | Purpose | Read Time |
|----------|---------|-----------|
| [05_Extension_Points.md](05_Extension_Points.md) | All extension mechanisms | 15 min |
| [06_Plugin_System.md](06_Plugin_System.md) | Plugin lifecycle, management | 15 min |
| [07_Data_Providers.md](07_Data_Providers.md) | Custom data sources | 20 min |
| [08_Exporters.md](08_Exporters.md) | Custom output formats | 20 min |
| [09_Rendering.md](09_Rendering.md) | Custom rendering backends | 20 min |

### Reference
| Document | Purpose | Location |
|----------|---------|----------|
| [Architectural Decision Records](Appendix_ADRs.md) | Why design decisions were made | This guide |
| [Original Specifications](../) | Detailed technical specs | Parent `docs/` folder |

---

## 🏗️ System Architecture at a Glance

```
┌─────────────────────────────────────────────────────┐
│  Your Application                                   │
│  (ASP.NET Core, Console, Blazor, etc.)             │
└──────────────────┬──────────────────────────────────┘
                   │
        ┌──────────▼───────────┐
        │  ReportEngine API    │  ← Entry point
        └──────────┬───────────┘
                   │
    ┌──────────────┼──────────────┐
    ▼              ▼              ▼
┌─────────┐  ┌──────────┐  ┌─────────────┐
│  Layout │  │ Expression│ │ Data        │
│ Engine  │  │ Evaluator │ │ Providers   │
└────┬────┘  └──────────┘  └─────────────┘
     │              ▲              ▲
     │              │              │
     └──────────────┼──────────────┘
                    │
            ┌───────▼────────┐
            │  LayoutTree    │  ← Immutable output
            │  (Positioned   │     of engine
            │   elements)    │
            └───────┬────────┘
                    │
    ┌───────────────┼────────────────┐
    ▼               ▼                ▼
 ┌──────┐      ┌────────┐       ┌────────┐
 │HTML  │      │Excel   │       │ PDF    │
 │Export│      │Export  │       │Renderer│
 └──────┘      └────────┘       └────────┘
    │               │               │
    └───────────────┼───────────────┘
                    ▼
            ┌───────────────┐
            │Output (HTML,  │
            │Excel, PDF,    │
            │etc.)          │
            └───────────────┘
```

**Key Points:**
- **Input**: JSON report definition + data
- **Processing**: Layout engine calculates positions
- **Output**: Immutable LayoutTree 
- **Rendering**: Exporters convert to target formats
- **Extension**: Plugins inject custom providers, exporters, renderers

---

## 🔌 Extension Points Summary

KineticReports is designed to be extended at multiple levels:

### Level 1: Plugin-Based (Recommended)
Load custom DLLs at runtime via the Plugin Manager.

```csharp
// In your ASP.NET Core app
builder.Services.AddPluginManager(pluginDirectory);
```

Implement and deploy:
- **Data Providers** — Custom data sources (REST APIs, NoSQL, etc.)
- **Exporters** — New output formats
- **Renderers** — Alternative rendering backends
- **Custom Plugins** — Any arbitrary extension

→ See [Plugin System](06_Plugin_System.md)

### Level 2: Direct DI Registration
Register implementations directly at startup.

```csharp
// For your own implementation
builder.Services.AddSingleton<IDataProvider>(new MyCustomProvider());
builder.Services.AddSingleton<IRenderer>(new MyCustomRenderer());
```

Best for:
- Internal implementations
- Testing and mocking
- Tightly integrated extensions

### Level 3: Inheritance & Override
Extend base classes and override virtual methods.

```csharp
public class MyPlugin : PluginBase
{
    protected override Task OnInitializeAsync(CancellationToken ct)
    {
        // Custom initialization logic
    }
}
```

→ See [Plugin System](06_Plugin_System.md)

---

## 📋 Common Tasks

### Task: Build Your First Report
1. Read [Getting Started](01_Getting_Started.md)
2. Review [Report Definitions](04_Report_Definitions.md)
3. Create a `.json` report definition
4. Use ReportEngine to generate output

**Time**: ~30 minutes

### Task: Add Custom Data Source
1. Read [Core Concepts](03_Core_Concepts.md) (data section)
2. Review [Data Providers](07_Data_Providers.md)
3. Implement `IDataProvider` interface
4. Register in DI container
5. Test with sample report

**Time**: ~1 hour

### Task: Add New Output Format (e.g., CSV)
1. Read [Extension Points](05_Extension_Points.md)
2. Review [Exporters](08_Exporters.md)
3. Implement `IRenderer` interface
4. Convert LayoutTree elements to CSV
5. Register and test

**Time**: ~2 hours

### Task: Build Custom Plugin
1. Read [Plugin System](06_Plugin_System.md)
2. Choose what to extend (data, export, render, custom)
3. Implement `IPlugin` (extend `PluginBase`)
4. Create class library project
5. Deploy to plugins folder
6. Plugin auto-loads on app startup

**Time**: ~1-2 hours depending on complexity

---

## 🎯 Key Principles

### 1. UI Framework Agnostic
KineticReports doesn't depend on any UI framework. Use with:
- ASP.NET Core Web API
- Blazor (server or WebAssembly)
- Console applications
- Windows Forms
- WPF
- Electron / Tauri
- Any .NET environment

### 2. Data Source Agnostic
Reports don't care where data comes from:
- SQL Server, PostgreSQL, MySQL, Oracle
- REST APIs
- MongoDB, Cosmos DB
- CSV, Excel files
- In-memory collections
- GraphQL queries

### 3. Output Format Agnostic
Same report definition can output to:
- HTML (web browser)
- PDF (print)
- Excel (.xlsx)
- Custom formats (CSV, JSON, XML, etc.)

### 4. Deterministic Rendering
Same report + same data = same output, always.
- No floating point errors
- No random variation
- Pixel-perfect positioning
- Device-independent measurements

### 5. Immutable After Layout
Once layout is complete, it cannot change.
- Thread-safe
- Cacheable
- Reproducible
- Verifiable

---

## 💡 Mental Model

Think of KineticReports as a **pipeline**:

```
Definition + Data → Engine → LayoutTree → Exporter → Output
   (JSON)     (IDataProvider)    (immutable)   (IRenderer)  (HTML/PDF/Excel)
```

**Each stage is independently extensible:**
- Inject custom `IDataProvider` to source data differently
- Extend `PluginBase` to add initialization logic
- Implement `IRenderer` to output new formats
- Chain multiple exporters for complex workflows

---

## 🔧 Development Environment Setup

### Prerequisites
- .NET 10.0 SDK or later
- Visual Studio 2025 / VS Code
- Git

### Clone & Build
```bash
git clone https://github.com/your-org/kinetic-reports.git
cd kinetic-reports
dotnet build KineticReports.slnx
dotnet test KineticReports.slnx
```

### Explore Sample Projects
```bash
# Console app sample
cd src/Samples/KineticReports.Samples.Console
dotnet run

# Blazor interactive sample
cd src/Samples/KineticReports.Samples.Blazor
dotnet run
# Browse to https://localhost:7001
```

---

## 📞 Getting Help

### Documentation
- **[API Reference](10_API_Reference.md)** — Main classes and interfaces
- **[Architectural Decision Records](Appendix_ADRs.md)** — Why things work the way they do
- **[Original Specifications](../)** — Detailed technical specs

### Code Examples
- **[Console Sample](../../src/Samples/KineticReports.Samples.Console/)** — Simple report generation
- **[Web API Sample](../../src/Samples/KineticReports.Samples.WebApi/)** — Integration with ASP.NET Core
- **[Blazor Sample](../../src/Samples/KineticReports.Samples.Blazor/)** — Interactive UI with plugins
- **[Unit Tests](../../tests/unit-tests/)** — Comprehensive test examples

### Contributing
- Check [Architectural Decision Records](Appendix_ADRs.md) before making design changes
- Run full test suite: `dotnet test KineticReports.slnx`
- Follow coding standards documented in root [copilot-instructions.md](../../.github/copilot-instructions.md)

---

## 📈 What's Next?

**Choose your path:**

1. **I want to use KineticReports in my app**
   → Start with [Getting Started](01_Getting_Started.md)

2. **I want to understand how it works**
   → Read [Architecture Overview](02_Architecture_Overview.md)

3. **I want to build a custom data provider**
   → Follow [Data Providers](07_Data_Providers.md)

4. **I want to create a new export format**
   → Follow [Exporters](08_Exporters.md)

5. **I want to build a plugin**
   → Read [Plugin System](06_Plugin_System.md)

6. **I want to understand design decisions**
   → Review [ADRs](Appendix_ADRs.md)

---

## 📄 Quick Reference: Document Map

### Getting Started
```
README.md (you are here)
├── 01_Getting_Started.md ................... Installation & first steps
├── 02_Architecture_Overview.md ............ System design & data flow
└── 03_Core_Concepts.md .................... Foundations (DIPs, styles, etc.)
```

### Building Reports
```
04_Report_Definitions.md ................... JSON schema & examples
├── ../06_Style_System_Specification.md ... Styling & typography
└── ../05_Expression_Language_Specification.md ... Bindings & expressions
```

### Integration
```
10_API_Reference.md ....................... Main API entry points
├── ../03_Rendering_Pipeline_Specification.md .. Output generation
└── ../07_Runtime_Kernel_Specification.md ... Data & expressions
```

### Extension & Customization
```
05_Extension_Points.md .................... Where/how to extend
├── 06_Plugin_System.md ................... Plugin lifecycle
├── 07_Data_Providers.md .................. Custom data sources
├── 08_Exporters.md ...................... New output formats
└── 09_Rendering.md ...................... Custom rendering
```

### Reference
```
Appendix_ADRs.md .......................... Architectural decisions
```

---

**Last Updated**: 2026-07-20  
**KineticReports Version**: 1.0.0-alpha  
**.NET Target**: 10.0

