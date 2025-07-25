using System;

using System.Runtime.InteropServices;

namespace SumSharp.Internal;

[StructLayout(LayoutKind.Explicit)]
public struct PrimitiveStorage
{
    [FieldOffset(0)] public bool _bool;
    [FieldOffset(0)] public byte _byte;
    [FieldOffset(0)] public sbyte _sbyte;
    [FieldOffset(0)] public short _short;
    [FieldOffset(0)] public ushort _ushort;
    [FieldOffset(0)] public int _int;
    [FieldOffset(0)] public uint _uint;
    [FieldOffset(0)] public long _long;
    [FieldOffset(0)] public ulong _ulong;
    [FieldOffset(0)] public float _float;
    [FieldOffset(0)] public double _double;
    [FieldOffset(0)] public nint _nint;
    [FieldOffset(0)] public nuint _nuint;
}
