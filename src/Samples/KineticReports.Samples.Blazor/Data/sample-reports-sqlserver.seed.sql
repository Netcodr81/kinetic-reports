IF OBJECT_ID('dbo.RegionalPerformance', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RegionalPerformance
    (
        Region NVARCHAR(64) NOT NULL,
        Revenue DECIMAL(18, 2) NOT NULL,
        Target DECIMAL(18, 2) NOT NULL,
        Variance DECIMAL(18, 2) NOT NULL,
        Accounts INT NOT NULL
    );
END;

IF OBJECT_ID('dbo.TopAccounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TopAccounts
    (
        Customer NVARCHAR(128) NOT NULL,
        Segment NVARCHAR(64) NOT NULL,
        AnnualRevenue DECIMAL(18, 2) NOT NULL,
        RenewalDate NVARCHAR(32) NOT NULL,
        Owner NVARCHAR(128) NOT NULL
    );
END;

IF OBJECT_ID('dbo.MonthlyTrend', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MonthlyTrend
    (
        MonthOrder INT NOT NULL,
        [Month] NVARCHAR(32) NOT NULL,
        Billings DECIMAL(18, 2) NOT NULL,
        GrossMargin NVARCHAR(16) NOT NULL,
        NewLogos INT NOT NULL
    );
END;

IF OBJECT_ID('dbo.SalesOrdersDetail', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesOrdersDetail
    (
        OrderNumber NVARCHAR(32) NOT NULL,
        Customer NVARCHAR(128) NOT NULL,
        Region NVARCHAR(64) NOT NULL,
        OrderDate NVARCHAR(32) NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Status NVARCHAR(32) NOT NULL
    );
END;

IF OBJECT_ID('dbo.ImplementationMilestones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ImplementationMilestones
    (
        Customer NVARCHAR(128) NOT NULL,
        Workstream NVARCHAR(128) NOT NULL,
        Stage NVARCHAR(64) NOT NULL,
        GoLiveDate NVARCHAR(32) NOT NULL,
        ExecutiveSponsor NVARCHAR(128) NOT NULL
    );
END;

IF OBJECT_ID('dbo.SupportEscalations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupportEscalations
    (
        Ticket NVARCHAR(32) NOT NULL,
        Customer NVARCHAR(128) NOT NULL,
        Severity NVARCHAR(32) NOT NULL,
        Owner NVARCHAR(128) NOT NULL,
        DaysOpen INT NOT NULL,
        Status NVARCHAR(256) NOT NULL
    );
END;

IF OBJECT_ID('dbo.RenewalPipeline', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RenewalPipeline
    (
        Customer NVARCHAR(128) NOT NULL,
        RenewalQuarter NVARCHAR(32) NOT NULL,
        ARR DECIMAL(18, 2) NOT NULL,
        ExpansionPotential DECIMAL(18, 2) NOT NULL,
        Stage NVARCHAR(64) NOT NULL
    );
END;

DELETE FROM dbo.RegionalPerformance;
DELETE FROM dbo.TopAccounts;
DELETE FROM dbo.MonthlyTrend;
DELETE FROM dbo.SalesOrdersDetail;
DELETE FROM dbo.ImplementationMilestones;
DELETE FROM dbo.SupportEscalations;
DELETE FROM dbo.RenewalPipeline;

INSERT INTO dbo.RegionalPerformance (Region, Revenue, Target, Variance, Accounts)
VALUES
    ('North America', 4285000.00, 4000000.00, 285000.00, 42),
    ('EMEA', 3190000.00, 3400000.00, -210000.00, 28),
    ('APAC', 2745000.00, 2500000.00, 245000.00, 25),
    ('Latin America', 1180000.00, 1100000.00, 80000.00, 14);

INSERT INTO dbo.TopAccounts (Customer, Segment, AnnualRevenue, RenewalDate, Owner)
VALUES
    ('Acme Manufacturing', 'Enterprise', 845000.00, '2026-09-15', 'Morgan Lee'),
    ('Bright Retail Group', 'Mid-Market', 610000.00, '2026-08-01', 'Avery Shah'),
    ('Contour Labs', 'Enterprise', 575000.00, '2026-11-12', 'Noah Perez'),
    ('Delta Foods', 'Commercial', 490000.00, '2026-10-03', 'Jules Carter'),
    ('Evergreen Health', 'Enterprise', 455000.00, '2026-12-09', 'Morgan Lee');

INSERT INTO dbo.MonthlyTrend (MonthOrder, [Month], Billings, GrossMargin, NewLogos)
VALUES
    (1, 'January', 970000.00, '42%', 4),
    (2, 'February', 1015000.00, '43%', 5),
    (3, 'March', 1082000.00, '41%', 6),
    (4, 'April', 1120000.00, '44%', 5),
    (5, 'May', 1184000.00, '45%', 7),
    (6, 'June', 1233000.00, '44%', 6);

INSERT INTO dbo.SalesOrdersDetail (OrderNumber, Customer, Region, OrderDate, Amount, Status)
VALUES
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

INSERT INTO dbo.ImplementationMilestones (Customer, Workstream, Stage, GoLiveDate, ExecutiveSponsor)
VALUES
    ('Acme Manufacturing', 'ERP Integration', 'UAT', '2026-08-18', 'Dana Brooks'),
    ('Bright Retail Group', 'Store Analytics', 'Build', '2026-09-02', 'Samir Patel'),
    ('Contour Labs', 'Compliance Pack', 'Design', '2026-10-10', 'Mia Foster'),
    ('Evergreen Health', 'Claims Automation', 'UAT', '2026-08-27', 'Jordan Price'),
    ('Frontier Logistics', 'Scheduling', 'Pilot', '2026-09-14', 'Leo Kim'),
    ('Granite Energy', 'Field Service', 'Discovery', '2026-11-05', 'Priya Nair');

INSERT INTO dbo.SupportEscalations (Ticket, Customer, Severity, Owner, DaysOpen, Status)
VALUES
    ('INC-2401', 'Acme Manufacturing', 'High', 'Platform Ops', 2, 'Mitigation in progress'),
    ('INC-2408', 'Contour Labs', 'Medium', 'Data Services', 4, 'Awaiting customer validation'),
    ('INC-2410', 'Delta Foods', 'High', 'Integration Team', 1, 'Hotfix scheduled'),
    ('INC-2417', 'Evergreen Health', 'Medium', 'Reporting Team', 6, 'Root cause identified'),
    ('INC-2420', 'Frontier Logistics', 'Low', 'Support', 3, 'Monitoring');

INSERT INTO dbo.RenewalPipeline (Customer, RenewalQuarter, ARR, ExpansionPotential, Stage)
VALUES
    ('Harbor Financial', 'Q3 FY26', 220000.00, 90000.00, 'Proposal'),
    ('Ivory Telecom', 'Q3 FY26', 185000.00, 40000.00, 'Negotiation'),
    ('Juniper Systems', 'Q4 FY26', 310000.00, 120000.00, 'Executive review'),
    ('Keystone Bank', 'Q4 FY26', 275000.00, 65000.00, 'Proposal'),
    ('Luma Devices', 'Q1 FY27', 198000.00, 55000.00, 'Discovery'),
    ('North Ridge Health', 'Q1 FY27', 342000.00, 135000.00, 'Negotiation');
