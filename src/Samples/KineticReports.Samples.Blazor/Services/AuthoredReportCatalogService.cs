namespace KineticReports.Samples.Blazor.Services;

using System.Text.Json;

/// <summary>
/// Represents a saved authored report catalog item.
/// </summary>
/// <param name="Id">Stable item identifier.</param>
/// <param name="Name">User-visible report name.</param>
/// <param name="SampleId">Source sample id used to author the report.</param>
/// <param name="ProviderType">Provider type associated with this report.</param>
/// <param name="UpdatedUtc">Last update timestamp (UTC).</param>
public sealed record AuthoredReportCatalogItem(
    string Id,
    string Name,
    string SampleId,
    string ProviderType,
    DateTime UpdatedUtc);

/// <summary>
/// Represents a saved authored report including JSON payload.
/// </summary>
/// <param name="Item">Catalog metadata.</param>
/// <param name="Json">Serialized authored report JSON.</param>
public sealed record AuthoredReportCatalogEntry(AuthoredReportCatalogItem Item, string Json);

/// <summary>
/// Contract for persisted authored report catalog operations.
/// </summary>
public interface IAuthoredReportCatalogService
{
    /// <summary>
    /// Lists all saved authored reports.
    /// </summary>
    Task<IReadOnlyList<AuthoredReportCatalogItem>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates an authored report entry.
    /// </summary>
    Task<AuthoredReportCatalogEntry> SaveAsync(
        string? id,
        string name,
        string sampleId,
        string providerType,
        string json,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a saved authored report entry.
    /// </summary>
    Task<AuthoredReportCatalogEntry?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a saved authored report entry.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

/// <summary>
/// File-based implementation of <see cref="IAuthoredReportCatalogService"/>.
/// </summary>
public sealed class AuthoredReportCatalogService : IAuthoredReportCatalogService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _catalogDirectory;

    /// <summary>
    /// Initializes a new <see cref="AuthoredReportCatalogService"/>.
    /// </summary>
    public AuthoredReportCatalogService()
    {
        var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _catalogDirectory = Path.Combine(baseDirectory, "KineticReports", "SampleCatalog");
        Directory.CreateDirectory(_catalogDirectory);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuthoredReportCatalogItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<AuthoredReportCatalogItem>();

        foreach (var path in Directory.EnumerateFiles(_catalogDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            var entry = await ReadEntryAsync(path, cancellationToken).ConfigureAwait(false);
            if (entry is null)
                continue;

            results.Add(entry.Item);
        }

        return results
            .OrderByDescending(item => item.UpdatedUtc)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<AuthoredReportCatalogEntry> SaveAsync(
        string? id,
        string name,
        string sampleId,
        string providerType,
        string json,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Report name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(sampleId))
            throw new ArgumentException("Sample id is required.", nameof(sampleId));

        if (string.IsNullOrWhiteSpace(providerType))
            throw new ArgumentException("Provider type is required.", nameof(providerType));

        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("Report JSON is required.", nameof(json));

        var resolvedId = string.IsNullOrWhiteSpace(id)
            ? Guid.NewGuid().ToString("N")
            : id;

        var entry = new PersistedCatalogEntry
        {
            Id = resolvedId,
            Name = name.Trim(),
            SampleId = sampleId,
            ProviderType = providerType,
            Json = json,
            UpdatedUtc = DateTime.UtcNow
        };

        var path = GetPath(resolvedId);
        var payload = JsonSerializer.Serialize(entry, SerializerOptions);
        await File.WriteAllTextAsync(path, payload, cancellationToken).ConfigureAwait(false);

        return ToPublicEntry(entry);
    }

    /// <inheritdoc/>
    public async Task<AuthoredReportCatalogEntry?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var path = GetPath(id);
        return await ReadEntryAsync(path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult(false);

        var path = GetPath(id);
        if (!File.Exists(path))
            return Task.FromResult(false);

        File.Delete(path);
        return Task.FromResult(true);
    }

    private async Task<AuthoredReportCatalogEntry?> ReadEntryAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            return null;

        var content = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        var persisted = JsonSerializer.Deserialize<PersistedCatalogEntry>(content, SerializerOptions);
        if (persisted is null || string.IsNullOrWhiteSpace(persisted.Id))
            return null;

        return ToPublicEntry(persisted);
    }

    private static AuthoredReportCatalogEntry ToPublicEntry(PersistedCatalogEntry persisted)
    {
        return new AuthoredReportCatalogEntry(
            new AuthoredReportCatalogItem(
                persisted.Id,
                persisted.Name,
                persisted.SampleId,
                persisted.ProviderType,
                persisted.UpdatedUtc),
            persisted.Json);
    }

    private string GetPath(string id)
    {
        var safeId = string.Concat(id.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_'));
        if (string.IsNullOrWhiteSpace(safeId))
            safeId = Guid.NewGuid().ToString("N");

        return Path.Combine(_catalogDirectory, $"{safeId}.json");
    }

    private sealed class PersistedCatalogEntry
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string SampleId { get; set; } = string.Empty;

        public string ProviderType { get; set; } = string.Empty;

        public string Json { get; set; } = string.Empty;

        public DateTime UpdatedUtc { get; set; }
    }
}
