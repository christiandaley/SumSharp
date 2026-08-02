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
        IntOrStringOrOther<bool> x = 5;

        Assert.True(x switch
        {
            int i => true,
            float f => false,
            bool b => false,
        });

        IntOrStringOrOther<bool> y = 3.4f;

        Assert.True(y switch
        {
            int i => false,
            float f => true,
            bool b => false,
        });

        IntOrStringOrOther<bool> z = true;

        Assert.True(z switch
        {
            int i => false,
            float f => false,
            bool b => true,
        });
    }
}
