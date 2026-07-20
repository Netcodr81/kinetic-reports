# Report Definitions

**Objective**: Learn how to create report definitions in JSON and understand the schema.

---

## Quick Start: Minimal Report

The simplest valid report:

```json
{
  "schemaVersion": "1.0",
  "id": "hello-world",
  "name": "Hello World",
  "parameters": [],
  "dataSources": [],
  "styles": [],
  "pages": [
    {
      "id": "page1",
      "type": "Page",
      "width": 612,
      "height": 792,
      "body": {
        "id": "body1",
        "type": "Container",
        "children": [
          {
            "id": "text1",
            "type": "Text",
            "content": "Hello, World!"
          }
        ]
      }
    }
  ]
}
```

---

## Top-Level Structure

```json
{
  "schemaVersion": "1.0",
  "id": "unique-report-id",
  "name": "Display Name",
  "author": "Your Name",
  "description": "What this report does",
  "metadata": {},
  "parameters": [],
  "dataSources": [],
  "styles": [],
  "pages": []
}
```

### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `schemaVersion` | string | ✅ | Schema version (e.g., "1.0") |
| `id` | string | ✅ | Unique identifier |
| `name` | string | ✅ | Display name |
| `author` | string | ❌ | Report author |
| `description` | string | ❌ | Report description |
| `metadata` | object | ❌ | Custom metadata |
| `parameters` | array | ✅ | Report parameters |
| `dataSources` | array | ✅ | Data source definitions |
| `styles` | array | ✅ | Style definitions |
| `pages` | array | ✅ | Page definitions |

---

## Parameters

Parameters allow users to customize reports at runtime.

### Definition

```json
{
  "parameters": [
    {
      "id": "startDate",
      "name": "Start Date",
      "type": "DateTime",
      "required": true,
      "defaultValue": "2024-01-01"
    },
    {
      "id": "country",
      "name": "Country",
      "type": "String",
      "required": true,
      "allowedValues": ["USA", "UK", "Canada"]
    },
    {
      "id": "includeDetails",
      "name": "Include Details",
      "type": "Boolean",
      "defaultValue": false
    }
  ]
}
```

### Parameter Types
- String
- Integer
- Decimal
- Boolean
- DateTime
- Date
- Time

### Usage in Report

```json
{
  "type": "Text",
  "content": "Report for: {Parameters.country}"
}
```

---

## Data Sources

Data sources define where data comes from and what queries to run.

### Definition

```json
{
  "dataSources": [
    {
      "id": "customers",
      "providerType": "SqlServer",
      "connectionString": "Server=localhost;Database=Reports",
      "query": "SELECT * FROM Customers WHERE Country = @country",
      "parameters": [
        {
          "name": "country",
          "value": "{Parameters.country}"
        }
      ],
      "timeout": 30000
    },
    {
      "id": "sales",
      "providerType": "SqlServer",
      "query": "SELECT * FROM Sales WHERE CustomerId = @customerId",
      "parameters": [
        {
          "name": "customerId",
          "value": "{Customers.CustomerId}"
        }
      ]
    }
  ]
}
```

### Data Source Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `id` | string | ✅ | Unique identifier |
| `providerType` | string | ✅ | Data provider type (SqlServer, Rest, etc.) |
| `connectionString` | string | ❌ | Connection info (SQL connection string, API URL, etc.) |
| `query` | string | ✅ | Query text or endpoint |
| `parameters` | array | ❌ | Query parameters |
| `timeout` | integer | ❌ | Timeout in milliseconds (default: 30000) |

### Using Data in Report

```json
{
  "type": "Band",
  "datasetName": "customers",
  "children": [
    {
      "type": "Text",
      "content": "{customers.Name}"
    },
    {
      "type": "Text",
      "content": "{customers.Email}"
    }
  ]
}
```

---

## Styles

Styles define the visual appearance of elements.

### Definition

```json
{
  "styles": [
    {
      "id": "Heading1",
      "font": {
        "family": "Arial",
        "size": 24,
        "bold": true,
        "italic": false,
        "underline": false
      },
      "color": "#003366",
      "backgroundColor": "#F0F0F0",
      "alignment": {
        "horizontal": "Left",
        "vertical": "Middle"
      },
      "padding": {
        "top": 12,
        "bottom": 12,
        "left": 8,
        "right": 8
      },
      "border": {
        "top": {
          "style": "Solid",
          "width": 2,
          "color": "#000000"
        }
      }
    },
    {
      "id": "BodyText",
      "font": {
        "family": "Calibri",
        "size": 11
      },
      "color": "#333333"
    }
  ]
}
```

### Style Properties

#### Font
```json
{
  "font": {
    "family": "Arial",
    "size": 12,
    "bold": false,
    "italic": false,
    "underline": false,
    "strikethrough": false
  }
}
```

#### Color & Background
```json
{
  "color": "#RRGGBB",
  "backgroundColor": "#RRGGBB"
}
```

