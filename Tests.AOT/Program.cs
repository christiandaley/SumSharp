// See https://aka.ms/new-console-template for more information

using SumSharp;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Tests.AOT;


[Case("Case0", "T")]
[Case("Case1", "U")]
[JsonConverter(typeof(GenericUnion.StandardJsonConverter))]
[EnableJsonSerialization(AddJsonConverterAttribute: false)]
partial class GenericUnion<T, U>
{

}

static partial class GenericUnion
{
    public partial class StandardJsonConverter : JsonConverterFactory
    {

    }
}

[JsonSerializable(typeof(GenericUnion<string, double>))]
[JsonSerializable(typeof(GenericUnion<Dictionary<int, string[]>, byte>))]
[JsonSerializable(typeof(Dictionary<int, string[]>))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(double))]
partial class JsonSerializerContext : System.Text.Json.Serialization.JsonSerializerContext
{

}

public class Program
{
    static void Test1()
    {
        var value = GenericUnion<string, double>.Case1(4.567);

        var json = JsonSerializer.Serialize(value, JsonSerializerContext.Default.GenericUnionStringDouble);

        var passed = JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""1"": 4.567
        }"));

        if (!passed)
        {
            throw new Exception($"Invalid json: {json}");
        }

        var deserializedValue = JsonSerializer.Deserialize(json, JsonSerializerContext.Default.GenericUnionStringDouble);

        if (value != deserializedValue)
        {
            throw new Exception($"{value} is not equal to {deserializedValue}");
        }
    }

    static void Test2()
    {
        var value = GenericUnion<Dictionary<int, string[]>, byte>.Case0(new ()
        {
            [0] = ["0"],
            [-1] = ["a", "b", "cdef"]
        });

        var json = JsonSerializer.Serialize(value, JsonSerializerContext.Default.GenericUnionDictionaryInt32StringArrayByte);

        var passed = JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(@"
        {
            ""0"": {
                ""0"": [""0""],
                ""-1"": [""a"", ""b"", ""cdef""]
            }
        }"));

        if (!passed)
        {
            throw new Exception($"Invalid json: {json}");
        }

        var deserializedValue = JsonSerializer.Deserialize(json, JsonSerializerContext.Default.GenericUnionDictionaryInt32StringArrayByte);

        if (value.AsCase0[0].Single() != "0")
        {
            throw new Exception($"{value} is not equal to {deserializedValue}");
        }
        if (!Enumerable.SequenceEqual(value.AsCase0[-1], ["a", "b", "cdef"]))
        {
            throw new Exception($"{value} is not equal to {deserializedValue}");
        }
    }

    public static void Main()
    {
        Test1();

        Test2();
    }
}
