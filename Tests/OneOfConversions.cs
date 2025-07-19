namespace Tests;

using SumSharp;
using Newtonsoft.Json.Linq;
using OneOf;
using OneOf.Types;

public partial class OneOfConversions
{

    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(string))]
    [Case("Case2", typeof(double))]
    [EnableOneOfConversions]
    partial class IntOrStringOrDouble
    {

    }

    [Case("Case0", typeof(IEnumerable<int>))]
    [Case("Case1", typeof(float))]
    [EnableOneOfConversions]
    partial class ContainsInterface
    {

    }

    [Case("Case0", "T")]
    [Case("Case1", typeof(float))]
    [EnableOneOfConversions]
    partial class Wrapper<T>
    {

    }

    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(string))]
    [Case("Case2")]
    [Case("Case3", typeof(double))]
    [Case("Case4")]
    [EnableOneOfConversions]
    partial class HasEmptyCases
    {

    }
    struct CustomEmpty
    {

    }

    [Case("Case0", typeof(int))]
    [Case("Case1")]
    [EnableOneOfConversions(typeof(CustomEmpty))]
    partial class HasCustomEmptyCases
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

    [Fact]
    public void Interfaces()
    {
        OneOf<IEnumerable<int>, float> value1 = new int[] { 1, 2 };

        ContainsInterface convertedValue1 = value1;

        Assert.True(convertedValue1.IsCase0);
        Assert.Equal([1, 2], convertedValue1.AsCase0);

        Assert.Equal(value1, (OneOf<IEnumerable<int>, float>)convertedValue1);
    }

    [Fact]
    public void Generic()
    {
        OneOf<IEnumerable<int>, float> value1 = new int[] { 1, 2 };

        Wrapper<IEnumerable<int>> convertedValue1 = value1;

        Assert.True(convertedValue1.IsCase0);
        Assert.Equal([1, 2], convertedValue1.AsCase0);

        Assert.Equal(value1, (OneOf<IEnumerable<int>, float>)convertedValue1);
    }

    [Fact]
    public void EmptyCases()
    {
        OneOf<int, string, None, double, None> value1 = 5;

        OneOf<int, string, None, double, None> value2 = "abc";

        var value3 = OneOf<int, string, None, double, None>.FromT4(new());

        Assert.Equal(HasEmptyCases.Case0(5), (HasEmptyCases)value1);
        Assert.Equal(value1, (OneOf<int, string, None, double, None>)HasEmptyCases.Case0(5));

        Assert.Equal(HasEmptyCases.Case1("abc"), (HasEmptyCases)value2);
        Assert.Equal(value2, (OneOf<int, string, None, double, None>)HasEmptyCases.Case1("abc"));

        Assert.Equal(HasEmptyCases.Case4, (HasEmptyCases)value3);
        Assert.Equal(value3, (OneOf<int, string, None, double, None>)HasEmptyCases.Case4);
    }

    [Fact]
    public void CustomEmptyCase()
    {
        OneOf<int, CustomEmpty> value1 = 5;
        OneOf<int, CustomEmpty> value2 = new CustomEmpty();

        Assert.Equal(HasCustomEmptyCases.Case0(5), (HasCustomEmptyCases)value1);
        Assert.Equal(value1, (OneOf<int, CustomEmpty>)HasCustomEmptyCases.Case0(5));

        Assert.Equal(HasCustomEmptyCases.Case1, (HasCustomEmptyCases)value2);
        Assert.Equal(value2, (OneOf<int, CustomEmpty>)HasCustomEmptyCases.Case1);
    }
}