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
    [UnionCase("Case2", "(T, U)")]
    [UnionCase("Case3", "ValueTuple<Dictionary<T, List<U>>, (T, U)>")]

    partial class ContainsTuple<T, U> where T : notnull
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

        Assert.True(ContainsTuple<bool, long>.Case2(true, 100).IfCase2Else((b, l) =>
        {
            return b && l == 100;
        }, false));

        var dict = new Dictionary<bool, List<long>>
        {
            [true] = [1],
            [false] = [2]
        };

        Assert.True(ContainsTuple<bool, long>.Case3(dict, (false, 5)).IfCase3Else((d, t) =>
        {
            return d.Count == 2 && !t.Item1 && t.Item2 == 5;
        }, () => false));
    }

    [Fact]
    public async Task TupleIfAsync()
    {
        await ContainsTuple<bool, long>.Case0(1, 5.0).IfCase0((i, d) =>
        {
            Assert.Equal(1, i);
            Assert.Equal(5.0, d);

            return Task.CompletedTask;
        });

        await ContainsTuple<bool, long>.Case1(1, 5.0).IfCase1Else((i, d) =>
        {
            Assert.Equal(1, i);
            Assert.Equal(5.0, d);

            return Task.CompletedTask;
        }, () =>
        {
            Assert.True(false);

            return Task.CompletedTask;
        });

        Assert.True(await ContainsTuple<bool, long>.Case2(true, 100).IfCase2Else((b, l) =>
        {
            return Task.FromResult(b && l == 100);
        }, false));

        var dict = new Dictionary<bool, List<long>>
        {
            [true] = [1],
            [false] = [2]
        };

        Assert.True(await ContainsTuple<bool, long>.Case3(dict, (false, 5)).IfCase3Else((d, t) =>
        {
            return Task.FromResult(d[true].Single() == 1 && d[false].Single() == 2 && !t.Item1 && t.Item2 == 5);
        }, () => Task.FromResult(false)));
    }
}