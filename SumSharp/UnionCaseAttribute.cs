using System;

namespace SumSharp;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class UnionCaseAttribute : Attribute
{
    /// <summary>
    /// An empty case with name <paramref name="Name"/>
    /// </summary>
    /// <param name="Name">The name that uniquely identifies the case</param>
    public UnionCaseAttribute(string Name)
    {

    }

    /// <summary>
    /// A case with name <paramref name="Name"/> and type <paramref name="Type"/>. The method of storage is
    /// determined by <paramref name="Storage"/>
    /// </summary>
    /// <param name="Name">The name that uniquely identifies the case</param>
    /// <param name="Type">The type of value associated with the case</param>
    /// <param name="Storage">The type of storage to use</param>
    public UnionCaseAttribute(
        string Name, 
        Type Type, 
        UnionCaseStorage Storage = UnionCaseStorage.Default)
    {

    }

    /// <summary>
    /// A case with name <paramref name="Name"/> and generic type <paramref name="GenericTypeName"/>. The method of storage is
    /// determined by <paramref name="Storage"/>. <paramref name="IsInterface"/> specifies whether or not the generic type
    /// is an interface. <paramref name="GenericTypeInfo"/> can be used to specify whether the generic type is always reference 
    /// or value type.
    /// </summary>
    /// <param name="Name">The name that uniquely identifies the case</param>
    /// <param name="GenericTypeName">A string that represents the name of the generic type associated with the case</param>
    /// <param name="Storage">The type of storage to use</param>
    /// <param name="UseUnmanagedStorage">If true, indicates that the type meets the unmanaged constraint and may share storage with other unmanaged types.
    /// SumSharp will never use unmanaged storage for a generic type unless <paramref name="UseUnmanagedStorage"/> is true. The UnmanagedStorageSize must 
    /// be explicitly set in the <see cref="UnionStorageAttribute"/> to use unmanaged storage for generic types.</param>
    /// <param name="GenericTypeInfo">Info about whether the generic type is a value type, reference type, or potentially both. Providing this is never required
    /// but can help SumSharp emit more efficient code</param>
    /// <param name="IsInterface">Whether or not the generic type is an interface</param>
    public UnionCaseAttribute(
        string Name, 
        string GenericTypeName, 
        UnionCaseStorage Storage = UnionCaseStorage.Default,
        bool UseUnmanagedStorage = false,
        GenericTypeInfo GenericTypeInfo = GenericTypeInfo.ReferenceType | GenericTypeInfo.ValueType,
        bool IsInterface = false)
    {

    }
}