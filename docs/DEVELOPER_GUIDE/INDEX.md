# Developer Guide Index

**Complete reference to all KineticReports developer documentation.**

---

## 📚 Main Guides (in DEVELOPER_GUIDE folder)

| Document | Purpose | Read Time | Audience |
|----------|---------|-----------|----------|
| **[README.md](README.md)** | Entry point & learning paths | 10 min | Everyone |
| **[01_Getting_Started.md](01_Getting_Started.md)** | Installation & first report | 5 min | Beginners |
| **[02_Architecture_Overview.md](02_Architecture_Overview.md)** | System design & data flow | 20 min | Developers |
| **[03_Core_Concepts.md](03_Core_Concepts.md)** | DIPs, styles, layout | 15 min | Designers |
| **[04_Report_Definitions.md](04_Report_Definitions.md)** | JSON schema & examples | 20 min | Designers |
| **[05_Extension_Points.md](05_Extension_Points.md)** | Where & how to extend | 25 min | Developers |
| **[06_Plugin_System.md](06_Plugin_System.md)** | Building & loading plugins | 30 min | Plugin Devs |
| **[Appendix_ADRs.md](Appendix_ADRs.md)** | Design decisions | 20 min | Contributors |

---

## 📖 Technical Specifications (parent docs/ folder)

### Core Architecture
- [00_Architecture_Handbook.md](../00_Architecture_Handbook.md)
- [07_Runtime_Kernel_Specification.md](../07_Runtime_Kernel_Specification.md)
- [01_Report_Layout_Engine_Specification.md](../01_Report_Layout_Engine_Specification.md)

### Report Definition
- [02_Report_Definition_JSON_Specification.md](../02_Report_Definition_JSON_Specification.md)

### Styling & Typography
- [06_Style_System_Specification.md](../06_Style_System_Specification.md)
- [13_Graphics_and_Typography_Specification.docx](../13_Graphics_and_Typography_Specification.docx)

### Rendering & Export
- [03_Rendering_Pipeline_Specification.md](../03_Rendering_Pipeline_Specification.md)
- [14_Export_SDK_Specification.docx](../14_Export_SDK_Specification.docx)

### Features
- [04_Table_Layout_Algorithm_Specification.md](../04_Table_Layout_Algorithm_Specification.md)
- [05_Expression_Language_Specification.md](../05_Expression_Language_Specification.md)

### Extensibility
- [08_Plugin_SDK_Specification.md](../08_Plugin_SDK_Specification.md)
- [09_Data_Provider_SDK_Specification.md](../09_Data_Provider_SDK_Specification.md)
- [12_Plugin_Manager_Implementation.md](../12_Plugin_Manager_Implementation.md)

### Advanced
- [10_Report_Compiler_Specification.docx](../10_Report_Compiler_Specification.docx)
- [11_Intermediate_Representation_Specification.docx](../11_Intermediate_Representation_Specification.docx)
- [12_Diagnostics_Framework_Specification.docx](../12_Diagnostics_Framework_Specification.docx)
- [15_Viewer_Architecture_Specification.docx](../15_Viewer_Architecture_Specification.docx)
- [17_Public_API_and_Fluent_Builder_Specification.docx](../17_Public_API_and_Fluent_Builder_Specification.docx)
- [18_Resource_Management_Specification.docx](../18_Resource_Management_Specification.docx)
- [19_Localization_and_Formatting_Specification.docx](../19_Localization_and_Formatting_Specification.docx)
- [20_Conformance_Test_Specification.docx](../20_Conformance_Test_Specification.docx)
- [21_Platform_Roadmap_and_Governance.docx](../21_Platform_Roadmap_and_Governance.docx)

### How-To Guides
- [10_How_To_Add_New_Exporter.md](../10_How_To_Add_New_Exporter.md)
- [11_How_To_Implement_Data_Provider.md](../11_How_To_Implement_Data_Provider.md)

