# 18 - Named Data Sources Quick Start

Use this guide when you want multiple providers of the same type (for example, multiple SQLite connections).

## Why this exists

`DataSourceDefinition` now requires:

1. `ProviderType`
2. `SourceName`

Resolution is explicit by **SourceName + ProviderType**.

## Register providers in host startup

Use direct named registration in `Program.cs`:

```csharp
var primarySqlite = builder.Configuration.GetConnectionString("SqlLitePrimary")
	?? "Data Source=Data/primary.db;Mode=ReadWriteCreate;Cache=Shared";

var archiveSqlite = builder.Configuration.GetConnectionString("SqlLiteArchive")
	?? "Data Source=Data/archive.db;Mode=ReadWriteCreate;Cache=Shared";

builder.Services
	.AddKeneticReports()
	.AddSqlLiteDataProvider("PrimarySqlLite", primarySqlite)
	.AddSqlLiteDataProvider("ArchiveSqlLite", archiveSqlite)
	.AddSqlServerDataProvider("OperationalSqlServer", builder.Configuration.GetConnectionString("SqlServer")!)
	.AddBlazorViewer();
```

Notes:

- `sourceName` must be unique for each registration.
- You can register multiple providers of the same type.
- For in-memory datasets, the default source is `DefaultInMemory` with provider type `Poco`.

## Reference a specific source in report JSON

```json
{
  "id": "sales-orders",
  "name": "Sales Orders",
  "providerType": "SqlLite",
  "sourceName": "PrimarySqlLite",
  "properties": {
	"Query": "SELECT * FROM SalesOrders;"
  }
}
```

Switching to another same-type source is just a `sourceName` change:

- `PrimarySqlLite`
- `ArchiveSqlLite`

## Code-first example

```csharp
new DataSourceDefinition
{
	Id = "sales-orders",
	Name = "Sales Orders",
	ProviderType = "SqlLite",
	SourceName = "PrimarySqlLite",
	Properties = new Dictionary<string, string>
	{
		["Query"] = "SELECT * FROM SalesOrders;"
	}
};
```

## Common mistakes

1. Missing `SourceName` on a data source.
2. `SourceName` not matching any registered provider.
3. `ProviderType` mismatch (for example `SqlLite` data source against a `SqlServer` registration).
4. Missing `Query` for SQL-based providers when your resolver expects it.

## Recommended naming pattern

Use environment-agnostic logical names:

- `PrimarySqlLite`
- `ArchiveSqlLite`
- `OperationalSqlServer`
- `AnalyticsSqlServer`

Keep connection strings in host configuration, not in report definitions.
