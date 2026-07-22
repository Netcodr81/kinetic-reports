namespace KineticReports.Samples.Blazor.Services;

using Microsoft.Data.Sqlite;

/// <summary>
/// Creates and seeds the sample SQLite database used by sample reports.
/// </summary>
internal static class SampleSqlLiteDatabaseInitializer
{
    private const string SeedScriptFileName = "sample-reports.seed.sql";
    private const string DatabaseFileName = "sample-reports.db";

    /// <summary>
    /// Ensures the SQLite database exists and is populated with deterministic sample rows.
    /// </summary>
    /// <param name="contentRootPath">The application content root path.</param>
    public static void EnsureSeeded(string contentRootPath)
    {
        var dataDirectory = Path.Combine(contentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);

        var seedScriptPath = Path.Combine(dataDirectory, SeedScriptFileName);
        if (!File.Exists(seedScriptPath))
        {
            throw new FileNotFoundException($"Seed script not found: {seedScriptPath}");
        }

        var dbPath = Path.Combine(dataDirectory, DatabaseFileName);
        var connectionString = $"Data Source={dbPath}";
        var script = File.ReadAllText(seedScriptPath);

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = script;
        command.ExecuteNonQuery();
    }
}