---

## 🎯 Learning Paths

### Path 1: I want to USE KineticReports
**Goal**: Integrate into my application and generate reports

**Reading Order**:
1. [Getting Started](01_Getting_Started.md) (5 min)
2. [Architecture Overview](02_Architecture_Overview.md) (20 min)
3. [API Reference](10_API_Reference.md) — coming soon
4. [Web API Sample](../../src/Samples/KineticReports.Samples.WebApi/)
5. [Blazor Sample](../../src/Samples/KineticReports.Samples.Blazor/)

**Time**: ~1 hour  
**Outcome**: Able to generate reports from your app

---

### Path 2: I want to DESIGN reports
**Goal**: Create beautiful report definitions

**Reading Order**:
1. [Core Concepts](03_Core_Concepts.md) (15 min)
2. [Report Definitions](04_Report_Definitions.md) (20 min)
3. [Style System Specification](../06_Style_System_Specification.md)
4. [Report Definition JSON Schema](../02_Report_Definition_JSON_Specification.md)

**Time**: ~1 hour  
**Outcome**: Able to create complex report definitions

---

### Path 3: I want to EXTEND KineticReports
**Goal**: Build custom data providers, exporters, or renderers

**Reading Order**:
1. [Architecture Overview](02_Architecture_Overview.md) (20 min)
2. [Extension Points](05_Extension_Points.md) (25 min)
3. Choose your extension type:
   - **Data Provider** → [Data Providers](07_Data_Providers.md) + [09_Data_Provider_SDK_Specification.md](../09_Data_Provider_SDK_Specification.md)
   - **Exporter** → [Exporters](08_Exporters.md) + [10_How_To_Add_New_Exporter.md](../10_How_To_Add_New_Exporter.md)
   - **Renderer** → [Rendering](09_Rendering.md) + [03_Rendering_Pipeline_Specification.md](../03_Rendering_Pipeline_Specification.md)

**Time**: ~2 hours  
**Outcome**: Able to implement custom extensions

---

### Path 4: I want to BUILD PLUGINS
**Goal**: Create runtime-loadable plugins for custom features

**Reading Order**:
1. [Extension Points](05_Extension_Points.md) (25 min)
2. [Plugin System](06_Plugin_System.md) (30 min)
3. [Plugin Manager Implementation](../12_Plugin_Manager_Implementation.md)
4. [Plugin SDK Specification](../08_Plugin_SDK_Specification.md)

**Time**: ~2 hours  
**Outcome**: Able to develop and deploy plugins

---

### Path 5: I want to UNDERSTAND DESIGN DECISIONS
**Goal**: Learn why KineticReports was designed this way

**Reading Order**:
1. [Appendix: ADRs](Appendix_ADRs.md) (20 min) — All design decisions
2. [Architecture Overview](02_Architecture_Overview.md) (20 min) — Context
3. Specific specs based on interest

**Time**: ~1-2 hours  
**Outcome**: Deep understanding of architecture and trade-offs

---

### Path 6: I want to CONTRIBUTE to KineticReports
**Goal**: Contribute code to the core project

**Reading Order**:
1. [Appendix: ADRs](Appendix_ADRs.md) (20 min)
2. [Architecture Overview](02_Architecture_Overview.md) (20 min)
3. [Root Coding Standards](../../.github/copilot-instructions.md)
4. Relevant technical specs
5. Review existing tests and code examples

**Time**: ~2-3 hours  
**Outcome**: Ready to contribute with proper understanding

---

## 🔍 Quick Reference by Topic

