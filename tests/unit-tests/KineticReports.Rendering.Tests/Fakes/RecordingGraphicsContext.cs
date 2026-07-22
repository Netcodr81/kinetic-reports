namespace KineticReports.Rendering.Tests.Fakes;

using KineticReports.Core.Layout;
using KineticReports.Core.Rendering;

/// <summary>
/// An <see cref="IGraphicsContext"/> that records every draw call for assertion in tests.
/// </summary>
public sealed class RecordingGraphicsContext : IGraphicsContext
{
    private readonly List<string> _calls = [];

    /// <summary>Gets the ordered list of recorded calls.</summary>
    public IReadOnlyList<string> Calls => _calls;

    public void FillRectangle(Rect bounds, Color color) => Record($"FillRect({bounds.X},{bounds.Y},{bounds.Width},{bounds.Height})");
    public void FillEllipse(Rect bounds, Color color) => Record($"FillEllipse({bounds.X},{bounds.Y})");
    public void StrokeRectangle(Rect bounds, Color color, float strokeWidth) => Record($"StrokeRect({bounds.X},{bounds.Y})");
    public void StrokeEllipse(Rect bounds, Color color, float strokeWidth) => Record($"StrokeEllipse({bounds.X},{bounds.Y})");
    public void DrawLine(Point from, Point to, Color color, float strokeWidth) => Record($"DrawLine({from.X},{from.Y}->{to.X},{to.Y})");
    public void DrawPath(PathGeometry path) => Record("DrawPath");
    public void DrawText(TextRun run) => Record($"DrawText({run.Text})");
    public void DrawImage(Rect destinationBounds, ImageReference image, ImageStretch stretch) => Record($"DrawImage({image.SourceKey})");
    public void PushClip(Rect clip) => Record("PushClip");
    public void PopClip() => Record("PopClip");
    public void PushOpacity(float opacity) => Record($"PushOpacity({opacity:F2})");
    public void PopOpacity() => Record("PopOpacity");

    private void Record(string call) => _calls.Add(call);
}
