namespace Tests;

using Dotsum;

public partial class Match
{

    [Case("Case0", typeof(string))]
    [Case("Case1")]
    partial class StringOrNone
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
}