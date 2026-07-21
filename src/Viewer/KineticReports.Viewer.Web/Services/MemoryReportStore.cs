namespace KineticReports.Viewer.Web.Services;

using KineticReports.Core.Layout;

/// <summary>
/// In-memory implementation of report storage with automatic expiration.
/// </summary>
public sealed class MemoryReportStore : IReportStore
{
    private readonly Dictionary<string, CachedReport> _cache = new();
    private readonly TimeSpan _expirationTime;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes the store with a default expiration time of 1 hour.
    /// </summary>
    public MemoryReportStore(TimeSpan? expirationTime = null)
    {
        _expirationTime = expirationTime ?? TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Saves a report document with the given operation ID.
    /// </summary>
    public Task SaveAsync(string operationId, ReportDocument reportDocument, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(operationId))
            throw new ArgumentException("Operation ID is required", nameof(operationId));
        if (reportDocument == null)
            throw new ArgumentNullException(nameof(reportDocument));

        lock (_lock)
        {
            _cache[operationId] = new CachedReport
            {
                Document = reportDocument,
                ExpiresAt = DateTime.UtcNow.Add(_expirationTime)
            };
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a stored report document by operation ID.
    /// </summary>
    public Task<ReportDocument?> RetrieveAsync(string operationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(operationId))
            return Task.FromResult((ReportDocument?)null);

        lock (_lock)
        {
            if (!_cache.TryGetValue(operationId, out var cached))
                return Task.FromResult((ReportDocument?)null);

            if (DateTime.UtcNow > cached.ExpiresAt)
            {
                _cache.Remove(operationId);
                return Task.FromResult((ReportDocument?)null);
            }

            return Task.FromResult((ReportDocument?)cached.Document);
        }
    }

    /// <summary>
    /// Clears the stored result.
    /// </summary>
    public Task ClearAsync(string operationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(operationId))
            return Task.CompletedTask;

        lock (_lock)
        {
            _cache.Remove(operationId);
        }

        return Task.CompletedTask;
    }

    private sealed class CachedReport
    {
        public required ReportDocument Document { get; init; }
        public required DateTime ExpiresAt { get; init; }
    }
}
