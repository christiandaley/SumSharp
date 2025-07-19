using System;

namespace SumSharp;

/// <summary>
/// Info about a generic type's kind (i.e reference or value type)
/// </summary>
public enum GenericTypeInfo
{
    /// <summary>
    /// The generic type is a reference type
    /// </summary>
    ReferenceType = 1,
    /// <summary>
    /// The generic type is a value type
    /// </summary>
    ValueType = 2,
}