using System;

namespace SumSharp;

/// <summary>
/// THe overall storage strategy for a type
/// </summary>
public enum StorageStrategy
{
    /// <summary>
    /// If there is a single unique type across all cases, <see cref="StorageMode.Inline">Inline</see> storage
    /// mode is used. Otherwise all cases share a single <see cref="object"/> field for their storage
    /// </summary>
    Default = 0,
    /// <summary>
    /// All cases share a single <see cref="object"/> field for their storage
    /// </summary>
    OneObject = 1,
    /// <summary>
    /// All reference type cases share a single <see cref="object"/> field for their storage. All
    /// value type cases are stored <see cref="StorageMode.Inline">Inline</see>
    /// </summary>
    InlineValueTypes = 2,
}