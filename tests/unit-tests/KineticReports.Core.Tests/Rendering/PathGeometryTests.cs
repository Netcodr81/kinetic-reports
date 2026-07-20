namespace KineticReports.Core.Tests.Rendering;

using Shouldly;
using KineticReports.Core.Geometry;
using KineticReports.Core.Rendering;
using KineticReports.Core.Styling;

public class PathGeometryTests
{
    [Fact]
    public void PathGeometry_WithFillAndStroke_IsCreated()
    {
        var commands = new[]
        {
            new PathCommand(PathCommandKind.MoveTo, new[] { new Point(0, 0) }),
            new PathCommand(PathCommandKind.LineTo, new[] { new Point(100, 0) })
        };
        var path = new PathGeometry
        {
            Commands = commands,
            Fill = Color.FromRgb(0, 0, 255),  // Blue
            Stroke = Color.Black,
            StrokeWidth = 2f
        };
        path.Commands.Count.ShouldBe(2);
        path.Fill.ShouldBe(Color.FromRgb(0, 0, 255));
        path.Stroke.ShouldBe(Color.Black);
        path.StrokeWidth.ShouldBe(2f);
    }

    [Fact]
    public void PathGeometry_StrokeWidth_DefaultsToOne()
    {
        var path = new PathGeometry
        {
            Commands = new List<PathCommand>(),
            Fill = Color.FromRgb(255, 0, 0)  // Red
        };
        path.StrokeWidth.ShouldBe(1f);
    }

    [Fact]
    public void PathCommand_MoveTo_HasSinglePoint()
    {
        var command = new PathCommand(PathCommandKind.MoveTo, new[] { new Point(10, 20) });
        command.Kind.ShouldBe(PathCommandKind.MoveTo);
        command.Points.Count.ShouldBe(1);
    }

    [Fact]
    public void PathCommand_CubicBezierTo_HasThreePoints()
    {
        var points = new[] {
            new Point(10, 20),
            new Point(30, 40),
            new Point(50, 60)
        };
        var command = new PathCommand(PathCommandKind.CubicBezierTo, points);
        command.Kind.ShouldBe(PathCommandKind.CubicBezierTo);
        command.Points.Count.ShouldBe(3);
    }

    [Fact]
    public void PathCommand_Close_HasNoPoints()
    {
        var command = new PathCommand(PathCommandKind.Close, new List<Point>());
        command.Kind.ShouldBe(PathCommandKind.Close);
        command.Points.Count.ShouldBe(0);
    }
}
