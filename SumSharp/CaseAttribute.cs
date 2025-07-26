using System;

namespace SumSharp;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class CaseAttribute : Attribute
{
    /// <summary>
    /// An empty case with name <paramref name="Name"/>
    /// </summary>
    /// <param name="Name">The name that uniquely identifies the case</param>
    public CaseAttribute(string Name)
    {

    }

    /// <summary>
    /// A case with name <paramref name="Name"/> and type <paramref name="Type"/>. The method of storage is
    /// determined by <paramref name="StorageMode"/>
    /// </summary>
    /// <param name="Name">The name that uniquely identifies the case</param>
    /// <param name="Type">The type of value associated with the case</param>
    /// <param name="StorageMode">The storage mode to use</param>
    public CaseAttribute(
        string Name, 
        Type Type, 
        StorageMode StorageMode = StorageMode.Default,
        int UnmanagedStorageSize = 0)
    {

    }

    /// <summary>
    /// A case with name <paramref name="Name"/> and generic type <paramref name="GenericTypeName"/>. The method of storage is
    /// determined by <paramref name="StorageMode"/>. <paramref name="IsInterface"/> specifies whether or not the generic type
    /// is an interface. <paramref name="GenericTypeInfo"/> can be used to specify whether the generic type is always reference 
    /// or value type.
    /// </summary>
    /// <param name="Name">The name that uniquely identifies the case</param>
    /// <param name="GenericTypeName">A string that represents the name of the generic type associated with the case</param>
    /// <param name="StorageMode">The storage mode to use</param>
    /// <param name="GenericTypeInfo">Info about whether the generic type is a value type, reference type, or potentially both</param>
    /// <param name="IsInterface">Whether or not the generic type is an interface</param>
    public CaseAttribute(
        string Name, 
        string GenericTypeName, 
        StorageMode StorageMode = StorageMode.Default,
        int UnmanagedStorageSize = 0,
        GenericTypeInfo GenericTypeInfo = GenericTypeInfo.ReferenceType | GenericTypeInfo.ValueType, 
        bool IsInterface = false)
    {

    }
}