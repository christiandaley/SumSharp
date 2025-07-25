using System;

namespace SumSharp;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class EnableJsonSerializationAttribute(JsonSerializationSupport Support = JsonSerializationSupport.Standard, bool AddJsonConverterAttribute = true) : Attribute
{

}