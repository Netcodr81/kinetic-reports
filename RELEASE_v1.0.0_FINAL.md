# KineticReports v1.0.0 - Initial Release

**Release Date:** July 29, 2026  
**Status:** Production Ready  

---

## 🎉 What's New in v1.0.0

KineticReports v1.0.0 is the **initial release** of a deterministic reporting engine for .NET 10. It brings comprehensive report generation, multiple export formats, and extensive extensibility.

### Core Features

**Report Generation**
- ✅ Deterministic document generation (identical input = identical output)
- ✅ Data resolution from multiple sources
- ✅ Unified layout engine with pagination
- ✅ Built-in HTML and PDF export
- ✅ Comprehensive styling system

**Architecture**
- ✅ Unified ContentBlock model with BlockContentType discriminator
- ✅ Immutable ReportDocument after layout
- ✅ Device Independent Pixel (DIP) coordinate system
- ✅ Centralized text layout service
- ✅ Plugin architecture for extensibility

**Viewer & Components**
- ✅ Blazor viewer components
- ✅ ASP.NET Core hosting
- ✅ Responsive design
- ✅ Export support
- ✅ Search functionality

**Data Integration**
- ✅ SQL Server data provider
- ✅ Pluggable data resolver pattern
- ✅ Support for multiple data sources per report
- ✅ Parameter binding and expressions

**Extensibility**
- ✅ 11+ extension points
- ✅ Plugin system with ordered execution
- ✅ Custom exporters
- ✅ Custom renderers
- ✅ Custom data providers
- ✅ Expression language customization

### Key Improvements Over Previous Approaches

- **Unified Model:** Single ContentBlock type instead of separate classes
- **Performance:** Optimized enum-based dispatch for fast type checking
- **Simplicity:** Easier to learn and extend
- **Determinism:** Guaranteed consistent output
- **Flexibility:** Works with any data source, UI framework, export format

---

## 📦 What's Included

### NuGet Packages

1. **KineticReports.Core.1.0.0**
   - Complete reporting engine
   - Authoring, layout, rendering
   - Built-in exporters (HTML, PDF)
   - Plugin infrastructure
   - Data providers

2. **KineticReports.Viewer.Blazor.1.0.0**
   - Blazor viewer components
   - Rendering, export, search
   - Responsive design
   - Multiple render modes supported

### Source Code

```
src/
├── Core/KineticReports.Core/
├── Viewer/KineticReports.Viewer.{Blazor,Mvc,Web}
├── Plugins/KineticReports.Samples.Plugins
└── Samples/KineticReports.Samples.Blazor
```

### Documentation

- **Comprehensive Developer Guide** - All-in-one learning resource
- **API Reference** - Full type documentation
- **Extension Guides** - Plugin, exporter, data provider tutorials
- **Sample Application** - Working Blazor example

---

## 🚀 Getting Started

### Installation

```bash
# Install core package
dotnet add package KineticReports.Core

# Optional: Blazor viewer
dotnet add package KineticReports.Viewer.Blazor
```

### Quick Start

```csharp
// 1. Get engine from DI
var engine = serviceProvider.GetRequiredService<IReportEngine>();

// 2. Define report
var definition = new ReportDefinition
{
    Id = "sales-report",
    Name = "Sales Report",
    Blocks = new object[] { /* ... */ }
};

// 3. Generate document
var document = await engine.RunAsync(definition);

// 4. Export
var exporter = serviceProvider.GetRequiredService<IReportDocumentExporter>();
await exporter.ExportAsync(document, stream);
```

---

## 📊 Testing & Quality

**Test Coverage:**
- 398 tests across all modules
- 100% unit test pass rate
- Integration tests for exporters and viewers
- Performance benchmarks included

**Build Status:**
- ✅ 0 compilation errors
- ✅ 0 code analysis warnings
- ✅ Release configuration validated

---

## 🔧 Supported .NET Versions

- **.NET 10.0** (primary target)

### Dependencies

- **SkiaSharp 4.x** - Graphics and PDF rendering
- **System.Text.Json** - JSON serialization
- **Microsoft.AspNetCore 8.0+** - Web hosting (optional)

---

## 📝 Release Notes

### New in 1.0.0

#### Architecture
- Unified ContentBlock model with BlockContentType enum
- Deterministic report generation pipeline
- Immutable ReportDocument after layout/pagination
- Device Independent Pixel (DIP) coordinate system

#### Core Engine
- IReportEngine orchestration
- ILayoutEngine with Measure → Arrange → Paginate stages
- IDataResolver for multi-source data binding
- IExpressionEvaluator for dynamic content

#### Rendering & Export
- SkiaSharp-backed PDF renderer
- HTML exporter with SVG shapes
- Deterministic output guarantee
- Extension point for custom exporters

#### Viewers
- Blazor component library
- Responsive design
- Multiple render modes (Static, Interactive, Auto)
- Export, search, hit-test capabilities

#### Plugins
- Plugin architecture with lifecycle hooks
- Ordered execution (by Order then Id)
- Per-report plugin toggling
- Block post-processors
- Export negotiation and post-processing

#### Data Integration
- SQL Server data provider
- Multi-source support
- Parameter binding
- Expression evaluation in data context

---

## 🎯 API Stability

**v1.0.0 provides stable APIs for:**
- Report definition and authoring
- Document generation and layout
- Rendering and export
- Plugin architecture
- Core type system

**No breaking changes expected within v1.x**

---

## 🤝 Contributing & Support

- **Issues:** github.com/michaeleckel/kinetic-reports/issues
- **Documentation:** See `docs/` folder
- **Samples:** See `src/Samples/` folder

---

## 📄 License

MIT License - See LICENSE file for details

---

## 🙏 Acknowledgments

Built with:
- .NET 10.0
- SkiaSharp 4.x
- Blazor
- xUnit (testing)
- Shouldly (assertions)

---

**KineticReports v1.0.0**  
*Deterministic Reporting for .NET*  
*July 2026*
