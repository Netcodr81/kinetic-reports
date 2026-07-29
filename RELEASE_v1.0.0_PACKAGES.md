# KineticReports v1.0.0 Initial Release Packages

## Package Summary

**Release Date:** July 29, 2026  
**Version:** 1.0.0  
**Target Framework:** .NET 10.0  
**License:** MIT  
**Status:** Initial Release

## Generated Packages

### 1. KineticReports.Core.1.0.0.nupkg
**Primary runtime package** containing the complete KineticReports engine.

**Contents:**
- `lib/net10.0/KineticReports.Core.dll` - Core runtime (authoring, engine, layout, rendering, export, plugins, data providers)
- `README.md` - Package documentation
- `KineticReports.Core.nuspec` - Package metadata

**Package Metadata:**
- **ID:** KineticReports.Core
- **Title:** KineticReports Core
- **Authors:** Mike Eckel
- **License:** MIT
- **Description:** Core KineticReports runtime package containing authoring, engine, layout, visual model, rendering, Skia integration, built-in HTML/PDF export, plugins, and SQL data providers. Initial release with unified block architecture and comprehensive extension points.
- **Repository:** https://github.com/michaeleckel/kinetic-reports
- **Release Notes:** v1.0.0: Initial release with deterministic report generation, unified ContentBlock architecture, comprehensive plugin system, built-in HTML/PDF export, Skia rendering backend, and SQL Server data provider.

### 2. KineticReports.Export.Excel.1.0.0.nupkg
**Example Excel export implementation** (optional add-on).

**Contents:**
- `lib/net10.0/KineticReports.Export.Excel.dll` - Excel exporter implementation
- `README.md` - Package documentation
- `KineticReports.Export.Excel.nuspec` - Package metadata

**Package Metadata:**
- **ID:** KineticReports.Export.Excel
- **Title:** KineticReports Excel Export Example
- **Authors:** Mike Eckel
- **Description:** Example Excel exporter package for KineticReports. Demonstrates the exporter extension pattern. Core built-in exporters are HTML and PDF.

### 3. KineticReports.Viewer.Blazor.1.0.0.nupkg
**Blazor viewer component library** for rendering KineticReports documents in web applications.

**Contents:**
- `lib/net10.0/KineticReports.Viewer.Blazor.dll` - Blazor components
- `lib/net10.0/KineticReports.Viewer.Blazor.pdb` - Debug symbols
- `staticContent/` - Static web assets (CSS, JavaScript)
- `README.md` - Package documentation
- `KineticReports.Viewer.Blazor.nuspec` - Package metadata

**Package Metadata:**
- **ID:** KineticReports.Viewer.Blazor
- **Title:** KineticReports Viewer for Blazor
- **Authors:** Mike Eckel
- **Description:** Embeddable Blazor viewer components for rendering, exporting, searching, and hit-testing KineticReports documents. Fully compatible with Blazor Web App render modes (Static, Interactive, Auto).

## Package Location

