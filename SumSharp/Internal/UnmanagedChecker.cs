using System;
using System.Runtime.InteropServices;

namespace SumSharp.Internal;

public static class UnmanagedChecker
{
    public static void Check<TStorage, TUnion>(Type unmanagedType)
    {
        if (unmanagedType.IsEnum)
        {
           unmanagedType = unmanagedType.GetEnumUnderlyingType();
        }

        var storage = Marshal.SizeOf<TStorage>();

        var requiredStorage = Marshal.SizeOf(unmanagedType);

        if (storage < requiredStorage)
        {
            throw new ArgumentException($"The unmanaged type {unmanagedType.Name} requires {requiredStorage} bytes of storage but {typeof(TUnion).Name} has only {storage} bytes available to store unmanaged types");
        }
    }
}
