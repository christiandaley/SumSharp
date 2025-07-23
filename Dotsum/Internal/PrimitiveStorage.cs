using System;

namespace Dotsum.Internal;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
public struct PrimitiveStorage
{
    [System.Runtime.InteropServices.FieldOffset(0)] public bool _bool;
    [System.Runtime.InteropServices.FieldOffset(0)] public byte _byte;
    [System.Runtime.InteropServices.FieldOffset(0)] public sbyte _sbyte;
    [System.Runtime.InteropServices.FieldOffset(0)] public short _short;
    [System.Runtime.InteropServices.FieldOffset(0)] public ushort _ushort;
    [System.Runtime.InteropServices.FieldOffset(0)] public int _int;
    [System.Runtime.InteropServices.FieldOffset(0)] public uint _uint;
    [System.Runtime.InteropServices.FieldOffset(0)] public long _long;
    [System.Runtime.InteropServices.FieldOffset(0)] public ulong _ulong;
    [System.Runtime.InteropServices.FieldOffset(0)] public float _float;
    [System.Runtime.InteropServices.FieldOffset(0)] public double _double;
}
