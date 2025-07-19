namespace Tests;

using Dotsum;
using Xunit.Abstractions;

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

        Assert.Equal(optional, IntOptional.None);

        Assert.Equal(1, optional.Index);
    }
}