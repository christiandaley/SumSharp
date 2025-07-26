using System;

namespace SumSharp;

/// <summary>
/// Info about a generic type. Can help the SumSharp generator produce more efficient code in
/// some cases.
/// </summary>
public enum GenericTypeInfo
{
    ReferenceType = 1,
    ValueType = 2,
}