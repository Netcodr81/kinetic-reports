CREATE TABLE IF NOT EXISTS quote_daily (
    heading TEXT NOT NULL,
    quote TEXT NOT NULL,
    generated_on TEXT NOT NULL,
    author TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS sales_orders (
    order_id TEXT NOT NULL,
    region TEXT NOT NULL,
    sales_person TEXT NOT NULL,
    amount REAL NOT NULL,
    status TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS customer_activity (
    customer_name TEXT NOT NULL,
    tier TEXT NOT NULL,
    last_invoice TEXT NOT NULL,
    balance REAL NOT NULL,
    assigned_rep TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS kpi_summary (
    metric TEXT NOT NULL,
    value TEXT NOT NULL,
    trend TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS regional_performance (
    region TEXT NOT NULL,
    quota TEXT NOT NULL,
    actual TEXT NOT NULL,
    attainment TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS style_showcase (
    label TEXT NOT NULL,
    preview TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS expression_demo (
    customer_name TEXT NOT NULL,
    tier TEXT NOT NULL,
    balance REAL NOT NULL,
    assigned_rep TEXT NOT NULL
);

DELETE FROM quote_daily;
DELETE FROM sales_orders;
DELETE FROM customer_activity;
DELETE FROM kpi_summary;
DELETE FROM regional_performance;
DELETE FROM style_showcase;
DELETE FROM expression_demo;

INSERT INTO quote_daily (heading, quote, generated_on, author) VALUES
('Daily Operations Snapshot', 'Reliable reports start with deterministic data.', '2026-01-15T09:30:00Z', 'KineticReports Samples');

INSERT INTO sales_orders (order_id, region, sales_person, amount, status) VALUES
('SO-1001', 'North', 'Alicia', 12450.75, 'Complete'),
('SO-1002', 'West', 'Marcus', 8975.40, 'Shipped'),
('SO-1003', 'South', 'Priya', 15320.00, 'Pending');

INSERT INTO customer_activity (customer_name, tier, last_invoice, balance, assigned_rep) VALUES
('Acme Manufacturing', 'Gold', 'INV-4421', 1400.00, 'Noah'),
('Bright Retail', 'Silver', 'INV-4428', 0.00, 'Avery'),
('Contour Labs', 'Platinum', 'INV-4432', 510.25, 'Iris');

INSERT INTO kpi_summary (metric, value, trend) VALUES
('Monthly Revenue', '$348,210', '+8.2%'),
('Gross Margin', '41.8%', '+1.1%'),
('Open Projects', '27', '-2');

INSERT INTO regional_performance (region, quota, actual, attainment) VALUES
('North', '$120,000', '$128,500', '107%'),
('South', '$95,000', '$92,750', '98%'),
('West', '$105,000', '$110,400', '105%');

INSERT INTO style_showcase (label, preview) VALUES
('Primary Heading', 'Bold text with spacing and high contrast'),
('Body Content', 'Readable default styling for long-form text'),
('Accent Note', 'Emphasized content for callouts');

INSERT INTO expression_demo (customer_name, tier, balance, assigned_rep) VALUES
('Acme Manufacturing', 'Gold', 1400.00, 'Noah'),
('Bright Retail', 'Silver', 0.00, 'Avery'),
('Contour Labs', 'Platinum', 510.25, 'Iris');
