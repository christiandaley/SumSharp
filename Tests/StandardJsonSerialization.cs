namespace Tests;

using Dotsum;
using System.Text.Json;
using System.Text.Json.Nodes;


public partial class StandardJsonSerialization
{
    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(NestedRecord1))]
    [Case("Case2")]
    [EnableJsonSerialization]
    partial class NonGenericType
    {
        public record NestedRecord1(int Arg1, double Arg2, NestedRecord1.NestedRecord2 Arg3)
        {
            public record NestedRecord2(string Arg1);
        }
    }

    [Fact]
    public void NonGenericTypeCase0()
    {
        var value = NonGenericType.Case0("abc");

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""0"": ""abc""
        }")));

        var deserializedValue = JsonSerializer.Deserialize<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase0Null()
    {
        var value = NonGenericType.Case0(null);

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""0"": null
        }")));

        var deserializedValue = JsonSerializer.Deserialize<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase1()
    {
        var value = NonGenericType.Case1(new(4, 3.3, new("abc")));

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""1"": {
                ""Arg1"": 4,
                ""Arg2"": 3.3,
                ""Arg3"": {
                    ""Arg1"": ""abc""
                }
            }
        }")));

        var deserializedValue = JsonSerializer.Deserialize<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase2()
    {
        var value = NonGenericType.Case2;

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""2"": null
        }")));

        var deserializedValue = JsonSerializer.Deserialize<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }
}