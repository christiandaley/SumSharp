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
    partial class Optional<T>
    {

    }

    /*[UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(string))]
    partial class RepeatedCases
    {

    }*/

    [Fact]
    public void Value()
    {
        Assert.Equal(5, IntOrStringOrOther<bool>.Int(5).Value);
        Assert.Equal("abc", IntOrStringOrOther<bool>.String("abc").Value);
        Assert.Equal(true, IntOrStringOrOther<bool>.Other(true).Value);
        Assert.Equal(4, IntOrStringOrOther<int>.Other(4).Value);
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

        Assert.True(Optional<int>.Some(1).HasValue);
        Assert.True(Optional<int?>.Some(1).HasValue);
        Assert.True(Optional<string>.Some("abc").HasValue);

        Assert.False(Optional<int?>.Some(null).HasValue);
        Assert.False(Optional<string>.Some(null!).HasValue);

        Assert.True(Optional<int>.None.HasValue);
    }


    [Fact]
    public void SimpleSwitch()
    {
        IntOrStringOrOther<bool> w = 5;

        Assert.True(w switch
        {
            int i => i == 5,
            string s => false,
            bool b => false,
        });

        IntOrStringOrOther<bool> x = "abc";

        Assert.True(x switch
        {
            int i => false,
            string s => s == "abc",
            bool b => false,
        });

        IntOrStringOrOther<bool> y = true;

        Assert.True(y switch
        {
            int i => false,
            string s => false,
            bool b => b,
        });

        var z = IntOrStringOrOther<int>.Other(4);

        Assert.True(z switch
        {
            int i => i == 4,
            string s => false,
        });
    }
}
