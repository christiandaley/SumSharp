namespace Tests;

using Dotsum;
using OneOf;

public partial class OneOfConversions
{

    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(string))]
    [Case("Case2", typeof(double))]
    partial class IntOrStringOrDouble
    {

    }

    [Fact]
    public void FromOneOf()
    {
        OneOf<int, string, double> value1 = 5;

        OneOf<int, string, double> value2 = "abc";

        OneOf<int, string, double> value3 = 9.999;

        IntOrStringOrDouble convertedValue1 = value1;

        IntOrStringOrDouble convertedValue2 = value2;

        IntOrStringOrDouble convertedValue3 = value3;

        Assert.Equal(IntOrStringOrDouble.Case0(5), convertedValue1);

        Assert.Equal(IntOrStringOrDouble.Case1("abc"), convertedValue2);

        Assert.Equal(IntOrStringOrDouble.Case2(9.999), convertedValue3);
    }
}