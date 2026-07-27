using KineticReports.Core.Data;
using KineticReports.Samples.Blazor.Components;
using KineticReports.Samples.Blazor.DependencyInjection;
using KineticReports.Samples.Blazor.Services;
using Microsoft.Data.Sqlite;

// ============================================================================
// KineticReports Blazor Sample Application (Interactive Server)
// ============================================================================
// This sample demonstrates:
// 1. Loading report definitions from JSON and code
// 2. Running the same reports with InMemory or SQLite providers
// 3. Rendering with the Blazor viewer using the standard engine pipeline
// 4. Default plugin loading using KineticReports.Core plugin manager
// 5. Realistic multi-page output with headers, footers, and multiple tables
//
// To run: dotnet run
// To build: dotnet build
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var sqlLiteConnectionString = builder.Configuration.GetConnectionString("SqlLite")
    ?? "Data Source=Data/kinetic-reports-samples.db;Mode=ReadWriteCreate;Cache=Shared";

builder.Services
    .AddKeneticReports()
    .ConfigureHtmlExporter(options => options.StylesheetHref = "/kinetic-report.css")
    .AddBlazorViewer()
    .AddSqlLiteDataProvider(sqlLiteConnectionString);

builder.Services.AddSingleton<ISampleReportCatalogService, SampleReportCatalogService>();

var app = builder.Build();

SeedInMemoryData(app.Services);
SeedSqlLiteData(sqlLiteConnectionString);

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

