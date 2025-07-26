using System;

namespace SumSharp;

/// <summary>
/// The storage mode for an individual case
/// </summary>
public enum StorageMode
{
    /// <summary>
    /// The storage mode is determined by the overall <see cref="StorageStrategy"/>
    /// </summary>
    Default = 0,
    /// <summary>
    /// The case's value is stored in an <see cref="object"/> field. All cases stored as an <see cref="object"/>
    /// share a single field for their storage. Value types will be boxed, resulting in a heap allocation.
    /// </summary>
    AsObject = 1,
    /// <summary>
    /// The case's value is stored "inline". Primitive types (<see cref="int"/>, <see cref="double"/>, etc.) and 
    /// <see cref="Enum"/> types share a single field for their storage. Other cases are given a dedicated field of their corresponding 
    /// type. Prevents heap allocation for value type cases.
    /// </summary>
    Inline = 2,
}