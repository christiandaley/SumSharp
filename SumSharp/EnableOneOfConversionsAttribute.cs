using System;

namespace SumSharp;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class EnableOneOfConversionsAttribute : Attribute
{
    /// <summary>
    /// Enables implicit conversions between the type and a corresponding OneOf type
    /// </summary>
    public EnableOneOfConversionsAttribute()
    {

    }

    /// <summary>
    /// Enables implicit conversions between the type and a corresponding OneOf type. Uses
    /// <paramref name="EmptyCase"/> to represent an empty case in the OneOf type
    /// </summary>
    /// <param name="EmptyCase">The type that represents an empty case in the OneOf type. Must have a parameterless constructor</param>
    public EnableOneOfConversionsAttribute(Type EmptyCase)
    {

    }
}