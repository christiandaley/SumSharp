using System;

namespace SumSharp;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class DisableValueEqualityAttribute : Attribute
{

}