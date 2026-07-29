using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Loggers;

namespace KineticReports.Perf.Tests;

/// <summary>
/// Global performance benchmarking configuration.
/// Produces detailed CSV reports and memory diagnostics.
/// </summary>
public class PerformanceConfig : ManualConfig
{
    public PerformanceConfig()
    {
        // Add diagnosers for detailed analysis
        AddDiagnoser(MemoryDiagnoser.Default);

        // Export results to multiple formats
        AddExporter(CsvExporter.Default);
        AddExporter(HtmlExporter.Default);
        AddExporter(MarkdownExporter.GitHub);

        // Add console logger
        AddLogger(ConsoleLogger.Default);

        // Set job configuration
        WithOption(ConfigOptions.DisableOptimizationsValidator, true);
    }
}
