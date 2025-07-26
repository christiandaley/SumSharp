using System;

namespace SumSharp;

public class EnableOneOfConversionsAttribute : Attribute
{
    public EnableOneOfConversionsAttribute()
    {

    }

    public EnableOneOfConversionsAttribute(Type EmptyCase)
    {

    }
}