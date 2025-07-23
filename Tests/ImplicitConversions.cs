namespace Tests;

using Dotsum;

public partial class Conversions
{

    [Case("Case0", typeof(double))]
    [Case("Case1")]
    partial class DoubleOrNone
    {

    }

    [Case("Case0", "T")]
    [Case("Case1", typeof(int))]
    [Case("Case2", typeof(string))]
    [Case("Case3", typeof(double))]
    [Case("Case4", typeof(int))]
    [Case("Case5")]
    partial class GenericType<T>
    {

    }

    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(IEnumerable<int>))]
    partial class IntOrIntEnumerable
    {

    }

    [Case("Case0", "T")]
    [Case("Case1")]
    partial class Wrapper<T>
    {

    }

    [Fact]
    public void DoubleOrNoneConversion()
    {
        DoubleOrNone d = 5.0;

        Assert.Equal(DoubleOrNone.Case0(5.0), d);
    }

    [Fact]
    public void GenericTypeConversion()
    {
        GenericType<byte> x1 = 1;
        Assert.Equal(GenericType<byte>.Case0(1), x1);

        GenericType<byte> x2 = "hello";
        Assert.Equal(GenericType<byte>.Case2("hello"), x2);

        GenericType<byte> x3 = 3.5;
        Assert.Equal(GenericType<byte>.Case3(3.5), x3);
    }

    [Fact]
    public void IntOrIntEnumerableConversion()
    {
        var x1 = (IntOrIntEnumerable)5;
        Assert.Equal(IntOrIntEnumerable.Case0(5), x1);
    }

    [Fact]
    public void InterfaceGenericArgumentConversion()
    {
        var d = new List<double> { 1.0, 2.0 };

        Wrapper<IEnumerable<double>> converted = d;

        Assert.True(converted.IsCase0);
        Assert.Equal([1.0, 2.0], converted.AsCase0);
    }
}