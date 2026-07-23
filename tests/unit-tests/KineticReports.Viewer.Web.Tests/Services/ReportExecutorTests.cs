using KineticReports.Viewer.Web.Services;
using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace KineticReports.Viewer.Web.Tests;

public class ReportExecutorTests
{
    private sealed class NoOpTextLayout : ITextLayout
    {
        public Size MeasureText(string text, AppliedStyle style, float maxWidth) => new(0f, 0f);

        public IReadOnlyList<TextRun> ShapeText(string text, AppliedStyle style, Rect bounds) => [];

        public float GetAscent(FontDescriptor descriptor) => 0f;

        public float GetDescent(FontDescriptor descriptor) => 0f;

        public float GetLineGap(FontDescriptor descriptor) => 0f;
    }

    [Fact]
    public void ReportExecutor_WithNullEngine_ThrowsArgumentNullException()
    {
        var textLayout = new NoOpTextLayout();
        var options = Options.Create(new RenderingPipelineOptions());
        var logger = NullLogger<ReportExecutor>.Instance;

        // Act & Assert
        Should.Throw<ArgumentNullException>(
            () => new ReportExecutor(null!, textLayout, options, logger));
    }
}
