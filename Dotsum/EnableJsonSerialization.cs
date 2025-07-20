using System;

namespace Dotsum;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class EnableJsonSerializationAttribute(bool UsingSourceGen = false) : Attribute
{
    
}