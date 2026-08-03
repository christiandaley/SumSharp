using System;

namespace SumSharp;

/// <summary>
/// Thrown when the a generated IUnionMembers.Create static factory method is called with a type that has more than one matching case in the union
/// </summary>
/// <param name="unionType">The type of the union that failed to be constructed</param>
/// <param name="caseType">The type of the case that failed to be constructed</param>
/// <param name="candidateCaseNames">The multiple cases that match the given type</param>
public sealed class CreateFailureException(Type unionType, Type caseType, string[] candidateCaseNames) : Exception($"Failed to construct a {unionType} with underlying type {caseType}. There are multiple candidate cases of this type")
{
    /// <summary>
    /// The type of the union that failed to be constructed
    /// </summary>
    public Type UnionType => unionType;

    /// <summary>
    /// The type of the case that failed to be constructed
    /// </summary>
    public Type CaseType => caseType;

    /// <summary>
    /// The multiple cases that match the given type
    /// </summary>
    public string[] CandidateCaseNames => candidateCaseNames;
}
