namespace KineticReports.Engine.Tests.Fakes;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Engine.Building;
using KineticReports.Engine.Data;
using KineticReports.Engine.Expressions;

/// <summary>
/// A configurable <see cref="IReportBuilder"/> stub that returns a pre-set band list.
/// </summary>
public sealed class StubReportBuilder : IReportBuilder
{
    private static readonly ResolvedStyle DefaultStyle = new() { FontFamily = "Arial", FontSize = 12f };

    private readonly IReadOnlyList<BandElement> _bands;

    public StubReportBuilder(IReadOnlyList<BandElement>? bands = null)
    {
        _bands = bands ?? [MakeBand()];
    }

    /// <inheritdoc/>
    public IReadOnlyList<BandElement> Build(DataContext dataContext, IExpressionEvaluator evaluator)
        => _bands;

    private static BandElement MakeBand() => new()
    {
        Id = "detail-1",
        Style = DefaultStyle,
        Kind = BandKind.Detail,
        Children = []
    };
}
