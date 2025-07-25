namespace Tests;

using SumSharp;

public partial class Is
{

    [Case("Case0", typeof(string))]
    [Case("Case1")]
    partial class StringOrNone
    {

    }

    [Fact]
    public void Case0()
    {
        Assert.True(StringOrNone.Case0("3").IsCase0);
    }

    [Fact]
    public void Case1()
    {
        Assert.True(StringOrNone.Case1.IsCase1);
    }
}