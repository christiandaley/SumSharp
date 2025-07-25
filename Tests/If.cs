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
        Assert.Equal(2.5, DoubleOrNone.Case0(2.5).IfCase0Else(value => value, 1.0));
        Assert.Equal(3.5, DoubleOrNone.Case0(3.5).IfCase0Else(value => value, static () => 1.0));
        Assert.Equal(6.5, DoubleOrNone.Case0(4.5).IfCase0Else((1.0, 2.0), static (ctx, value) => value + ctx.Item2, static ctx => ctx.Item1 + ctx.Item2));

        Assert.Equal(1.0, DoubleOrNone.Case1.IfCase0Else(value => value, 1.0));
        Assert.Equal(1.0, DoubleOrNone.Case1.IfCase0Else(value => value, static () => 1.0));
        Assert.Equal(3.0, DoubleOrNone.Case1.IfCase0Else((1.0, 2.0), static (ctx, value) => value + ctx.Item2, static ctx => ctx.Item1 + ctx.Item2));
    }
}