namespace Tests;

using SumSharp;

public partial class As
{

    [Case("Case0", typeof(string))]
    [Case("Case1")]
    partial class StringOrNone
    {

    }

    [Case("Case0", typeof(int))]
    [Case("Case1")]
    partial class IntOrNone
    {

    }

    [Fact]
    public void AsCase0()
    {
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0);
    }

    [Fact]
    public void AsCase0Throws()
    {
        Assert.Throws<InvalidOperationException>(() => StringOrNone.Case1.AsCase0);
    }

    [Fact]
    public async Task AsCase0Or()
    {
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0OrDefault);
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0Or("2"));
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0Or(() => "1"));
        Assert.Equal("3", await StringOrNone.Case0("3").AsCase0Or(() => Task.FromResult("1")));

        Assert.Null(StringOrNone.Case1.AsCase0OrDefault);
        Assert.Equal("2", StringOrNone.Case1.AsCase0Or("2"));
        Assert.Equal("1", StringOrNone.Case1.AsCase0Or(() => "1"));
        Assert.Equal("0", await StringOrNone.Case1.AsCase0Or(() => Task.FromResult("0")));
    }
}