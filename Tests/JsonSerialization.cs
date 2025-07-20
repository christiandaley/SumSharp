namespace Tests;

using Dotsum;
using System.Text.Json;

public class JsonSerialization
{

    [Fact]
    public void Result()
    {
        Result<double, string> okResult = 7.6;

        Result<double, string> errorResult = "fail";

        var json1 = JsonSerializer.Serialize(okResult);

        var json2 = JsonSerializer.Serialize(errorResult);

        Assert.Equal(okResult, JsonSerializer.Deserialize<Result<double, string>>(json1));

        Assert.Equal(errorResult, JsonSerializer.Deserialize<Result<double, string>>(json2));
    }

    [Fact]
    public void IntOptionalSerialization()
    {
        IntOptional i1 = 7;

        IntOptional i2 = IntOptional.None;

        var json1 = JsonSerializer.Serialize(i1);

        var json2 = JsonSerializer.Serialize(i2);

        Assert.Equal(i1, JsonSerializer.Deserialize<IntOptional>(json1));

        Assert.Equal(i2, JsonSerializer.Deserialize<IntOptional>(json2));
    }

}