using SumSharp;

namespace Tests.Net11;

public partial class Union
{
    [UnionCase("Int", typeof(int))]
    [UnionCase("String", typeof(string))]
    [UnionCase("Other", "T")]

    partial class IntOrStringOrOther<T>
    {

    }

    [UnionCase("Some", "T")]
    [UnionCase("None")]
    partial class Option<T>
    {

    }

    [UnionCase("IntArray", typeof(int[]))]
    [UnionCase("EmptyCase1")]
    [UnionCase("EmptyCase2")]
    partial class EmptyCases1
    {

    }

    [UnionCase("EmptyCase0")]
    [UnionCase("EmptyCase1")]
    [UnionCase("FloatArray", typeof(float[]))]
    partial class EmptyCases2
    {

    }

    [Fact]
    public void Value()
    {
        Assert.Equal(new Int(5), IntOrStringOrOther<bool>.Int(5).Value);
        Assert.Equal(new String("abc"), IntOrStringOrOther<bool>.String("abc").Value);
        Assert.Equal(new Other<bool>(true), IntOrStringOrOther<bool>.Other(true).Value);
        Assert.Equal(new Other<int>(4), IntOrStringOrOther<int>.Other(4).Value);
        Assert.Equal(new None(), Option<float>.None.Value);
        Assert.Equal(new EmptyCase1(), EmptyCases1.EmptyCase1.Value);
        Assert.Equal(new EmptyCase2(), EmptyCases1.EmptyCase2.Value);
        Assert.Equal(new EmptyCase0(), EmptyCases2.EmptyCase0.Value);
        Assert.Equal(new EmptyCase1(), EmptyCases2.EmptyCase1.Value);
    }

    [Fact]
    public void HasValue()
    {
        Assert.True(IntOrStringOrOther<bool>.Int(5).HasValue);
        Assert.True(IntOrStringOrOther<bool>.String("abc").HasValue);
        Assert.True(IntOrStringOrOther<bool>.Other(true).HasValue);
        Assert.True(IntOrStringOrOther<int>.Other(4).HasValue);
        Assert.True(IntOrStringOrOther<int?>.Other(3).HasValue);

        Assert.True(IntOrStringOrOther<int?>.Other(null).HasValue);
        Assert.True(IntOrStringOrOther<int>.String(null!).HasValue);

        Assert.True(Option<int>.Some(1).HasValue);
        Assert.True(Option<int?>.Some(1).HasValue);
        Assert.True(Option<string>.Some("abc").HasValue);

        Assert.True(Option<int?>.Some(null).HasValue);
        Assert.True(Option<string>.Some(null!).HasValue);

        Assert.True(Option<int>.None.HasValue);
        Assert.True(EmptyCases1.EmptyCase1.HasValue);
        Assert.True(EmptyCases1.EmptyCase2.HasValue);
        Assert.True(EmptyCases2.EmptyCase0.HasValue);
        Assert.True(EmptyCases2.EmptyCase1.HasValue);
    }


    [Fact]
    public void Switch()
    {
        Assert.True(IntOrStringOrOther<bool>.Int(5) switch
        {
            Int(var i) => i == 5,
            String(var s) => false,
            Other<bool>(var b) => false,
        });

        Assert.True(IntOrStringOrOther<bool>.String("abc") switch
        {
            Int(var i) => false,
            String(var s) => s == "abc",
            Other<bool>(var b) => false,
        });

        Assert.True(IntOrStringOrOther<bool>.String(null!) switch
        {
            Int(var i) => false,
            String(var s) => false,
            Other<bool>(var b) => false,
        });

        Assert.True(IntOrStringOrOther<bool>.Other(true) switch
        {
            Int(var i) => false,
            String(var s) => false,
            Other<bool>(var b) => b,
        });

        Assert.True(IntOrStringOrOther<int>.Other(4) switch
        {
            Int(var i) => false,
            String(var s) => false,
            Other<int>(var i) => i == 4,
        });

        Assert.True(IntOrStringOrOther<int?>.Other(null) switch
        {
            Int(var i) => false,
            String(var s) => false,
            Other<int?>(var i) => !i.HasValue,
        });

        Assert.True(IntOrStringOrOther<float[]>.Other(null!) switch
        {
            Int(var i) => false,
            String(var s) => false,
            Other<float[]>(var f) => f is null,
        });

        Assert.True(Option<string>.Some("abc") switch
        {
            Some<string>("abc") => true,
            Some<string> => false,
            None => false,
        });

        Assert.True(Option<string>.None switch
        {
            Some<string> => false,
            None => true,
        });

        Assert.True(EmptyCases1.IntArray([0]) switch
        {
            IntArray([0]) => true,
            IntArray => false,
            EmptyCase1 => false,
            EmptyCase2 => false,
        });

        Assert.True(EmptyCases2.EmptyCase1 switch
        {
            EmptyCase0 => false,
            EmptyCase1 => true,
            FloatArray => false,
        });
    }
}
