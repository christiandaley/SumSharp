namespace Tests;

using Dotsum;

[Case("Ok", "T")]
[Case("Error", "U")]
public partial class Result<T, U>
{

}

public class ResultTests
{

    [Fact]
    public void Ok()
    {
        var result = Result<double, string>.Ok(4.5);

        Assert.Equal(0, result.Index);

        Assert.Equal(result, Result<double, string>.Ok(4.5));
    }

    [Fact]
    public void Error()
    {
        var result = Result<double, string>.Error("error");

        Assert.Equal(1, result.Index);

        // value equality
        Assert.Equal(Result<double, string>.Error("error"), result);

        // reference equality
        Assert.True(Result<double, string>.Error("error") == result);
    }

    [Fact]
    public void Switch()
    {
        var result = Result<double, string>.Ok(4.5);

        bool passed = false;

        result.Switch(
            _ => { passed = true; },
            _ => { });

        Assert.True(passed);
    }

    [Fact]
    public void Match()
    {
        var result = Result<double, string>.Error("3.0");

        var value = result.Match(
            i => i + 1.0,
            err => double.Parse(err));

        Assert.Equal(3.0, value);
    }
}