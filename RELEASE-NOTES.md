# Release Notes

## Unreleased

### v3.0.0 - Layout Consolidation & Unified ContentBlock

#### Breaking Changes

**Consolidated 12 Layout Block Types into Unified `ContentBlock`**

The following 12 concrete `LayoutBlock` subclasses have been removed in favor of a single `ContentBlock` sealed class with a `BlockContentType` enum discriminator:

- ❌ `TextBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Text }`
- ❌ `ImageBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Image }`
- ❌ `ShapeBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Shape }`
- ❌ `ChartBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Chart }`
- ❌ `BarcodeBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Barcode }`
- ❌ `ContainerBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Container }`
- ❌ `TableBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Table }`
- ❌ `RowBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Row }`
- ❌ `CellBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Cell }`
- ❌ `ReportBlock` → ✅ `ContentBlock { ContentType = BlockContentType.ReportSection }`
- ❌ `PageSectionBlock` → ✅ `ContentBlock { ContentType = BlockContentType.PageSection }`
- ❌ `PageBlock` → ✅ `ContentBlock { ContentType = BlockContentType.Page }`

**Benefits:**
- Single polymorphic type reduces type checking overhead
- Faster renderer dispatch via `enum` switch vs. 12 `is` checks
- Simplified factory API through unified `ContentBlockFactory`
- Easier JSON/YAML serialization for authoring workflows
- Reduced package size and complexity

#### Migration Path

**For Code-First Builders:**

```csharp
// OLD (v2.x):
var textBlock = new TextBlock { Id = "t1", Text = "Hello" };
var imageBlock = new ImageBlock { Id = "i1", SourceKey = "logo.png" };

// NEW (v3.0.0+):
var textBlock = new ContentBlock { Id = "t1", ContentType = BlockContentType.Text, Text = "Hello" };
var imageBlock = new ContentBlock { Id = "i1", ContentType = BlockContentType.Image, SourceKey = "logo.png" };

// Or via factory:
var textBlock = ContentBlockFactory.CreateText("Hello");
var imageBlock = ContentBlockFactory.CreateImage("logo.png", ImageStretch.Uniform);
```

**For Custom Renderers:**

```csharp
// OLD (v2.x):
public void RenderElement(LayoutBlock block, IGraphicsContext context)
{
    if (block is TextBlock tb) RenderText(tb, context);
    else if (block is ImageBlock ib) RenderImage(ib, context);
    else if (block is ShapeBlock sb) RenderShape(sb, context);
    // ... 9 more type checks
}

// NEW (v3.0.0+):
public void RenderElement(LayoutBlock block, IGraphicsContext context)
{
    if (block is ContentBlock content)
    {
        switch (content.ContentType)
        {
            case BlockContentType.Text: RenderText(content, context); break;
            case BlockContentType.Image: RenderImage(content, context); break;
            case BlockContentType.Shape: RenderShape(content, context); break;
            // ... 9 more enum cases (more efficient)
        }
    }
}
```

**For JSON/YAML Report Definitions:**

```yaml
# OLD (v2.x):
- type: TextBlock
  id: header
  text: "Monthly Report"
- type: ImageBlock
  id: logo
  sourceKey: "logo.png"

# NEW (v3.0.0+):
- contentType: Text
  id: header
  text: "Monthly Report"
- contentType: Image
  id: logo
  sourceKey: "logo.png"
```

See [MIGRATION_GUIDE_v3.0.0.md](docs/MIGRATION_GUIDE_v3.0.0.md) for detailed migration instructions.

#### Other Changes

- Moved `IReportService` from `KineticReports.Viewer.Blazor.Services` to `KineticReports.Core.Viewer.Services`.
- Renamed `LocalReportService` to `DefaultReportService` and moved it to `KineticReports.Core.Viewer.Services`.
- Updated `KineticReports.Viewer.Blazor` DI registration to resolve `IReportService` to `DefaultReportService`.
- Renamed MVC `LocalReportMvcService` to `DefaultReportMvcService` for naming consistency with default host services.
- Updated the Blazor sample to use Core's default plugin manager registration (`AddPluginManager(...)`) instead of sample-specific plugin manager adapters.

## 1.0.0

- Consolidated the default runtime into `KineticReports.Core`.
- Moved authoring, engine, layout, visual model, rendering, Skia, built-in HTML/PDF export, plugins, and SQL data providers into the Core package.
- Kept `KineticReports.Viewer.Blazor`, `KineticReports.Viewer.Mvc`, and `KineticReports.Viewer.Web` as optional host-specific viewer packages.
- Kept `KineticReports.Export.Excel` as a separate example exporter package.
- Moved the sample watermark plugin into `src/Plugins/KineticReports.Samples.Plugins`.
- Updated the sample Blazor app to consume `KineticReports.Core` plus `KineticReports.Viewer.Blazor`.
