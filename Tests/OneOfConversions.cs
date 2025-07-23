namespace Tests;

using Dotsum;
using OneOf;

public partial class OneOfConversions
{

    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(string))]
    [Case("Case2", typeof(double))]
    [EnableOneOfConversions]
    partial class IntOrStringOrDouble
    {

    }

    [Fact]
    public void FromOneOf()
    {
        OneOf<int, string, double> value1 = 5;

        OneOf<int, string, double> value2 = "abc";

        OneOf<int, string, double> value3 = 9.999;

        Assert.Equal(IntOrStringOrDouble.Case0(5), (IntOrStringOrDouble)value1);

        Assert.Equal(IntOrStringOrDouble.Case1("abc"),(IntOrStringOrDouble)value2);

        Assert.Equal(IntOrStringOrDouble.Case2(9.999), (IntOrStringOrDouble)value3);
    }

    [Fact]
    public void ToOneOf()
    {
        IntOrStringOrDouble value1 = 5;

        IntOrStringOrDouble value2 = "abc";

        IntOrStringOrDouble value3 = 9.999;

        Assert.Equal((OneOf<int, string, double>)5, (OneOf<int, string, double>)value1);

        Assert.Equal((OneOf<int, string, double>)"abc", (OneOf<int, string, double>)value2);

        Assert.Equal((OneOf<int, string, double>)9.999, (OneOf<int, string, double>)value3);
    }
}