#### Alignment
```json
{
  "alignment": {
    "horizontal": "Left|Center|Right|Justify",
    "vertical": "Top|Middle|Bottom"
  }
}
```

#### Padding
```json
{
  "padding": {
    "top": 12,
    "bottom": 12,
    "left": 8,
    "right": 8
  }
}
```

#### Border
```json
{
  "border": {
    "top": { "style": "Solid", "width": 1, "color": "#000000" },
    "bottom": { "style": "Solid", "width": 1, "color": "#000000" },
    "left": { "style": "Dashed", "width": 1, "color": "#CCCCCC" },
    "right": { "style": "Dashed", "width": 1, "color": "#CCCCCC" }
  }
}
```

---

## Pages

Pages contain the actual report content.

### Page Structure

```json
{
  "pages": [
    {
      "id": "page1",
      "type": "Page",
      "width": 612,
      "height": 792,
      "header": {...},    // Optional
      "body": {...},      // Required
      "footer": {...}     // Optional
    }
  ]
}
```

### Common Page Sizes

```json
{
  "Letter": { "width": 612, "height": 792 },
  "A4": { "width": 595, "height": 842 },
  "Legal": { "width": 612, "height": 1008 },
  "Tabloid": { "width": 792, "height": 1224 }
}
```

All measurements are in DIPs (Device Independent Pixels).

### Page Header & Footer

```json
{
  "header": {
    "id": "header1",
    "type": "Container",
    "height": 72,
    "children": [
      {
        "id": "title",
        "type": "Text",
        "content": "{ReportName}",
        "style": "Heading1"
      },
      {
        "id": "date",
        "type": "Text",
        "content": "=TODAY()",
        "alignment": "Right"
      }
    ]
  }
}
```

Headers and footers repeat on each page.

---

## Layout Elements

### Text Element

```json
{
  "id": "text1",
  "type": "Text",
  "content": "Static text or {Binding}",
  "style": "BodyText",
  "x": 100,
  "y": 100,
  "width": 400,
  "height": 30
}
```

### Container Element

```json
{
  "id": "container1",
  "type": "Container",
  "x": 100,
  "y": 100,
  "width": 400,
  "height": 500,
  "children": [
    {...},
    {...}
  ]
}
```

### Image Element

```json
{
  "id": "image1",
  "type": "Image",
  "source": "https://example.com/logo.png",
  "x": 100,
  "y": 100,
  "width": 200,
  "height": 100,
  "altText": "Company Logo"
}
```

### Table Element

```json
{
  "id": "table1",
  "type": "Table",
  "x": 100,
  "y": 100,
  "width": 400,
  "height": 300,
  "columns": [
    { "name": "Id", "width": 50 },
    { "name": "Name", "width": 150 },
    { "name": "Email", "width": 200 }
  ],
  "datasetName": "customers",
  "rowHeight": 30
}
```

### Band Element (Repeating Section)

```json
{
  "id": "band1",
  "type": "Band",
  "datasetName": "customers",
  "children": [
    {
      "id": "customerName",
      "type": "Text",
      "content": "{customers.Name}"
    },
    {
      "id": "customerEmail",
      "type": "Text",
      "content": "{customers.Email}"
    }
  ]
}
```

Band repeats for each row in the dataset.

---

## Common Patterns

### Pattern 1: Simple List Report

```json
{
  "schemaVersion": "1.0",
  "id": "customer-list",
  "name": "Customer List",
  "parameters": [],
  "dataSources": [
    {
      "id": "customers",
      "providerType": "SqlServer",
      "query": "SELECT * FROM Customers ORDER BY Name"
    }
  ],
  "styles": [...],
  "pages": [
    {
      "id": "page1",
      "type": "Page",
      "width": 612,
      "height": 792,
      "body": {
        "type": "Container",
        "children": [
          {
            "type": "Text",
            "content": "Customer List",
            "style": "Heading1"
          },
          {
            "type": "Band",
            "datasetName": "customers",
            "children": [
              {
                "type": "Text",
                "content": "{customers.Name} - {customers.Email}"
              }
            ]
          }
        ]
      }
    }
  ]
}
```

### Pattern 2: Parameterized Report

```json
{
  "parameters": [
    {
      "id": "startDate",
      "type": "DateTime",
      "required": true
    }
  ],
  "dataSources": [
    {
      "query": "SELECT * FROM Orders WHERE OrderDate >= @startDate",
      "parameters": [
        {
          "name": "startDate",
          "value": "{Parameters.startDate}"
        }
      ]
    }
  ]
}
```

### Pattern 3: Multi-Section Report

```json
{
  "pages": [
    {
      "body": {
        "children": [
          {
            "type": "Container",
            "children": [
              { "type": "Text", "content": "Section 1", "style": "Heading1" },
              { "type": "Band", "datasetName": "data1", "children": [...] }
            ]
          },
          {
            "type": "Container",
            "breakBefore": true,
            "children": [
              { "type": "Text", "content": "Section 2", "style": "Heading1" },
              { "type": "Band", "datasetName": "data2", "children": [...] }
            ]
          }
        ]
      }
    }
  ]
}
```

