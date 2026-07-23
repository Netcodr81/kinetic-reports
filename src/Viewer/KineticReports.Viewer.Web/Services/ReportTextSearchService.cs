namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Visual;
using KineticReports.Viewer.Web.Models;

/// <summary>
/// Default implementation of <see cref="IReportTextSearchService"/>.
/// </summary>
public sealed class ReportTextSearchService : IReportTextSearchService
{
    private readonly IReportStore _reportStore;
    private readonly IVisualDocumentBuilder _visualDocumentBuilder;
    private readonly VisualTextSearchIndexBuilder _searchIndexBuilder;

    /// <summary>
    /// Initializes a new <see cref="ReportTextSearchService"/>.
    /// </summary>
    public ReportTextSearchService(
        IReportStore reportStore,
        IVisualDocumentBuilder visualDocumentBuilder,
        VisualTextSearchIndexBuilder searchIndexBuilder)
    {
        _reportStore = reportStore ?? throw new ArgumentNullException(nameof(reportStore));
        _visualDocumentBuilder = visualDocumentBuilder ?? throw new ArgumentNullException(nameof(visualDocumentBuilder));
        _searchIndexBuilder = searchIndexBuilder ?? throw new ArgumentNullException(nameof(searchIndexBuilder));
    }

    /// <inheritdoc/>
    public async Task<(bool Found, ReportTextSearchResponse Response)> SearchAsync(
        string operationId,
        string query,
        int? pageNumber = null,
        CancellationToken ct = default)
    {
        var reportDocument = await _reportStore.RetrieveAsync(operationId, ct).ConfigureAwait(false);
        if (reportDocument == null)
        {
            return (false, new ReportTextSearchResponse
            {
                Query = query,
                PageNumber = pageNumber,
                MatchCount = 0,
                Matches = []
            });
        }

        var visualDocument = _visualDocumentBuilder.Build(reportDocument);
        var index = _searchIndexBuilder.Build(visualDocument);
        var matches = index.Search(query, pageNumber)
            .Select(match => new ReportTextSearchMatchResponse
            {
                PageNumber = match.PageNumber,
                LayerName = match.LayerName,
                ElementId = match.ElementId,
                Text = match.Text
            })
            .ToList();

        return (true, new ReportTextSearchResponse
        {
            Query = query,
            PageNumber = pageNumber,
            MatchCount = matches.Count,
            Matches = matches
        });
    }
}
