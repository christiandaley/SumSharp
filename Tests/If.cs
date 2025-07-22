namespace Tests;

using Dotsum;

public partial class If
{

    [Case("Case0", typeof(double))]
    [Case("Case1")]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class DoubleOrNone
    {

    }

    [Fact]
    public void Case0()
    {
        var passed = false;

        DoubleOrNone.Case0(2.5).IfCase0(value => passed = value == 2.5);
        DoubleOrNone.Case0(3.0).IfCase1(() => { passed = false; });

        Assert.True(passed);
    }

    [Fact]
    public async Task Case0Async()
    {
        var passed = false;

        await DoubleOrNone.Case0(2.5).IfCase0(value =>
        {
            passed = value == 2.5;

            return Task.CompletedTask;
        });

        await DoubleOrNone.Case0(3.0).IfCase1(() => 
        { 
            passed = false; 

            return Task.CompletedTask;
        });

        Assert.True(passed);
    }

    [Fact]
    public void Case1()
    {
        var passed = false;

        DoubleOrNone.Case1.IfCase1(() => { passed = true; });
        DoubleOrNone.Case1.IfCase0(_ => { passed = false; });

        Assert.True(passed);
    }

    [Fact]
    public async Task Case1Async()
    {
        var passed = false;

        await DoubleOrNone.Case1.IfCase1(() =>
        {
            passed = true;

            return Task.CompletedTask;
        });

        await DoubleOrNone.Case1.IfCase0(value =>
        {
            passed = false;

            return Task.CompletedTask;
        });

        Assert.True(passed);
    }
}