static void SeedSqlLiteData(string connectionString)
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = """
        CREATE TABLE IF NOT EXISTS RegionalPerformance (
            Region TEXT NOT NULL,
            Revenue NUMERIC NOT NULL,
            Target NUMERIC NOT NULL,
            Variance NUMERIC NOT NULL,
            Accounts INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS TopAccounts (
            Customer TEXT NOT NULL,
            Segment TEXT NOT NULL,
            AnnualRevenue NUMERIC NOT NULL,
            RenewalDate TEXT NOT NULL,
            Owner TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS MonthlyTrend (
            MonthOrder INTEGER NOT NULL,
            Month TEXT NOT NULL,
            Billings NUMERIC NOT NULL,
            GrossMargin TEXT NOT NULL,
            NewLogos INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS SalesOrdersDetail (
            OrderNumber TEXT NOT NULL,
            Customer TEXT NOT NULL,
            Region TEXT NOT NULL,
            OrderDate TEXT NOT NULL,
            Amount NUMERIC NOT NULL,
            Status TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS ImplementationMilestones (
            Customer TEXT NOT NULL,
            Workstream TEXT NOT NULL,
            Stage TEXT NOT NULL,
            GoLiveDate TEXT NOT NULL,
            ExecutiveSponsor TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS SupportEscalations (
            Ticket TEXT NOT NULL,
            Customer TEXT NOT NULL,
            Severity TEXT NOT NULL,
            Owner TEXT NOT NULL,
            DaysOpen INTEGER NOT NULL,
            Status TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS RenewalPipeline (
            Customer TEXT NOT NULL,
            RenewalQuarter TEXT NOT NULL,
            ARR NUMERIC NOT NULL,
            ExpansionPotential NUMERIC NOT NULL,
            Stage TEXT NOT NULL
        );

        DELETE FROM RegionalPerformance;
        DELETE FROM TopAccounts;
        DELETE FROM MonthlyTrend;
        DELETE FROM SalesOrdersDetail;
        DELETE FROM ImplementationMilestones;
        DELETE FROM SupportEscalations;
        DELETE FROM RenewalPipeline;

        INSERT INTO RegionalPerformance (Region, Revenue, Target, Variance, Accounts) VALUES
            ('North America', 4285000.00, 4000000.00, 285000.00, 42),
            ('EMEA', 3190000.00, 3400000.00, -210000.00, 28),
            ('APAC', 2745000.00, 2500000.00, 245000.00, 25),
            ('Latin America', 1180000.00, 1100000.00, 80000.00, 14);

        INSERT INTO TopAccounts (Customer, Segment, AnnualRevenue, RenewalDate, Owner) VALUES
            ('Acme Manufacturing', 'Enterprise', 845000.00, '2026-09-15', 'Morgan Lee'),
            ('Bright Retail Group', 'Mid-Market', 610000.00, '2026-08-01', 'Avery Shah'),
            ('Contour Labs', 'Enterprise', 575000.00, '2026-11-12', 'Noah Perez'),
            ('Delta Foods', 'Commercial', 490000.00, '2026-10-03', 'Jules Carter'),
            ('Evergreen Health', 'Enterprise', 455000.00, '2026-12-09', 'Morgan Lee');

        INSERT INTO MonthlyTrend (MonthOrder, Month, Billings, GrossMargin, NewLogos) VALUES
            (1, 'January', 970000.00, '42%', 4),
            (2, 'February', 1015000.00, '43%', 5),
            (3, 'March', 1082000.00, '41%', 6),
            (4, 'April', 1120000.00, '44%', 5),
            (5, 'May', 1184000.00, '45%', 7),
            (6, 'June', 1233000.00, '44%', 6);

        INSERT INTO SalesOrdersDetail (OrderNumber, Customer, Region, OrderDate, Amount, Status) VALUES
            ('SO-1048', 'Acme Manufacturing', 'North America', '2026-06-03', 184500.00, 'Booked'),
            ('SO-1051', 'Bright Retail Group', 'EMEA', '2026-06-05', 126000.00, 'Implementation'),
            ('SO-1059', 'Contour Labs', 'North America', '2026-06-09', 212500.00, 'Booked'),
            ('SO-1064', 'Delta Foods', 'Latin America', '2026-06-11', 78500.00, 'Shipped'),
            ('SO-1072', 'Evergreen Health', 'North America', '2026-06-14', 265000.00, 'Implementation'),
            ('SO-1079', 'Frontier Logistics', 'APAC', '2026-06-17', 94000.00, 'Booked'),
            ('SO-1086', 'Granite Energy', 'EMEA', '2026-06-20', 156750.00, 'Shipped'),
            ('SO-1092', 'Harbor Financial', 'North America', '2026-06-23', 301000.00, 'Booked'),
            ('SO-1099', 'Ivory Telecom', 'EMEA', '2026-06-25', 118500.00, 'Implementation'),
            ('SO-1104', 'Juniper Systems', 'APAC', '2026-06-27', 221250.00, 'Booked');

        INSERT INTO ImplementationMilestones (Customer, Workstream, Stage, GoLiveDate, ExecutiveSponsor) VALUES
            ('Acme Manufacturing', 'ERP Integration', 'UAT', '2026-08-18', 'Dana Brooks'),
            ('Bright Retail Group', 'Store Analytics', 'Build', '2026-09-02', 'Samir Patel'),
            ('Contour Labs', 'Compliance Pack', 'Design', '2026-10-10', 'Mia Foster'),
            ('Evergreen Health', 'Claims Automation', 'UAT', '2026-08-27', 'Jordan Price'),
            ('Frontier Logistics', 'Scheduling', 'Pilot', '2026-09-14', 'Leo Kim'),
            ('Granite Energy', 'Field Service', 'Discovery', '2026-11-05', 'Priya Nair');

        INSERT INTO SupportEscalations (Ticket, Customer, Severity, Owner, DaysOpen, Status) VALUES
            ('INC-2401', 'Acme Manufacturing', 'High', 'Platform Ops', 2, 'Mitigation in progress'),
            ('INC-2408', 'Contour Labs', 'Medium', 'Data Services', 4, 'Awaiting customer validation'),
            ('INC-2410', 'Delta Foods', 'High', 'Integration Team', 1, 'Hotfix scheduled'),
            ('INC-2417', 'Evergreen Health', 'Medium', 'Reporting Team', 6, 'Root cause identified'),
            ('INC-2420', 'Frontier Logistics', 'Low', 'Support', 3, 'Monitoring');

        INSERT INTO RenewalPipeline (Customer, RenewalQuarter, ARR, ExpansionPotential, Stage) VALUES
            ('Harbor Financial', 'Q3 FY26', 220000.00, 90000.00, 'Proposal'),
            ('Ivory Telecom', 'Q3 FY26', 185000.00, 40000.00, 'Negotiation'),
            ('Juniper Systems', 'Q4 FY26', 310000.00, 120000.00, 'Executive review'),
            ('Keystone Bank', 'Q4 FY26', 275000.00, 65000.00, 'Proposal'),
            ('Luma Devices', 'Q1 FY27', 198000.00, 55000.00, 'Discovery'),
            ('North Ridge Health', 'Q1 FY27', 342000.00, 135000.00, 'Negotiation');
        """;

    command.ExecuteNonQuery();
}
