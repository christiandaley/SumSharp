namespace Tests;

using SumSharp;

public partial class MatchVoid
{

    [UnionCase("Case0", typeof(int))]
    [UnionCase("Case1")]
    partial class IntOrNone
    {

    }

    [UnionCase("Case0", typeof(ValueTuple<bool, byte>))]
    [UnionCase("Case1", "(T, T)")]

    partial class ContainsTuple<T>
    {

    }

    [UnionCase("Ok", "T")]
    [UnionCase("Error", "E")]
    partial class Result<T, E>
    {

    }

    [Fact]
    public void Case0()
    {
        bool passed = false;

        IntOrNone.Case0(19).Match(value => { passed = value == 19; }, () => { });

        Assert.True(passed);
    }

    [Fact]
    public void Case1()
    {
        bool passed = false;

        IntOrNone.Case1.Match(_ => { }, () => { passed = true; });

        Assert.True(passed);
    }

    [Fact]
    public void TupleMatch()
    {
        var passed = false;

        ContainsTuple<string>.Case0(true, 1).Match(
            (b, i) => passed = b && i == 1,
            (_, _) => { });

        Assert.True(passed);
    }

    [Fact]
    public void NamedMatchNoDefault()
    {
        bool passed = false;

        Result<string, Exception>.Ok("abc").Match(
            Ok: str => passed = str == "abc",
            Error: _ => { });

        Assert.True(passed);
    }

    [Fact]
    public void NamedMatchWithDefault()
    {
        bool passed = false;

        Result<string, Exception>.Error(new InvalidOperationException()).Match(
            Ok: str => { },
            _: () => passed = true);

        Assert.True(passed);
    }

    [Fact]
    public void NonExhaustiveMatch()
    {
        var err = Assert.Throws<MatchFailureException>(() =>
        {
            Result<string, Exception>.Ok("abc").Match(
                Error: _ => { });
        });

        Assert.Equal("Ok", err.CaseName);
    }
}