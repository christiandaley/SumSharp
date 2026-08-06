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

    public partial class  OuterGeneric<T>
    {
        public partial class InnerGeneric<U, V>
        {
            [UnionCase("Case0", "T")]
            [UnionCase("Case1", "U[]")]
            [UnionCase("Case2", "Dictionary<V, X>")]
            [UnionCase("Case3", "W[]")]
            [UnionCase("Case4", "X")]
            public partial struct ComplexGeneric<W, X>
            {

            }
        }
    }

    [Fact]
    public void Value()
    {
        Assert.Equal(new Int(5), ((IntOrStringOrOther<bool>.IUnionMembers)IntOrStringOrOther<bool>.Int(5)).Value);
        Assert.Equal(new String("abc"), ((IntOrStringOrOther<bool>.IUnionMembers)IntOrStringOrOther<bool>.String("abc")).Value);
        Assert.Equal(new Other<bool>(true), ((IntOrStringOrOther<bool>.IUnionMembers)IntOrStringOrOther<bool>.Other(true)).Value);
        Assert.Equal(new Other<int>(4), ((IntOrStringOrOther<int>.IUnionMembers)IntOrStringOrOther<int>.Other(4)).Value);
        Assert.Equal(new None(), ((Option<float>.IUnionMembers)Option<float>.None).Value);
        Assert.Equal(new EmptyCase1(), ((EmptyCases1.IUnionMembers)EmptyCases1.EmptyCase1).Value);
        Assert.Equal(new EmptyCase2(), ((EmptyCases1.IUnionMembers)EmptyCases1.EmptyCase2).Value);
        Assert.Equal(new EmptyCase0(), ((EmptyCases2.IUnionMembers)EmptyCases2.EmptyCase0).Value);
        Assert.Equal(new EmptyCase1(), ((EmptyCases2.IUnionMembers)EmptyCases2.EmptyCase1).Value);
    }

    [Fact]
    public void HasValue()
    {
        Assert.True(((IntOrStringOrOther<bool>.IUnionMembers)IntOrStringOrOther<bool>.Int(5)).HasValue);
        Assert.True(((IntOrStringOrOther<int?>.IUnionMembers)IntOrStringOrOther<int?>.Other(null)).HasValue);

        Assert.True(((Option<int>.IUnionMembers)Option<int>.Some(1)).HasValue);
        Assert.True(((Option<int>.IUnionMembers)Option<int>.None).HasValue);

        Assert.True(((EmptyCases1.IUnionMembers)EmptyCases1.EmptyCase1).HasValue);
        Assert.True(((EmptyCases1.IUnionMembers)EmptyCases1.EmptyCase2).HasValue);
        Assert.True(((EmptyCases2.IUnionMembers)EmptyCases2.EmptyCase0).HasValue);
        Assert.True(((EmptyCases2.IUnionMembers)EmptyCases2.EmptyCase1).HasValue);
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
            String(null) => true,
            String => false,
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

    [Fact]
    public void ComplexGeneric()
    {
        Assert.True(OuterGeneric<string>.InnerGeneric<float[], byte>.ComplexGeneric<double, List<int>>.Case0("abc") switch
        {
            OuterGeneric<string>.InnerGeneric<float[], byte>.Case0("abc") => true,
            _ => false
        });

        Assert.True(OuterGeneric<string>.InnerGeneric<float[], byte>.ComplexGeneric<double, List<int>>.Case1([[1.0f], [2.0f, 3.0f]]) switch
        {
            OuterGeneric<string>.InnerGeneric<float[], byte>.Case1([[1.0f], [2.0f, 3.0f]]) => true,
            _ => false
        });

        Assert.True(OuterGeneric<string>.InnerGeneric<float[], byte>.ComplexGeneric<double, List<int>>.Case2(new() { [4] = [1, 2] }) switch
        {
            OuterGeneric<string>.InnerGeneric<float[], byte>.Case2(var dict) => dict[4] is [1, 2],
            _ => false
        });

        Assert.True(OuterGeneric<string>.InnerGeneric<float[], byte>.ComplexGeneric<double, List<int>>.Case3([5.5, 5.6]) switch
        {
            OuterGeneric<string>.InnerGeneric<float[], byte>.Case3<double>([5.5, 5.6]) => true,
            _ => false
        });

        Assert.True(OuterGeneric<string>.InnerGeneric<float[], byte>.ComplexGeneric<double, List<int>>.Case4([1, 2, 3]) switch
        {
            OuterGeneric<string>.InnerGeneric<float[], byte>.Case4<List<int>>([1, 2, 3]) => true,
            _ => false
        });
    }
}
