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

    [UnionCase("NullableInt", typeof(int?))]
    [UnionCase("Bool", typeof(bool?))]
    partial class NullableValueTypes
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
        Assert.Equal(5, IntOrStringOrOther<bool>.Int(5).Value);
        Assert.Equal("abc", IntOrStringOrOther<bool>.String("abc").Value);
        Assert.Equal(true, IntOrStringOrOther<bool>.Other(true).Value);
        Assert.Equal(4, IntOrStringOrOther<int>.Other(4).Value);
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

        Assert.True(NullableValueTypes.NullableInt(1).HasValue);
        Assert.True(NullableValueTypes.Bool(false).HasValue);
        Assert.True(NullableValueTypes.NullableInt(null).HasValue);
        Assert.True(NullableValueTypes.Bool(null).HasValue);

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
            int i => i == 5,
            string s => false,
            bool b => false,
        });

        Assert.True(IntOrStringOrOther<bool>.String("abc") switch
        {
            int i => false,
            string s => s == "abc",
            bool b => false,
        });

        Assert.True(IntOrStringOrOther<bool>.String(null!) switch
        {
            int i => false,
            string s => false,
            bool b => false,
            null => true,
        });

        Assert.True(IntOrStringOrOther<bool>.Other(true) switch
        {
            int i => false,
            string s => false,
            bool b => b,
        });

        Assert.True(IntOrStringOrOther<int>.Other(4) switch
        {
            int i => i == 4,
            string s => false,
        });

        Assert.True(IntOrStringOrOther<int?>.Other(null) switch
        {
            int i => false,
            string s => false,
            null => true,
        });

        Assert.True(IntOrStringOrOther<float[]>.Other(null!) switch
        {
            int i => false,
            string s => false,
            float[] => false,
            null => true,
        });

        Assert.True(NullableValueTypes.NullableInt(null) switch
        {
            int i => false,
            bool b => false,
            null => true,
        });

        Assert.True(NullableValueTypes.Bool(null) switch
        {
            int i => false,
            bool b => false,
            null => true,
        });
    }

    [Fact]
    public void EmptyTypesSwitch()
    {
        Assert.True(Option<string>.Some("abc") switch
        {
            string s => true,
            None => false,
        });

        Assert.True(Option<string>.None switch
        {
            string s => false,
            None => true,
        });

        Assert.True(EmptyCases1.IntArray([0]) switch
        {
            int[] ints => ints.Single() == 0,
            EmptyCase1 => false,
            EmptyCase2 => false,
        });

        Assert.True(EmptyCases1.EmptyCase1 switch
        {
            int[] ints => false,
            EmptyCase1 => true,
            EmptyCase2 => false,
        });

        Assert.True(EmptyCases1.EmptyCase2 switch
        {
            int[] ints => false,
            EmptyCase1 => false,
            EmptyCase2 => true,
        });

        Assert.True(EmptyCases2.EmptyCase0 switch
        {
            EmptyCase0 => true,
            EmptyCase1 => false,
            float[] floats => false,
        });

        Assert.True(EmptyCases2.EmptyCase1 switch
        {
            EmptyCase0 => false,
            EmptyCase1 => true,
            float[] floats => false,
        });

        Assert.True(EmptyCases2.FloatArray([1.1f]) switch
        {
            EmptyCase0 => false,
            EmptyCase1 => false,
            float[] floats => floats.Single() == 1.1f,
        });
    }
}
