namespace Tests;

using Dotsum;

[Case("Some", "T")]
[Case("None")]
public partial struct StructOptional<T>
{

}

[Case("Ok", "T")]
[Case("Error", "U")]
public partial struct ResultOptional<T, U>
{

}

public class StructTypeTests
{

    [Fact]
    public void ImplicitConversion()
    {
        StructOptional<string> o1 = "a";

        Assert.Equal(0, o1.Index);

        ResultOptional<int, string> o2 = "b";

        Assert.Equal(1, o2.Index);
    }

    
}