---

## Best Practices

### 1. Use Meaningful IDs
```json
// ✅ Good
{
  "id": "customer_name_text",
  "type": "Text"
}

// ❌ Avoid
{
  "id": "text1",
  "type": "Text"
}
```

### 2. Organize Styles
```json
// ✅ Good
{
  "styles": [
    { "id": "Heading1", ... },
    { "id": "Heading2", ... },
    { "id": "BodyText", ... },
    { "id": "TableHeader", ... },
    { "id": "TableRow", ... }
  ]
}
```

### 3. Use Named Styles
```json
// ✅ Good
{
  "type": "Text",
  "content": "Customer Name",
  "style": "Heading1"
}

// ❌ Avoid
{
  "type": "Text",
  "content": "Customer Name",
  "font": { "family": "Arial", "size": 24, "bold": true }
}
```

### 4. Comment Complex Logic
```json
{
  "_comment": "This band filters and groups customers by region",
  "type": "Band",
  "datasetName": "customers"
}
```

### 5. Keep Consistent Spacing
All measurements in DIPs (96 per inch):
```json
{
  "x": 96,        // 1 inch
  "y": 144,       // 1.5 inches
  "width": 480,   // 5 inches
  "height": 240   // 2.5 inches
}
```

---

## Validation

### Schema Validation
Reports should validate against JSON Schema:
```bash
# Using a JSON Schema validator
ajv validate -s report-schema.json report.json
```

### Common Validation Errors

| Error | Cause | Solution |
|-------|-------|----------|
| Missing `id` | Element without identifier | Add unique `id` |
| Invalid `schemaVersion` | Wrong version format | Use "1.0" |
| Unknown `type` | Invalid element type | Use one of 12 types |
| Circular reference | Dataset references itself | Break circular dependency |
| Missing dataset | Band references non-existent dataset | Define dataSources |

---

## Example: Complete Invoice Report

```json
{
  "schemaVersion": "1.0",
  "id": "invoice",
  "name": "Invoice",
  "author": "Accounting Department",
  "metadata": {
    "category": "Financial",
    "frequency": "Ad-hoc"
  },
  "parameters": [
    {
      "id": "invoiceNumber",
      "name": "Invoice Number",
      "type": "String",
      "required": true
    }
  ],
  "dataSources": [
    {
      "id": "invoice",
      "providerType": "SqlServer",
      "query": "SELECT * FROM Invoices WHERE InvoiceNumber = @invoiceNumber",
      "parameters": [
        {
          "name": "invoiceNumber",
          "value": "{Parameters.invoiceNumber}"
        }
      ]
    },
    {
      "id": "lineItems",
      "providerType": "SqlServer",
      "query": "SELECT * FROM InvoiceLineItems WHERE InvoiceId = @invoiceId",
      "parameters": [
        {
          "name": "invoiceId",
          "value": "{invoice.InvoiceId}"
        }
      ]
    }
  ],
  "styles": [
    {
      "id": "CompanyName",
      "font": { "family": "Arial", "size": 24, "bold": true },
      "color": "#003366"
    },
    {
      "id": "SectionHeader",
      "font": { "family": "Arial", "size": 14, "bold": true },
      "padding": { "top": 12, "bottom": 8 }
    },
    {
      "id": "TableHeader",
      "font": { "family": "Arial", "size": 11, "bold": true },
      "backgroundColor": "#E0E0E0",
      "padding": { "top": 6, "bottom": 6, "left": 4, "right": 4 }
    }
  ],
  "pages": [
    {
      "id": "invoicePage",
      "type": "Page",
      "width": 612,
      "height": 792,
      "body": {
        "type": "Container",
        "children": [
          {
            "type": "Text",
            "content": "INVOICE",
            "style": "CompanyName",
            "x": 50,
            "y": 50
          },
          {
            "type": "Text",
            "content": "Invoice #: {invoice.InvoiceNumber}",
            "x": 50,
            "y": 100
          },
          {
            "type": "Text",
            "content": "Date: =TODAY()",
            "x": 50,
            "y": 130
          },
          {
            "type": "Table",
            "x": 50,
            "y": 200,
            "width": 512,
            "datasetName": "lineItems",
            "columns": [
              { "name": "Description", "width": 300 },
              { "name": "Quantity", "width": 80 },
              { "name": "UnitPrice", "width": 80 },
              { "name": "Total", "width": 52 }
            ],
            "style": "TableHeader"
          },
          {
            "type": "Text",
            "content": "Total: {invoice.TotalAmount}",
            "x": 400,
            "y": 500,
            "style": "SectionHeader"
          }
        ]
      }
    }
  ]
}
```

---

**Time to read**: ~20 minutes  
**Difficulty**: Intermediate  
**Related**: [Core Concepts](03_Core_Concepts.md), [Report Definitions Spec](../02_Report_Definition_JSON_Specification.md)
