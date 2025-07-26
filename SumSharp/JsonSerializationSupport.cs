using System;

namespace SumSharp;

/// <summary>
/// Support for Json serialization.
/// </summary>
public enum JsonSerializationSupport
{
    /// <summary>
    /// System.Text.Json
    /// </summary>
    Standard = 1,
    /// <summary>
    /// Newtonsoft.Json
    /// </summary>
    Newtonsoft = 2,
}
