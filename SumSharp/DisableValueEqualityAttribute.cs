using System;

namespace SumSharp;

///<summary>
/// Disables generation of <see cref="IEquatable{T}"/> implementation, <see cref="object.Equals(object)"/> override, and
/// == and != operators. Has no effect on record and record struct types.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class DisableValueEqualityAttribute : Attribute
{

}