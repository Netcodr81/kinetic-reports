using KineticReports.Core.Data;

namespace KineticReports.Data.SqlServer.Tests;

public class DataTypesTests
{
    [Fact]
    public void ColumnSchema_CreatesValidInstance()
    {
        // Arrange & Act
        var schema = new ColumnSchema
        {
            Name = "Id",
            ClrType = typeof(int),
            Ordinal = 0,
            IsNullable = false,
            DisplayName = "Identifier"
        };

        // Assert
        schema.Name.ShouldBe("Id");
        schema.ClrType.ShouldBe(typeof(int));
        schema.Ordinal.ShouldBe(0);
        schema.IsNullable.ShouldBeFalse();
        schema.DisplayName.ShouldBe("Identifier");
    }

    [Fact]
    public void DataRow_AccessValueByOrdinal()
    {
        // Arrange
        var row = new DataRow
        {
            Values = [1, "Test", null, 3.14]
        };

        // Act & Assert
        row[0].ShouldBe(1);
        row[1].ShouldBe("Test");
        row[2].ShouldBeNull();
        row[3].ShouldBe(3.14);
        row[99].ShouldBeNull();
    }

    [Fact]
    public void DataRow_GetValueByColumnName()
    {
        // Arrange
        var schema = new List<ColumnSchema>
        {
            new ColumnSchema { Name = "Id", ClrType = typeof(int), Ordinal = 0, IsNullable = false },
            new ColumnSchema { Name = "Name", ClrType = typeof(string), Ordinal = 1, IsNullable = true }
        };
        var row = new DataRow
        {
            Values = [1, "Alice"]
        };

        // Act
        var id = row.GetValue(schema, "Id");
        var name = row.GetValue(schema, "Name");
        var notFound = row.GetValue(schema, "NonExistent");

        // Assert
        id.ShouldBe(1);
        name.ShouldBe("Alice");
        notFound.ShouldBeNull();
    }

    [Fact]
    public void DataRow_GetValueByColumnName_CaseInsensitive()
    {
        // Arrange
        var schema = new List<ColumnSchema>
        {
            new ColumnSchema { Name = "Id", ClrType = typeof(int), Ordinal = 0, IsNullable = false }
        };
        var row = new DataRow
        {
            Values = [42]
        };

        // Act
        var valueUpper = row.GetValue(schema, "ID");
        var valueMixed = row.GetValue(schema, "Id");
        var valueLower = row.GetValue(schema, "id");

        // Assert
        valueUpper.ShouldBe(42);
        valueMixed.ShouldBe(42);
        valueLower.ShouldBe(42);
    }

    [Fact]
    public void QueryRequest_CreatesValidInstance()
    {
        // Arrange & Act
        var request = new QueryRequest
        {
            DatasetName = "Employees",
            QueryText = "SELECT * FROM Employees WHERE DepartmentId = @DeptId",
            Parameters = new Dictionary<string, object?> { { "DeptId", 5 } },
            TimeoutMs = 60000
        };

        // Assert
        request.DatasetName.ShouldBe("Employees");
        request.QueryText.ShouldContain("@DeptId");
        request.Parameters["DeptId"].ShouldBe(5);
        request.TimeoutMs.ShouldBe(60000);
    }

    [Fact]
    public void QueryRequest_DefaultTimeoutIs30Seconds()
    {
        // Arrange & Act
        var request = new QueryRequest
        {
            DatasetName = "Test",
            QueryText = "SELECT 1"
        };

        // Assert
        request.TimeoutMs.ShouldBe(30000);
    }

    [Fact]
    public void QueryRequest_DefaultParametersIsEmpty()
    {
        // Arrange & Act
        var request = new QueryRequest
        {
            DatasetName = "Test",
            QueryText = "SELECT 1"
        };

        // Assert
        request.Parameters.Count.ShouldBe(0);
    }

    [Fact]
    public void QueryResult_CreatesValidInstance()
    {
        // Arrange
        var schema = new List<ColumnSchema>
        {
            new ColumnSchema { Name = "Id", ClrType = typeof(int), Ordinal = 0, IsNullable = false }
        };
        var rows = new List<DataRow>
        {
            new DataRow { Values = [1] },
            new DataRow { Values = [2] }
        };

        // Act
        var result = new QueryResult
        {
            Schema = schema,
            Rows = rows,
            RowCount = 2,
            ExecutionTimeMs = 150,
            DiagnosticsMessage = "Success"
        };

        // Assert
        result.Schema.Count.ShouldBe(1);
        result.Rows.Count.ShouldBe(2);
        result.RowCount.ShouldBe(2);
        result.ExecutionTimeMs.ShouldBe(150);
        result.DiagnosticsMessage.ShouldBe("Success");
    }
}
