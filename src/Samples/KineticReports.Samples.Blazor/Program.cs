using KineticReports.Core.Data;
using KineticReports.Samples.Blazor.Components;
using KineticReports.Samples.Blazor.DependencyInjection;
using KineticReports.Samples.Blazor.Services;

// ============================================================================
// KineticReports Blazor Sample Application (Interactive Server)
// ============================================================================
// This sample demonstrates:
// 1. Loading report definitions from JSON and code
// 2. Running the same reports with InMemory or SQL Server providers
// 3. Rendering with the Blazor viewer using the standard engine pipeline
// 4. Realistic multi-page output with headers, footers, and multiple tables
//
// To run: dotnet run
// To build: dotnet build
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var sqlServerConnectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=KineticReportsSamples;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services
    .AddKeneticReports()
    .AddBlazorViewer()
    .AddSqlServerDataProvider(sqlServerConnectionString);

builder.Services.AddSingleton<ISampleReportCatalogService, SampleReportCatalogService>();

var app = builder.Build();

SeedInMemoryData(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static void SeedInMemoryData(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var provider = scope.ServiceProvider.GetRequiredService<PocoDataProvider>();

    provider.SetDataSource(
        "regional-performance",
        [
            new { Region = "North America", Revenue = 4285000m, Target = 4000000m, Variance = 285000m, Accounts = 42 },
            new { Region = "EMEA", Revenue = 3190000m, Target = 3400000m, Variance = -210000m, Accounts = 28 },
            new { Region = "APAC", Revenue = 2745000m, Target = 2500000m, Variance = 245000m, Accounts = 25 },
            new { Region = "Latin America", Revenue = 1180000m, Target = 1100000m, Variance = 80000m, Accounts = 14 }
        ]);

    provider.SetDataSource(
        "top-accounts",
        [
            new { Customer = "Acme Manufacturing", Segment = "Enterprise", AnnualRevenue = 845000m, RenewalDate = "2026-09-15", Owner = "Morgan Lee" },
            new { Customer = "Bright Retail Group", Segment = "Mid-Market", AnnualRevenue = 610000m, RenewalDate = "2026-08-01", Owner = "Avery Shah" },
            new { Customer = "Contour Labs", Segment = "Enterprise", AnnualRevenue = 575000m, RenewalDate = "2026-11-12", Owner = "Noah Perez" },
            new { Customer = "Delta Foods", Segment = "Commercial", AnnualRevenue = 490000m, RenewalDate = "2026-10-03", Owner = "Jules Carter" },
            new { Customer = "Evergreen Health", Segment = "Enterprise", AnnualRevenue = 455000m, RenewalDate = "2026-12-09", Owner = "Morgan Lee" }
        ]);

    provider.SetDataSource(
        "monthly-trend",
        [
            new { Month = "January", Billings = 970000m, GrossMargin = "42%", NewLogos = 4 },
            new { Month = "February", Billings = 1015000m, GrossMargin = "43%", NewLogos = 5 },
            new { Month = "March", Billings = 1082000m, GrossMargin = "41%", NewLogos = 6 },
            new { Month = "April", Billings = 1120000m, GrossMargin = "44%", NewLogos = 5 },
            new { Month = "May", Billings = 1184000m, GrossMargin = "45%", NewLogos = 7 },
            new { Month = "June", Billings = 1233000m, GrossMargin = "44%", NewLogos = 6 }
        ]);

    provider.SetDataSource(
        "implementation-milestones",
        [
            new { Customer = "Acme Manufacturing", Workstream = "ERP Integration", Stage = "UAT", GoLiveDate = "2026-08-18", ExecutiveSponsor = "Dana Brooks" },
            new { Customer = "Bright Retail Group", Workstream = "Store Analytics", Stage = "Build", GoLiveDate = "2026-09-02", ExecutiveSponsor = "Samir Patel" },
            new { Customer = "Contour Labs", Workstream = "Compliance Pack", Stage = "Design", GoLiveDate = "2026-10-10", ExecutiveSponsor = "Mia Foster" },
            new { Customer = "Evergreen Health", Workstream = "Claims Automation", Stage = "UAT", GoLiveDate = "2026-08-27", ExecutiveSponsor = "Jordan Price" },
            new { Customer = "Frontier Logistics", Workstream = "Scheduling", Stage = "Pilot", GoLiveDate = "2026-09-14", ExecutiveSponsor = "Leo Kim" },
            new { Customer = "Granite Energy", Workstream = "Field Service", Stage = "Discovery", GoLiveDate = "2026-11-05", ExecutiveSponsor = "Priya Nair" }
        ]);

    provider.SetDataSource(
        "support-escalations",
        [
            new { Ticket = "INC-2401", Customer = "Acme Manufacturing", Severity = "High", Owner = "Platform Ops", DaysOpen = 2, Status = "Mitigation in progress" },
            new { Ticket = "INC-2408", Customer = "Contour Labs", Severity = "Medium", Owner = "Data Services", DaysOpen = 4, Status = "Awaiting customer validation" },
            new { Ticket = "INC-2410", Customer = "Delta Foods", Severity = "High", Owner = "Integration Team", DaysOpen = 1, Status = "Hotfix scheduled" },
            new { Ticket = "INC-2417", Customer = "Evergreen Health", Severity = "Medium", Owner = "Reporting Team", DaysOpen = 6, Status = "Root cause identified" },
            new { Ticket = "INC-2420", Customer = "Frontier Logistics", Severity = "Low", Owner = "Support", DaysOpen = 3, Status = "Monitoring" }
        ]);

    provider.SetDataSource(
        "renewal-pipeline",
        [
            new { Customer = "Harbor Financial", RenewalQuarter = "Q3 FY26", ARR = 220000m, ExpansionPotential = 90000m, Stage = "Proposal" },
            new { Customer = "Ivory Telecom", RenewalQuarter = "Q3 FY26", ARR = 185000m, ExpansionPotential = 40000m, Stage = "Negotiation" },
            new { Customer = "Juniper Systems", RenewalQuarter = "Q4 FY26", ARR = 310000m, ExpansionPotential = 120000m, Stage = "Executive review" },
            new { Customer = "Keystone Bank", RenewalQuarter = "Q4 FY26", ARR = 275000m, ExpansionPotential = 65000m, Stage = "Proposal" },
            new { Customer = "Luma Devices", RenewalQuarter = "Q1 FY27", ARR = 198000m, ExpansionPotential = 55000m, Stage = "Discovery" },
            new { Customer = "North Ridge Health", RenewalQuarter = "Q1 FY27", ARR = 342000m, ExpansionPotential = 135000m, Stage = "Negotiation" }
        ]);

    provider.SetDataSource(
        "sales-orders-detail",
        [
            new { OrderNumber = "SO-1048", Customer = "Acme Manufacturing", Region = "North America", OrderDate = "2026-06-03", Amount = 184500m, Status = "Booked" },
            new { OrderNumber = "SO-1051", Customer = "Bright Retail Group", Region = "EMEA", OrderDate = "2026-06-05", Amount = 126000m, Status = "Implementation" },
            new { OrderNumber = "SO-1059", Customer = "Contour Labs", Region = "North America", OrderDate = "2026-06-09", Amount = 212500m, Status = "Booked" },
            new { OrderNumber = "SO-1064", Customer = "Delta Foods", Region = "Latin America", OrderDate = "2026-06-11", Amount = 78500m, Status = "Shipped" },
            new { OrderNumber = "SO-1072", Customer = "Evergreen Health", Region = "North America", OrderDate = "2026-06-14", Amount = 265000m, Status = "Implementation" },
            new { OrderNumber = "SO-1079", Customer = "Frontier Logistics", Region = "APAC", OrderDate = "2026-06-17", Amount = 94000m, Status = "Booked" },
            new { OrderNumber = "SO-1086", Customer = "Granite Energy", Region = "EMEA", OrderDate = "2026-06-20", Amount = 156750m, Status = "Shipped" },
            new { OrderNumber = "SO-1092", Customer = "Harbor Financial", Region = "North America", OrderDate = "2026-06-23", Amount = 301000m, Status = "Booked" },
            new { OrderNumber = "SO-1099", Customer = "Ivory Telecom", Region = "EMEA", OrderDate = "2026-06-25", Amount = 118500m, Status = "Implementation" },
            new { OrderNumber = "SO-1104", Customer = "Juniper Systems", Region = "APAC", OrderDate = "2026-06-27", Amount = 221250m, Status = "Booked" }
        ]);
}
