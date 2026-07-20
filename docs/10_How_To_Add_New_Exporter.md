# Adding a New Exporter to KineticReports

## Overview

This guide explains how to implement a new exporter for KineticReports. Exporters transform an immutable `LayoutTree` into a specific output format (PDF, JSON, XML, Markdown, etc.).

## Architecture Principles

Exporters follow these core principles:

1. **Immutable LayoutTree Input**: Exporters receive a complete, immutable `LayoutTree` with all pages, sections, and elements pre-measured and positioned.
2. **No Layout Responsibility**: Exporters MUST NOT perform layout, measurement, or pagination. That work is done before the exporter receives input.
3. **Async-First Design**: Exporters use async I/O for file writing and network calls.
4. **Deterministic Output**: For identical input, exporters MUST produce identical output (critical for reports).
5. **Error Handling**: Exporters MUST validate input and provide meaningful error messages.

## Project Structure

### File Organization

```
src/
  Export/
    KineticReports.Export.MyFormat/
      IMyFormatExporter.cs           # Public interface
      MyFormatExporter.cs            # Concrete implementation
      KineticReports.Export.MyFormat.csproj

tests/
  integration-tests/
    KineticReports.Export.MyFormat.Tests/
      GlobalUsings.cs
      MyFormatExporterTests.cs       # Integration tests
      KineticReports.Export.MyFormat.Tests.csproj
```

### Naming Convention

- **Interface**: `I{Format}Exporter` (e.g., `IJsonExporter`, `IPdfExporter`)
- **Implementation**: `{Format}Exporter` (e.g., `JsonExporter`, `PdfExporter`)
- **NuGet Package**: `KineticReports.Export.{Format}`

## Step 1: Define the Interface

Create `I{Format}Exporter.cs`:

```csharp
namespace KineticReports.Export.MyFormat;

using KineticReports.Core.Layout;

/// <summary>
/// Exports a LayoutTree to MyFormat output.
/// </summary>
public interface IMyFormatExporter
{
    /// <summary>
    /// Exports the layout tree to the specified stream in MyFormat.
    /// </summary>
    /// <param name="tree">The immutable layout tree to export.</param>
    /// <param name="output">The output stream to write to.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task ExportAsync(LayoutTree tree, Stream output, CancellationToken cancellationToken = default);
}
```

## Step 2: Implement the Exporter

Create `{Format}Exporter.cs`:

```csharp
namespace KineticReports.Export.MyFormat;

using System.Diagnostics;
using KineticReports.Core.Layout;

/// <summary>
/// Sealed implementation of IMyFormatExporter.
/// </summary>
public sealed class MyFormatExporter : IMyFormatExporter
{
    /// <inheritdoc/>
    public async Task ExportAsync(
        LayoutTree tree,
        Stream output,
        CancellationToken cancellationToken = default)
    {
        if (tree == null)
            throw new ArgumentNullException(nameof(tree));
        if (output == null)
            throw new ArgumentNullException(nameof(output));

        using var writer = new StreamWriter(output, leaveOpen: true);

        // Write format header (e.g., XML declaration, JSON opening)
        await writer.WriteLineAsync("<!-- MyFormat Export -->", cancellationToken);

        // Iterate through pages
        foreach (var page in tree.Pages)
        {
            await ExportPageAsync(page, writer, cancellationToken);
        }

        // Write format footer
        await writer.FlushAsync(cancellationToken);
    }

    private async Task ExportPageAsync(PageElement page, StreamWriter writer, CancellationToken ct)
    {
        await writer.WriteLineAsync($"<Page number=\"{page.PageNumber}\" width=\"{page.PageWidth}\" height=\"{page.PageHeight}\">", ct);

        // Render page children (sections, text, shapes, images, tables, etc.)
        foreach (var element in page.Children)
        {
            await RenderElementAsync(element, writer, ct);
        }

        await writer.WriteLineAsync("</Page>", ct);
    }

    private async Task RenderElementAsync(LayoutElement element, StreamWriter writer, CancellationToken ct)
    {
        // Dispatch on element type
        switch (element)
        {
            case TextElement text:
                await RenderTextElementAsync(text, writer, ct);
                break;
            case ImageElement image:
                await RenderImageElementAsync(image, writer, ct);
                break;
            case TableElement table:
                await RenderTableElementAsync(table, writer, ct);
                break;
            // ... handle all 12 layout element types
            default:
                // Unsupported element type
                await writer.WriteLineAsync($"<!-- Unsupported element type: {element.GetType().Name} -->", ct);
                break;
        }
    }

    private async Task RenderTextElementAsync(TextElement text, StreamWriter writer, CancellationToken ct)
    {
        await writer.WriteLineAsync($"<Text x=\"{text.Bounds.X}\" y=\"{text.Bounds.Y}\">", ct);
        foreach (var run in text.TextRuns)
        {
            await writer.WriteLineAsync($"  <Run font=\"{run.Style.FontFamily}\" size=\"{run.Style.FontSize}\">{EscapeXml(run.Text)}</Run>", ct);
        }
        await writer.WriteLineAsync("</Text>", ct);
    }

    private async Task RenderImageElementAsync(ImageElement image, StreamWriter writer, CancellationToken ct)
    {
        // For binary formats, store reference or embed as base64
        var base64 = Convert.ToBase64String(image.PixelData);
        await writer.WriteLineAsync($"<Image x=\"{image.Bounds.X}\" y=\"{image.Bounds.Y}\" data=\"{base64}\"/>", ct);
    }

    private async Task RenderTableElementAsync(TableElement table, StreamWriter writer, CancellationToken ct)
    {
        await writer.WriteLineAsync($"<Table x=\"{table.Bounds.X}\" y=\"{table.Bounds.Y}\" rows=\"{table.Rows.Count}\">", ct);
        foreach (var row in table.Rows)
        {
            await writer.WriteLineAsync("  <Row>", ct);
            foreach (var cell in row.Cells)
            {
                await writer.WriteLineAsync($"    <Cell>{EscapeXml(cell.ToString() ?? "")}</Cell>", ct);
            }
            await writer.WriteLineAsync("  </Row>", ct);
        }
        await writer.WriteLineAsync("</Table>", ct);
    }

    private static string EscapeXml(string text) => 
        text.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
}
```

