using System;

namespace Dotsum;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class CaseAttribute : Attribute
{
    public CaseAttribute(string Name, Type Type)
    {

    }

    public CaseAttribute(string Name, string TypeName)
    {

    }

    public CaseAttribute(string Name)
    {

    }
}