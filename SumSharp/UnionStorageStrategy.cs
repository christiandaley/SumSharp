using System;

namespace SumSharp;

/// <summary>
/// The overall storage strategy for a union type
/// </summary>
public enum UnionStorageStrategy
{
    /// <summary>
    /// If there is a single unique type across all cases, <see cref="UnionCaseStorage.Inline">Inline</see> storage
    /// is used. Otherwise all cases share a single <see cref="object"/> field for their storage
    /// </summary>
    InlineValueTypes = 0,
    /// <summary>
    /// All cases share a single <see cref="object"/> field for their storage
    /// </summary>
    OneObject = 1,
}