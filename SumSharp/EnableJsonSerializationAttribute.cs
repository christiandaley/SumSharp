using System;

namespace SumSharp;

/// <summary>
/// Enables support for JsonSerialization.  
/// </summary>
/// <param name="Support">The type of serialization to support. Specify <see cref="JsonSerializationSupport.Standard">Standard</see>
/// | <see cref="JsonSerializationSupport.Newtonsoft">Newtonsoft</see> for both.</param>
/// <param name="AddJsonConverterAttribute">If true, adds a JsonConverter attribute to the generated type.
/// This parameter is ignored if the generated type is nested inside a generic type.</param>
/// <param name="UsingAOTCompilation">If true, indicates that the project is using AOT compilation. Causes SumSharp to emit code that
/// prevents trimming of the generated StandardJsonConverter and forces <paramref name="AddJsonConverterAttribute"/> to be false</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class EnableJsonSerializationAttribute(JsonSerializationSupport Support = JsonSerializationSupport.Standard, bool AddJsonConverterAttribute = true, bool UsingAOTCompilation = false) : Attribute
{

}