namespace Tests;

using Dotsum;

[Case("A", typeof(string))]
[Case("B", typeof(int))]
[Case("C", typeof(string))]
[Case("D", typeof(double))]
[Case("E", typeof(int))]
public partial class RepeatedTypes
{

}

public class RepeatedTypesTests
{

    [Fact]
    public void ImplicitConversion()
    {
        RepeatedTypes d = 5.0;

        Assert.Equal(3, d.Index);
    }

    
}