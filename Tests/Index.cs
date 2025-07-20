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

    [Case("Case0", typeof(int))]
    [Case("Case1")]
    partial struct IntOrNoneStruct
    {

    }

    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(string))]
    partial struct IntOrStringStruct
    {

    }

    [Fact]
    public void Classes()
    {
        Assert.Equal(0, IntOrNone.Case0(4).Index);
        Assert.Equal(1, IntOrNone.Case1.Index);

        Assert.Equal(0, IntOrString.Case0(2).Index);
        Assert.Equal(1, IntOrString.Case1("a").Index);
    }

    [Fact]
    public void Structs()
    {
        Assert.Equal(0, IntOrNoneStruct.Case0(4).Index);
        Assert.Equal(1, IntOrNoneStruct.Case1.Index);

        Assert.Equal(0, IntOrStringStruct.Case0(2).Index);
        Assert.Equal(1, IntOrStringStruct.Case1("a").Index);
    }
}