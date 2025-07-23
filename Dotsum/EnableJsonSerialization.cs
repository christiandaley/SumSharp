using System;

namespace Dotsum;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class EnableJsonSerializationAttribute(JsonSerialization Serialization = JsonSerialization.Standard, bool AddJsonConverterAttribute = true) : Attribute
{

}