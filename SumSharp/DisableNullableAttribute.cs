using System;

namespace SumSharp;

/// <summary>
/// Disables nullable annotations in generated code
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class DisableNullableAttribute : Attribute
{

}