### Topic: Data Sources
- [03_Core_Concepts.md#data-binding](03_Core_Concepts.md#data-binding-and-expressions)
- [07_Data_Providers.md](07_Data_Providers.md)
- [09_Data_Provider_SDK_Specification.md](../09_Data_Provider_SDK_Specification.md)
- [11_How_To_Implement_Data_Provider.md](../11_How_To_Implement_Data_Provider.md)

### Topic: Styling
- [03_Core_Concepts.md#styles](03_Core_Concepts.md#styles-and-style-cascade)
- [04_Report_Definitions.md#styles](04_Report_Definitions.md#styles)
- [06_Style_System_Specification.md](../06_Style_System_Specification.md)
- [13_Graphics_and_Typography_Specification.docx](../13_Graphics_and_Typography_Specification.docx)

### Topic: Layout & Pagination
- [03_Core_Concepts.md#pagination](03_Core_Concepts.md#pagination)
- [01_Report_Layout_Engine_Specification.md](../01_Report_Layout_Engine_Specification.md)
- [04_Table_Layout_Algorithm_Specification.md](../04_Table_Layout_Algorithm_Specification.md)

### Topic: Rendering & Export
- [03_Rendering_Pipeline_Specification.md](../03_Rendering_Pipeline_Specification.md)
- [08_Exporters.md](08_Exporters.md) — coming soon
- [10_How_To_Add_New_Exporter.md](../10_How_To_Add_New_Exporter.md)
- [14_Export_SDK_Specification.docx](../14_Export_SDK_Specification.docx)

### Topic: Expressions & Bindings
- [05_Expression_Language_Specification.md](../05_Expression_Language_Specification.md)
- [07_Runtime_Kernel_Specification.md](../07_Runtime_Kernel_Specification.md)

### Topic: Plugins
- [06_Plugin_System.md](06_Plugin_System.md)
- [08_Plugin_SDK_Specification.md](../08_Plugin_SDK_Specification.md)
- [12_Plugin_Manager_Implementation.md](../12_Plugin_Manager_Implementation.md)

### Topic: Architecture
- [02_Architecture_Overview.md](02_Architecture_Overview.md)
- [00_Architecture_Handbook.md](../00_Architecture_Handbook.md)
- [Appendix_ADRs.md](Appendix_ADRs.md)

---

## 📊 Document Map

```
DEVELOPER_GUIDE/
├── README.md (START HERE)
├── 01_Getting_Started.md
├── 02_Architecture_Overview.md
├── 03_Core_Concepts.md
├── 04_Report_Definitions.md
├── 05_Extension_Points.md
├── 06_Plugin_System.md
├── 07_Data_Providers.md (future)
├── 08_Exporters.md (future)
├── 09_Rendering.md (future)
├── 10_API_Reference.md (future)
├── Appendix_ADRs.md
└── INDEX.md (you are here)

../ (parent docs folder)
├── 00_Architecture_Handbook.md
├── 01_Report_Layout_Engine_Specification.md
├── 02_Report_Definition_JSON_Specification.md
├── 03_Rendering_Pipeline_Specification.md
├── ... (and more specs)
├── 10_How_To_Add_New_Exporter.md
├── 11_How_To_Implement_Data_Provider.md
└── 12_Plugin_Manager_Implementation.md
```

---

## 🚀 Quick Start Checklists

### Checklist 1: First Time Setup
- [ ] Read [Getting Started](01_Getting_Started.md)
- [ ] Install .NET 10.0 SDK
- [ ] Clone repository
- [ ] Build solution: `dotnet build KineticReports.slnx`
- [ ] Run tests: `dotnet test KineticReports.slnx`
- [ ] Explore samples in `src/Samples/`

### Checklist 2: Create First Report
- [ ] Read [Core Concepts](03_Core_Concepts.md)
- [ ] Review [Report Definitions](04_Report_Definitions.md)
- [ ] Create report.json using examples
- [ ] Write C# code to execute report
- [ ] Verify output in browser/PDF/Excel

### Checklist 3: Implement Custom Data Provider
- [ ] Read [Extension Points](05_Extension_Points.md)
- [ ] Study [Data Providers](07_Data_Providers.md)
- [ ] Review existing SqlServerDataProvider
- [ ] Implement IDataProvider interface
- [ ] Write unit tests
- [ ] Integrate into application

### Checklist 4: Build a Plugin
- [ ] Read [Plugin System](06_Plugin_System.md)
- [ ] Create class library project
- [ ] Extend PluginBase
- [ ] Implement OnInitializeAsync()
- [ ] Register custom services (IDataProvider, IRenderer)
- [ ] Build and deploy to plugins folder

---

## 📞 Finding Information

### By Role
- **Report Designer** → [Core Concepts](03_Core_Concepts.md) → [Report Definitions](04_Report_Definitions.md)
- **Backend Developer** → [Getting Started](01_Getting_Started.md) → [Architecture Overview](02_Architecture_Overview.md)
- **Plugin Developer** → [Extension Points](05_Extension_Points.md) → [Plugin System](06_Plugin_System.md)
- **Framework Contributor** → [Appendix: ADRs](Appendix_ADRs.md) → Relevant specs

### By Technology
- **SQL Server** → [How_To_Implement_Data_Provider](../11_How_To_Implement_Data_Provider.md)
- **REST APIs** → [How_To_Implement_Data_Provider](../11_How_To_Implement_Data_Provider.md)
- **HTML Export** → [10_How_To_Add_New_Exporter.md](../10_How_To_Add_New_Exporter.md)
- **PDF Generation** → [03_Rendering_Pipeline_Specification.md](../03_Rendering_Pipeline_Specification.md)
- **Excel Export** → [10_How_To_Add_New_Exporter.md](../10_How_To_Add_New_Exporter.md)

### By Complexity
- **Beginner** (0-2 hours) → Getting Started + Core Concepts
- **Intermediate** (2-5 hours) → Architecture + Extension Points
- **Advanced** (5+ hours) → Full specs + Contribution path

---

## 🔗 External Links

### Sample Projects
- [Console Sample](../../src/Samples/KineticReports.Samples.Console/)
- [Web API Sample](../../src/Samples/KineticReports.Samples.WebApi/)
- [Blazor Sample](../../src/Samples/KineticReports.Samples.Blazor/)
- [Plugin Sample](../../src/Samples/KineticReports.Samples.Plugins/)

### Test Examples
- [Core Tests](../../tests/unit-tests/KineticReports.Core.Tests/)
- [Layout Tests](../../tests/unit-tests/KineticReports.Layout.Tests/)
- [Engine Tests](../../tests/unit-tests/KineticReports.Engine.Tests/)
- [Plugin Tests](../../tests/unit-tests/KineticReports.Plugins.Tests/)

### Project Files
- [Repository Root](../../)
- [Source Code](../../src/)
- [Tests](../../tests/)
- [Root Instructions](../../.github/copilot-instructions.md)

---

## 📝 How to Use This Index

1. **First time here?** → Start with [README.md](README.md)
2. **Looking for specific topic?** → Use "Quick Reference by Topic" section
3. **Want to learn by role?** → Use "Learning Paths" section
4. **Need API details?** → Scroll to "Quick Reference by Topic"
5. **Contributing to core?** → Follow "I want to CONTRIBUTE" learning path

---

## 🆘 Troubleshooting

### Can't find what you're looking for?
1. Check "Quick Reference by Topic" section
2. Review [README.md](README.md) for overview
3. Search in parent `docs/` folder
4. Check sample projects for code examples

### Document seems incomplete?
Some guides (07, 08, 09, 10) are marked as "future" but referenced specs exist in parent folder.

### Still stuck?
- Review [Appendix: ADRs](Appendix_ADRs.md) for design rationale
- Check test code for examples
- Study sample projects

---

**Last Updated**: 2026-07-20  
**KineticReports Version**: 1.0.0-alpha  
**.NET Target**: 10.0

**Navigation**: [README](README.md) | [Getting Started](01_Getting_Started.md) | [Architecture](02_Architecture_Overview.md)
