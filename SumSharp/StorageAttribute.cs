using System;

namespace SumSharp;

/// <summary>
/// Specifies the storage strategy for the type. A non-default <see cref="StorageMode"/> 
/// specified for an individual case will override the overall storage strategy for that case.
/// </summary>
/// <param name="Strategy">The storage strategy to use</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class StorageAttribute(StorageStrategy Strategy = StorageStrategy.Default, int UnmanagedStorageSize = 0) : Attribute
{

}