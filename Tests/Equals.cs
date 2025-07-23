namespace Tests;

using Dotsum;

public partial class Equals
{

    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(double))]
    partial class StringOrDouble
    {

    }

    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(double))]
    [DisableValueEquality]
    partial class StringOrDoubleNoValueEquality
    {

    }

    [Fact]
    public void ValueEquality()
    {
        Assert.Equal(StringOrDouble.Case0("abc"), StringOrDouble.Case0("abc"));
        Assert.Equal(StringOrDouble.Case1(3.45), StringOrDouble.Case1(3.45));

        Assert.NotEqual(StringOrDouble.Case0("abc"), StringOrDouble.Case1(3.45));
        Assert.NotEqual(StringOrDouble.Case1(3.45), StringOrDouble.Case0("abc"));
    }

    [Fact]
    public void NoValueEquality()
    {
        Assert.NotEqual(StringOrDoubleNoValueEquality.Case0("abc"), StringOrDoubleNoValueEquality.Case0("abc"));
        Assert.NotEqual(StringOrDoubleNoValueEquality.Case1(3.45), StringOrDoubleNoValueEquality.Case1(3.45));

        Assert.NotEqual(StringOrDoubleNoValueEquality.Case0("abc"), StringOrDoubleNoValueEquality.Case1(3.45));
        Assert.NotEqual(StringOrDoubleNoValueEquality.Case1(3.45), StringOrDoubleNoValueEquality.Case0("abc"));
    }
}