namespace KineticReports.Core.Tests.Definition;

using Shouldly;
using KineticReports.Core.Definition;

public class ParameterDefinitionTests
{
    [Fact]
    public void ParameterDefinition_WithRequiredFields_IsCreated()
    {
        var param = new ParameterDefinition
        {
            Id = "StartDate",
            Name = "Start Date",
            Type = ParameterType.DateTime
        };
        param.Id.ShouldBe("StartDate");
        param.Name.ShouldBe("Start Date");
        param.Type.ShouldBe(ParameterType.DateTime);
    }

    [Fact]
    public void ParameterDefinition_DefaultValues()
    {
        var param = new ParameterDefinition
        {
            Id = "TextParam",
            Name = "Text"
        };
        param.Type.ShouldBe(ParameterType.String);
        param.IsRequired.ShouldBeFalse();
        param.DefaultValue.ShouldBeNull();
        param.Description.ShouldBeNull();
    }

    [Fact]
    public void ParameterDefinition_WithOptionalFields()
    {
        var param = new ParameterDefinition
        {
            Id = "Amount",
            Name = "Amount",
            Type = ParameterType.Decimal,
            IsRequired = true,
            DefaultValue = "100.00",
            Description = "Purchase amount in USD"
        };
        param.IsRequired.ShouldBeTrue();
        param.DefaultValue.ShouldBe("100.00");
        param.Description.ShouldBe("Purchase amount in USD");
    }
}
