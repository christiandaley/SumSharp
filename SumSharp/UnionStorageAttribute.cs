using System;

namespace SumSharp;

/// <summary>
/// Configurable storage settings for a discriminated union
/// </summary>
/// <param name="Strategy">The storage strategy to use. A non-default <see cref="UnionCaseStorage"/> 
/// specified for an individual case will override the union's storage strategy</param>
/// <param name="UnmanagedStorageSize">The number of bytes to reserve for unmanaged type storage. Must be at least 
/// the size of the largest unmanaged type that will be stored. Providing this value is only neccessary if you want to
/// used shared storage for generic unmanaged types</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class UnionStorageAttribute(UnionStorageStrategy Strategy = UnionStorageStrategy.Default, int UnmanagedStorageSize = 0) : Attribute
{

}