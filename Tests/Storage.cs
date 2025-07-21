namespace Tests;

using Dotsum;

public partial class Storage
{
    [Case("Case0", "T")]
    [Case("Case1", "T")]
    partial class OneType<T>
    {

    }

    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(float))]
    [Case("Case2", typeof(double))]
    [Case("Case3", typeof(string), StorageMode: StorageMode.AsObject)]
    [Case("Case4", typeof(int[]), StorageMode: StorageMode.AsObject)]
    [Case("Case5", typeof(int))]
    [Case("Case6", typeof(float))]
    [Case("Case7", typeof(long))]
    [Case("Case8", typeof(double))]
    [Case("Case9", typeof(long))]
    [Case("Case10", typeof(ushort))]
    [Case("Case11", typeof(short))]
    [Case("Case12", typeof(ushort))]
    [Case("Case13", typeof(short))]
    [Case("Case14", typeof(ulong))]
    [Storage(StorageMode.AsDeclaredType)]
    partial class AsDeclaredType
    {
        
    }

    [Fact]
    public void AsDeclaredTypeProperties()
    {
        Assert.Equal(typeof(int), typeof(AsDeclaredType).GetProperty("_value0")?.GetType());
        Assert.Equal(typeof(float), typeof(AsDeclaredType).GetProperty("_value1")?.GetType());
        Assert.Equal(typeof(double), typeof(AsDeclaredType).GetProperty("_value2")?.GetType());
        Assert.Equal(typeof(object), typeof(AsDeclaredType).GetProperty("_value3")?.GetType());
        Assert.Equal(typeof(long), typeof(AsDeclaredType).GetProperty("_value4")?.GetType());
        Assert.Equal(typeof(ushort), typeof(AsDeclaredType).GetProperty("_value5")?.GetType());
        Assert.Equal(typeof(short), typeof(AsDeclaredType).GetProperty("_value6")?.GetType());
    }

    [Fact]
    public void AsDeclaredTypeAccess()
    {
        Assert.Equal(-10, AsDeclaredType.Case0(-10).AsCase0);

        Assert.Equal(1.5f, AsDeclaredType.Case1(1.5f).AsCase1);

        Assert.Equal(2.4, AsDeclaredType.Case2(2.4).AsCase2);

        Assert.Equal("abc", AsDeclaredType.Case3("abc").AsCase3);

        Assert.Equal([0, 1, 2], AsDeclaredType.Case4([0, 1, 2]).AsCase4);

        Assert.Equal(8, AsDeclaredType.Case5(8).AsCase5);

        Assert.Equal(3.9f, AsDeclaredType.Case6(3.9f).AsCase6);

        Assert.Equal(200, AsDeclaredType.Case7(200).AsCase7);

        Assert.Equal(200.1, AsDeclaredType.Case8(200.1).AsCase8);

        Assert.Equal(8000, AsDeclaredType.Case9(8000).AsCase9);

        Assert.Equal(1600, AsDeclaredType.Case10(1600).AsCase10);

        Assert.Equal(1900, AsDeclaredType.Case11(1900).AsCase11);

        Assert.Equal(256, AsDeclaredType.Case12(256).AsCase12);

        Assert.Equal(-128, AsDeclaredType.Case13(-128).AsCase13);

        Assert.Equal(123456789ul, AsDeclaredType.Case14(123456789).AsCase14);
    }
}