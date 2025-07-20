namespace Tests;

using Dotsum;

public partial class Switch
{

    [Case("Case0", typeof(int))]
    [Case("Case1")]
    partial class IntOrNone
    {

    }

    [Fact]
    public void Case0()
    {
        bool passed = false;

        IntOrNone.Case0(3).Switch(_ => { passed = true; }, () => { });

        Assert.True(passed);
    }

    [Fact]
    public void Case1()
    {
        bool passed = false;

        IntOrNone.Case1.Switch(_ => { }, () => { passed = true; });

        Assert.True(passed);
    }
}