namespace Tests;

using SumSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class NewtonsoftJsonSerialization
{
    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(NestedRecord1))]
    [UnionCase("Case2")]
    [UnionCase("Case3", typeof(int))]
    [UnionCase("Case4", typeof(double))]
    [UnionStorage(UnionStorageStrategy.InlineValueTypes)]
    [EnableJsonSerialization(JsonSerializationSupport.Newtonsoft)]
    partial class NonGenericType
    {
        public record NestedRecord1(int Arg1, double Arg2, NestedRecord1.NestedRecord2 Arg3)
        {
            public record NestedRecord2(string Arg1);
        }
    }

    [UnionCase("Case0", "T")]
    [UnionCase("Case1", "U[]")]
    [UnionCase("Case2", "GenericType<V, Dictionary<T, T>, U>")]
    [EnableJsonSerialization(JsonSerializationSupport.Newtonsoft)]
    partial struct GenericType<T, U, V>
        where T : notnull
        where U : notnull
        where V : notnull
    {

    }

    [UnionCase("Case0", "T")]
    [UnionCase("Case1", "InnerStruct<T>")]
    [UnionCase("Case2", "(StandardAndNewtonsoft<U, T>, StandardAndNewtonsoft<T, U>)")]
    [EnableJsonSerialization(JsonSerializationSupport.Standard | JsonSerializationSupport.Newtonsoft)]
    partial class StandardAndNewtonsoft<T, U>
    {
        public struct InnerStruct<TValue>
        {
            public TValue Value { get; init; }
        }
    }

    internal partial class Outer<T>
    {
        public partial class Nested1<U>
        {
            [UnionCase("Case0", "T")]
            [UnionCase("Case1", "U")]
            [UnionCase("Case2", "V")]
            [EnableJsonSerialization(JsonSerializationSupport.Newtonsoft)]
            public partial class Nested2<V>
            {

            }
        }
    }
    partial record struct Container()
    {
        [UnionCase("Case0", typeof(int))]
        [EnableJsonSerialization]
        public partial class ReferenceType
        {

        }

        [UnionCase("Case0", typeof(int))]
        [EnableJsonSerialization]
        public partial struct ValueType
        {

        }

        public ReferenceType R { get; set; } = default!;

        public ValueType V { get; set; } = ValueType.Case0(0);
    }

    [Fact]
    public void NonGenericTypeCase0()
    {
        var value = NonGenericType.Case0("abc");

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""0"": ""abc""
        }")));

        var deserializedValue = JsonConvert.DeserializeObject<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase0Null()
    {
        var value = NonGenericType.Case0(null!);

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""0"": null
        }")));

        var deserializedValue = JsonConvert.DeserializeObject<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase1()
    {
        var value = NonGenericType.Case1(new(4, 3.3, new("abc")));

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""1"": {
                ""Arg1"": 4,
                ""Arg2"": 3.3,
                ""Arg3"": {
                    ""Arg1"": ""abc""
                }
            }
        }")));

        var deserializedValue = JsonConvert.DeserializeObject<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase2()
    {
        var value = NonGenericType.Case2;

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""2"": null
        }")));

        var deserializedValue = JsonConvert.DeserializeObject<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase3()
    {
        var value = NonGenericType.Case3(-157);

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""3"": -157
        }")));

        var deserializedValue = JsonConvert.DeserializeObject<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NonGenericTypeCase4()
    {
        var value = NonGenericType.Case4(18.54321);

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""4"": 18.54321
        }")));

        var deserializedValue = JsonConvert.DeserializeObject<NonGenericType>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void GenericTypeCase0()
    {
        var value = GenericType<string, double, int>.Case0("abc");

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""0"": ""abc""
        }")));

        var deserializedValue = JsonConvert.DeserializeObject<GenericType<string, double, int>>(json);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void GenericTypeCase1()
    {
        var value = GenericType<string, double, int>.Case1([2.1, 3.2]);

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""1"": [2.1, 3.2]
        }")));

        var deserializedValue = JsonConvert.DeserializeObject<GenericType<string, double, int>>(json)!;

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

        var json = JsonConvert.SerializeObject(value);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
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

        var deserializedValue = JsonConvert.DeserializeObject<GenericType<string, double, int>>(json)!;

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

        var settings = new JsonSerializerSettings
        {
            Converters = { new Outer<string>.Nested1<byte>.Nested2.NewtonsoftJsonConverter() },
        };

        var json = JsonConvert.SerializeObject(value, settings);

        Assert.True(JObject.DeepEquals(JObject.Parse(json), JObject.Parse(@"
        {
            ""1"": 30
        }")));

        var deserializedValue = JsonConvert.DeserializeObject(json, value.GetType(), settings)!;

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NewtonsoftToStandard()
    {
        var left = StandardAndNewtonsoft<double, string>.Case0(8.9);

        var right = StandardAndNewtonsoft<string, double>.Case1(
            new()
            {
                Value = "abc",
            });

        var value = StandardAndNewtonsoft<string, double>.Case2((left, right));

        var json = JsonConvert.SerializeObject(value);

        // IncludeFields is needed for it to work on tuple types
        var options = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };

        var deserializedValue = System.Text.Json.JsonSerializer.Deserialize(json, value.GetType(), options);

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void StandardToNewtonsoft()
    {
        var left = StandardAndNewtonsoft<double, string>.Case0(8.9);

        var right = StandardAndNewtonsoft<string, double>.Case1(
            new()
            {
                Value = "abc",
            });

        var value = StandardAndNewtonsoft<string, double>.Case2((left, right));

        // IncludeFields is needed for it to work on tuple types
        var options = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };

        var json = System.Text.Json.JsonSerializer.Serialize(value, options);

        var deserializedValue = JsonConvert.DeserializeObject(json, value.GetType());

        Assert.Equal(value, deserializedValue);
    }

    [Fact]
    public void NullClassValue()
    {
        var container = new Container();

        var json = JsonConvert.SerializeObject(container);

        var deserializedValue = JsonConvert.DeserializeObject<Container>(json)!;

        Assert.Null(deserializedValue.R);
    }

    [Fact]
    public void NullStructValue()
    {
        var json = @"{ ""V"": null }";

        Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Container>(json)!);
    }
}