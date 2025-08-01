using System;

namespace SumSharp;

/// <summary>
/// The storage type for an individual case
/// </summary>
public enum UnionCaseStorage
{
    /// <summary>
    /// The storage is determined by the overall <see cref="UnionStorageStrategy"/>
    /// </summary>
    Default = 0,
    /// <summary>
    /// The case's value is stored in an <see cref="object"/> field. All cases stored as an <see cref="object"/>
    /// share a single field for their storage. Value types will be boxed, resulting in a heap allocation
    /// </summary>
    AsObject = 1,
    /// <summary>
    /// The case's value is stored "inline". Primitive types (<see cref="int"/>, <see cref="double"/>, <see cref="bool"/>, etc.), 
    /// <see cref="Enum"/> types, and struct types that SumSharp determines to meet the unmanaged constraint will share a 
    /// single field for their storage. Other cases are given a dedicated field of their corresponding type. Prevents
    /// boxing of value types
    /// </summary>
    Inline = 2,
}