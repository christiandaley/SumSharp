#pragma warning disable CS0649

namespace Tests;

using System.Reflection;
using System.Runtime.CompilerServices;
using SumSharp;

public partial class Storage
{
    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(bool))]
    [UnionCase("Case2", typeof(byte[]))]
    partial struct DefaultStorage
    {

    }

    [UnionCase("Case0", typeof(double))]
    [UnionCase("Case1", typeof(double))]
    partial class SingleUniqueType
    {

    }

    [UnionCase("Case0", typeof(string), StorageMode.Inline)]
    [UnionCase("Case1", typeof(bool))]
    [UnionCase("Case2", typeof(byte[]))]
    [UnionCase("Case3", typeof(int), StorageMode.AsObject)]
    [UnionCase("Case4", typeof(float))]
    [UnionCase("Case5", typeof(IntPtr))]
    [UnionCase("Case6", typeof(UIntPtr))]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class VariousStorageModes
    {

    }


    [UnionCase("Case0", typeof(int))]
    [UnionCase("Case1", typeof(float))]
    [UnionCase("Case2", typeof(double))]
    [UnionCase("Case3", typeof(string))]
    [UnionCase("Case4", typeof(int[]))]
    [UnionCase("Case5", typeof(int))]
    [UnionCase("Case6", typeof(Single))]
    [UnionCase("Case7", typeof(long))]
    [UnionCase("Case8", typeof(double))]
    [UnionCase("Case9", typeof(long))]
    [UnionCase("Case10", typeof(ushort))]
    [UnionCase("Case11", typeof(short))]
    [UnionCase("Case12", typeof(ushort))]
    [UnionCase("Case13", typeof(short))]
    [UnionCase("Case14", typeof(InnerStruct), ForceUnmanagedStorage: true)]
    [UnionCase("Case15", typeof(ulong))]
    [UnionCase("Case16", typeof(InnerStruct), ForceUnmanagedStorage: true)]
    [UnionCase("Case17", typeof(InnerEnum))]
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


    [UnionCase("Case0", "T")]
    [UnionCase("Case1", "U")]
    [UnionCase("Case2", "V")]
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

            [UnionCase("Case0", "T")]
            [UnionCase("Case1", "U")]
            [UnionCase("Case2", "V")]
            [UnionCase("Case3", "W")]
            [Storage(StorageStrategy.InlineValueTypes)]

            public partial struct NestedGeneric<W> where W : class
            {

            }
        }
    }

    [UnionCase("Case0", "Dictionary<int, T>", GenericTypeInfo: GenericTypeInfo.ReferenceType)]
    [UnionCase("Case1", "InnerStruct<U>", GenericTypeInfo: GenericTypeInfo.ValueType)]
    [UnionCase("Case2", "IEnumerable<V>", IsInterface: true)]
    [UnionCase("Case3", "InnerStruct<V>", GenericTypeInfo: GenericTypeInfo.ValueType)]
    [UnionCase("Case4", "InnerClass<V>", GenericTypeInfo: GenericTypeInfo.ReferenceType)]
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

    [UnionCase("Case0", typeof(InnerStruct), StorageMode: StorageMode.Inline, ForceUnmanagedStorage: true)]
    [UnionCase("Case1")]
    [Storage(UnmanagedStorageSize: 1)]
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


    [UnionCase("Case0", "InnerStruct<(T, T)>", ForceUnmanagedStorage: true)]
    [UnionCase("Case1", typeof(TypeCode))]
    [UnionCase("Case2", "T", ForceUnmanagedStorage: true)]
    [UnionCase("Case3")]
    [Storage(StorageStrategy.InlineValueTypes, UnmanagedStorageSize: 4)]
    partial class GenericUnmanagedType<T> where T : unmanaged
    {
        public struct InnerStruct<U>
        {
            public U Value1;
            public U Value2;
        }
    }

    [UnionCase("Case0", typeof(InnerStruct))]
    [UnionCase("Case1")]
    [Storage(StorageStrategy.InlineValueTypes)]
    partial class InsideAssemblyStruct
    {
        public struct InnerStruct
        {
            public double Value1;
            public double Value2;
        }
    }

    [UnionCase("Case0", typeof(HashCode), ForceUnmanagedStorage: true)]
    [UnionCase("Case1")]
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
    public void GenericUnmanagedTypeProperties()
    {
        Assert.Equal(typeof(SumSharp.Internal.Generated.Tests_Storage_GenericUnmanagedType_T_.UnmanagedStorage), typeof(GenericUnmanagedType<byte>).GetField("_unmanagedStorage", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(2, typeof(GenericUnmanagedType<byte>).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Length);
    }

    [Fact]
    public void GenericUnmanagedTypeThrows()
    {
        Assert.Throws<TypeInitializationException>(() => GenericUnmanagedType<double>.Case3);
        Assert.Throws<TypeInitializationException>(() => GenericUnmanagedType<int>.Case3);
        Assert.Throws<TypeInitializationException>(() => GenericUnmanagedType<short>.Case3);

        Assert.True(GenericUnmanagedType<byte>.UnmanagedStorageSize >= 4);
    }


    [Fact]
    public void InsideAssemblyStructProperties()
    {
        Assert.NotNull(typeof(InsideAssemblyStruct).GetField("_unmanagedStorage", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType);
        Assert.Equal(2, typeof(InsideAssemblyStruct).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Length);
    }

    [Fact]
    public void OutsideAssemblyStructStorageSize()
    {
        Assert.True(OutsideAssemblyStruct.UnmanagedStorageSize >= Unsafe.SizeOf<HashCode>());
    }
}