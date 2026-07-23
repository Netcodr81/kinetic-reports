namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Geometry;
using KineticReports.Viewer.Web.Models;
using KineticReports.Visual;

/// <summary>
/// Default implementation of <see cref="IReportHitTestService"/>.
/// </summary>
public sealed class ReportHitTestService : IReportHitTestService
{
    private readonly IReportStore _reportStore;
    private readonly IVisualDocumentBuilder _visualDocumentBuilder;
    private readonly VisualHitTestIndexBuilder _hitTestIndexBuilder;

    /// <summary>
    /// Initializes a new <see cref="ReportHitTestService"/>.
    /// </summary>
    public ReportHitTestService(
        IReportStore reportStore,
        IVisualDocumentBuilder visualDocumentBuilder,
        VisualHitTestIndexBuilder hitTestIndexBuilder)
    {
        _reportStore = reportStore ?? throw new ArgumentNullException(nameof(reportStore));
        _visualDocumentBuilder = visualDocumentBuilder ?? throw new ArgumentNullException(nameof(visualDocumentBuilder));
        _hitTestIndexBuilder = hitTestIndexBuilder ?? throw new ArgumentNullException(nameof(hitTestIndexBuilder));
    }

    /// <inheritdoc/>
    public async Task<(bool Found, ReportHitTestResponse Response)> HitTestAsync(
        string operationId,
        int pageNumber,
        float x,
        float y,
        CancellationToken ct = default)
    {
        var reportDocument = await _reportStore.RetrieveAsync(operationId, ct).ConfigureAwait(false);
        if (reportDocument == null)
        {
            return (false, new ReportHitTestResponse
            {
                Hit = false,
                PageNumber = pageNumber,
                X = x,
                Y = y
            });
        }

        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        var hitIndex = _hitTestIndexBuilder.Build(visualDocument);
        var hit = hitIndex.HitTest(pageNumber, new Point(x, y));

        if (hit == null)
        {
            return (true, new ReportHitTestResponse
            {
                Hit = false,
                PageNumber = pageNumber,
                X = x,
                Y = y
            });
        }

        return (true, new ReportHitTestResponse
        {
            Hit = true,
            PageNumber = pageNumber,
            X = x,
            Y = y,
            ElementId = hit.Element.Id,
            LayerName = hit.LayerName,
            Metadata = hit.Element.Metadata
        });
    }
}
