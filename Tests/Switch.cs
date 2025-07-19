namespace Tests;

using SumSharp;

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

        IntOrNone.Case0(19).Switch(value => { passed = value == 19; }, () => { });

        Assert.True(passed);
    }

    [Fact]
    public async Task Case0Async()
    {
        bool passed = false;

        await IntOrNone.Case0(3).Switch(
            value => 
            { 
                passed = value == 3;

                return Task.CompletedTask;
            }, 
            () => Task.CompletedTask);

        Assert.True(passed);
    }

    [Fact]
    public void Case1()
    {
        bool passed = false;

        IntOrNone.Case1.Switch(_ => { }, () => { passed = true; });

        Assert.True(passed);
    }

    [Fact]
    public async Task Case1Async()
    {
        bool passed = false;

        await IntOrNone.Case1.Switch(
            _ => Task.CompletedTask,
            () =>
            {
                passed = true;

                return Task.CompletedTask;
            });

        Assert.True(passed);
    }
}