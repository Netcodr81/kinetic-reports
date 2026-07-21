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
    /// Saves a report layout with the given operation ID.
    /// </summary>
    public Task SaveAsync(string operationId, ReportLayout ReportLayout, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(operationId))
            throw new ArgumentException("Operation ID is required", nameof(operationId));
        if (ReportLayout == null)
            throw new ArgumentNullException(nameof(ReportLayout));

        lock (_lock)
        {
            _cache[operationId] = new CachedReport
            {
                ReportLayout = ReportLayout,
                ExpiresAt = DateTime.UtcNow.Add(_expirationTime)
            };
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a stored report layout by operation ID.
    /// </summary>
    public Task<ReportLayout?> RetrieveAsync(string operationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(operationId))
            return Task.FromResult((ReportLayout?)null);

        lock (_lock)
        {
            if (!_cache.TryGetValue(operationId, out var cached))
                return Task.FromResult((ReportLayout?)null);

            if (DateTime.UtcNow > cached.ExpiresAt)
            {
                _cache.Remove(operationId);
                return Task.FromResult((ReportLayout?)null);
            }

            return Task.FromResult((ReportLayout?)cached.ReportLayout);
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
        public required ReportLayout ReportLayout { get; init; }
        public required DateTime ExpiresAt { get; init; }
    }
}
