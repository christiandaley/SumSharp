namespace Tests;

using SumSharp;

public partial class If
{

    [UnionCase("Case0", typeof(double))]
    [UnionCase("Case1")]
    [UnionStorage(UnionStorageStrategy.InlineValueTypes)]
    partial class DoubleOrNone
    {

    }

    [UnionCase("Case0", typeof((int, double)))]
    [UnionCase("Case1", typeof(ValueTuple<int, double>))]
    [UnionCase("Case2", typeof(Tuple<int, double>))]
    [UnionCase("Case3", "(T, U)")]
    [UnionCase("Case4", "ValueTuple<U, T>")]
    [UnionCase("Case5", "Tuple<T, U>")]

    partial class ContainsTuple<T, U>
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

    [Fact]
    public void TupleIf()
    {
        ContainsTuple<bool, long>.Case0(1, 5.0).IfCase0((i, d) =>
        {
            Assert.Equal(1, i);
            Assert.Equal(5.0, d);
        });

        ContainsTuple<bool, long>.Case1(1, 5.0).IfCase1Else((i, d) =>
        {
            Assert.Equal(1, i);
            Assert.Equal(5.0, d);
        }, () =>
        {
            Assert.True(false);
        });

        Assert.True(ContainsTuple<bool, long>.Case2(1, 5.0).IfCase2Else((i, d) =>
        {
            return i == 1 && d == 5.0;
        }, false));

        Assert.True(ContainsTuple<bool, long>.Case3(true, 100).IfCase3Else((b, l) =>
        {
            return b && l == 100;
        }, _ => false));

        ContainsTuple<bool, long>.Case4(100, true).IfCase4((l, b) =>
        {
            Assert.True(b);
            Assert.Equal(100, l);
        });

        ContainsTuple<bool, long>.Case5(true, 100).IfCase5((b, l) =>
        {
            Assert.True(b);
            Assert.Equal(100, l);
        });
    }
}