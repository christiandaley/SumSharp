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

    [Case("Case0", typeof(double))]
    [Case("Case1", typeof(double))]
    partial class SingleUniqueType
    {

    }

    [Case("Case0", typeof(string), StorageMode.Inline)]
    [Case("Case1", typeof(bool))]
    [Case("Case2", typeof(byte[]))]
    [Case("Case3", typeof(int), StorageMode.AsObject)]
    [Case("Case4", typeof(float))]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class VariousStorageModes
    {

    }


    [Case("Case0", typeof(int))]
    [Case("Case1", typeof(float))]
    [Case("Case2", typeof(double))]
    [Case("Case3", typeof(string))]
    [Case("Case4", typeof(int[]))]
    [Case("Case5", typeof(int))]
    [Case("Case6", typeof(Single))]
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
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class InlineValueTypes
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
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class InlineValueTypesGeneric<T, U, V> where U : struct
    {

    }


    [Fact]
    public void DefaultStorageProperties()
    {
        Assert.Equal(typeof(object), typeof(DefaultStorage).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void SingleUniqueTypeProperties()
    {
        Assert.NotNull(typeof(SingleUniqueType).GetField("_primitiveStorage", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void VariousStorageModesProperties()
    {
        Assert.Equal(typeof(string), typeof(VariousStorageModes).GetField("_string", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(Dotsum.Internal.PrimitiveStorage), typeof(VariousStorageModes).GetField("_primitiveStorage", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(object), typeof(VariousStorageModes).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void InlineValueTypesProperties()
    {
        Assert.Equal(typeof(InlineValueTypes.InnerStruct), typeof(InlineValueTypes).GetField("_Tests_Storage_InlineValueTypes_InnerStruct", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(object), typeof(InlineValueTypes).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(Dotsum.Internal.PrimitiveStorage), typeof(InlineValueTypes).GetField("_primitiveStorage", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void InlineValueTypesAccess()
    {
        Assert.Equal(-10, InlineValueTypes.Case0(-10).AsCase0);

        Assert.Equal(1.5f, InlineValueTypes.Case1(1.5f).AsCase1);

        Assert.Equal(2.4, InlineValueTypes.Case2(2.4).AsCase2);

        Assert.Equal("abc", InlineValueTypes.Case3("abc").AsCase3);

        Assert.Equal([0, 1, 2], InlineValueTypes.Case4([0, 1, 2]).AsCase4);

        Assert.Equal(8, InlineValueTypes.Case5(8).AsCase5);

        Assert.Equal(3.9f, InlineValueTypes.Case6(3.9f).AsCase6);

        Assert.Equal(200, InlineValueTypes.Case7(200).AsCase7);

        Assert.Equal(200.1, InlineValueTypes.Case8(200.1).AsCase8);

        Assert.Equal(8000, InlineValueTypes.Case9(8000).AsCase9);

        Assert.Equal(1600, InlineValueTypes.Case10(1600).AsCase10);

        Assert.Equal(1900, InlineValueTypes.Case11(1900).AsCase11);

        Assert.Equal(256, InlineValueTypes.Case12(256).AsCase12);

        Assert.Equal(-128, InlineValueTypes.Case13(-128).AsCase13);

        Assert.Equal(new InlineValueTypes.InnerStruct { X = 2, Y = 3 }, InlineValueTypes.Case14(new InlineValueTypes.InnerStruct { X = 2, Y = 3 }).AsCase14);

        Assert.Equal(123456789ul, InlineValueTypes.Case15(123456789).AsCase15);

        Assert.Equal(new InlineValueTypes.InnerStruct { X = 100, Y = -200 }, InlineValueTypes.Case16(new InlineValueTypes.InnerStruct { X = 100, Y = -200 }).AsCase16);
    }

    [Fact]
    public void InlineValueTypesGenericProperties()
    {
        Assert.Equal(typeof(object), typeof(InlineValueTypesGeneric<double, float, int>).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(float), typeof(InlineValueTypesGeneric<double, float, int>).GetField("_U", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }
}