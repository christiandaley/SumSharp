using System;

namespace SumSharp;

public sealed class MatchFailureException(string caseName) : Exception($"Match failed to handle case {caseName}")
{
    public string CaseName => caseName;
}
