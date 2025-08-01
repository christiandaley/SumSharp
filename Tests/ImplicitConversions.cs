namespace Tests;

using SumSharp;

public partial class Conversions
{

    [UnionCase("Case0", typeof(double))]
    [UnionCase("Case1")]
    partial class DoubleOrNone
    {

    }

    [UnionCase("Case0", "T")]
    [UnionCase("Case1", typeof(int))]
    [UnionCase("Case2", typeof(string))]
    [UnionCase("Case3", typeof(double))]
    [UnionCase("Case4", typeof(int))]
    [UnionCase("Case5")]
    partial class GenericType<T>
    {

    }

    [UnionCase("Case0", typeof(int))]
    [UnionCase("Case1", typeof(IEnumerable<int>))]
    partial class IntOrIntEnumerable
    {

    }

    [UnionCase("Case0", "T")]
    [UnionCase("Case1")]
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