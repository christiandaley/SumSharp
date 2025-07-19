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
}