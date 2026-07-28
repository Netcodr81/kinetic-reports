# 18 - Create a Report with the Fluent Builder (Step-by-Step)

This guide shows only one task: creating a report with the fluent builder API.

## Prerequisites

1. Target framework: `net10.0`
2. Reference `KineticReports.Core`
3. Add these namespaces:

```csharp
using KineticReports.Core.Authoring.Compilation;
using KineticReports.Core.Authoring.Components;
using KineticReports.Core.Authoring.Documents;
using KineticReports.Core.Authoring.Serialization;
using KineticReports.Core.Definition;
```

## Step 1: Start a new designer document builder

```csharp
var builder = ReportDesignerDocumentBuilder.Create("sales-summary", "Sales Summary")
    .WithSchemaVersion("1.0")
    .WithDescription("Simple report created with fluent API")
    .WithAuthor("Kinetic Team");
```

## Step 2: Add components to the report

```csharp
builder
    .AddComponent("title", ReportComponentType.Text, c =>
        c.Named("Title")
         .WithBinding("Text", "Sales Summary Report")
         .WithPlacement(24, 24, 540, 32))
    .AddComponent("subtitle", ReportComponentType.Text, c =>
        c.Named("Subtitle")
         .WithBinding("Text", "Generated from fluent builder")
         .WithPlacement(24, 64, 540, 24));
```

## Step 3: Build the immutable designer document

```csharp
var designerDocument = builder.Build();
```

## Step 4: Compile to a runtime report definition

```csharp
IDesignerDocumentCompiler compiler = new DefaultDesignerDocumentCompiler();
ReportDefinition definition = compiler.Compile(designerDocument);
```

## Step 5: Save both JSON artifacts (optional)

Save the authoring JSON:

```csharp
IReportDesignerDocumentSerializer designerSerializer =
    new SystemTextJsonReportDesignerDocumentSerializer();

var authoringJson = designerSerializer.Serialize(designerDocument);
File.WriteAllText("sales-summary.designer.json", authoringJson);
```

Save the compiled runtime definition JSON:

```csharp
var runtimeSerializer = new SystemTextJsonReportDefinitionSerializer();
var runtimeJson = runtimeSerializer.Serialize(definition);
File.WriteAllText("sales-summary.runtime.json", runtimeJson);
```

## One-file complete example

```csharp
using KineticReports.Core.Authoring.Compilation;
using KineticReports.Core.Authoring.Components;
using KineticReports.Core.Authoring.Documents;
using KineticReports.Core.Authoring.Serialization;
using KineticReports.Core.Definition;

var builder = ReportDesignerDocumentBuilder.Create("sales-summary", "Sales Summary")
    .WithSchemaVersion("1.0")
    .WithDescription("Simple report created with fluent API")
    .WithAuthor("Kinetic Team")
    .AddComponent("title", ReportComponentType.Text, c =>
        c.Named("Title")
         .WithBinding("Text", "Sales Summary Report")
         .WithPlacement(24, 24, 540, 32))
    .AddComponent("subtitle", ReportComponentType.Text, c =>
        c.Named("Subtitle")
         .WithBinding("Text", "Generated from fluent builder")
         .WithPlacement(24, 64, 540, 24));

var designerDocument = builder.Build();

IDesignerDocumentCompiler compiler = new DefaultDesignerDocumentCompiler();
ReportDefinition definition = compiler.Compile(designerDocument);

IReportDesignerDocumentSerializer designerSerializer =
    new SystemTextJsonReportDesignerDocumentSerializer();

var authoringJson = designerSerializer.Serialize(designerDocument);
File.WriteAllText("sales-summary.designer.json", authoringJson);

var runtimeSerializer = new SystemTextJsonReportDefinitionSerializer();
var runtimeJson = runtimeSerializer.Serialize(definition);
File.WriteAllText("sales-summary.runtime.json", runtimeJson);
```

## What you now have

1. A report created entirely in C# fluent code
2. A compiled runtime `ReportDefinition`
3. Optional JSON outputs for persistence or transport