## Step 3: Create Tests

Create integration tests in `tests/integration-tests/{Format}ExporterTests.cs`:

```csharp
namespace KineticReports.Export.MyFormat.Tests;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

public class MyFormatExporterTests
{
    [Fact]
    public async Task ExportAsync_WithValidLayoutTree_ProducesOutput()
    {
        // Arrange
        var exporter = new MyFormatExporter();
        var page = new PageElement
        {
            Id = "page-1",
            PageWidth = 800,
            PageHeight = 600,
            PageNumber = 1,
            Children = [],
            Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f }
        };
        var tree = new LayoutTree { Pages = [page] };

        using var output = new MemoryStream();

        // Act
        await exporter.ExportAsync(tree, output);

        // Assert
        output.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ExportAsync_WithNullLayoutTree_ThrowsArgumentNullException()
    {
        // Arrange
        var exporter = new MyFormatExporter();
        using var output = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await exporter.ExportAsync(null!, output));
    }

    [Fact]
    public async Task ExportAsync_WithNullStream_ThrowsArgumentNullException()
    {
        // Arrange
        var exporter = new MyFormatExporter();
        var page = new PageElement { /* ... */ };
        var tree = new LayoutTree { Pages = [page] };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await exporter.ExportAsync(tree, null!));
    }
}
```

## Step 4: Configure Project File

Create `KineticReports.Export.MyFormat.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Core\KineticReports.Core\KineticReports.Core.csproj" />
  </ItemGroup>

  <!-- Add format-specific dependencies here -->
  <!-- Examples:
  <ItemGroup>
    <PackageReference Include="PdfSharpCore" Version="6.2.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
  -->

</Project>
```

## Step 5: Handle All Layout Element Types

Your exporter must handle all 12 layout element types:

1. **PageElement** – Container for a single page
2. **SectionElement** – Logical grouping (body, header, footer)
3. **BandElement** – Repeating group of rows (table bands)
4. **RowElement** – Single table row
5. **CellElement** – Table cell
6. **ContainerElement** – Generic layout container
7. **TextElement** – Text content with styled runs
8. **ImageElement** – Raster image with pixel data
9. **ShapeElement** – Vector shape (rectangle, ellipse, line, polygon)
10. **TableElement** – Complete table with structure
11. **LineElement** – Decorative line
12. **RectangleElement** – Decorative rectangle

## Step 6: Register with Dependency Injection (Optional)

If using ASP.NET Core, register your exporter:

```csharp
// In Program.cs
services.AddScoped<IMyFormatExporter>(sp => new MyFormatExporter());

// Usage in controllers/services
public class ReportController
{
    private readonly IMyFormatExporter _exporter;

    public ReportController(IMyFormatExporter exporter)
    {
        _exporter = exporter;
    }

    public async Task Export(LayoutTree tree)
    {
        using var output = new MemoryStream();
        await _exporter.ExportAsync(tree, output);
        // Return output to client...
    }
}
```

## Testing Guidelines

1. **Unit Tests**: Test individual rendering methods (text, shapes, tables).
2. **Integration Tests**: Test complete export flow with real or constructed LayoutTrees.
3. **Golden Tests**: Store reference outputs to verify deterministic behavior.
4. **Error Cases**:
   - Null input handling
   - Invalid element combinations
   - Stream write failures
5. **Performance**: Benchmark with large trees (100+ pages).

## Best Practices

1. **Stream to Output**: Write incrementally to avoid large memory allocations.
2. **Async/Await**: Always use async I/O for scalability.
3. **Coordinate Precision**: Preserve DIPs (Device Independent Pixels) without lossy conversion.
4. **Color Fidelity**: Use exact RGB/ARGB values from `ResolvedStyle`.
5. **Font Handling**: Verify font families exist in the target format; fallback gracefully.
6. **Determinism**: Ensure identical output for identical input (critical for reports).
7. **Documentation**: Include XML doc comments on all public members.
8. **Error Messages**: Provide actionable error messages for failures.

## Example: JSON Exporter

For a quick example of a real exporter, see `KineticReports.Export.Html` in the repository. It demonstrates:

- Type-based element dispatch
- CSS style generation for text and shapes
- Image encoding (base64 data URLs)
- Proper namespace organization
- Comprehensive test coverage

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Exporter produces invalid output | Verify all element types are handled in the dispatcher |
| Output is missing content | Check that all `Children` collections are recursively rendered |
| Colors/fonts not preserved | Ensure `ResolvedStyle` properties map correctly to target format |
| Tests fail with null reference | Validate that required properties (Id, Bounds, Style) are initialized |
| Performance is slow | Stream output incrementally; avoid buffering entire tree in memory |

## Next Steps

- Implement your exporter following this guide
- Write comprehensive integration tests
- Add to `KineticReports.slnx` solution
- Create NuGet package for distribution
- Document format-specific limitations or extensions

