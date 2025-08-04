namespace Tests;

using SumSharp;

public partial class Switch
{

    [UnionCase("Case0", typeof(int))]
    [UnionCase("Case1")]
    partial class IntOrNone
    {

    }

    [UnionCase("Case0", typeof(ValueTuple<bool, byte>))]
    [UnionCase("Case1", "(T, T)")]

    partial class ContainsTuple<T>
    {

    }

    [UnionCase("Ok", "T")]
    [UnionCase("Error", "E")]
    partial class Result<T, E>
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

    [Fact]
    public void TupleSwitch()
    {
        var passed = false;

        ContainsTuple<string>.Case0(true, 1).Switch(
            (b, i) => passed = b && i == 1,
            (_, _) => { });

        Assert.True(passed);
    }


    [Fact]
    public async Task TupleSwitchAsync()
    {
        var passed = false;

        await ContainsTuple<string>.Case1("a", "b").Switch(
            (_, _) => Task.CompletedTask,
            (s1, s2) => 
            {
                passed = s1 == "a" && s2 == "b";

                return Task.CompletedTask;
            });

        Assert.True(passed);
    }

    [Fact]
    public void NamedSwitchNoDefault()
    {
        bool passed = false;

        Result<string, Exception>.Ok("abc").Switch(
            Ok: str => passed = str == "abc",
            Error: _ => { });

        Assert.True(passed);
    }

    [Fact]
    public async Task NamedSwitchNoDefaultAsync()
    {
        bool passed = false;

        await Result<string, Exception>.Ok("abc").Switch(
            Ok: str => 
            {
                passed = str == "abc";

                return Task.CompletedTask;
            },
            Error: _ => Task.CompletedTask);

        Assert.True(passed);
    }

    [Fact]
    public void NamedSwitchWithDefault()
    {
        bool passed = false;

        Result<string, Exception>.Error(new InvalidOperationException()).Switch(
            Ok: str => { },
            _: () => passed = true);

        Assert.True(passed);
    }

    [Fact]
    public async Task NamedSwitchWithDefaultAsync()
    {
        bool passed = false;

        await Result<string, Exception>.Error(new InvalidOperationException()).Switch(
            Ok: str => Task.CompletedTask,
            _: () =>
            {
                passed = true;
                return Task.CompletedTask;
            });

        Assert.True(passed);
    }

    [Fact]
    public void UnhandledCaseException()
    {
        var err = Assert.Throws<MatchFailureException>(() =>
        {
            Result<string, Exception>.Ok("abc").Switch(
                Error: _ => { });
        });

        Assert.Equal("Ok", err.CaseName);
    }

    [Fact]
    public async Task UnhandledCaseExceptionAsync()
    {
        var err = await Assert.ThrowsAsync<MatchFailureException>(async () =>
        {
            await Result<string, Exception>.Ok("abc").Switch(
                Ok: _ => Task.CompletedTask);
        });

        Assert.Equal("Ok", err.CaseName);
    }
}