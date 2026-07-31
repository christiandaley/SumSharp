#pragma warning disable CS0183

namespace Tests;

using SumSharp;

using System;

public partial class Equals
{

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(double))]
    partial class StringOrDouble
    {

    }

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(double))]
    [DisableValueEquality]
    partial class StringOrDoubleNoValueEquality
    {

    }

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(double))]
    partial record StringOrDoubleRecord
    {

    }

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(double))]
    partial record struct StringOrDoubleRecordStruct
    {

    }

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(double))]
    [UnionCase("Case2", typeof(string))]
    [UnionCase("Case3", typeof(double))]
    [UnionCase("Case4", "T")]
    partial class StringOrDoubleExtended<T>
    {

    }

    [Fact]
    public void ValueEquality()
    {
        Assert.Equal(StringOrDouble.Case0("abc"), StringOrDouble.Case0("abc"));
        Assert.Equal(StringOrDouble.Case1(3.45), StringOrDouble.Case1(3.45));

        Assert.NotEqual(StringOrDouble.Case0("abc"), StringOrDouble.Case1(3.45));
        Assert.NotEqual(StringOrDouble.Case1(3.45), StringOrDouble.Case0("abc"));

        Assert.True(StringOrDouble.Case0("") is IEquatable<StringOrDouble>);
    }

    [Fact]
    public void NoValueEquality()
    {
        Assert.NotEqual(StringOrDoubleNoValueEquality.Case0("abc"), StringOrDoubleNoValueEquality.Case0("abc"));
        Assert.NotEqual(StringOrDoubleNoValueEquality.Case1(3.45), StringOrDoubleNoValueEquality.Case1(3.45));

        Assert.NotEqual(StringOrDoubleNoValueEquality.Case0("abc"), StringOrDoubleNoValueEquality.Case1(3.45));
        Assert.NotEqual(StringOrDoubleNoValueEquality.Case1(3.45), StringOrDoubleNoValueEquality.Case0("abc"));

        Assert.True(StringOrDouble.Case0("") is not IEquatable<StringOrDoubleNoValueEquality>);
    }

    [Fact]
    public void RecordEquality()
    {
        Assert.Equal(StringOrDoubleRecord.Case0("abc"), StringOrDoubleRecord.Case0("abc"));
        Assert.Equal(StringOrDoubleRecord.Case1(3.45), StringOrDoubleRecord.Case1(3.45));

        Assert.NotEqual(StringOrDoubleRecord.Case0("abc"), StringOrDoubleRecord.Case1(3.45));
        Assert.NotEqual(StringOrDoubleRecord.Case1(3.45), StringOrDoubleRecord.Case0("abc"));

        Assert.True(StringOrDoubleRecord.Case0("") is IEquatable<StringOrDoubleRecord>);
    }

    [Fact]
    public void RecordStructEquality()
    {
        Assert.Equal(StringOrDoubleRecordStruct.Case0("abc"), StringOrDoubleRecordStruct.Case0("abc"));
        Assert.Equal(StringOrDoubleRecordStruct.Case1(3.45), StringOrDoubleRecordStruct.Case1(3.45));

        Assert.NotEqual(StringOrDoubleRecordStruct.Case0("abc"), StringOrDoubleRecordStruct.Case1(3.45));
        Assert.NotEqual(StringOrDoubleRecordStruct.Case1(3.45), StringOrDoubleRecordStruct.Case0("abc"));

        Assert.True(StringOrDoubleRecordStruct.Case0("") is IEquatable<StringOrDoubleRecordStruct>);
    }

    [Fact]
    public void UnderlyingValueEquality()
    {
        Assert.True("abc" == StringOrDoubleExtended<string>.Case0("abc"));
        Assert.True("efg" != StringOrDoubleExtended<string>.Case0("abc"));
        Assert.True(3.45 != StringOrDoubleExtended<string>.Case0("abc"));

        Assert.True(StringOrDoubleExtended<string>.Case0("abc") == "abc");
        Assert.True(StringOrDoubleExtended<string>.Case0("abc") != "efg");
        Assert.True(StringOrDoubleExtended<string>.Case0("abc") != 3.45);

        Assert.True(StringOrDoubleExtended<string>.Case1(3.45) == 3.45);
        Assert.True(StringOrDoubleExtended<string>.Case1(3.45) != 3.46);
        Assert.True(StringOrDoubleExtended<string>.Case1(3.45) != "abc");

        Assert.True(3.45 == StringOrDoubleExtended<string>.Case1(3.45));
        Assert.True(3.46 != StringOrDoubleExtended<string>.Case1(3.45));
        Assert.True("abc" != StringOrDoubleExtended<string>.Case1(3.45));

        Assert.True("abc" == StringOrDoubleExtended<string>.Case2("abc"));
        Assert.True("efg" != StringOrDoubleExtended<string>.Case2("abc"));
        Assert.True(3.45 != StringOrDoubleExtended<string>.Case2("abc"));

        Assert.True(StringOrDoubleExtended<string>.Case2("abc") == "abc");
        Assert.True(StringOrDoubleExtended<string>.Case2("abc") != "efg");
        Assert.True(StringOrDoubleExtended<string>.Case2("abc") != 3.45);

        Assert.True(StringOrDoubleExtended<string>.Case3(3.45) == 3.45);
        Assert.True(StringOrDoubleExtended<string>.Case3(3.45) != 3.46);
        Assert.True(StringOrDoubleExtended<string>.Case3(3.45) != "abc");

        Assert.True(3.45 == StringOrDoubleExtended<string>.Case3(3.45));
        Assert.True(3.46 != StringOrDoubleExtended<string>.Case3(3.45));
        Assert.True("abc" != StringOrDoubleExtended<string>.Case3(3.45));

        Assert.True("abc" == StringOrDoubleExtended<string>.Case4("abc"));
        Assert.True("efg" != StringOrDoubleExtended<string>.Case4("abc"));
        Assert.True(3.45 != StringOrDoubleExtended<string>.Case4("abc"));

        Assert.True(StringOrDoubleExtended<string>.Case4("abc") == "abc");
        Assert.True(StringOrDoubleExtended<string>.Case4("abc") != "efg");
        Assert.True(StringOrDoubleExtended<string>.Case4("abc") != 3.45);
    }
}