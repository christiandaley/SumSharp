using System;

namespace Dotsum;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class CaseAttribute : Attribute
{
    public CaseAttribute(string Name, Type Type, StorageMode StorageMode = StorageMode.Default)
    {

    }

    public CaseAttribute(string Name, string TypeName, StorageMode StorageMode = StorageMode.Default)
    {

    }

    public CaseAttribute(string Name)
    {

    }
}