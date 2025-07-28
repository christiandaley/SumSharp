#pragma warning disable CS0649

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

struct GenericStruct<T>
{
    public T Value1;
    public T Value2;
}


[Case("Case0", "(GenericStruct<T>, GenericStruct<U>)", ForceUnmanagedStorage: true)]
[Storage(StorageStrategy.InlineValueTypes, UnmanagedStorageSize: 16)]
partial class GenericUnmanagedStorage<T, U> 
    where T : unmanaged
    where U : unmanaged
{

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
    static void JsonSerializationTest1()
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

    static void JsonSerializationTest2()
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

    public static void UnmanagedStorageTest1()
    {

        try
        {
            var _ = GenericUnmanagedStorage<double, ulong>.Case0(default);
        }
        catch (TypeInitializationException ex)
        {
            //Console.WriteLine(ex);

            return;
        }

        throw new Exception("UnmanagedStorageTest1 failed");
    }

    public static void UnmanagedStorageTest2()
    {

        try
        {
            var _ = GenericUnmanagedStorage<float, ulong>.Case0(default);
        }
        catch (TypeInitializationException ex)
        {
            //Console.WriteLine(ex);

            return;
        }

        throw new Exception("UnmanagedStorageTest2 failed");
    }

    public static void UnmanagedStorageTest3()
    {
        var _ = GenericUnmanagedStorage<int, float>.Case0(default);
    }

    public static void Main()
    {
        JsonSerializationTest1();

        JsonSerializationTest2();

        UnmanagedStorageTest1();

        UnmanagedStorageTest2();

        UnmanagedStorageTest3();
    }
}
