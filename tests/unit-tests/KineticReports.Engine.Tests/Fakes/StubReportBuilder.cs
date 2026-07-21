namespace KineticReports.Engine.Tests.Fakes;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Engine.Building;
using KineticReports.Engine.Data;
using KineticReports.Engine.Expressions;

/// <summary>
/// A configurable <see cref="IReportBuilder"/> stub that returns a pre-set content-region list.
/// </summary>
public sealed class StubReportBuilder : IReportBuilder
{
    private static readonly AppliedStyle DefaultStyle = new() { FontFamily = "Arial", FontSize = 12f };

    private readonly IReadOnlyList<ReportBlock> _contentRegions;

    public StubReportBuilder(IReadOnlyList<ReportBlock>? contentRegions = null)
    {
        _contentRegions = contentRegions ?? [MakeContentRegion()];
    }

    /// <inheritdoc/>
    public IReadOnlyList<ReportBlock> Build(DataContext dataContext, IExpressionEvaluator evaluator)
        => _contentRegions;

    private static DetailBlock MakeContentRegion() => new()
    {
        Id = "detail-1",
        Style = DefaultStyle,
        Children = []
    };
}
