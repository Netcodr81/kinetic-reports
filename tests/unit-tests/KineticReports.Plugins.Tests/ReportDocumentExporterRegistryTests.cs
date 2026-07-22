namespace KineticReports.Plugins.Tests;

using KineticReports.Core.Layout;
using Shouldly;
using Xunit;

/// <summary>
/// Tests for <see cref="ReportDocumentExporterRegistry"/>.
/// </summary>
public class ReportDocumentExporterRegistryTests
{
    [Fact]
    public void GetAvailableFormats_ReturnsRegisteredFormats()
    {
        // Arrange
        var registry = new ReportDocumentExporterRegistry(
        [
            new FakeExporter("html", "HTML", "text/html", "html"),
            new FakeExporter("pdf", "PDF", "application/pdf", "pdf")
        ]);

        // Act
        var formats = registry.GetAvailableFormats();

        // Assert
        formats.Select(f => f.FormatId).ShouldBe(["html", "pdf"]);
    }

    [Fact]
    public void TryGetExporter_WithCaseInsensitiveFormatId_ResolvesExporter()
    {
        // Arrange
        var expected = new FakeExporter("html", "HTML", "text/html", "html");
        var registry = new ReportDocumentExporterRegistry([expected]);

        // Act
        var found = registry.TryGetExporter("HTML", out var exporter);

        // Assert
        found.ShouldBeTrue();
        exporter.ShouldBeSameAs(expected);
    }

    [Fact]
    public void TryGetExporter_WithUnknownFormat_ReturnsFalse()
    {
        // Arrange
        var registry = new ReportDocumentExporterRegistry(
        [
            new FakeExporter("html", "HTML", "text/html", "html")
        ]);

        // Act
        var found = registry.TryGetExporter("xlsx", out var exporter);

        // Assert
        found.ShouldBeFalse();
        exporter.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithDuplicateFormatIds_PicksDeterministically()
    {
        // Arrange
        var alpha = new AlphaHtmlExporter();
        var zeta = new ZetaHtmlExporter();

        // Act
        var registry = new ReportDocumentExporterRegistry([zeta, alpha]);
        var found = registry.TryGetExporter("html", out var exporter);

        // Assert
        found.ShouldBeTrue();
        exporter.ShouldBeOfType<AlphaHtmlExporter>();
    }

    private sealed class FakeExporter(string id, string name, string mimeType, string ext) : IReportDocumentExporter
    {
        public ExportFormatDescriptor Format => new(id, name, mimeType, ext);

        public Task<byte[]> ExportAsync(ReportDocument ReportDocument, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Array.Empty<byte>());
        }
    }

    private sealed class AlphaHtmlExporter : IReportDocumentExporter
    {
        public ExportFormatDescriptor Format => new("html", "HTML", "text/html", "html");

        public Task<byte[]> ExportAsync(ReportDocument ReportDocument, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Array.Empty<byte>());
        }
    }

    private sealed class ZetaHtmlExporter : IReportDocumentExporter
    {
        public ExportFormatDescriptor Format => new("html", "HTML", "text/html", "html");

        public Task<byte[]> ExportAsync(ReportDocument ReportDocument, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Array.Empty<byte>());
        }
    }
}
