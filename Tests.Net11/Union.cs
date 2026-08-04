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

    [UnionCase("Int", typeof(int?))]
    [UnionCase("Bool", typeof(bool?))]
    partial class NullableValueTypes
    {
         
    }

    [UnionCase("Some", "T")]
    [UnionCase("None")]
    partial class Optional<T>
    {

    }

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(string))]
    [UnionCase("Case2", typeof(int))]
    partial class RepeatedTypes
    {

    }

    [UnionCase("Case0", typeof(int[]))]
    [UnionCase("Case1")]
    [UnionCase("Case2")]
    partial class EmptyCases1
    {

    }

    [UnionCase("Case0")]
    [UnionCase("Case1")]
    [UnionCase("Case2", typeof(int[]))]
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
        Assert.Equal(new None(), Optional<float>.None.Value);
        Assert.Equal(new Case1(), EmptyCases1.Case1.Value);
        Assert.Equal(new Case2(), EmptyCases1.Case2.Value);
        Assert.Equal(new Case0(), EmptyCases2.Case0.Value);
        Assert.Equal(new Case1(), EmptyCases2.Case1.Value);
    }

    [Fact]
    public void HasValue()
    {
        Assert.True(IntOrStringOrOther<bool>.Int(5).HasValue);
        Assert.True(IntOrStringOrOther<bool>.String("abc").HasValue);
        Assert.True(IntOrStringOrOther<bool>.Other(true).HasValue);
        Assert.True(IntOrStringOrOther<int>.Other(4).HasValue);
        Assert.True(IntOrStringOrOther<int?>.Other(3).HasValue);

        Assert.False(IntOrStringOrOther<int?>.Other(null).HasValue);
        Assert.False(IntOrStringOrOther<int>.String(null!).HasValue);

        Assert.True(NullableValueTypes.Int(1).HasValue);
        Assert.True(NullableValueTypes.Bool(false).HasValue);
        Assert.False(NullableValueTypes.Int(null).HasValue);
        Assert.False(NullableValueTypes.Bool(null).HasValue);

        Assert.True(Optional<int>.Some(1).HasValue);
        Assert.True(Optional<int?>.Some(1).HasValue);
        Assert.True(Optional<string>.Some("abc").HasValue);

        Assert.False(Optional<int?>.Some(null).HasValue);
        Assert.False(Optional<string>.Some(null!).HasValue);

        Assert.True(Optional<int>.None.HasValue);
        Assert.True(EmptyCases1.Case1.HasValue);
        Assert.True(EmptyCases1.Case2.HasValue);
        Assert.True(EmptyCases2.Case0.HasValue);
        Assert.True(EmptyCases2.Case1.HasValue);
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

        Assert.True(NullableValueTypes.Int(null) switch
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
    public void RepeatedTypesSwitch()
    {
        Assert.True(RepeatedTypes.Case0("abc") switch
        {
            string s => s == "abc",
            int i => false,
            null => false,
        });

        Assert.True(RepeatedTypes.Case0(null!) switch
        {
            string s => false,
            int i => false,
            null => true,
        });

        Assert.True(RepeatedTypes.Case1("abc") switch
        {
            string s => s == "abc",
            int i => false,
            null => false,
        });

        Assert.True(RepeatedTypes.Case1(null!) switch
        {
            string s => false,
            int i => false,
            null => true,
        });

        Assert.True(RepeatedTypes.Case2(4) switch
        {
            string s => false,
            int i => i == 4,
        });
    }

    [Fact]
    public void RepeatedTypesAmbiguousCaseException()
    {
        var ex = Assert.Throws<AmbiguousCaseException>(() =>
        {
            RepeatedTypes x = "abc";
        });

        Assert.Equal(typeof(RepeatedTypes), ex.UnionType);
        Assert.Equal(typeof(string), ex.CaseType);
        Assert.Equal(["Case0", "Case1"], ex.CandidateCaseNames);
    }
}
