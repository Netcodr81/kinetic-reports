using KineticReports.Viewer.Web.Services;

namespace KineticReports.Viewer.Web.Tests;

public class ReportExecutorTests
{
    [Fact]
    public void ReportExecutor_WithNullEngine_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(
            () => new ReportExecutor(null!, null!));
    }
}
