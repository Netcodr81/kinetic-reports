using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Viewer.Web.Services;

namespace KineticReports.Viewer.Web.Tests;

public class MemoryReportStoreTests
{
    private static ReportLayout CreateTestLayoutTree()
    {
        var defaultStyle = new ResolvedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f
        };

        var page = new PageElement
        {
            Id = "page-1",
            PageWidth = 800,
            PageHeight = 600,
            PageNumber = 1,
            Header = null,
            Footer = null,
            Children = [],
            Style = defaultStyle
        };

        return new ReportLayout { Pages = [page] };
    }

    [Fact]
    public async Task SaveAsync_WithValidOperationId_StoresLayoutTree()
    {
        // Arrange
        var store = new MemoryReportStore();
        var ReportLayout = CreateTestLayoutTree();
        var operationId = "test-op-1";

        // Act
        await store.SaveAsync(operationId, ReportLayout);

        // Assert
        var retrieved = await store.RetrieveAsync(operationId);
        retrieved.ShouldNotBeNull();
        retrieved.Pages.Count.ShouldBe(1);
    }

    [Fact]
    public async Task RetrieveAsync_WithNonExistentOperationId_ReturnsNull()
    {
        // Arrange
        var store = new MemoryReportStore();

        // Act
        var result = await store.RetrieveAsync("non-existent");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SaveAsync_WithNullLayoutTree_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new MemoryReportStore();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(
            () => store.SaveAsync("test-id", null!));
    }

    [Fact]
    public async Task SaveAsync_WithEmptyOperationId_ThrowsArgumentException()
    {
        // Arrange
        var store = new MemoryReportStore();
        var ReportLayout = CreateTestLayoutTree();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => store.SaveAsync("", ReportLayout));
    }

    [Fact]
    public async Task RetrieveAsync_WithEmptyOperationId_ReturnsNull()
    {
        // Arrange
        var store = new MemoryReportStore();

        // Act
        var result = await store.RetrieveAsync("");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ClearAsync_RemovesStoredResult()
    {
        // Arrange
        var store = new MemoryReportStore();
        var ReportLayout = CreateTestLayoutTree();
        var operationId = "test-op-2";
        await store.SaveAsync(operationId, ReportLayout);

        // Act
        await store.ClearAsync(operationId);

        // Assert
        var result = await store.RetrieveAsync(operationId);
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RetrieveAsync_AfterExpiration_ReturnsNull()
    {
        // Arrange
        var store = new MemoryReportStore(TimeSpan.FromMilliseconds(100));
        var ReportLayout = CreateTestLayoutTree();
        var operationId = "test-op-3";
        await store.SaveAsync(operationId, ReportLayout);

        // Act
        await Task.Delay(150); // Wait for expiration
        var result = await store.RetrieveAsync(operationId);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RetrieveAsync_BeforeExpiration_ReturnsLayoutTree()
    {
        // Arrange
        var store = new MemoryReportStore(TimeSpan.FromSeconds(5));
        var ReportLayout = CreateTestLayoutTree();
        var operationId = "test-op-4";
        await store.SaveAsync(operationId, ReportLayout);

        // Act
        var result = await store.RetrieveAsync(operationId);

        // Assert
        result.ShouldNotBeNull();
        result.Pages.Count.ShouldBe(1);
    }
}
