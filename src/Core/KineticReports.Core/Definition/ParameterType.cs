namespace KineticReports.Core.Definition;

/// <summary>Specifies the data type of a report <see cref="ParameterDefinition"/>.</summary>
public enum ParameterType
{
    /// <summary>A Unicode string value.</summary>
    String,

    /// <summary>A 64-bit integer value.</summary>
    Integer,

    /// <summary>A 128-bit decimal value.</summary>
    Decimal,

    /// <summary>A Boolean (true/false) value.</summary>
    Boolean,

    /// <summary>A date and time value.</summary>
    DateTime,

    /// <summary>A globally unique identifier.</summary>
    Guid,

    /// <summary>An enumeration value represented as a string key.</summary>
    Enum,
}
