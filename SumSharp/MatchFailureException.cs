using System;

namespace SumSharp;

public class MatchFailureException(string caseName) : Exception($"Match failed to handle case {caseName}")
{
    public string CaseName => caseName;
}
