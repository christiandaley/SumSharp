namespace Tests;

using Dotsum;

[Case("Some", typeof(int))]
[Case("None")]
public partial class IntOptional
{

}

public class IntOptionalTests
{

    [Fact]
    public void None()
    {
        var optional = IntOptional.None;

        Assert.Equal(1, optional.Index);

        Assert.Equal(optional, IntOptional.None);
    }

    [Fact]
    public void Some()
    {
        var optional = IntOptional.Some(5);

        Assert.Equal(0, optional.Index);

        // value equality
        Assert.Equal(IntOptional.Some(5), optional);

        // reference equality
        Assert.False(IntOptional.Some(5) == optional);
    }
}