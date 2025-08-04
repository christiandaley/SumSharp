namespace Tests;

using SumSharp;

public partial class Match
{

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1")]
    partial class StringOrNone
    {

    }

    [UnionCase("Case0", typeof((string, ushort)))]
    [UnionCase("Case1", "System.ValueTuple<T, T>")]

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
        var passed =
            StringOrNone.Case0("3").Match(
                value => value == "3",
                () => false);

        Assert.True(passed);
    }

    [Fact]
    public async Task Case0Async()
    {
        var passed =
            await StringOrNone.Case0("3").Match(
                value => Task.FromResult(value == "3"),
                () => Task.FromResult(false));

        Assert.True(passed);
    }

    [Fact]
    public void Case1()
    {
        var passed =
            StringOrNone.Case1.Match(
                _ => false,
                () => true);

        Assert.True(passed);
    }

    [Fact]
    public async Task Case1Async()
    {
        var passed =
            await StringOrNone.Case1.Match(
                _ => Task.FromResult(false),
                () => Task.FromResult(true));

        Assert.True(passed);
    }

    [Fact]
    public void TupleMatch()
    {
        var passed =
            ContainsTuple<double>.Case0("abc", 5).Match(
            (s, i) => s == "abc" && i == 5,
            (_, _) => false);

        Assert.True(passed);
    }

    [Fact]
    public void NamedMatchNoDefault()
    {
        var passed =
            Result<string, Exception>.Ok("abc").Match(
                Ok: str => str == "abc",
                Error: _ => false);

        Assert.True(passed);
    }

    [Fact]
    public void NamedMatchWithDefault()
    {
        var passed =
            Result<string, Exception>.Error(new InvalidOperationException()).Match(
                Ok: str => false,
                _: () => true);

        Assert.True(passed);
    }

    [Fact]
    public void NonExhaustiveMatch()
    {
        var err = Assert.Throws<MatchFailureException>(() =>
        {
            Result<string, Exception>.Ok("abc").Match(
                Error: _ => true);
        });

        Assert.Equal("Ok", err.CaseName);
    }
}