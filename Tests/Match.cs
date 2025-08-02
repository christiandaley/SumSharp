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

    /*[Fact]
    public async Task TupleMatchAsync()
    {
        var passed =
            await ContainsTuple<double>.Case1(2.0, 3.0).Match(
            (_, _) => Task.FromResult(false),
            (d1, d2) => Task.FromResult(d1 == 2.0 && d2 == 3.0));
    }*/
}