namespace Tests;

using SumSharp;

public partial class Equals
{

    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(double))]
    partial class StringOrDouble
    {

    }

    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(double))]
    [DisableValueEquality]
    partial class StringOrDoubleNoValueEquality
    {

    }

    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(double))]
    partial record StringOrDoubleRecord
    {

    }

    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(double))]
    partial record struct StringOrDoubleRecordStruct
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
}