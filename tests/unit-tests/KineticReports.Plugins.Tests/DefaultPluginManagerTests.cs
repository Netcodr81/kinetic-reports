namespace KineticReports.Plugins.Tests;

using KineticReports.Core.Plugins;
using Shouldly;
using Xunit;

/// <summary>
/// Tests for <see cref="DefaultPluginManager"/>.
/// </summary>
public class DefaultPluginManagerTests
{
    [Fact]
    public void Constructor_CreatesNewInstance()
    {
        // Act
        var manager = new DefaultPluginManager();

        // Assert
        manager.ShouldNotBeNull();
        manager.LoadedPlugins.ShouldBeEmpty();
    }

    [Fact]
    public void LoadedPlugins_ReturnsEmptyListInitially()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act
        var plugins = manager.LoadedPlugins;

        // Assert
        plugins.ShouldNotBeNull();
        plugins.ShouldBeEmpty();
    }

    [Fact]
    public async Task DiscoverAndLoadPluginsAsync_WithNullDirectory_ThrowsArgumentException()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var serviceProvider = new DummyServiceProvider();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => manager.DiscoverAndLoadPluginsAsync(null!, serviceProvider));
    }

    [Fact]
    public async Task DiscoverAndLoadPluginsAsync_WithEmptyDirectory_ThrowsArgumentException()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var serviceProvider = new DummyServiceProvider();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => manager.DiscoverAndLoadPluginsAsync(string.Empty, serviceProvider));
    }

    [Fact]
    public async Task DiscoverAndLoadPluginsAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var serviceProvider = new DummyServiceProvider();
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        await Should.ThrowAsync<DirectoryNotFoundException>(
            () => manager.DiscoverAndLoadPluginsAsync(nonExistentDir, serviceProvider));
    }

    [Fact]
    public async Task DiscoverAndLoadPluginsAsync_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var tempDir = Path.GetTempPath();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(
            () => manager.DiscoverAndLoadPluginsAsync(tempDir, null!));
    }

    [Fact]
    public async Task DiscoverAndLoadPluginsAsync_WithEmptyDirectory_ReturnsWithoutError()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var serviceProvider = new DummyServiceProvider();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            await manager.DiscoverAndLoadPluginsAsync(tempDir, serviceProvider);

            // Assert
            manager.LoadedPlugins.ShouldBeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task LoadPluginAsync_WithNullAssemblyPath_ThrowsArgumentException()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var serviceProvider = new DummyServiceProvider();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => manager.LoadPluginAsync(null!, serviceProvider));
    }

    [Fact]
    public async Task LoadPluginAsync_WithEmptyAssemblyPath_ThrowsArgumentException()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var serviceProvider = new DummyServiceProvider();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => manager.LoadPluginAsync(string.Empty, serviceProvider));
    }

    [Fact]
    public async Task LoadPluginAsync_WithNonExistentAssembly_ThrowsFileNotFoundException()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var serviceProvider = new DummyServiceProvider();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dll");

        // Act & Assert
        await Should.ThrowAsync<FileNotFoundException>(
            () => manager.LoadPluginAsync(nonExistentFile, serviceProvider));
    }

    [Fact]
    public async Task LoadPluginAsync_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var manager = new DefaultPluginManager();
        var currentAssembly = typeof(DefaultPluginManagerTests).Assembly.Location;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(
            () => manager.LoadPluginAsync(currentAssembly, null!));
    }

    [Fact]
    public async Task UnloadPluginAsync_WithNullId_ThrowsArgumentException()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => manager.UnloadPluginAsync(null!));
    }

    [Fact]
    public async Task UnloadPluginAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => manager.UnloadPluginAsync(string.Empty));
    }

    [Fact]
    public async Task UnloadPluginAsync_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            () => manager.UnloadPluginAsync("non.existent.plugin"));
    }

    [Fact]
    public void GetPluginById_WithNullId_ReturnsNull()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act
        var result = manager.GetPluginById(null!);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetPluginById_WithEmptyId_ReturnsNull()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act
        var result = manager.GetPluginById(string.Empty);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetPluginById_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act
        var result = manager.GetPluginById("non.existent.plugin");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task UnloadAllAsync_WithNoPlugins_DoesNotThrow()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act & Assert
        await manager.UnloadAllAsync();
    }

    [Fact]
    public async Task DisposeAsync_CallsUnloadAll()
    {
        // Arrange
        var manager = new DefaultPluginManager();

        // Act & Assert
        await ((IAsyncDisposable)manager).DisposeAsync();
    }
}

/// <summary>
/// Dummy service provider for testing.
/// </summary>
internal sealed class DummyServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return null;
    }
}
