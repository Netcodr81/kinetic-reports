# KineticReports

Package layout:
- `KineticReports.Core` is the primary runtime package and contains authoring, engine, layout, rendering, built-in HTML/PDF export, plugin contracts/runtime, and SQL data providers.
- `KineticReports.Viewer.Blazor`, `KineticReports.Viewer.Mvc`, and `KineticReports.Viewer.Web` are optional viewer packages layered on Core.
- `KineticReports.Export.Excel` remains an example exporter package.
- `KineticReports.Samples.Blazor` consumes Core plus Viewer.Blazor.

The complete project documentation is in [docs/COMPREHENSIVE_DEVELOPER_GUIDE.md](./docs/COMPREHENSIVE_DEVELOPER_GUIDE.md).

Start with:
- [Comprehensive Developer Guide](./docs/COMPREHENSIVE_DEVELOPER_GUIDE.md)

## Default Engine Registration

`KineticReports.Core` provides a default DI registration path for the report pipeline:

```csharp
using KineticReports.Core.Engine.DependencyInjection;

builder.Services.AddKineticReportsEngine();
```

This registers defaults for:
- `IExpressionEvaluator` -> `LiteralEvaluator`
- `IDataResolver` -> `CompositeDataResolver`
- `IReportBuilder` -> `DefaultReportBuilder`
- `ILayoutEngine` -> `LayoutEngine`
- `IReportEngine` -> `ReportEngine`

You can still override any service afterwards with your own implementation.

## Viewer Registration (Opt-In)

Viewer services are not registered by default. Choose the viewer package (or custom implementation) you want.

Blazor viewer:

```csharp
using KineticReports.Samples.Blazor.DependencyInjection;

builder.Services
	.AddKeneticReports()
	.AddBlazorViewer();
```

MVC viewer:

```csharp
using KineticReports.Samples.Blazor.DependencyInjection;
using KineticReports.Viewer.Mvc;

builder.Services
	.AddKeneticReports()
	.AddViewer(services => services.AddKineticReportsViewerMvc());
```

Custom viewer registration:

```csharp
using KineticReports.Samples.Blazor.DependencyInjection;

builder.Services
	.AddKeneticReports()
	.AddViewer(services =>
	{
		services.AddScoped<IMyViewerService, MyViewerService>();
	});
```

## DefaultReportBuilder (Fluent)

`DefaultReportBuilder` is now a full fluent builder for code-first report construction.

```csharp
using KineticReports.Core.Engine.Building;
using KineticReports.Core.Layout;

var reportBuilder = DefaultReportBuilder.Create()
	.AddTextRegion("title", "Quarterly Sales", BlockType.ReportHeader)
	.ForEachRowText("orders", "order-line", "{CustomerName}")
	.AddDataSourceTable(
		dataSourceId: "orders",
		regionId: "orders-region",
		tableId: "orders-table",
		columns:
		[
			new DefaultReportBuilder.DataSourceTableColumn
			{
				Header = "Customer",
				ValueExpression = "{CustomerName}",
				Column = new TableColumn { MinWidth = 120f, Grow = 1f }
			},
			new DefaultReportBuilder.DataSourceTableColumn
			{
				Header = "Amount",
				ValueExpression = "{Amount}",
				Column = new TableColumn { Width = 90f, MinWidth = 90f, Grow = 0f }
			}
		],
		groupByExpression: "{Region}",
		footerAggregates:
		[
			new DefaultReportBuilder.TableAggregateDefinition
			{
				ColumnIndex = 1,
				ValueExpression = "{Amount}",
				Kind = DefaultReportBuilder.TableAggregateKind.Sum,
				FormatString = "0.00"
			}
		],
		footerLabel: "Grand Total");
```

If your scenario differs, implement `IReportBuilder` directly and replace the registration.
