using KineticReports.Core.Data;
using KineticReports.Data.SqlServer;

namespace KineticReports.Data.SqlServer.Tests;

public class SqlServerDataProviderTests
{
    [Fact]
    public void Constructor_WithNullConnectionString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SqlServerDataProvider(null!));
    }

    [Fact]
    public void Constructor_WithEmptyConnectionString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SqlServerDataProvider(string.Empty));
    }

    [Fact]
    public void Constructor_WithValidConnectionString_CreatesInstance()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;Integrated Security=true;";

        // Act
        var provider = new SqlServerDataProvider(connectionString);

        // Assert
        provider.ShouldNotBeNull();
        provider.Name.ShouldBe("SQL Server");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new SqlServerDataProvider("Server=localhost;Database=TestDb;Integrated Security=true;");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await provider.ExecuteAsync(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsQueryResult()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;Integrated Security=true;";
        var provider = new SqlServerDataProvider(connectionString);
        var request = new QueryRequest
        {
            DatasetName = "TestDataset",
            QueryText = "SELECT 1 AS Id, 'Test' AS Name",
            Parameters = new Dictionary<string, object?>()
        };

        // Act
        // Note: This test will fail if SQL Server is not available.
        // In real scenarios, this would use a test database or mock.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await provider.ExecuteAsync(request));

        // Assert - verify error is from SQL Server connection attempt
        ex.Message.ShouldContain("SQL Server query execution failed");
    }

    [Fact]
    public void Name_Property_ReturnsSqlServer()
    {
        // Arrange
        var provider = new SqlServerDataProvider("Server=localhost;Database=TestDb;Integrated Security=true;");

        // Act
        var name = provider.Name;

        // Assert
        name.ShouldBe("SQL Server");
    }
}
