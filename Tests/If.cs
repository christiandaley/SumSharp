namespace Tests;

using SumSharp;

public partial class If
{

    [Case("Case0", typeof(double))]
    [Case("Case1")]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class DoubleOrNone
    {

    }

    [Fact]
    public void IfCase0()
    {
        var passed = false;

        DoubleOrNone.Case0(2.5).IfCase0(value => passed = value == 2.5);

        Assert.True(passed);
    }

    [Fact]
    public async Task IfCase0Async()
    {
        var passed = false;

        await DoubleOrNone.Case0(2.5).IfCase0(value =>
        {
            passed = value == 2.5;

            return Task.CompletedTask;
        });

        Assert.True(passed);
    }

    [Fact]
    public void IfCase0Else()
    {
        bool passed = false;
        DoubleOrNone.Case0(2.5).IfCase0Else(value =>
        {
            passed = value == 2.5;
        }, () => passed = false);
        Assert.True(passed);

        Assert.Equal(2.5, DoubleOrNone.Case0(2.5).IfCase0Else(value => value, 1.0));
        Assert.Equal(3.5, DoubleOrNone.Case0(3.5).IfCase0Else(value => value, static () => 1.0));

        passed = false;
        DoubleOrNone.Case1.IfCase0Else(value =>
        {
            passed = false;
        }, () => passed = true);
        Assert.True(passed);

        Assert.Equal(1.0, DoubleOrNone.Case1.IfCase0Else(value => value, 1.0));
        Assert.Equal(1.0, DoubleOrNone.Case1.IfCase0Else(value => value, static () => 1.0));
    }

    [Fact]
    public async Task IfCase0ElseAsync()
    {
        bool passed = false;
        await DoubleOrNone.Case0(2.5).IfCase0Else(value =>
        {
            passed = value == 2.5;

            return Task.CompletedTask;
        }, () =>
        {
            passed = false;

            return Task.CompletedTask;
        });
        Assert.True(passed);

        Assert.Equal("2.5", await DoubleOrNone.Case0(2.5).IfCase0Else(value => Task.FromResult(value.ToString()), "0"));
        Assert.Equal("3.5", await DoubleOrNone.Case0(3.5).IfCase0Else(value => Task.FromResult(value.ToString()), static () => Task.FromResult("1")));

        passed = false;
        await DoubleOrNone.Case1.IfCase0Else(value =>
        {
            passed = false;

            return Task.CompletedTask;
        }, () =>
        {
            passed = true;

            return Task.CompletedTask;
        });
        Assert.True(passed);

        Assert.Equal("0", await DoubleOrNone.Case1.IfCase0Else(value => Task.FromResult(value.ToString()), "0"));
        Assert.Equal("1", await DoubleOrNone.Case1.IfCase0Else(value => Task.FromResult(value.ToString()), static () => Task.FromResult("1")));
    }
}