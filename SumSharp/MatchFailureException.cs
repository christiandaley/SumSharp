using System;

namespace SumSharp;

/// <summary>
/// Thrown when a Match invocation on a union lacks a handler for the active case
/// </summary>
/// <param name="caseName">The name of the active case held by the union</param>
public sealed class MatchFailureException(string caseName) : Exception($"Failed to handle case {caseName}")
{
    /// <summary>
    /// The name of the active case held by the union
    /// </summary>
    public string CaseName => caseName;
}
