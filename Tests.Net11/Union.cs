using SumSharp;

namespace Tests.Net11;

public partial class Union
{
    [UnionCase("Int", typeof(int))]
    [UnionCase("Float", typeof(float))]
    [UnionCase("Other", "T")]

    partial class IntOrStringOrOther<T>
    {

    }


    [Fact]
    public void SimpleSwitch()
    {
        IntOrStringOrOther<bool> w = 5;

        Assert.True(w switch
        {
            int i => true,
            float f => false,
            bool b => false,
        });

        IntOrStringOrOther<bool> x = 3.4f;

        Assert.True(x switch
        {
            int i => false,
            float f => true,
            bool b => false,
        });

        IntOrStringOrOther<bool> y = true;

        Assert.True(y switch
        {
            int i => false,
            float f => false,
            bool b => true,
        });

        var z = IntOrStringOrOther<int>.Other(4);

        Assert.True(z switch
        {
            int i => true,
            float f => false,
        });
    }
}