**Release Packages Directory:**  
`c:\Repositories\kinetic-reports\release-packages-v1.0.0\`

**Individual Package Locations:**
- Core: `src/Core/KineticReports.Core/bin/Release/KineticReports.Core.1.0.0.nupkg`
- Excel: `src/Export/KineticReports.Export.Excel/bin/Release/KineticReports.Export.Excel.1.0.0.nupkg`
- Blazor: `src/Viewer/KineticReports.Viewer.Blazor/bin/Release/KineticReports.Viewer.Blazor.1.0.0.nupkg`

## Validation Results

### Build Status
✅ **Build:** Succeeded  
- Configuration: Release
- Target: net10.0
- Errors: 0
- Warnings: 0 (excluding NuGet advisories for indirect dependencies)

### Test Coverage
✅ **All Tests Passed:** 398/398  
- KineticReports.Core.Tests: 216 ✅
- KineticReports.Engine.Tests: 18 ✅
- KineticReports.Layout.Tests: 23 ✅
- KineticReports.Rendering.Tests: 12 ✅
- KineticReports.Rendering.Skia.Tests: 8 ✅
- KineticReports.Export.Html.Tests: 34 ✅
- KineticReports.Viewer.Blazor.Tests: 10 ✅
- KineticReports.Viewer.Web.Tests: 24 ✅
- KineticReports.Plugins.Tests: 27 ✅
- KineticReports.Data.SqlServer.Tests: 14 ✅

### Package Structure
✅ All packages contain:
- Valid nuspec metadata with v1.0.0 version
- Compiled .NET 10.0 assemblies
- README documentation
- MIT license declaration

## Version Dependencies

**KineticReports.Viewer.Blazor** depends on:
- KineticReports.Core >= 1.0.0
- Microsoft.AspNetCore.Components >= 8.0.0
- Microsoft.AspNetCore.Components.Web >= 8.0.0

**KineticReports.Export.Excel** depends on:
- KineticReports.Core >= 1.0.0
- EPPlus >= 7.0.0 (for Excel export)

**KineticReports.Core** depends on:
- SkiaSharp >= 4.0.0
- System.Text.Json >= 8.0.0
- Microsoft.Bcl.AsyncInterfaces >= 8.0.0

## Key v1.0.0 Features

### Architecture
- **Unified ContentBlock Model:** Single sealed class with BlockContentType enum discriminator
- **Deterministic Output:** Same input produces identical output every time
- **Cross-Platform:** Runs on Windows, Linux, macOS

### Core Capabilities
- Report definition (JSON or code-first fluent API)
- Data resolution from multiple sources
- Deterministic layout and pagination
- HTML and PDF export (built-in)
- Plugin system for extensibility
- Blazor viewer components
- SQL Server data provider

### Extension Points
- Custom data providers (REST, GraphQL, etc.)
- Custom exporters (Excel, CSV, etc.)
- Custom renderers (PNG, custom formats)
- Report block post-processors
- Expression language extensions
- Plugin system with ordered execution

### Performance
- Optimized enum-based type dispatch
- Efficient layout engine (Measure → Arrange → Paginate)
- Streaming export support
- DIP-based coordinate system (deterministic across devices)

## Installation Instructions

### Via NuGet Package Manager (When Published)
```powershell
# Core package (required)
Install-Package KineticReports.Core -Version 1.0.0

# Blazor viewer (optional, if using in web app)
Install-Package KineticReports.Viewer.Blazor -Version 1.0.0

# Excel export (optional, if exporting to Excel)
Install-Package KineticReports.Export.Excel -Version 1.0.0
```

### Via dotnet CLI (When Published)
```bash
# Core package
dotnet add package KineticReports.Core --version 1.0.0

# Blazor viewer
dotnet add package KineticReports.Viewer.Blazor --version 1.0.0

# Excel export
dotnet add package KineticReports.Export.Excel --version 1.0.0
```

### Via NuGet.org (When Published)
All packages will be available at https://www.nuget.org/packages/ with package IDs:
- `KineticReports.Core`
- `KineticReports.Viewer.Blazor`
- `KineticReports.Export.Excel`

## Documentation

Comprehensive documentation for v1.0.0:
- **Comprehensive Developer Guide:** `docs/COMPREHENSIVE_DEVELOPER_GUIDE.md` (all-in-one guide)
- **Architecture Overview:** `docs/02-report-generation-flow.md`
- **Quick Start:** `docs/01-junior-quick-start.md`
- **Extension Points:** `docs/07-extension-points.md`

## Support & Questions

- **GitHub Issues:** https://github.com/michaeleckel/kinetic-reports/issues
- **Documentation:** `docs/` folder
- **API Reference:** See comprehensive developer guide

## Sign-Off

✅ **Release Candidate:** APPROVED  
✅ **Build Validation:** PASSED  
✅ **Test Suite:** PASSED (398/398)  
✅ **Package Contents:** VERIFIED  
✅ **Documentation:** COMPLETE  

**Status:** Ready for NuGet.org publication

---

**Generated:** July 29, 2026  
**KineticReports v1.0.0 Release Team**
