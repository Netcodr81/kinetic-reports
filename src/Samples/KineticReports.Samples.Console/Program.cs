using System.Diagnostics;
using KineticReports.Core.Definition;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;
using KineticReports.Engine;
using KineticReports.Export.Html;
using KineticReports.Rendering.Skia;
using KineticReports.Layout;

// ============================================================================
// KineticReports Sample: Console Report Generator
// ============================================================================
// This sample demonstrates how to:
// 1. Create a report definition programmatically
// 2. Execute the report with the layout engine
// 3. Export to HTML
// 
// To extend this sample:
// - Load report definitions from JSON files
// - Use data providers to fetch live data from databases
// - Export to different formats (PDF, Excel, etc.)
// - Apply custom styling and branding
// ============================================================================

Console.WriteLine("KineticReports Console Sample");
Console.WriteLine("==============================\n");

try
{
    // Initialize font metrics (required for layout)
    Console.WriteLine("Step 1: Initializing components...");
    var fontMetrics = new SkiaFontMetrics();

    // Create a sample report definition
    Console.WriteLine("Step 2: Creating report definition...");
    var report = new ReportDefinition
    {
        SchemaVersion = "1.0",
        Id = "sample-report-001",
        Name = "Sales Report",
        Author = "KineticReports Team",
        Description = "Example report demonstrating report generation"
    };

    // Create an empty layout tree for demonstration
    Console.WriteLine("Step 3: Creating empty layout tree...");
    var page = new PageElement
    {
        Id = "page-1",
        PageWidth = 800,
        PageHeight = 600,
        PageNumber = 1,
        Children = [],
        Style = new ResolvedStyle { FontFamily = "Arial", FontSize = 12f }
    };
    var layoutTree = new LayoutTree { Pages = [page] };

    // Initialize exporter
    var htmlExporter = new HtmlExporter();

    // Export to HTML
    Console.WriteLine("Step 4: Exporting to HTML...");
    var outputPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "report-sample.html");

    using (var outputStream = File.Create(outputPath))
    {
        await htmlExporter.ExportAsync(layoutTree, outputStream);
    }

    Console.WriteLine($"   ✓ Report exported to: {outputPath}");

    // Display summary
    Console.WriteLine("\n" + new string('=', 50));
    Console.WriteLine("Summary");
    Console.WriteLine(new string('=', 50));
    Console.WriteLine($"Report:           {report.Name}");
    Console.WriteLine($"Pages:            {layoutTree.Pages.Count}");
    Console.WriteLine($"Output Format:    HTML");
    Console.WriteLine($"Output Location:  {outputPath}");
    Console.WriteLine("\nSample completed successfully!");
    Console.WriteLine("\nNote: This sample creates a minimal report for demonstration.");
    Console.WriteLine("To use real data and full layout capabilities:");
    Console.WriteLine("  1. Use ReportEngine with IDataResolver and ILayoutEngine");
    Console.WriteLine("  2. Implement data providers for your data sources");
    Console.WriteLine("  3. Define report content in ReportDefinition");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine($"Details: {ex}");
    Environment.Exit(1);
}
