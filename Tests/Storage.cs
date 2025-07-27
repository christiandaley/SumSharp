namespace Tests;

using System.Reflection;
using System.Runtime.CompilerServices;
using SumSharp;

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
    [Case("Case5", typeof(IntPtr))]
    [Case("Case6", typeof(UIntPtr))]
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
    [Case("Case14", typeof(InnerStruct), UnmanagedTypeSize: 24)]
    [Case("Case15", typeof(ulong))]
    [Case("Case16", typeof(InnerStruct), UnmanagedTypeSize: 24)]
    [Case("Case17", typeof(InnerEnum))]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class InlineValueTypes
    {
        internal struct InnerStruct
        {
            public long X;
            public long Y;
            public InnerEnum InnerEnum;
        }

        public enum InnerEnum : sbyte
        {
            Value1 = -2,
            Value2 = -1,
        }
    }


    [Case("Case0", "T")]
    [Case("Case1", "U")]
    [Case("Case2", "V")]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class InlineValueTypesGeneric<T, U, V> where U : struct
    {

    }

    public static partial class Generic1<T, U> 
        where T : struct
        where U : class
    {
        public static partial class Generic2<V> where V : struct
        {

            [Case("Case0", "T")]
            [Case("Case1", "U")]
            [Case("Case2", "V")]
            [Case("Case3", "W")]
            [Storage(StorageStrategy.InlineValueTypes)]

            public partial struct NestedGeneric<W> where W : class
            {

            }
        }
    }

    [Case("Case0", "Dictionary<int, T>", GenericTypeInfo: GenericTypeInfo.ReferenceType)]
    [Case("Case1", "InnerStruct<U>", GenericTypeInfo: GenericTypeInfo.ValueType)]
    [Case("Case2", "IEnumerable<V>", IsInterface: true)]
    [Case("Case3", "InnerStruct<V>", GenericTypeInfo: GenericTypeInfo.ValueType)]
    [Case("Case4", "InnerClass<V>", GenericTypeInfo: GenericTypeInfo.ReferenceType)]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class GenericWithTypeInfo<T, U, V>
    {
        public struct InnerStruct<W>
        {

        }

        public class InnerClass<W>
        {

        }
    }

    [Case("Case0", typeof(InnerStruct), StorageMode: StorageMode.Inline, UnmanagedTypeSize: 24)]
    [Case("Case1")]
    partial class InsufficientStorage
    {
        public struct InnerStruct
        {
            public long Value1;
            public long Value2;
            public long Value3;
            public long Value4;
        }
    }


    [Case("Case0", "InnerStruct<(T, T)>", UnmanagedTypeSize: 1)]
    [Case("Case1", typeof(TypeCode))]
    [Case("Case2", "T", UnmanagedTypeSize: 1)]
    [Case("Case3")]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class GenericUnmanagedType<T> where T : unmanaged
    {
        public struct InnerStruct<U>
        {
            public U Value1;
            public U Value2;
        }
    }

    [Case("Case0", typeof(HashCode), IsUnmanaged: true)]
    [Case("Case1")]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class OutsideAssemblyStruct
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
        Assert.NotNull(typeof(SingleUniqueType).GetField("_unmanagedStorage", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void VariousStorageModesProperties()
    {
        Assert.Equal(typeof(string), typeof(VariousStorageModes).GetField("_string", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.NotNull(typeof(VariousStorageModes).GetField("_unmanagedStorage", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(object), typeof(VariousStorageModes).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(4, typeof(VariousStorageModes).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Length);
    }

    [Fact]
    public void InlineValueTypesProperties()
    {
        Assert.Equal(typeof(object), typeof(InlineValueTypes).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.NotNull(typeof(InlineValueTypes).GetField("_unmanagedStorage", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(3, typeof(InlineValueTypes).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Length);
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

        Assert.Equal(InlineValueTypes.InnerEnum.Value2, InlineValueTypes.Case17(InlineValueTypes.InnerEnum.Value2).AsCase17);
    }

    [Fact]
    public void InlineValueTypesGenericProperties()
    {
        Assert.Equal(typeof(object), typeof(InlineValueTypesGeneric<double, float, int>).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(float), typeof(InlineValueTypesGeneric<double, float, int>).GetField("_U", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void NestedGenericProperties()
    {
        Assert.Equal(typeof(bool), typeof(Generic1<bool, string>.Generic2<double>.NestedGeneric<byte[]>).GetField("_T", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(object), typeof(Generic1<bool, string>.Generic2<double>.NestedGeneric<byte[]>).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(double), typeof(Generic1<bool, string>.Generic2<double>.NestedGeneric<byte[]>).GetField("_V", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Null(typeof(Generic1<bool, string>.Generic2<double>.NestedGeneric<byte[]>).GetField("_W", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void GenericWithTypeInfoProperties()
    {
        Assert.Equal(typeof(object), typeof(GenericWithTypeInfo<int, float, double>).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(GenericWithTypeInfo<int, float, double>.InnerStruct<float>), typeof(GenericWithTypeInfo<int, float, double>).GetField("_InnerStruct_U_", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(typeof(GenericWithTypeInfo<int, float, double>.InnerStruct<double>), typeof(GenericWithTypeInfo<int, float, double>).GetField("_InnerStruct_V_", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
    }

    [Fact]
    public void InsufficientStorageThrows()
    {
        Assert.Throws<TypeInitializationException>(() => InsufficientStorage.Case1);
    }

    [Fact]
    public void GenericUnmanagedTypeStorage()
    {
        Assert.Throws<TypeInitializationException>(() => GenericUnmanagedType<double>.Case3);
        Assert.Throws<TypeInitializationException>(() => GenericUnmanagedType<int>.Case3);
        Assert.Throws<TypeInitializationException>(() => GenericUnmanagedType<short>.Case3);

        Assert.True(GenericUnmanagedType<byte>.UnmanagedStorageSize >= 4);
    }

    [Fact]
    public void OutsideAssemblyStructStorageSize()
    {
        Assert.True(OutsideAssemblyStruct.UnmanagedStorageSize >= Unsafe.SizeOf<HashCode>());
    }
}