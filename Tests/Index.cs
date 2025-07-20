namespace Tests;

using Dotsum;

public partial class Index
{

    [Case("Case0", typeof(int))]
    [Case("Case1")]
    partial class IntOrNone
    {

    }

    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(string))]
    partial class IntOrString
    {

    }

    [Fact]
    public void StandardClasses()
    {
        Assert.Equal(0, IntOrNone.Case0(4).Index);
        Assert.Equal(1, IntOrNone.Case1.Index);

        Assert.Equal(0, IntOrString.Case0(2).Index);
        Assert.Equal(1, IntOrString.Case1("a").Index);
    }
}