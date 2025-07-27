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
    /// <param name="IsUnmanaged">If true, indicates that the type meets the unmanaged constraint. If false, SumSharp will attempt to determine if the type meets the unmanaged constraint 
    /// only if the type and all of its members are defined in the same assembly as the generated code. If the type or any of its members are defined in an outside assembly, SumSharp will not be
    /// able to determine if the type is unmanaged and will assume that it is managed</param>
    /// <param name="UnmanagedTypeSize">The minimum number of bytes needed to store the unmanaged type (i.e the type's size). Setting this parameter implies <paramref name="IsUnmanaged"/> == true</param>
    public CaseAttribute(
        string Name, 
        Type Type, 
        StorageMode StorageMode = StorageMode.Default,
        bool IsUnmanaged = false,
        int UnmanagedTypeSize = 0)
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
    /// <param name="IsUnmanaged">If true, indicates that the type meets the unmanaged constraint. If false, SumSharp will assume that the type is not
    /// unmanaged unless <paramref name="UnmanagedTypeSize"/> is greater than 0</param>
    /// <param name="UnmanagedTypeSize">The minimum number of bytes needed to store the unmanaged type (i.e the type's size). Setting this parameter implies <paramref name="IsUnmanaged"/> == true</param>
    /// <param name="GenericTypeInfo">Info about whether the generic type is a value type, reference type, or potentially both</param>
    /// <param name="IsInterface">Whether or not the generic type is an interface</param>
    public CaseAttribute(
        string Name, 
        string GenericTypeName, 
        StorageMode StorageMode = StorageMode.Default,
        bool IsUnmanaged = false,
        int UnmanagedTypeSize = 0,
        GenericTypeInfo GenericTypeInfo = GenericTypeInfo.ReferenceType | GenericTypeInfo.ValueType, 
        bool IsInterface = false)
    {

    }
}