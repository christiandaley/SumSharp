namespace Tests;

using System.Reflection;

using Dotsum;

public partial class Storage
{
    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(bool))]
    [Case("Case2", typeof(byte[]))]
    partial struct DefaultStorage
    {

    }

    [Case("Case0", typeof(string), StorageMode.Inline)]
    [Case("Case1", typeof(bool))]
    [Case("Case2", typeof(byte[]))]
    [Case("Case3", typeof(int), StorageMode.AsObject)]
    [Storage(StorageStrategy.NoBoxing)]
    partial class VariousStorageModes
    {

    }


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
    [Case("Case14", typeof(InnerStruct))]
    [Case("Case15", typeof(ulong))]
    [Case("Case16", typeof(InnerStruct))]
    [Storage(StorageStrategy.NoBoxing)]
    partial class NoBoxing
    {
        public struct InnerStruct
        {
            public int X;
            public int Y;
        }
    }


    [Case("Case0", "T")]
    [Case("Case1", "U")]
    [Case("Case2", "V")]
    [Storage(StorageStrategy.NoBoxing)]
    partial class NoBoxingGeneric<T, U, V> where U : struct
    {

    }


    [Fact]
    public void DefaultStorageProperties()
    {
        Assert.Equal(typeof(object), typeof(DefaultStorage).GetField("_value0", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void VariousStorageModesProperties()
    {
        Assert.Equal(typeof(string), typeof(VariousStorageModes).GetField("_value0", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(ulong), typeof(VariousStorageModes).GetField("_value1", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(object), typeof(VariousStorageModes).GetField("_value2", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Null(typeof(VariousStorageModes).GetField("_value3", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void NoBoxingProperties()
    {
        Assert.Equal(typeof(ulong), typeof(NoBoxing).GetField("_value0", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(object), typeof(NoBoxing).GetField("_value1", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(NoBoxing.InnerStruct), typeof(NoBoxing).GetField("_value2", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Null(typeof(NoBoxing).GetField("_value3", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void NoBoxingAccess()
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

        Assert.Equal(new NoBoxing.InnerStruct { X = 2, Y = 3 }, NoBoxing.Case14(new NoBoxing.InnerStruct { X = 2, Y = 3 }).AsCase14);

        Assert.Equal(123456789ul, NoBoxing.Case15(123456789).AsCase15);

        Assert.Equal(new NoBoxing.InnerStruct { X = 100, Y = -200 }, NoBoxing.Case16(new NoBoxing.InnerStruct { X = 100, Y = -200 }).AsCase16);
    }

    [Fact]
    public void NoBoxingGenericProperties()
    {
        Assert.Equal(typeof(object), typeof(NoBoxingGeneric<double, float, int>).GetField("_value0", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(float), typeof(NoBoxingGeneric<double, float, int>).GetField("_value1", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }
}