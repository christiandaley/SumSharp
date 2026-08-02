using System;

namespace SumSharp.Internal;

public sealed class Box<T>(T value) : IEquatable<Box<T>>
{
    public readonly T Value = value;

    public bool Equals(Box<T> other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
            
        return Equals(Value, other.Value);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;

        return Equals(obj as Box<T>);
    }

    public override int GetHashCode() => Value.GetHashCode();
}
