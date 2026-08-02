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
    public void Test1()
    {

    }
}
