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

    /*[UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(string))]
    partial class RepeatedCases
    {

    }*/

    [Fact]
    public void Value()
    {
        IntOrStringOrOther<bool> w = 5;

        Assert.Equal(5, IntOrStringOrOther<bool>.Int(5).Value);
        Assert.Equal("abc", IntOrStringOrOther<bool>.String("abc").Value);
        Assert.Equal(true, IntOrStringOrOther<bool>.Other(true).Value);
        Assert.Equal(4, IntOrStringOrOther<int>.Other(4).Value);
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
