namespace Tests;

using Dotsum;
using System.Text.Json;
using System.Text.Json.Nodes;


public partial class StandardJsonSerialization
{
    [Case("Case0", typeof(string))]
    [Case("Case1", typeof(NestedRecord1))]
    [Case("Case2")]
    [Case("Case3", typeof(int))]
    [Case("Case4", typeof(double))]
    [Storage(StorageStrategy.InlineValueTypes)]
    [EnableJsonSerialization]
    partial class NonGenericType
    {
        public record NestedRecord1(int Arg1, double Arg2, NestedRecord1.NestedRecord2 Arg3)
        {
            public record NestedRecord2(string Arg1);
        }
    }

    [Case("Case0", "T")]
    [Case("Case1", "U[]")]
    [Case("Case2", "GenericType<V, Dictionary<T, T>, U>")]
    [EnableJsonSerialization]
    partial class GenericType<T, U, V> 
        where T : notnull
        where U : notnull
        where V : notnull
    {

    }

    partial class Outer<T>
    {
        public partial class Nested1<U>
        {
            [Case("Case0", "T")]
            [Case("Case1", "U")]
            [Case("Case2", "V")]
            [EnableJsonSerialization]
            public partial class Nested2<V>
            {

            }
        }
    }

    partial record Container
    {
        [Case("Case0", typeof(int))]
        [EnableJsonSerialization]
        public partial class ReferenceType
        {

        }

        [Case("Case0", typeof(int))]
        [EnableJsonSerialization]
        public partial struct ValueType
        {

        }

        public ReferenceType R { get; set; } = default!;

        public ValueType V { get; set; } = default;
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

    [Fact]
    public void NonGenericTypeCase3()
    {
        var value = NonGenericType.Case3(-157);

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""3"": -157
        }")));

        var deserializedValue = JsonSerializer.Deserialize<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase4()
    {
        var value = NonGenericType.Case4(18.54321);

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""4"": 18.54321
        }")));

        var deserializedValue = JsonSerializer.Deserialize<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void GenericTypeCase0()
    {
        var value = GenericType<string, double, int>.Case0("abc");

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""0"": ""abc""
        }")));

        var deserializedValue = JsonSerializer.Deserialize<GenericType<string, double, int>>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void GenericTypeCase1()
    {
        var value = GenericType<string, double, int>.Case1([2.1, 3.2]);

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""1"": [2.1, 3.2]
        }")));

        var deserializedValue = JsonSerializer.Deserialize<GenericType<string, double, int>>(json)!;

        Assert.Equal([2.1, 3.2], deserializedValue.AsCase1);
    }

    [Fact]
    public void GenericTypeCase2()
    {
        var value = GenericType<string, double, int>.Case2(
            GenericType<int, Dictionary<string, string>, double>.Case2(
                GenericType<double, Dictionary<int, int>, Dictionary<string, string>>.Case1([
                    new()
                    {
                        [0] = 1,
                        [2] = 3,
                    },
                    new()
                    {
                        [4] = 5,
                    }
                    ])
                )
            );

        var json = JsonSerializer.Serialize(value);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""2"": {
                ""2"": {
                    ""1"": [{
                        ""0"": 1,
                        ""2"": 3
                    }, {
                        ""4"": 5
                    }]
                }
            }
        }")));

        var deserializedValue = JsonSerializer.Deserialize<GenericType<string, double, int>>(json)!;

        Assert.True(deserializedValue.IsCase2);

        Assert.Collection(deserializedValue.AsCase2.AsCase2.AsCase1, dict1 =>
        {
            Assert.Equal(2, dict1.Count);
            Assert.Equal(1, dict1[0]);
            Assert.Equal(3, dict1[2]);
        }, 
        dict2 =>
        {
            Assert.Single(dict2);
            Assert.Equal(5, dict2[4]);
        });
    }

    [Fact]
    public void NestedGenericTypes()
    {
        var value = Outer<string>.Nested1<byte>.Nested2<double[]>.Case1(30);

        var options = new JsonSerializerOptions
        {
            Converters = { new Outer<string>.Nested1<byte>.Nested2.StandardJsonConverter() }
        };

        var json = JsonSerializer.Serialize(value, options);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""1"": 30
        }")));

        var deserializedValue = JsonSerializer.Deserialize(json, value.GetType(), options)!;

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NullClassValue()
    {
        var container = new Container();

        var json = JsonSerializer.Serialize(container);

        var deserializedValue = JsonSerializer.Deserialize<Container>(json)!;

        Assert.Null(deserializedValue.R);
    }

    [Fact]
    public void NullStructValue()
    {
        var container = new Container();

        var json = @"{ ""V"": null }";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Container>(json)!);
    }
}