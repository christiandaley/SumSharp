namespace Tests;

using Dotsum;
using System.Runtime.InteropServices;

public partial class Boxing
{
    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(float))]
    [Case("Case2", typeof(double))]
    [Case("Case3", typeof(string))]
    [Case("Case4", typeof(int[]))]
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
    [DisableBoxing]
    partial struct NoBoxing
    {
        // The Index + all the unique types should take up 48 bytes.
    }

    [Fact]
    public void Size()
    {
        unsafe
        {
            Assert.Equal(48, sizeof(NoBoxing));
        }
    }

    [Fact]
    public void TestCaseAccess()
    {
        Assert.Equal(-10, NoBoxing.Case0(-10).AsCase0);

        Assert.Equal(1.5f, NoBoxing.Case1(1.5f).AsCase1);

        Assert.Equal(2.4, NoBoxing.Case2(2.4).AsCase2);

        Assert.Equal("abc", NoBoxing.Case3("abc").AsCase3);

        Assert.Equal([0, 1, 2], NoBoxing.Case4([0, 1, 2]).AsCase4);

        Assert.Equal(8, NoBoxing.Case5(8).AsCase5);

        Assert.Equal(3.9f, NoBoxing.Case6(3.9f).AsCase6);

        Assert.Equal(200, NoBoxing.Case7(200).AsCase7);

        Assert.Equal(200.1, NoBoxing.Case8(200.1).AsCase8);

        Assert.Equal(8000, NoBoxing.Case9(8000).AsCase9);

        Assert.Equal(1600, NoBoxing.Case10(1600).AsCase10);

        Assert.Equal(1900, NoBoxing.Case11(1900).AsCase11);

        Assert.Equal(256, NoBoxing.Case12(256).AsCase12);

        Assert.Equal(-128, NoBoxing.Case13(-128).AsCase13);

        Assert.Equal(123456789ul, NoBoxing.Case14(123456789).AsCase14);
    }
}