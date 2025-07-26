using System;
using System.Runtime.InteropServices;

namespace SumSharp.Internal;

public static class UnmanagedChecker<T>
{
    public static void Check(int storageSize)
    {
        var requiredStorage = Marshal.SizeOf<T>();

        if (storageSize < requiredStorage)
        {
            throw new ArgumentException($"The type {typeof(T).Name} requires {requiredStorage} bytes of storage but the UnmanagedStorageSize given is {storageSize}");
        }
    }
}
