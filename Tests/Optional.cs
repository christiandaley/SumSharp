namespace Tests;

using Dotsum;

[Case("Some", "T")]
[Case("None")]
public partial class Optional<T>
{

}

public class OptionalTests
{

    [Fact]
    public void None()
    {
        var optional = Optional<int>.None;

        Assert.Equal(1, optional.Index);

        Assert.Equal(optional, Optional<int>.None);

        Assert.True(optional.IsNone);
        Assert.False(optional.IsSome);
    }

    [Fact]
    public void Some()
    {
        var optional = Optional<int>.Some(5);

        Assert.Equal(0, optional.Index);

        // value equality
        Assert.Equal(Optional<int>.Some(5), optional);

        // reference equality
        Assert.True(Optional<int>.Some(5) == optional);

        Assert.False(optional.IsNone);
        Assert.True(optional.IsSome);
    }

    [Fact]
    public void Switch()
    {
        var optional = Optional<int>.Some(5);

        bool passed = false;

        optional.Switch(
            _ => { passed = true; },
            () => { });

        Assert.True(passed);
    }

    [Fact]
    public void Match()
    {
        var optional = Optional<int>.Some(5);

        var value = optional.Match(
            i => i + 1,
            () => -1);

        Assert.Equal(6, value);
    }

    [Fact]
    public void If()
    {
        bool passed = false;

        Optional<int>.Some(5).IfSome(value => passed = value == 5);

        Assert.True(passed);

        Optional<int>.None.IfSome(value => passed = false);

        Assert.True(passed);

        passed = false;

        Optional<int>.None.IfNone(() => passed = true);

        Assert.True(passed);
    }

    [Fact]
    public void IfAsync()
    {
        
    }
}