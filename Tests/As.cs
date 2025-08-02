namespace Tests;

using SumSharp;

public partial class As
{

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1")]
    partial class StringOrNone
    {

    }

    [UnionCase("Case0", typeof(int))]
    [UnionCase("Case1")]
    partial class IntOrNone
    {

    }

    [UnionCase("Case0", typeof((int IntValue, double DoubleValue)))]
    [UnionCase("Case1", "ValueTuple<Dictionary<T, List<U>> DictValue, (T, U) GenericTupleValue>")]

    partial class ContainsTuple<T, U> where T : notnull
    {

    }


    [Fact]
    public void AsCase0()
    {
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0);
    }

    [Fact]
    public void AsCase0Throws()
    {
        Assert.Throws<InvalidOperationException>(() => StringOrNone.Case1.AsCase0);
    }

    [Fact]
    public async Task AsCase0Or()
    {
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0OrDefault);
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0Or("2"));
        Assert.Equal("3", StringOrNone.Case0("3").AsCase0Or(() => "1"));
        Assert.Equal("3", await StringOrNone.Case0("3").AsCase0Or(() => Task.FromResult("1")));

        Assert.Null(StringOrNone.Case1.AsCase0OrDefault);
        Assert.Equal("2", StringOrNone.Case1.AsCase0Or("2"));
        Assert.Equal("1", StringOrNone.Case1.AsCase0Or(() => "1"));
        Assert.Equal("0", await StringOrNone.Case1.AsCase0Or(() => Task.FromResult("0")));
    }

    [Fact]
    public void AsTupleWithNamedFields()
    {
        ContainsTuple<float, string> value1 = (1, 2.0);

        Assert.Equal(1, value1.AsCase0.IntValue);
        Assert.Equal(2.0, value1.AsCase0.DoubleValue);

        var value2 = ContainsTuple<float, string>.Case1(
            new Dictionary<float, List<string>>
            {
                [1.5f] = ["a"]
            }, 
            (2.5f, "b"));

        Assert.Equal("a", value2.AsCase1.DictValue[1.5f]);
        Assert.Equal((2.5f, "b"), value2.AsCase1.GenericTupleValue);
    }
}