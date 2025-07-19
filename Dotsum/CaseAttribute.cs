using System;

namespace Dotsum;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class CaseAttribute : Attribute
{
    public CaseAttribute(string name, Type type)
    {

    }

    public CaseAttribute(string name, string typeName)
    {

    }

    public CaseAttribute(string name)
    {

    }
}