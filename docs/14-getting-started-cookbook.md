# KineticReports Getting Started Cookbook

This guide provides five working code examples to get you started with KineticReports. Each example builds on fundamental concepts and can be used as a template for your own reports.

## Table of Contents

1. [Simple Report (Title + Text Sections)](#1-simple-report)
2. [Data-Driven Table](#2-data-driven-table)
3. [Multi-Section Report (Header, Detail, Footer)](#3-multi-section-report)
4. [Using Parameters in Expressions](#4-parameters)
5. [Custom Styling](#5-custom-styling)

---

## 1. Simple Report

**Goal:** Create a basic report with a title and text content.

**Concepts:** `TextBlock`, `HeaderBlock`, `ReportDefinition`, basic layout.

```csharp
using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Core.Geometry;

// Create the report definition
var definition = new ReportDefinition
{
    Blocks =
    [
        new HeaderBlock
        {
            Blocks =
            [
                new TextBlock
                {
                    Text = "Monthly Sales Report",
                    Style = new StyleDefinition
                    {
                        FontSize = 28,
                        FontWeight = 700,
                        TextAlignment = TextAlignment.Center,
                    }
                },
                new TextBlock
                {
                    Text = "October 2024",
                    Style = new StyleDefinition
                    {
                        FontSize = 14,
                        TextAlignment = TextAlignment.Center,
                        TextColor = new Color { R = 128, G = 128, B = 128 }
                    }
                }
            ]
        },
        new DetailBlock
        {
            Blocks =
            [
                new TextBlock
                {
                    Text = "This report summarizes sales performance for the current month.",
                    Style = new StyleDefinition { FontSize = 12, LineHeight = 1.5f }
                },
                new TextBlock
                {
                    Text = "Key Metrics: Total sales, region breakdown, top performers.",
                    Style = new StyleDefinition { FontSize = 12, LineHeight = 1.5f }
                }
            ]
        }
    ]
};

// Run the report
var reportService = serviceProvider.GetRequiredService<IReportService>();
var document = await reportService.ExecuteAsync(definition);

// Export to HTML or PDF
using (var outputStream = new FileStream("report.html", FileMode.Create))
{
    await document.ExportHtmlAsync(outputStream);
}
```

**Key Takeaways:**
- `HeaderBlock` contains report title and metadata (appears once at the top)
- `DetailBlock` contains main content (repeats for each data row, or once if no data)
- `TextBlock` displays static text
- `StyleDefinition` controls fonts, colors, and alignment
- Use `IReportService.ExecuteAsync()` to run the report

---

## 2. Data-Driven Table

**Goal:** Display tabular data (e.g., orders, invoices) in a grid.

**Concepts:** `TableBlock`, `RowBlock`, `CellBlock`, data binding with `{FieldName}`.

```csharp
using KineticReports.Core.Definition;
using KineticReports.Core.Data;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

// Prepare in-memory data
var orders = new[]
{
    new { OrderId = 1001, CustomerName = "Acme Corp", Amount = 5000m, Status = "Shipped" },
    new { OrderId = 1002, CustomerName = "TechStart", Amount = 3500m, Status = "Pending" },
    new { OrderId = 1003, CustomerName = "Global Inc", Amount = 8200m, Status = "Delivered" }
};

var dataProvider = new PocoDataProvider("OrderData", orders.AsEnumerable());

// Create the report definition
var definition = new ReportDefinition
{
    DataSources =
    [
        new DataSourceDefinition
        {
            Name = "OrderData",
            SourceName = "OrderData"
        }
    ],
    Blocks =
    [
        new HeaderBlock
        {
            Blocks =
            [
                new TextBlock
                {
                    Text = "Order Summary",
                    Style = new StyleDefinition { FontSize = 20, FontWeight = 700 }
                }
            ]
        },
        new DetailBlock
        {
            DataSourceName = "OrderData",
            Blocks =
            [
                new TableBlock
                {
                    Columns = new[] { "OrderId", "CustomerName", "Amount", "Status" },
                    Blocks =
                    [
                        new RowBlock
                        {
                            IsHeader = true,
                            Blocks =
                            [
                                new CellBlock { Blocks = [new TextBlock { Text = "Order ID" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "Customer" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "Amount" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "Status" }] }
                            ]
                        },
                        new RowBlock
                        {
                            Blocks =
                            [
                                new CellBlock { Blocks = [new TextBlock { Text = "{OrderId}" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "{CustomerName}" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "{Amount:C}" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "{Status}" }] }
                            ]
                        }
                    ]
                }
            ]
        }
    ]
};

// Configure and run the report
var layoutContext = serviceProvider.GetRequiredService<IMeasureContext>();
var layoutOptions = LayoutOptions.Presets.StandardLetter();

var document = await engine.RunAsync(
    definition,
    parameters: [],
    layoutContext,
    layoutOptions
);

// Export the result
using (var output = new FileStream("orders.html", FileMode.Create))
{
    await document.ExportHtmlAsync(output);
}
```

**Key Takeaways:**
- `TableBlock` contains table structure; use `RowBlock` for each row
- `CellBlock` contains cell content (can hold any block type, including `TextBlock`)
- Bind data to fields using `{FieldName}` syntax in text expressions
- `DataSourceName` on `DetailBlock` specifies which data source to iterate
- Table header rows use `IsHeader = true` on the row definition

---

## 3. Multi-Section Report

**Goal:** Create a report with header, repeating detail sections, and footer with totals.

**Concepts:** `HeaderBlock`, `DetailBlock`, `FooterBlock`, expressions with aggregation.

```csharp
using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

// Prepare invoice line items
var lineItems = new[]
{
    new { LineId = 1, Description = "Service", Quantity = 1, UnitPrice = 100m },
    new { LineId = 2, Description = "Support", Quantity = 3, UnitPrice = 50m },
    new { LineId = 3, Description = "Consulting", Quantity = 2, UnitPrice = 150m }
};

// Create the report
var definition = new ReportDefinition
{
    DataSources =
    [
        new DataSourceDefinition { Name = "LineItems", SourceName = "LineItems" }
    ],
    Blocks =
    [
        // Header: Invoice metadata
        new HeaderBlock
        {
            Blocks =
            [
                new TextBlock
                {
                    Text = "INVOICE",
                    Style = new StyleDefinition
                    {
                        FontSize = 24,
                        FontWeight = 700,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    }
                },
                new TextBlock
                {
                    Text = "Invoice #12345 | Date: {InvoiceDate} | Due: {DueDate}",
                    Style = new StyleDefinition { FontSize = 12 }
                },
                new TextBlock
                {
                    Text = "Bill To: {CustomerName}, {CustomerAddress}",
                    Style = new StyleDefinition { FontSize = 11, Margin = new Thickness(0, 10, 0, 0) }
                }
            ]
        },

        // Detail: Line items table
        new DetailBlock
        {
            DataSourceName = "LineItems",
            Blocks =
            [
                new TableBlock
                {
                    Blocks =
                    [
                        new RowBlock
                        {
                            IsHeader = true,
                            Blocks =
                            [
                                new CellBlock { Blocks = [new TextBlock { Text = "Description" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "Qty" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "Unit Price" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "Amount" }] }
                            ]
                        },
                        new RowBlock
                        {
                            Blocks =
                            [
                                new CellBlock { Blocks = [new TextBlock { Text = "{Description}" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "{Quantity}" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "{UnitPrice:C}" }] },
                                new CellBlock { Blocks = [new TextBlock { Text = "{Quantity * UnitPrice:C}" }] }
                            ]
                        }
                    ]
                }
            ]
        },

        // Footer: Totals
        new FooterBlock
        {
            Blocks =
            [
                new TextBlock
                {
                    Text = "SUBTOTAL: {Subtotal:C}",
                    Style = new StyleDefinition { FontSize = 12, FontWeight = 700, TextAlignment = TextAlignment.Right }
                },
                new TextBlock
                {
                    Text = "TAX: {Tax:C}",
                    Style = new StyleDefinition { FontSize = 12, TextAlignment = TextAlignment.Right }
                },
                new TextBlock
                {
                    Text = "TOTAL DUE: {TotalAmount:C}",
                    Style = new StyleDefinition
                    {
                        FontSize = 14,
                        FontWeight = 700,
                        TextAlignment = TextAlignment.Right,
                        Margin = new Thickness(0, 10, 0, 0),
                        BackgroundColor = new Color { R = 200, G = 200, B = 200 }
                    }
                }
            ]
        }
    ]
};

// Run with parameters
var parameters = new Dictionary<string, object?>
{
    { "InvoiceDate", DateTime.Now.ToString("MM/dd/yyyy") },
    { "DueDate", DateTime.Now.AddDays(30).ToString("MM/dd/yyyy") },
    { "CustomerName", "Acme Corporation" },
    { "CustomerAddress", "123 Business St, City, State 12345" },
    { "Subtotal", 550m },
    { "Tax", 44m },
    { "TotalAmount", 594m }
};

var document = await engine.RunAsync(definition, parameters, layoutContext, LayoutOptions.Presets.StandardLetter());

using (var output = new FileStream("invoice.html", FileMode.Create))
{
    await document.ExportHtmlAsync(output);
}
```

**Key Takeaways:**
- `HeaderBlock`: Report title and metadata (appears once)
- `DetailBlock`: Repeating section for each data row
- `FooterBlock`: Summary section (appears once at the end)
- Parameters are passed as `Dictionary<string, object?>` to `engine.RunAsync()`
- Expressions can use arithmetic: `{Quantity * UnitPrice}`

---

## 4. Using Parameters in Expressions

**Goal:** Display dynamic values using report parameters (filters, settings, constants).

**Concepts:** Parameters dictionary, expression evaluation, conditional text.

```csharp
using KineticReports.Core.Definition;
using KineticReports.Core.Layout;

// Create a report that uses parameters for filtering and configuration
var definition = new ReportDefinition
{
    Parameters =
    [
        new ParameterDefinition { Name = "ReportTitle", DefaultValue = "Sales Report" },
        new ParameterDefinition { Name = "Region", DefaultValue = "All Regions" },
        new ParameterDefinition { Name = "StartDate", DefaultValue = DateTime.Now.AddMonths(-1) },
        new ParameterDefinition { Name = "EndDate", DefaultValue = DateTime.Now },
        new ParameterDefinition { Name = "ShowDetails", DefaultValue = true }
    ],
    Blocks =
    [
        new HeaderBlock
        {
            Blocks =
            [
                // Title from parameter
                new TextBlock
                {
                    Text = "{ReportTitle}",
                    Style = new StyleDefinition { FontSize = 24, FontWeight = 700 }
                },
                // Region filter
                new TextBlock
                {
                    Text = "Region: {Region}",
                    Style = new StyleDefinition { FontSize = 12, TextColor = new Color { R = 100, G = 100, B = 100 } }
                },
                // Date range from parameters
                new TextBlock
                {
                    Text = "Period: {StartDate:MM/dd/yyyy} to {EndDate:MM/dd/yyyy}",
                    Style = new StyleDefinition { FontSize = 12 }
                }
            ]
        },
        new DetailBlock
        {
            Blocks =
            [
                // Use parameter in content display
                new TextBlock
                {
                    Text = "Details for {Region} region are shown below.",
                    Style = new StyleDefinition { FontSize = 11, Margin = new Thickness(0, 20, 0, 10) }
                }
                // Additional content would go here...
            ]
        }
    ]
};

// Pass parameters when running the report
var parameters = new Dictionary<string, object?>
{
    { "ReportTitle", "Q4 2024 Sales Analysis" },
    { "Region", "North America" },
    { "StartDate", new DateTime(2024, 10, 1) },
    { "EndDate", new DateTime(2024, 12, 31) },
    { "ShowDetails", true }
};

var document = await engine.RunAsync(
    definition,
    parameters,
    layoutContext,
    LayoutOptions.Presets.StandardLetter()
);

using (var output = new FileStream("sales-report.html", FileMode.Create))
{
    await document.ExportHtmlAsync(output);
}
```

**Key Takeaways:**
- Parameters are defined in `ReportDefinition.Parameters[]`
- Pass parameter values in `engine.RunAsync()` as a dictionary
- Reference parameters in expressions using `{ParameterName}`
- Use format specifiers like `{StartDate:MM/dd/yyyy}` for custom formatting
- Parameters are immutable and available throughout report execution

---

## 5. Custom Styling

**Goal:** Apply consistent styling to blocks (fonts, colors, spacing, borders).

**Concepts:** `StyleDefinition`, `Color`, `Thickness`, style inheritance.

```csharp
using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Core.Geometry;

// Define reusable styles
var titleStyle = new StyleDefinition
{
    FontSize = 24,
    FontWeight = 700,
    TextColor = new Color { R = 25, G = 25, B = 112 }, // Midnight blue
    TextAlignment = TextAlignment.Center,
    Margin = new Thickness(0, 0, 0, 20)
};

var sectionHeaderStyle = new StyleDefinition
{
    FontSize = 14,
    FontWeight = 600,
    TextColor = new Color { R = 50, G = 50, B = 50 },
    BackgroundColor = new Color { R = 220, G = 220, B = 220 },
    Padding = new Thickness(10),
    Margin = new Thickness(0, 15, 0, 10),
    Border = new Border
    {
        BottomWidth = 2,
        BottomColor = new Color { R = 25, G = 25, B = 112 }
    }
};

var bodyTextStyle = new StyleDefinition
{
    FontSize = 11,
    LineHeight = 1.5f,
    TextColor = new Color { R = 64, G = 64, B = 64 },
    Margin = new Thickness(0, 5, 0, 5)
};

var highlightBoxStyle = new StyleDefinition
{
    Padding = new Thickness(15),
    BackgroundColor = new Color { R = 255, G = 250, B = 205 }, // Light yellow
    Border = new Border
    {
        TopWidth = 2,
        BottomWidth = 2,
        TopColor = new Color { R = 255, G = 165, B = 0 },
        BottomColor = new Color { R = 255, G = 165, B = 0 }
    }
};

// Create report using the styles
var definition = new ReportDefinition
{
    Blocks =
    [
        new HeaderBlock
        {
            Blocks =
            [
                new TextBlock
                {
                    Text = "Styled Report Example",
                    Style = titleStyle
                }
            ]
        },
        new DetailBlock
        {
            Blocks =
            [
                new TextBlock
                {
                    Text = "Section 1: Key Information",
                    Style = sectionHeaderStyle
                },
                new TextBlock
                {
                    Text = "This is body text with standard styling applied.",
                    Style = bodyTextStyle
                },
                new TextBlock
                {
                    Text = "Important Notice: This box is highlighted to draw attention.",
                    Style = bodyTextStyle with { BackgroundColor = highlightBoxStyle.BackgroundColor }
                },

                new TextBlock
                {
                    Text = "Section 2: Additional Details",
                    Style = sectionHeaderStyle
                },
                new TextBlock
                {
                    Text = "More content follows standard styling conventions.",
                    Style = bodyTextStyle
                }
            ]
        }
    ]
};

var document = await engine.RunAsync(
    definition,
    parameters: [],
    layoutContext,
    LayoutOptions.Presets.StandardLetter()
);

using (var output = new FileStream("styled-report.html", FileMode.Create))
{
    await document.ExportHtmlAsync(output);
}
```

**Key Styling Properties:**
- `FontSize`: Point size (e.g., 12)
- `FontWeight`: 400 (normal) to 700 (bold)
- `TextColor`: RGB color value
- `BackgroundColor`: Cell/block background
- `Padding`: Internal spacing (top, right, bottom, left)
- `Margin`: External spacing around block
- `Border`: Decorative edges (width, color)
- `TextAlignment`: Left, Center, Right, Justified
- `LineHeight`: Line spacing multiplier (1.0 = normal, 1.5 = 1.5x)

**Key Takeaways:**
- Create reusable style definitions as variables
- Use `with` keyword to create style variations: `style with { FontSize = 14 }`
- All measurements are in Device Independent Pixels (DIPs)
- Styles cascade from report defaults → named styles → local overrides

---

## Summary

| Pattern | Use Case | Key Classes |
|---------|----------|-------------|
| Simple Report | Static content, titles, text sections | `TextBlock`, `HeaderBlock`, `DetailBlock` |
| Data-Driven Table | Tabular data (orders, inventory) | `TableBlock`, `RowBlock`, `CellBlock` |
| Multi-Section | Reports with header/detail/footer structure | `HeaderBlock`, `DetailBlock`, `FooterBlock` |
| Parameters | Dynamic content, filters, configuration | `ParameterDefinition`, `{ParameterName}` |
| Custom Styling | Consistent branding, visual hierarchy | `StyleDefinition`, `Color`, `Thickness` |

## Next Steps

- Refer to [BlockTypeHierarchyGuide](../src/Core/KineticReports.Core/Layout/BlockTypeHierarchyGuide.cs) for choosing the right block type
- Use [LayoutOptions.Presets](../src/Core/KineticReports.Core/LayoutEngine/LayoutOptions.cs) for standard page formats
- Review [ExpressionContext documentation](../src/Core/KineticReports.Core/Engine/Expressions/ExpressionContext.cs) for available fields in expressions
- Explore the sample projects in `src/Samples/` for complete working examples

---

*Last Updated: 2024 | KineticReports .NET 10+*
