namespace Tests;

using Dotsum;

public partial class As
{

    [Case("Case0", typeof(string))]
    [Case("Case1")]
    partial class StringOrNone
    {

    }

    [Fact]
    public void Case0()
    {
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0);
    }

    [Fact]
    public void Case1()
    {
        Assert.Throws<InvalidOperationException>(() => StringOrNone.Case1.AsCase0);
    }
}