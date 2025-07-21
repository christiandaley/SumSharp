using System;

namespace Dotsum;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class DisableBoxingAttribute(bool Disable = true) : Attribute
{
    
}